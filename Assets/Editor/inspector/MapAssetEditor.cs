using Editor.Assets;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Inspector
{

    [CustomEditor(typeof(MapAsset))]
    public class MapAssetEditor : UnityEditor.Editor
    {
        private MapAssetViewer m_viewer;
        private MapAssetStatistics m_stats;

        public override VisualElement CreateInspectorGUI()
        {
            MapAsset mapAsset = (MapAsset)target;
            VisualElement inspector = new VisualElement();

            SerializedProperty properties = serializedObject.FindProperty("m_data");
            inspector.TrackPropertyValue(properties, OnPropertyChanged);

            Label info = new Label("Info: Active inspector may cause performance issues");
            inspector.Add(info);

            m_viewer = new MapAssetViewer(mapAsset);
            inspector.Add(m_viewer.Foldout); //wrap preview map in its foldout

            m_stats = new MapAssetStatistics(mapAsset);
            inspector.Add(m_stats);

            MapAssetCleaner cleaner = new MapAssetCleaner(mapAsset);
            inspector.Add(cleaner);

            inspector.RegisterCallback<GeometryChangedEvent>(OnWindowSizeChanged);

            return inspector;
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(EntityId entityId, int line)
        {
            Object target = EditorUtility.EntityIdToObject(entityId);

            if (target is MapAsset)
            {
                var map = AssetDatabase.GetAssetPath(entityId);

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

        private void OnWindowSizeChanged(GeometryChangedEvent evt)
        {
            m_viewer?.UpdateRect();
        }

        private void OnPropertyChanged(SerializedProperty property)
        {
            m_viewer?.UpdateMap();
            m_stats?.Update();
        }
    }
}


