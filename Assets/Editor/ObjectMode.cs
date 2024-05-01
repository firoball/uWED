using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMode : BaseEditorMode
{
    public ObjectMode(MapData mapData) : base(mapData, new ObjectDrawer(mapData)) 
    {
    }

}
