using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorMode
{
    public enum Group : int
    {
        Objects = 0,
        Segments = 1,
        Regions = 2,
        Ways = 3,
    }
    public enum Construct : int
    {
        Idle = 0,
        Constructing = 1,
        Dragging = 2,
        Selecting = 3,
    }

    public enum Edit : int
    {
        Idle = 0,
        //TODO: add more
    }
}
