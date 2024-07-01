using UnityEngine;
using UnityEditor;
using SLZ.Data;
using SLZ.Combat;
using System.Net;
using SLZ.Marrow.Warehouse;
using System.Collections.Generic;
using System.Linq;
using EvroDev.LevelEditorTool.Tools;
using EvroDev.LevelEditorTool.Tabs;

namespace EvroDev.LevelEditorTool
{
    public class LevelEditorTool : EditorWindow
    {
        int tabValue = 0;
        EditorToolTab[] editorToolTabs;

        Vector2 overallScroll;

        private bool isDragging = false;
        public bool allowDrag = false;



        [MenuItem("Tools/EvroDev/Level Editor Tool")]
        public static void ShowWindow()
        {
            GetWindow<LevelEditorTool>("Level Editor Tool");
        }

        void OnEnable()
        {
            RegisterTabs();

            SceneView.duringSceneGui += OnSceneGUI;
        }

        void RegisterTabs()
        {
            editorToolTabs = new EditorToolTab[] { new LevelConstructorTab(this), new PrefabPlacerTab(this), new SpawnablesPlacerTab(this) };
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (!allowDrag) return;

            HandleMouseDrag(sceneView);
        }

        void OnGUI()
        {
            tabValue = GUILayout.Toolbar(tabValue, editorToolTabs.Select(obj => obj.Name).ToArray());
            overallScroll = GUILayout.BeginScrollView(overallScroll, GUILayout.Width(0));

            editorToolTabs[tabValue].RenderTab();

            GUILayout.EndScrollView();
        }

        void HandleMouseDrag(SceneView sceneView)
        {
            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            switch (e.type)
            {
                case EventType.MouseDown when e.button == 0:
                    isDragging = true;
                    e.Use();
                    GUIUtility.hotControl = controlID;

                    editorToolTabs[tabValue].OnClickDown(e.mousePosition);

                    break;

                case EventType.MouseDrag when isDragging:
                    editorToolTabs[tabValue].OnClickHold(e.mousePosition);

                    SceneView.RepaintAll();
                    break;

                case EventType.MouseMove:
                    editorToolTabs[tabValue].OnMouseMove(e.mousePosition);

                    SceneView.RepaintAll();
                    break;

                case EventType.MouseUp when e.button == 0 && isDragging:
                    isDragging = false;
                    e.Use();
                    GUIUtility.hotControl = 0;

                    editorToolTabs[tabValue].OnClickUp(e.mousePosition);

                    break;
            }
        }

    }

    public class LevelEditorTempPreviewer : MonoBehaviour
    {

        public Vector3 startPos;
        public Vector3 endPos;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawCube(startPos - (startPos - endPos) / 2, startPos - endPos);
            //Gizmos.DrawSphere(startPos, 0.1f);
        }
    }

    public class SpawnablePlacerTempPreviewer : MonoBehaviour
    {
        public Vector3 position;
        public Vector3 meshPosOffset = Vector3.zero;
        public Quaternion rotation;
        public Mesh mesh;
        public MeshOffset[] meshOffsets = new MeshOffset[0];

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            foreach (MeshOffset meshOffset in meshOffsets)
            {
                Vector3 rotatedPositionOffset = rotation * meshOffset.positionOffset;
                Vector3 finalPosition = position + rotatedPositionOffset;

                // Combine the overall rotation with the mesh's specific rotation offset
                Quaternion finalRotation = rotation * Quaternion.Inverse(meshOffset.rotationOffset);

                Gizmos.DrawMesh(meshOffset.mesh, 0, finalPosition /*position + meshOffset.positionOffset*/, rotation, meshOffset.meshScale);
            }
            if (mesh != null)
            {
                Gizmos.DrawMesh(mesh, 0, position + meshPosOffset, rotation);
            }
        }

        public void FillMeshOffsetsFromObject(GameObject obj)
        {
            var meshFilters = obj.GetComponentsInChildren<MeshFilter>();

            var list = new List<MeshOffset>();

            foreach (MeshFilter meshFilter in meshFilters)
            {
                // Calculate the local position offset taking into account the parent's rotation
                Vector3 localPositionOffset = meshFilter.transform.position - obj.transform.position;
                // Rotate the local position offset by the parent object's rotation to get the correctly oriented offset
                Vector3 rotatedPositionOffset = obj.transform.rotation * localPositionOffset;

                list.Add(new MeshOffset(rotatedPositionOffset, meshFilter.sharedMesh, meshFilter.transform.lossyScale, Quaternion.FromToRotation(Vector3.forward, meshFilter.transform.forward)));
            }

            meshOffsets = list.ToArray();
        }

        public struct MeshOffset
        {
            public Vector3 positionOffset;
            public Mesh mesh;
            public Vector3 meshScale;
            public Quaternion rotationOffset;

            public MeshOffset(Vector3 pos, Mesh mesh, Vector3 scale, Quaternion rotation)
            {
                this.positionOffset = pos;
                this.mesh = mesh;
                this.meshScale = scale;
                this.rotationOffset = rotation;
            }
        }
    }
}