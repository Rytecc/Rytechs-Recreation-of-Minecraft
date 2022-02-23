#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GenerationFeatureCreator : MonoBehaviour
{
    public string AssetName;
    public Transform BlocksParent;

    public void GenerateFeature()
    {
        BlockTypeIndicator[] BlockInformation = BlocksParent.gameObject.GetComponentsInChildren<BlockTypeIndicator>();

        GenerationFeature GeneratedFeature = new GenerationFeature();
        GeneratedFeature.StructuralBlockPositions = new BlockInfo[BlockInformation.Length];

        for(int i = 0; i < BlockInformation.Length; i++)
        {
            GeneratedFeature.StructuralBlockPositions[i].BlockType = BlockInformation[i].blockID;
            GeneratedFeature.StructuralBlockPositions[i].Pos = new Vector3Int(Mathf.RoundToInt(BlockInformation[i].transform.position.x),
                Mathf.RoundToInt(BlockInformation[i].transform.position.y),
                Mathf.RoundToInt(BlockInformation[i].transform.position.z));

        }

        Debug.Log("The rest of the parameters must be set within the inspector of the asset");

        AssetDatabase.CreateAsset(GeneratedFeature, $"Assets/{AssetName}.asset");
        AssetDatabase.Refresh();
    }

}

#endif
