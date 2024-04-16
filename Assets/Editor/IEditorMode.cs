using UnityEngine;

public interface IEditorMode
{
    public void StartDrag(CursorInfo ci);
    public void FinishDrag(CursorInfo ci, Vector2 mouseSnappedWorldPos);
    public bool StartConstruction(CursorInfo ci, Vector2 mouseSnappedWorldPos);
    public bool RevertConstruction();
    public bool ProgressConstruction(CursorInfo ci, Vector2 mouseSnappedWorldPos);
    public void EditObject(CursorInfo ci);
    public void DeleteObject(CursorInfo ci);
    public void ModifyObject(CursorInfo ci, Vector2 mouseWorldPos, EditorView ev);
    public void ModifyObjectAlt(CursorInfo ci, Vector2 mouseWorldPos, EditorView ev);

}