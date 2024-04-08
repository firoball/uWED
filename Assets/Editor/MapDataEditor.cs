using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Callbacks;

[CustomEditor(typeof(MapData))]
public class MapDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapData map = (MapData)target;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear", GUILayout.Height(26)))
        {
            map.Clear();
        }
        /*if (GUILayout.Button("Edit", GUILayout.Height(26)))
        {
            uWED.OpenWindow();
        }*/
        GUILayout.EndHorizontal();
        base.OnInspectorGUI();

    }

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        Object target = EditorUtility.InstanceIDToObject(instanceID);

        if (target is MapData)
        {
            var path = AssetDatabase.GetAssetPath(instanceID);

            Selection.activeObject = target;
            UWed.OpenWindow();
            return true;
        }

        return false;
    }
}
