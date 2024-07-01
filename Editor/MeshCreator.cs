using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;

namespace EvroDev.LevelEditorTool
{
    public static class MeshCreator
    {
        public static bool TryFindMesh(string name, string folderPath, out Mesh mesh)
        {
            string formattedFolderPath = "Assets/" + folderPath.TrimStart('/');

            // Search for assets in the specified folder and filter by Mesh type
            string filter = name + " t:Mesh";
            string[] foundMeshes = AssetDatabase.FindAssets(filter);

            Debug.Log(foundMeshes.Length);

            if (foundMeshes.Length > 0)
            {
                // Assuming the first found asset is the desired one
                string firstFoundAssetPath = AssetDatabase.GUIDToAssetPath(foundMeshes[0]);
                mesh = AssetDatabase.LoadAssetAtPath<Mesh>(firstFoundAssetPath);

                if(mesh.name != name)
                    return false;

                // Return true if the mesh was found
                return mesh != null;
            }
            else
            {
                mesh = null;
                return false;
            }
        }

        public static Mesh GetOrCreatePlaneMesh(float width, float length, float uvScale = 1)
        {
            Mesh mesh;

            string name = "plane_" + width + "mx" + length + (uvScale != 1 ? "m_(" + uvScale + "x)" : "m");

            if (!TryFindMesh(name, "LevelEditorTool/LevelMeshes/Planes", out mesh))
            {
                // Create the custom mesh (AI Generated)
                mesh = CreateMesh(new Vector3[] {
                    new Vector3(-width / 2, 0, -length / 2),
                    new Vector3(width / 2, 0, -length / 2),
                    new Vector3(width / 2, 0, length / 2),
                    new Vector3(-width / 2, 0, length / 2) },

                new int[] {
                    0, 2, 1,
                    0, 3, 2 },
                
                new Vector2[] {
                    new Vector2(0, 0),
                    new Vector2(width * uvScale, 0),
                    new Vector2(width * uvScale, length * uvScale),
                    new Vector2(0, length * uvScale)
                }, 

                new Vector3[]
                {
                    Vector3.up,
                    Vector3.up,
                },
                name);

                SaveMeshInAssets(mesh, "LevelEditorTool/LevelMeshes/Planes", name);
            }

            return mesh;
        }

        public static Mesh GetOrCreateCurveMesh(float size, float length, float uvScale = 1, int bevelSegments = 3)
        {
            Mesh mesh;

            string name = "curve_" + size + "mx" + length + (uvScale != 1 ? "m_(" + uvScale + "x)" : "m");

            if (!TryFindMesh(name, "LevelEditorTool/LevelMeshes/BevelEdge", out mesh))
            {
                List<Vector3> vertices = new List<Vector3>();
                List<int> tris = new List<int>();
                List<Vector2> uvCoords = new List<Vector2>();

                for (int i = 0; i < bevelSegments; i++)
                {
                    float angle = i / bevelSegments * 90;

                    float vert = Mathf.Sin(angle) * size;
                    float horizontal = Mathf.Cos(angle) * size;

                    vertices.Add(new Vector3(-size / 2 + horizontal, vert, -length / 2));
                    vertices.Add(new Vector3(-size / 2 + horizontal, vert, length / 2));
                    uvCoords.Add(new Vector2(0, i / bevelSegments * size * uvScale));
                    uvCoords.Add(new Vector2(length, i / bevelSegments * size * uvScale));
                }

                for (int i = 0; i < bevelSegments - 1; i++)
                {
                    int row = i * 2;
                    int nextRow = (i + 1) * 2;
                    tris.Add(row);
                    tris.Add(row + 1);
                    tris.Add(nextRow + 1);
                    tris.Add(row);
                    tris.Add(nextRow);
                    tris.Add(nextRow + 1);
                }

                mesh = null;//CreateMesh(vertices.ToArray(), tris.ToArray(), uvCoords.ToArray() name);

                SaveMeshInAssets(mesh, "LevelEditorTool/LevelMeshes/Planes", name);
            }

            return mesh;
        }

        public static Mesh CreateMesh(Vector3[] vertices, int[] triangles, Vector2[] uv, Vector3[] normals, string name)
        {
            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            //mesh.normals = normals;

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            mesh.name = name;

            return mesh;
        }
        public static Mesh GetOrCreateSlopeMesh(Vector2 size, float height, float uvScale = 1)
        {
            Mesh mesh;

            string name = "ramp_" + size.x + "x" + size.y + "x" + height + (uvScale != 1 ? "_(" + uvScale + "x)" : "");

            if (!TryFindMesh(name, "LevelEditorTool/LevelMeshes/Ramps", out mesh))
            {
                // Create the custom mesh (AI Generated)
                float left = -size.x / 2;
                float right = size.x / 2;
                float back = -size.y / 2;
                float front = size.y / 2;

                float slopeAngle = Mathf.Atan(height / size.y);

                Vector3 slopeNormal = new Vector3(0, Mathf.Sin(slopeAngle), Mathf.Cos(slopeAngle));

                mesh = CreateMesh(new Vector3[] {
                    new Vector3(right, 0, back),
                    new Vector3(left, 0, back),
                    new Vector3(left, height, front),
                    new Vector3(right, height, front),

                    new Vector3(left, 0, front),
                    new Vector3(right, 0, front),
                    new Vector3(right, 0, back),
                    new Vector3(left, 0, back),

                    new Vector3(left, height, front),
                    new Vector3(right, height, front),
                    new Vector3(right, 0, front),
                    new Vector3(left, 0, front),

                    new Vector3(left, 0, front),
                    new Vector3(left, 0, back),
                    new Vector3(left, height, front),

                    new Vector3(right, 0, front),
                    new Vector3(right, 0, back),
                    new Vector3(right, height, front),
                },

               new int[] {
                    0, 2, 1, // Inclined surface
                    0, 3, 2,

                    4, 6, 5, // Bottom face
                    4, 7, 6,

                    8, 10, 9, 
                    8, 11, 10,

                    12, 13, 14, //left face

                    15, 16, 17, //right face

               },

                new Vector2[] {
                    new Vector2(0, size.x * uvScale),
                    new Vector2(0, 0),
                    new Vector2(size.y * uvScale, 0),
                    new Vector2(size.y * uvScale, size.x * uvScale),

                    new Vector2(0, 0),
                    new Vector2(size.x * uvScale, 0),
                    new Vector2(size.x * uvScale, size.y * uvScale),
                    new Vector2(0, size.y * uvScale),


                    new Vector2(size.x * uvScale, height * uvScale),
                    new Vector2(0, height * uvScale),
                    new Vector2(0, 0),
                    new Vector2(size.x * uvScale, 0),

                    new Vector2(size.y * uvScale, 0),
                    new Vector2(0, 0),
                    new Vector2(size.y * uvScale, height * uvScale),

                    new Vector2(0, 0),
                    new Vector2(size.y * uvScale, 0),
                    new Vector2(0, height * uvScale),
                    },

                new Vector3[]
                {
                    slopeNormal,
                    slopeNormal,
                    slopeNormal,
                    slopeNormal,

                    Vector3.down,
                    Vector3.down,
                    Vector3.down,
                    Vector3.down,

                    Vector3.forward,
                    Vector3.forward,
                    Vector3.forward,
                    Vector3.forward,

                    Vector3.right,
                    Vector3.right,
                    Vector3.right,

                    Vector3.right,
                    Vector3.right,
                    Vector3.right,
                },

                name);

                SaveMeshInAssets(mesh, "LevelEditorTool/LevelMeshes/Ramps", name);
            }

            return mesh;
        }


        // AI Generated Code
        public static void SaveMeshInAssets(Mesh mesh, string assetFolderPath, string assetName)
        {
            if (mesh == null)
            {
                Debug.LogError("SaveMeshInAssets was given a null mesh.");
                return;
            }

            if (!System.IO.Directory.Exists(Application.dataPath + "/" + assetFolderPath))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/" + assetFolderPath);
                AssetDatabase.Refresh();
            }

            string assetPath = "Assets/" + assetFolderPath + "/" + assetName + ".asset";

            // Check if the asset already exists to avoid overwriting
            if (AssetDatabase.LoadAssetAtPath<Mesh>(assetPath) == null)
            {
                // Save the mesh as an asset
                AssetDatabase.CreateAsset(mesh, assetPath);
                AssetDatabase.SaveAssets();
                Debug.Log("Mesh saved: " + assetPath);
            }
            else
            {
                Debug.Log("Asset already exists: " + assetPath);
            }

            // Refresh the AssetDatabase to show the new asset in the Unity Editor
            AssetDatabase.Refresh();
        }
    }
}
