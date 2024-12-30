using UnityEngine.UIElements;

public class MapAssetCleaner : Foldout
{
    public MapAssetCleaner(MapAsset mapAsset)
    {
        text = "!! Danger Zone !!";
        value = false;
        Button clear = new Button();
        clear.text = "Clear Map Data";
        clear.RegisterCallback<ClickEvent>(evt => { mapAsset.Data.Clear(); });
        Add(clear);
        Add(new Label("Warning: Clearing cannot be undone!"));
    }
}