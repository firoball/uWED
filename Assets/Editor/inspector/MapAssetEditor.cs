using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.UIElements;

[CustomEditor(typeof(MapAsset))]
public class MapAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapAsset map = (MapAsset)target;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear", GUILayout.Height(26)))
        {
            map.Data.Clear();
        }
        /*if (GUILayout.Button("Edit", GUILayout.Height(26)))
        {
            uWED.OpenWindow();
        }*/
        GUILayout.EndHorizontal();
        base.OnInspectorGUI();

    }

    public override VisualElement CreateInspectorGUI()
    {
        MapAsset map = (MapAsset)target;
        VisualElement myInspector = new VisualElement();
        Button clear = new Button();
        clear.text = "Clear";
        clear.RegisterCallback<ClickEvent>((ClickEvent evt) => { map.Data.Clear(); });
        myInspector.Add(clear);
        myInspector.Add(new Label("This is a custom Inspector"));
        //myInspector.Add(base.CreateInspectorGUI());
        //return myInspector;
        return myInspector;
    }

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        Object target = EditorUtility.InstanceIDToObject(instanceID);

        if (target is MapAsset)
        {
            var map = AssetDatabase.GetAssetPath(instanceID);

            Selection.activeObject = target;
            UWed.OpenWindow();
            UWed.OpenMap(map);
            return true;
        }
        else
        {
            return false;
        }
    }
}
