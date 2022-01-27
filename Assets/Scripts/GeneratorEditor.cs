using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (TileSetGenerator))]
public class GeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TileSetGenerator myScript = (TileSetGenerator)target;

        if(GUILayout.Button("Generate tiles"))
        {
            myScript.Generate();
        }

        if(GUILayout.Button("Clear tiles"))
        {
            myScript.ClearTiles();
        }
    }
}
