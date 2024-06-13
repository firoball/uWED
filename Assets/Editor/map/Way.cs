using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class Way
{
    [SerializeReference]
    private List<Vertex> m_positions;

    public Way()
    {
        m_positions = new List<Vertex>();
    }

    public List<Vertex> Positions => m_positions;
}
