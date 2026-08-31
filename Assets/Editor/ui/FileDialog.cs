using UnityEditor;
using UnityEngine.UIElements;

public class FileDialog
{
    private static string s_directory = "."; //temp
    private static string s_currentFile = string.Empty;
    private static string s_defaultFile = "newmap.wmp";

    public void New(DropdownMenuAction item)
    {

    }

    public void Load(DropdownMenuAction item)
    {
        string file = EditorUtility.OpenFilePanel("Load Acknex3 map", s_directory, "wmp");
        //Debug.Log(file);
        EditorEventBus.Instance.LoadMap.Raise(new MapWmpLoader(), file);
    }

    public void Save(DropdownMenuAction item)
    {
        if (string.IsNullOrWhiteSpace(s_currentFile))
            SaveAs(item);
        else
            SaveInternal();
    }

    public void SaveAs(DropdownMenuAction item)
    {
        string file = EditorUtility.SaveFilePanel("Save Acknex3 map", s_directory, s_defaultFile, "wmp");
        s_currentFile = file;
        //Debug.Log(file);
        SaveInternal();
    }

    private void SaveInternal()
    {
        EditorEventBus.Instance.WriteMap.Raise(new MapWmpWriter(), s_currentFile);
    }
}