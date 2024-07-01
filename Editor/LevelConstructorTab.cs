using EvroDev.LevelEditorTool;
using EvroDev.LevelEditorTool.Tools;
using SLZ.Combat;
using SLZ.Data;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EvroDev.LevelEditorTool.Tabs
{

    public class LevelConstructorTab : EditorToolTab
    {
        public override string Name => "Level Constructor";

        private int constructionStage = 0;
        private List<Vector3> selectedPoints = new List<Vector3>();

        private float snapValue = 1;
        private static Material selectedMaterial;
        private bool showMaterialChoser;
        private static float uvScale = 1;
        private static SurfaceData surfaceData;
        private static float colliderDepth = 1;
        private static int selectedTab;
        private Vector2 matScrollPos;
        private Vector3 planeNormal;
        private Vector3 dragStartPoint;
        private LevelEditorTempPreviewer tempPreviewer;

        private int mouseSelectionSnapMode = 0; //0 = normal snap to grid, 1 = snap to grid on a plane, 2 = snap to grid on a line
        private Vector3 mouseTrackPlanePoint = Vector3.zero;
        private Vector3 mouseTrackPlaneNormal;
        private Vector3 mouseTrackLinePoint;
        private Vector3 mouseTrackLineDirection;

        public LevelConstructorTab(LevelEditorTool currentEditor) : base(currentEditor)
        {
        }

        public override void OnEnable()
        {

        }

        public override void RenderTab()
        {
            GUILayout.Label("Custom Mesh Creator", EditorStyles.boldLabel);

            RaycastTools.snapValue = EditorGUILayout.FloatField("Snap To Nearest", RaycastTools.snapValue);
            selectedMaterial = EditorGUILayout.ObjectField("Plane Material", selectedMaterial, typeof(Material), false) as Material;
            if (GUILayout.Button(showMaterialChoser ? "Close" : "Select Quick Material", GUILayout.Width(150))) showMaterialChoser = !showMaterialChoser;
            if (showMaterialChoser) RenderMaterialChoser();

            uvScale = EditorGUILayout.FloatField("UV Scale", uvScale);
            surfaceData = EditorGUILayout.ObjectField("Impact Properties", surfaceData, typeof(SurfaceData), false) as SurfaceData;
            colliderDepth = EditorGUILayout.FloatField("Collider Depth", colliderDepth);
            selectedTab = GUILayout.Toolbar(selectedTab, new string[] { "Floor/Ceiling", "Wall"/*, "Ramp", "Bevel Edge", "Bevel Corner"*/ });

            allowDrag = GUILayout.Toggle(allowDrag, "Toggle Object Dragging (breaks selecting while enabled)");
            //GUILayout.Label(constructionStage.ToString());
        }

        void RenderMaterialChoser()
        {
            matScrollPos = GUILayout.BeginScrollView(matScrollPos, GUILayout.Height(110));
            GUILayout.BeginHorizontal();

            foreach (string mat in GetQuickMaterialGUIDs())
            {
                Material thisMat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(mat));
                if (GUILayout.Button(thisMat.mainTexture, GUILayout.Width(100), GUILayout.Height(100))) selectedMaterial = thisMat;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }

        string[] GetQuickMaterialGUIDs()
        {
            string formattedFolderPath = "Assets/LevelEditorTool/QuickMaterials";

            string[] foundAssets = AssetDatabase.FindAssets("t:Material", new string[] { formattedFolderPath });

            return foundAssets;
        }

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
            if(tempPreviewer != null)
                tempPreviewer.endPos = FinalPositionGetter(mousePosition);
        }

        public override void OnClickHold(Vector2 mousePosition)
        {
        }

        public override void OnClickUp(Vector2 mousePosition)
        {
            // End of drag
            //Vector3 dragEndPoint = RaycastTools.GetPlaneIntersectionWithRay(mousePosition, planeNormal, dragStartPoint);
            //CreateCustomPlane(dragStartPoint, dragEndPoint, planeNormal); // Adjust this method to create plane based on start/end
            //DestroyImmediate(tempPreviewer.gameObject);
        }

        Vector3 FinalPositionGetter(Vector2 mousePosition)
        {
            if(mouseSelectionSnapMode == 0)
            {
                return RaycastTools.GetWorldPosition(mousePosition);
            }
            if(mouseSelectionSnapMode == 1)
            {
                return RaycastTools.GetPlaneIntersectionWithRay(mousePosition, planeNormal, selectedPoints[0]);
            }
            if(mouseSelectionSnapMode == 2)
            {
                return RaycastTools.GetNearestPointOnLine(mousePosition, mouseTrackLinePoint, mouseTrackLineDirection);
            }
            return Vector3.zero;
        }



        void AdvanceConstructionStage(Vector3 position)
        {
            constructionStage++;
            selectedPoints.Add(position);

            if(selectedTab == 0)
            {
                if(constructionStage == 1)
                {
                    tempPreviewer = new GameObject("Temp Previewer").AddComponent<LevelEditorTempPreviewer>();
                    tempPreviewer.startPos = position;
                    tempPreviewer.endPos = position;

                    planeNormal = CamAboveHorizonLine() ? Vector3.down : Vector3.up;

                    mouseSelectionSnapMode = 1;
                }
                else if (constructionStage == 2)
                {
                    CreateCustomPlane(selectedPoints[0], position, planeNormal);
                    ClearConstruction();
                }

            }

            if (selectedTab == 1)
            {
                if (constructionStage == 1)
                {
                    tempPreviewer = new GameObject("Temp Previewer").AddComponent<LevelEditorTempPreviewer>();
                    tempPreviewer.startPos = position;
                    tempPreviewer.endPos = position;

                    planeNormal = RaycastTools.GetCardinalDirectionTowardsSceneViewCamera();

                    mouseSelectionSnapMode = 1;
                }
                else if (constructionStage == 2)
                {
                    CreateCustomPlane(selectedPoints[0], position, planeNormal);
                    ClearConstruction();
                }

            }

            if(selectedTab == 2)
            {
                if (constructionStage == 1)
                {
                    tempPreviewer = new GameObject("Temp Previewer").AddComponent<LevelEditorTempPreviewer>();
                    tempPreviewer.startPos = position;
                    tempPreviewer.endPos = position;

                    planeNormal = Vector3.up;

                    Debug.Log("Switched to Plane at " + position);

                    mouseSelectionSnapMode = 1;
                }
                else if(constructionStage == 2)
                {
                    SwitchModeLine(position, Vector3.up);
                    Debug.Log("Switched to Line at " + position);
                }
                else if (constructionStage == 3)
                {
                    Debug.Log("Finalized at " + position);
                    CreateSlope(selectedPoints[0], selectedPoints[1], position, RaycastTools.GetCardinalDirectionTowardsSceneViewCamera(), false);
                    ClearConstruction();
                }
            }
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

            if(tempPreviewer != null)
            {
                DestroyImmediate(tempPreviewer.gameObject);
            }
        }



        void CreateCustomPlane(Vector3 startPos, Vector3 endPos, Vector3 normal)
        {
            Vector3 midpoint = startPos - (startPos - endPos) / 2;

            float width = 0;
            float length = 0;

            if (normal == Vector3.up)
            {
                width = Mathf.Abs(startPos.x - endPos.x);
                length = Mathf.Abs(startPos.z - endPos.z);


                CreateCustomPlane(midpoint, width, length, selectedMaterial, Quaternion.identity);
            }
            else if (normal == Vector3.down)
            {

                width = Mathf.Abs(startPos.x - endPos.x);
                length = Mathf.Abs(startPos.z - endPos.z);


                CreateCustomPlane(midpoint, width, length, selectedMaterial, Quaternion.Euler(180, 0, 0));
            }
            else if (normal == Vector3.left || normal == Vector3.right)
            {
                Debug.Log("Created Left/Right");
                width = Mathf.Abs(startPos.z - endPos.z);
                length = Mathf.Abs(startPos.y - endPos.y);


                CreateCustomPlane(midpoint, width, length, selectedMaterial, Quaternion.LookRotation(Vector3.up, -normal));
            }
            else if (normal == Vector3.forward || normal == Vector3.back)
            {
                Debug.Log("Created Forward/Back");
                width = Mathf.Abs(startPos.x - endPos.x);
                length = Mathf.Abs(startPos.y - endPos.y);


                CreateCustomPlane(midpoint, width, length, selectedMaterial, Quaternion.LookRotation(Vector3.up, -normal));
            }
        }

        void CreateCustomPlane(Vector3 position, float width, float length, Material material, Quaternion rotation)
        {
            if (width == 0 || length == 0)
                return;

            // Create a new GameObject to hold the mesh
            GameObject customPlane = new GameObject("plane_" + width + "mx" + length + "m");
            customPlane.transform.position = position;

            // Add a MeshFilter and MeshRenderer to the GameObject
            MeshFilter meshFilter = customPlane.AddComponent<MeshFilter>();
            customPlane.AddComponent<MeshRenderer>();

            // Assign the custom mesh to the MeshFilter
            meshFilter.mesh = MeshCreator.GetOrCreatePlaneMesh(width, length, uvScale);

            // Assign material if provided
            if (material != null)
                customPlane.GetComponent<Renderer>().material = material;


            //Create Collider
            GameObject col = new GameObject("col_box");
            col.transform.parent = customPlane.transform;
            col.transform.localPosition = Vector3.zero;

            BoxCollider collider = col.AddComponent<BoxCollider>();
            collider.size = new Vector3(width, colliderDepth, length);
            collider.center = new Vector3(0, colliderDepth / -2, 0);

            if (surfaceData != null)
            {
                var impactProperties = col.AddComponent<ImpactProperties>();
                impactProperties.colliders = new Collider[] { collider };
                impactProperties.surfaceData = surfaceData;
                impactProperties.DecalMeshObj = customPlane;
            }


            // Final Setting stuff
            customPlane.transform.rotation = rotation;

            customPlane.layer = 13;
            col.layer = 13;
            customPlane.isStatic = true;
            col.isStatic = true;

            Undo.RegisterCreatedObjectUndo(customPlane, "Plane Creation");
            Undo.RegisterCreatedObjectUndo(col, "Col Creation");

            Debug.Log("Custom Plane Created!");
        }

        void CreateCustomBevel(Vector3 position, float size, float length, Material material, Quaternion rotation)
        {
            if (size == 0 || length == 0)
                return;

            // Create a new GameObject to hold the mesh
            GameObject customPlane = new GameObject("plane_" + size + "mx" + length + "m");
            customPlane.transform.position = position;

            // Add a MeshFilter and MeshRenderer to the GameObject
            MeshFilter meshFilter = customPlane.AddComponent<MeshFilter>();
            customPlane.AddComponent<MeshRenderer>();

            // Assign the custom mesh to the MeshFilter
            meshFilter.mesh = MeshCreator.GetOrCreatePlaneMesh(size, length);

            // Assign material if provided
            if (material != null)
                customPlane.GetComponent<Renderer>().material = material;


            //Create Collider
            GameObject col = new GameObject("col_box");
            col.transform.parent = customPlane.transform;
            col.transform.localPosition = Vector3.zero;

            BoxCollider collider = col.AddComponent<BoxCollider>();
            collider.size = new Vector3(size, colliderDepth, length);
            collider.center = new Vector3(0, colliderDepth / -2, 0);

            if (surfaceData != null)
            {
                var impactProperties = col.AddComponent<ImpactProperties>();
                impactProperties.colliders = new Collider[] { collider };
                impactProperties.surfaceData = surfaceData;
                impactProperties.DecalMeshObj = customPlane;
            }


            // Final Setting stuff
            customPlane.transform.rotation = rotation;

            customPlane.layer = 13;
            col.layer = 13;
            customPlane.isStatic = true;
            col.isStatic = true;

            Undo.RegisterCreatedObjectUndo(customPlane, "Plane Creation");
            Undo.RegisterCreatedObjectUndo(col, "Col Creation");

            Debug.Log("Custom Plane Created!");
        }

        void CreateSlope(Vector3 position, Vector2 size, float height, Material material, Quaternion rotation)
        {
            if (size.x == 0 || size.y == 0 || height == 0)
                return;

            // Create a new GameObject to hold the mesh
            GameObject customRamp = new GameObject("ramp_" + size.x + "x" + height + "x" + size.y);
            customRamp.transform.position = position;

            // Add a MeshFilter and MeshRenderer to the GameObject
            MeshFilter meshFilter = customRamp.AddComponent<MeshFilter>();
            customRamp.AddComponent<MeshRenderer>();

            // Assign the custom mesh to the MeshFilter
            meshFilter.mesh = MeshCreator.GetOrCreateSlopeMesh(size, height);

            // Assign material if provided
            if (material != null)
                customRamp.GetComponent<Renderer>().material = material;

            MeshCollider collider = customRamp.AddComponent<MeshCollider>();
            collider.sharedMesh = meshFilter.sharedMesh;

            if (surfaceData != null)
            {
                var impactProperties = customRamp.AddComponent<ImpactProperties>();
                impactProperties.colliders = new Collider[] { collider };
                impactProperties.surfaceData = surfaceData;
                impactProperties.DecalMeshObj = customRamp;
            }


            // Final Setting stuff
            customRamp.transform.rotation = rotation;

            customRamp.layer = 13;
            customRamp.isStatic = true;

            Undo.RegisterCreatedObjectUndo(customRamp, "Plane Creation");

            Debug.Log("Custom Plane Created!");
        }

        void CreateSlope(Vector3 startPos, Vector3 endPos, Vector3 heightPos, Vector3 facingVector, bool isTop)
        {
            Vector3 midpoint = startPos - (startPos - endPos) / 2;

            float height = (endPos - heightPos).magnitude;

            Vector2 baseDim = Vector2.zero;

            Vector3 forwardTopBottom = isTop ? Vector3.down : Vector3.up;

            
            if (facingVector == Vector3.left || facingVector == Vector3.right)
            {
                baseDim = new Vector2(Mathf.Abs(startPos.z - endPos.z), Mathf.Abs(startPos.x - endPos.x));

                CreateSlope(midpoint, baseDim, height, selectedMaterial, Quaternion.LookRotation(facingVector, forwardTopBottom));
            }
            else if (facingVector == Vector3.forward || facingVector == Vector3.back)
            {
                baseDim = new Vector2(Mathf.Abs(startPos.x - endPos.x), Mathf.Abs(startPos.z - endPos.z));

                Debug.Log("Created Forward/Back with dimensions: " + baseDim + height);

                CreateSlope(midpoint, baseDim, height, selectedMaterial, Quaternion.LookRotation(facingVector, forwardTopBottom));
            }
        }

        public static bool CamAboveHorizonLine()
        {
            Camera sceneViewCamera = SceneView.lastActiveSceneView.camera;

            if (sceneViewCamera == null)
            {
                Debug.LogWarning("Scene View Camera is null.");
                return false;
            }

            Vector3 cameraForward = sceneViewCamera.transform.forward;

            return cameraForward.y > 0;
        }
    }
}
