using Editor.Platform;
using Runtime.Platform;
using UnityEditor;
using UWED.Platform;

namespace UWED.Editor
{
    [InitializeOnLoad]
    internal static class EditorBootstrap
    {
        static EditorBootstrap()
        {
            ServiceLocator.Clear();

            ServiceLocator.Register<IPrefsProvider>(new EditorPrefsProvider());
        }
    }
}
