#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenerationFeatureCreator))]
public class GenerationFeatureCreatorButton : Editor
{

    public override void OnInspectorGUI()
    {

        if(GUILayout.Button("Create Feature"))
        {
            GenerationFeatureCreator instance = (GenerationFeatureCreator)target;

            instance.GenerateFeature();
        }

        base.OnInspectorGUI();
    }

}
#endif