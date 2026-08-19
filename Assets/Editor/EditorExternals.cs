using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class EditorExternals
    {
        private static MethodInfo m_handleUtility;
        private static MethodInfo m_roundToPixelGrid;

        public static void Connect()
        {
            m_handleUtility = typeof(HandleUtility).GetMethod("ApplyWireMaterial",
                BindingFlags.NonPublic | BindingFlags.Static, Type.DefaultBinder, Type.EmptyTypes, null);
            if (m_handleUtility == null)
                Debug.LogError(
                    "Unable to bind 'HandleUtility.ApplyWireMaterial' - review whether Unity internals have changed");

            m_roundToPixelGrid = typeof(GUIUtility).GetMethod("RoundToPixelGrid",
                BindingFlags.NonPublic | BindingFlags.Static, Type.DefaultBinder, new Type[] { typeof(float) }, null);
            if (m_roundToPixelGrid == null)
                Debug.LogError(
                    "Unable to bind 'GUIUtility.RoundToPixelGrid' - review whether Unity internals have changed");

        }

        public StyleSheet GetStyleSheet(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(assetPath);
        }
    }
}