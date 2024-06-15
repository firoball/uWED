using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class FileDialog
{
    private static string s_directory = "."; //temp
    private static string s_currentFile = string.Empty;
    private static string s_defaultFile = "newmap.wmp";
    private EditorInterface m_interface;

    public FileDialog(EditorInterface editorInterface)
    {
        m_interface = editorInterface;
    }

    public void New(DropdownMenuAction item)
    {

    }

    public void Load(DropdownMenuAction item)
    {
        string file = EditorUtility.OpenFilePanel("Load Acknex3 map", s_directory, "wmp");
        //Debug.Log(file);
        m_interface?.OnLoadMap(new MapWmpLoader(), file);
    }

    public void Save(DropdownMenuAction item)
    {
        if (string.IsNullOrWhiteSpace(s_currentFile))
            SaveAs(item);
    }

    public void SaveAs(DropdownMenuAction item)
    {
        string file = EditorUtility.SaveFilePanel("Save Acknex3 map", s_directory, s_defaultFile, "wmp");
        //Debug.Log(file);
    }
}