using UnityEditor;
using uWED.Editor.Platform;
using uWED.Runtime.Platform;

namespace uWED.Editor.Bootstrap
{
    [InitializeOnLoad]
    internal static class EditorBootstrap
    {
        static EditorBootstrap()
        {
            ServiceLocator.Clear();

            ServiceLocator.Register<IPrefsProvider>(new EditorPrefsProvider());
            ServiceLocator.Register<IFileDialog>(new EditorFileDialog());
            ServiceLocator.Register<IDefaultsProvider>(new EditorDefaultsProvider());
        }
    }
}
