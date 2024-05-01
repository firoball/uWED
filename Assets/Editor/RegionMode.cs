using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionMode : BaseEditorMode
{
    public RegionMode(MapData mapData) : base(mapData, new RegionDrawer(mapData))
    {
    }

}
