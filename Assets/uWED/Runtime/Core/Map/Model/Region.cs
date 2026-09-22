using System;
using UnityEngine;

[Serializable]
public class Region : IndexedData
{
    //region boundaries - for multi-select
    private Vector2 m_min;
    private Vector2 m_max;
    
    //properties from map format
    [SerializeField]
    private string m_name;
    [SerializeField]
    private float m_floorHgt;
    [SerializeField]
    private float m_ceilHgt;

    public Region() : this (0f, 12f, null) { }
    
    public Region(float floorHgt, float ceilHgt, string name) 
    {
        m_floorHgt = floorHgt;
        m_ceilHgt = ceilHgt;
        m_name = !string.IsNullOrWhiteSpace(name) ? name : "defaultregion";

        m_min = new Vector2(float.MaxValue, float.MaxValue);
        m_max = new Vector2(float.MinValue, float.MinValue);
    }
    
    public Vector2 Min
    {
        get => m_min;
        set => m_min = value;
    }

    public Vector2 Max
    {
        get => m_max;
        set => m_max = value;
    }

    public string Name
    {
        get => m_name;
        set => m_name = value;
    }

    public float FloorHgt
    {
        get => m_floorHgt;
        set => m_floorHgt = value;
    }

    public float CeilHgt
    {
        get => m_ceilHgt;
        set => m_ceilHgt = value;
    }

}