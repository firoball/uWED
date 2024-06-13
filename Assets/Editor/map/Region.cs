using System;
using UnityEngine;

[Serializable]
public class Region
{
    [SerializeReference]
    private bool m_default;

    //region boundaries - for multi-select
    private Vector2 m_min;
    private Vector2 m_max;

    public Region()
    {
        m_default = true;
        m_min = new Vector2(float.MaxValue, float.MaxValue);
        m_max = new Vector2(float.MinValue, float.MinValue);
    }

    public bool Default { get => m_default; set => m_default = value; }
    public Vector2 Min { get => m_min; set => m_min = value; }
    public Vector2 Max { get => m_max; set => m_max = value; }
}
