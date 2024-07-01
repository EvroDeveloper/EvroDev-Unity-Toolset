using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Decal Setup", menuName = "Level Editor Tool/Decal Setup")]
public class DecalSetup : ScriptableObject
{
    public Material material;
    public Mesh mesh;
    public Vector3 objectScale;

    public DecalSetup(GameObject realDecal)
    {
        Mesh mesh = realDecal.GetComponent<MeshFilter>().sharedMesh;

        this.mesh = mesh;

        material = realDecal.GetComponent<MeshRenderer>().sharedMaterial;
        objectScale = realDecal.transform.lossyScale;
    }
}
