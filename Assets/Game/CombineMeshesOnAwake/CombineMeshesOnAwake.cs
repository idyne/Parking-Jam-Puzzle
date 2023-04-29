using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCombiner))]
public class CombineMeshesOnAwake : MonoBehaviour
{
    [SerializeField] public bool destroyChildren = false;
    private void Awake()
    {
        MeshCombiner meshCombiner = GetComponent<MeshCombiner>();
        meshCombiner.CreateMultiMaterialMesh = false;
        meshCombiner.DestroyCombinedChildren = destroyChildren;
        meshCombiner.CombineMeshes(true);
    }

}
