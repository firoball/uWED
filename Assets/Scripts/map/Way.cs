using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class Way : IndexedData
{
    //properties from map format
    [SerializeReference]
    private List<Vertex> m_positions;
    [SerializeField]
    private string m_name;
    
    public Way() : this (null, null) { }

    public Way (List<Vertex> positions, string name)
    {
        m_name = !string.IsNullOrWhiteSpace(name) ? name : "defaultway";
        if (positions != null)
            m_positions = positions;
        else
            m_positions = new List<Vertex>();
    }

    public List<Vertex> Positions
    {
        get => m_positions;
        set => m_positions = value;
    }

    public string Name
    {
        get => m_name;
        set => m_name = value;
    }
}
