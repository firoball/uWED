using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorStatus
{
    public enum Mode : int
    {
        Objects = 0,
        Segments = 1,
        Regions = 2,
        Ways = 3,
        Count = 4,
    }
    public enum Construct : int
    {
        Idle = 0,
        Constructing = 1,
        Dragging = 2,
        Selecting = 3,
        Count = 4,
    }

    public enum View : int
    {
        Construct = 0,
        Design = 1,
        Count = 2,
    }
}
