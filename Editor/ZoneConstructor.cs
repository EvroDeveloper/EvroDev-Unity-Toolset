using EvroDev.LevelEditorTool;
using EvroDev.LevelEditorTool.Tabs;
using EvroDev.LevelEditorTool.Tools;
using SLZ.Data;
using SLZ.Zones;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EvroDev.LevelEditorTool.Tabs
{
    public class ZoneConstructor : EditorToolTab
    {
        private Transform zonesParent;

        private int constructionStage;
        private Vector3 planeNormal;
        private int mouseSelectionSnapMode;
        private Vector3 mouseTrackLineDirection;
        private Vector3 mouseTrackLinePoint;
        private LevelEditorTempPreviewer tempPreviewer;
        private int selectedTab;

        private List<Vector3> selectedPoints = new List<Vector3>();

        public ZoneConstructor(LevelEditorTool currentEditor) : base(currentEditor)
        {
        }

        public override string Name => "Zone Constructor";



        public override void OnClickDown(Vector2 mousePosition)
        {
            if (tempPreviewer != null && constructionStage == 0)
            {
                DestroyImmediate(tempPreviewer.gameObject);
            }

            AdvanceConstructionStage(FinalPositionGetter(mousePosition));
        }

        public override void OnMouseMove(Vector2 mousePosition)
        {
            if (tempPreviewer != null)
                tempPreviewer.endPos = FinalPositionGetter(mousePosition);
        }

        public override void OnClickHold(Vector2 mousePosition)
        {
        }

        public override void OnClickUp(Vector2 mousePosition)
        {

        }

        Vector3 FinalPositionGetter(Vector2 mousePosition)
        {
            if (mouseSelectionSnapMode == 0)
            {
                return RaycastTools.GetWorldPosition(mousePosition);
            }
            if (mouseSelectionSnapMode == 1)
            {
                return RaycastTools.GetPlaneIntersectionWithRay(mousePosition, planeNormal, selectedPoints[0]);
            }
            if (mouseSelectionSnapMode == 2)
            {
                return RaycastTools.GetNearestPointOnLine(mousePosition, mouseTrackLinePoint, mouseTrackLineDirection);
            }
            return Vector3.zero;
        }



        void AdvanceConstructionStage(Vector3 position)
        {
            constructionStage++;
            selectedPoints.Add(position);

            if (selectedTab == 0)
            {
                if (constructionStage == 1)
                {
                    tempPreviewer = new GameObject("Temp Previewer").AddComponent<LevelEditorTempPreviewer>();
                    tempPreviewer.startPos = position;
                    tempPreviewer.endPos = position;

                    planeNormal = Vector3.up;

                    mouseSelectionSnapMode = 1;
                }
                else if (constructionStage == 2)
                {
                    SwitchModeLine(position, Vector3.up);
                }
                else if (constructionStage == 3)
                {
                    CreateZone(selectedPoints[0], position);
                    ClearConstruction();
                }

            }
            else if(selectedTab == 1)
            {
                if (constructionStage == 1)
                {
                    var output = RaycastTools.EvaluateRaycastFill(position);

                    CreateZoneCenterBounds(output.center, output.bounds);

                    ClearConstruction();
                }
            }
        }

        private void CreateZone(Vector3 startPos, Vector3 endPos)
        {
            GameObject zone = new GameObject("Scene Zone");

            Vector3 center = startPos - (startPos - endPos) / 2;
            Vector3 bounds = startPos - endPos;

            CreateZoneCenterBounds(center, bounds);
        }

        private void CreateZoneCenterBounds(Vector3 center, Vector3 bounds)
        {
            GameObject zone = new GameObject("Scene Zone");

            bounds.x = Mathf.Abs(bounds.x);
            bounds.y = Mathf.Abs(bounds.y);
            bounds.z = Mathf.Abs(bounds.z);

            zone.transform.position = center;

            BoxCollider zoneCol = zone.AddComponent<BoxCollider>();
            zoneCol.size = bounds;

            zone.transform.parent = zonesParent;

            zone.AddComponent<SceneZone>();
        }

        void SwitchModeLine(Vector3 linePosition, Vector3 lineDirection)
        {
            mouseSelectionSnapMode = 2;
            mouseTrackLineDirection = lineDirection;
            mouseTrackLinePoint = linePosition;
        }

        void ClearConstruction()
        {
            constructionStage = 0;
            selectedPoints.Clear();
            mouseSelectionSnapMode = 0;

            if (tempPreviewer != null)
            {
                DestroyImmediate(tempPreviewer.gameObject);
            }
        }

        public override void OnEnable()
        {
        }

        public override void RenderTab()
        {
            GUILayout.Label("Scene Zone Constructor", EditorStyles.boldLabel);

            zonesParent = EditorGUILayout.ObjectField("Zones Parent", zonesParent, typeof(Transform), true) as Transform;

            RaycastTools.snapValue = EditorGUILayout.FloatField("Snap To Nearest", RaycastTools.snapValue);

            selectedTab = GUILayout.Toolbar(selectedTab, new string[] { "Point Selections", "Raycast Fill (Experimental)" });

            allowDrag = GUILayout.Toggle(allowDrag, "Toggle Object Dragging (breaks selecting while enabled)");
        }
    }
}