using EvroDev.LevelEditorTool.Tools;
using SLZ.Marrow.Warehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace EvroDev.LevelEditorTool.Tabs
{
    internal class DecalPlacerTab : EditorToolTab
    {
        public DecalPlacerTab(LevelEditorTool currentEditor) : base(currentEditor)
        {
        }

        public override string Name => "Decal Placer";

        private DecalSetup selectedDecal;
        private bool useSnapping;
        private float rotationSnapping;
        private Vector3 dragStartPoint;
        private Vector3 dragStartNormal;

        public override void OnClickDown(Vector2 mousePosition)
        {
            dragStartPoint = RaycastTools.GetWorldPosition(mousePosition, false);
            dragStartNormal = RaycastTools.GetNormalOfRaycast(mousePosition);

            //tempPreviewer = new GameObject("Temp Previewer").AddComponent<SpawnablePlacerTempPreviewer>();
            //
            //tempPreviewer.position = dragStartPoint;
            //
            //tempPreviewer.mesh = selectedDecal.mesh;
        }

        public override void OnClickHold(Vector2 mousePosition)
        {
            throw new NotImplementedException();
        }

        public override void OnClickUp(Vector2 mousePosition)
        {
            throw new NotImplementedException();
        }

        public override void OnEnable()
        {

        }

        public override void RenderTab()
        {
            GUILayout.Label("Spawnable Placer");
            selectedDecal = EditorGUILayout.ObjectField("Selected Spawnable", selectedDecal, typeof(DecalSetup), false) as DecalSetup;

            useSnapping = EditorGUILayout.Toggle("Use Rotation Snapping", useSnapping);
            if (useSnapping)
                rotationSnapping = EditorGUILayout.FloatField("Nearest Degree", rotationSnapping);

            allowDrag = GUILayout.Toggle(allowDrag, "Toggle Click to Place");
        }

        public override void OnMouseMove(Vector2 mousePosition)
        {
            throw new NotImplementedException();
        }
    }
}
