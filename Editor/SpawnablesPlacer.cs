using EvroDev.LevelEditorTool.Tools;
using SLZ.Marrow.Warehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using System.Reflection;

namespace EvroDev.LevelEditorTool.Tabs
{
    internal class SpawnablesPlacerTab : EditorToolTab
    {
        //private SpawnableCrateReference spawnableCrate;
        private static CrateSpawner crate;
        private bool useSnapping;
        private float rotationSnapping;
        private SpawnablePlacerTempPreviewer tempPreviewer;
        private Vector3 dragStartPoint;
        private Vector3 dragStartNormal;
        private List<SpawnableCrate> recentsList = new List<SpawnableCrate>();
        private Vector2 matScrollPos;
        private bool showRecentsWindow;
        private SerializedObject serializedObject;
        private GameObject rotatingSpawnable;

        public SpawnablesPlacerTab(LevelEditorTool currentEditor) : base(currentEditor)
        {
        }

        public override string Name => "Spawnables Placer";

        public override void OnClickDown(Vector2 mousePosition)
        {
            dragStartNormal = RaycastTools.GetNormalOfRaycast(mousePosition);
            dragStartPoint = RaycastTools.GetWorldPosition(mousePosition, false) - dragStartNormal * (crate.spawnableCrateReference.Crate.ColliderBounds.center.y - crate.spawnableCrateReference.Crate.ColliderBounds.extents.y);

            //tempPreviewer = new GameObject("Temp Previewer").AddComponent<SpawnablePlacerTempPreviewer>();

            GameObject placer = new GameObject("TempName");
            placer.transform.position = dragStartPoint;
        
            placer.AddComponent<CrateSpawner>().spawnableCrateReference = new SpawnableCrateReference(crate.spawnableCrateReference.Barcode);

            rotatingSpawnable = placer;

            Undo.RegisterCreatedObjectUndo(placer, "Created Placer");
        }

        public override void OnClickHold(Vector2 mousePosition)
        {
            Quaternion rot = Quaternion.LookRotation((dragStartPoint - RaycastTools.GetPlaneIntersectionWithRay(mousePosition, dragStartNormal, dragStartPoint, false)) * -1, dragStartNormal);

            if (useSnapping) rot = rot.Round(rotationSnapping);

            rotatingSpawnable.transform.rotation = rot;
        }

        public override void OnClickUp(Vector2 mousePosition)
        {
            //Quaternion rot = Quaternion.LookRotation((dragStartPoint - RaycastTools.GetPlaneIntersectionWithRay(mousePosition, dragStartNormal, dragStartPoint, false)) * -1, dragStartNormal);
            //
            //if (useSnapping) rot = rot.Round(rotationSnapping);

            //PlaceSpawnable(dragStartPoint, rot);
            //DestroyImmediate(tempPreviewer.gameObject);
        }

        public override void OnEnable()
        {
        }

        public override void OnMouseMove(Vector2 mousePosition)
        {
        }

        public override void RenderTab()
        {
            GUILayout.Label("Spawnable Placer");

            if (crate == null)
            {
                if (GUILayout.Button("Add Crate Selector to Reference (required)"))
                {
                    var refObj = new GameObject("TempCrateReference");
                    crate = refObj.AddComponent<CrateSpawner>();
                    refObj.hideFlags = HideFlags.DontSave;
                    refObj.SetActive(false);

                    EditorUtility.OpenPropertyEditor(crate);
                }
            }
            else
            {
                //crate = (CrateSpawner)EditorGUILayout.ObjectField("Gather Crate From ", crate, typeof(CrateSpawner), true);
                if (GUILayout.Button("Open Crate Selector")) EditorUtility.OpenPropertyEditor(crate);
            }

            if (GUILayout.Button(showRecentsWindow ? "Close" : "Select Recent Crates", GUILayout.Width(150))) showRecentsWindow = !showRecentsWindow;
            if (showRecentsWindow) RenderRecents();

            useSnapping = EditorGUILayout.Toggle("Use Rotation Snapping", useSnapping);
            if (useSnapping)
                rotationSnapping = EditorGUILayout.FloatField("Nearest Degree", rotationSnapping);

            allowDrag = GUILayout.Toggle(allowDrag, "Toggle Click to Place");
        }

        void RenderRecents()
        {
            matScrollPos = GUILayout.BeginScrollView(matScrollPos, GUILayout.Height(100));
            //GUILayout.BeginHorizontal();

            foreach (SpawnableCrate spawnablecrate in recentsList)
            {
                if (GUILayout.Button(spawnablecrate.Title)) crate.spawnableCrateReference = new SpawnableCrateReference(spawnablecrate.Barcode);
            }

            //GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }

        void PlaceSpawnable(Vector3 position, Quaternion rotation)
        {
            GameObject placer = new GameObject("TempName");
            placer.transform.position = position;
            placer.transform.rotation = rotation;
        
            placer.AddComponent<CrateSpawner>().spawnableCrateReference = new SpawnableCrateReference(crate.spawnableCrateReference.Barcode);
        
            Undo.RegisterCreatedObjectUndo(placer, "Created Placer");
        
            if (recentsList.Contains(crate.spawnableCrateReference.Crate))
                recentsList.Remove(crate.spawnableCrateReference.Crate);
        
            if (recentsList.Count == 0)
                recentsList.Add(crate.spawnableCrateReference.Crate);
            else if (recentsList[0] != crate.spawnableCrateReference.Crate)
                recentsList.Insert(0, crate.spawnableCrateReference.Crate);
        
            if (recentsList.Count == 11)
                recentsList.RemoveAt(10);
        
            Selection.activeObject = placer;
        }
    }
}
