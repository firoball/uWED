using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

//make private fields of GridBackground accessible
public class FlexibleGridBackground : GridBackground
{
    private FieldInfo m_fispc;
    private FieldInfo m_fibgc;
    private FieldInfo m_filc;
    private FieldInfo m_fitlc;

    public float spacing 
    { 
        get { return (float) m_fispc?.GetValue(this); }
        set { m_fispc?.SetValue(this, value); }
    }

    public Color gridBackgroundColor
    {
        get { return (Color)m_fibgc?.GetValue(this); }
        set { m_fibgc?.SetValue(this, value); }
    }

    public Color lineColor
    {
        get { return (Color)m_filc?.GetValue(this); }
        set { m_filc?.SetValue(this, value); }
    }

    public Color thickLineColor
    {
        get { return (Color)m_fitlc?.GetValue(this); }
        set { m_fitlc?.SetValue(this, value); }
    }

    public FlexibleGridBackground() : base()
    {
        m_fispc = typeof(GridBackground).GetField("m_Spacing", BindingFlags.NonPublic | BindingFlags.Instance);
        m_fibgc = typeof(GridBackground).GetField("m_GridBackgroundColor", BindingFlags.NonPublic | BindingFlags.Instance);
        m_filc = typeof(GridBackground).GetField("m_LineColor", BindingFlags.NonPublic | BindingFlags.Instance);
        m_fitlc = typeof(GridBackground).GetField("m_ThickLineColor", BindingFlags.NonPublic | BindingFlags.Instance);
    }
}
