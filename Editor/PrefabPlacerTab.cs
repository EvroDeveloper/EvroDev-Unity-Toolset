using EvroDev.LevelEditorTool.Tools;
using SLZ.Marrow.Warehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EvroDev.LevelEditorTool.Tabs
{
    public class PrefabPlacerTab : EditorToolTab
    {
        private GameObject prefabToPlace;
        private float rotationSnapping;
        private Vector3 dragStartPoint;
        private Vector3 dragStartNormal;
        private SpawnablePlacerTempPreviewer tempPreviewer;
        private bool useSnapping;
        private Vector3 planeNormal;
        private Transform rotatingTransform;

        private List<GameObject> recentsList = new List<GameObject>();
        private bool showRecentsWindow;
        private Vector2 matScrollPos;

        public PrefabPlacerTab(LevelEditorTool currentEditor) : base(currentEditor)
        {
        }

        public override string Name => "Prefab Placer";

        public override void OnClickDown(Vector2 mousePosition)
        {
            if(tempPreviewer != null)
                DestroyImmediate(tempPreviewer.gameObject);

            Bounds prefabBounds = GetObjectBounds(prefabToPlace);

            dragStartNormal = RaycastTools.GetNormalOfRaycast(mousePosition);
            dragStartPoint = RaycastTools.GetWorldPosition(mousePosition, false) - dragStartNormal.normalized * (prefabBounds.center.y - prefabBounds.extents.y);
            
            PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(prefabToPlace);

            GameObject newObject;

            if (prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant)
                newObject = PrefabUtility.InstantiatePrefab(prefabToPlace) as GameObject;
            else
                newObject = Instantiate(prefabToPlace);

            newObject.transform.position = dragStartPoint;

            rotatingTransform = newObject.transform;

            Undo.RegisterCreatedObjectUndo(newObject, "Instatiate Prefab");
        }

        public override void OnClickHold(Vector2 mousePosition)
        {
            Quaternion rot = Quaternion.LookRotation((dragStartPoint - RaycastTools.GetPlaneIntersectionWithRay(mousePosition, dragStartNormal, dragStartPoint, false)) * -1, dragStartNormal);

            if (useSnapping) rot = rot.Round(rotationSnapping);

            rotatingTransform.rotation = rot;
        }

        public override void OnClickUp(Vector2 mousePosition)
        {
            //var lastPos = RaycastTools.GetPlaneIntersectionWithRay(mousePosition, dragStartNormal, dragStartPoint, false);
            //Quaternion rot = Quaternion.LookRotation(Vector3.up, dragStartNormal);
            //if (lastPos != dragStartPoint)
            //    rot = Quaternion.LookRotation((dragStartPoint - RaycastTools.GetPlaneIntersectionWithRay(mousePosition, dragStartNormal, dragStartPoint, false)) * -1, dragStartNormal);
            //
            //if (useSnapping) rot = rot.Round(rotationSnapping);

            //PlacePrefab(dragStartPoint, rot);
            //DestroyImmediate(tempPreviewer.gameObject);

            rotatingTransform = null;
        }

        Bounds GetObjectBounds(GameObject rootObject)
        {
            Bounds bounds = new Bounds(rootObject.transform.position, Vector3.zero);

            Renderer[] renderers = rootObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        public override void OnEnable()
        {
            //throw new NotImplementedException();
        }

        public override void OnMouseMove(Vector2 mousePosition)
        {
        }

        public override void RenderTab()
        {
            GUILayout.Label("Prefab Placer");

            prefabToPlace = EditorGUILayout.ObjectField("Selected Prefab", prefabToPlace, typeof(GameObject), true) as GameObject;

            if (GUILayout.Button(showRecentsWindow ? "Close" : "Select Recent Prefabs", GUILayout.Width(150))) showRecentsWindow = !showRecentsWindow;
            if (showRecentsWindow) RenderRecentMaterials();

            useSnapping = EditorGUILayout.Toggle("Use Rotation Snapping", useSnapping);
            if (useSnapping)
                rotationSnapping = EditorGUILayout.FloatField("Nearest Degree", rotationSnapping);

            allowDrag = GUILayout.Toggle(allowDrag, "Toggle Click to Place");
        }

        void RenderRecentMaterials()
        {
            matScrollPos = GUILayout.BeginScrollView(matScrollPos, GUILayout.Height(100));
            //GUILayout.BeginHorizontal();

            foreach (GameObject go in recentsList)
            {
                if (GUILayout.Button(go.name)) prefabToPlace = go;
            }

            //GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }

        void PlacePrefab(Vector3 position, Quaternion rotation)
        {
            PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(prefabToPlace);

            GameObject newObject;

            if (prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant)
                newObject = PrefabUtility.InstantiatePrefab(prefabToPlace) as GameObject;
            else
                newObject = Instantiate(prefabToPlace);

            newObject.transform.position = position;
            newObject.transform.rotation = rotation;

            Undo.RegisterCreatedObjectUndo(newObject, "Instatiate Prefab");

            if (recentsList.Contains(prefabToPlace))
                recentsList.Remove(prefabToPlace);

            if (recentsList.Count == 0)
                recentsList.Add(prefabToPlace);
            else if (recentsList[0] != prefabToPlace)
                recentsList.Insert(0, prefabToPlace);

            if (recentsList.Count == 11)
                recentsList.RemoveAt(10);

            Selection.activeObject = newObject;
        }
    }
}
