using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Block Switcher")]
public class BlockSwitcher : ScriptableObject
{
    [SerializeField] private GameObject roadBlockPrefab;
    public void SwitchBlocks()
    {
        int count = 0;
        GameObject obj = GameObject.Find("RoadBlock(Clone)");
        while (obj != null && count++ < 100)
        {
            Transform objTransform = obj.transform;
            Transform newRoadBlock = (PrefabUtility.InstantiatePrefab(roadBlockPrefab) as GameObject).transform;
            newRoadBlock.SetParent(objTransform.parent);
            newRoadBlock.SetPositionAndRotation(objTransform.position, objTransform.rotation);
            newRoadBlock.localScale = objTransform.localScale;
            DestroyImmediate(obj);
            EditorUtility.SetDirty(newRoadBlock);
            obj = GameObject.Find("RoadBlock(Clone)");
        }
    }
}
[CustomEditor(typeof(BlockSwitcher))]
public class BlockSwitcherEditor : Editor
{
    private BlockSwitcher blockSwitcher;
    private void OnEnable()
    {
        blockSwitcher = target as BlockSwitcher;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Switch"))
            blockSwitcher.SwitchBlocks();
    }
}