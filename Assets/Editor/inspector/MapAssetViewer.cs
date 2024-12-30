using System.Linq;
using UnityEngine.UIElements;
using UnityEngine;

public class MapAssetViewer : ImmediateModeElement
{
    private Color c_lineColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
    private Color c_backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
    
    private MeshManager m_segmentMgr;
    private MapAsset m_mapAsset;
    private MapDataSet m_mapData;
    private Material m_material;
    private Matrix4x4 m_matrix;
    private Vector2 m_mapMin;
    private Vector2 m_mapMax;
    private float m_aspect;
    private Foldout m_foldout;

    private const bool c_invertYPosition = true;
    private const float c_border = 5f;

    public Foldout Foldout { get => m_foldout; }

    public MapAssetViewer(MapAsset mapAsset)
    {
        m_foldout = CreateFoldOut();
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        m_material = new Material(shader);
        m_mapAsset = mapAsset;
        UpdateMap();
    }

    public void UpdateMap()
    {
        m_mapData = m_mapAsset.Data;
        m_mapMin = Min();
        m_mapMax = Max();
        m_aspect = (m_mapMax.y - m_mapMin.y) / (m_mapMax.x - m_mapMin.x);
        PrepareMesh();
        UpdateRect();
    }

    public void UpdateRect()
    {
        if (parent == null) return;

        IStyle style = this.style;
        style.width = hierarchy.parent.layout.width;
        style.height = hierarchy.parent.layout.width * m_aspect;
        style.backgroundColor = c_backgroundColor;
        style.overflow = Overflow.Hidden;
    }

    protected override void ImmediateRepaint()
    {
        if (!visible)
            return;

        m_matrix = WorldToScreenMatrix();
        m_material.SetPass(0);
        DrawMesh();
    }

    private void PrepareMesh()
    {
        m_segmentMgr = new MeshManager(MeshTopology.Lines);
        if (m_mapData.Segments.Count == 0)
            return;

        int step = 2;
        m_segmentMgr.PrepareBuffers(m_mapData.Segments.Count * step);

        Vector2 v1;
        Vector2 v2;
        Color color = c_lineColor;
        int idx;
        for (int i = 0; i < m_mapData.Segments.Count; i++)
        {
            idx = i * step;
            v1 = m_mapData.Segments[i].Vertex1.WorldPosition;
            v2 = m_mapData.Segments[i].Vertex2.WorldPosition;
            m_segmentMgr.Vertices[idx] = v1;
            m_segmentMgr.Vertices[idx + 1] = v2;
            System.Array.Fill(m_segmentMgr.Colors, color, idx, step);
        }

    }

    private void DrawMesh()
    {
        m_segmentMgr.DrawMesh(m_matrix);
    }

    private Matrix4x4 WorldToScreenMatrix()
    {
        //layout offset - this is already in screen coordinates
        Vector3 layoutTranslate = new Vector3(c_border, c_border);

        //translate
        Vector3 translate = -m_mapMin;

        //scale
        Vector2 len = m_mapMax - m_mapMin;
        float scaleFactorX = (layout.width - 2 * c_border) / len.x;
        float scaleFactorY = (layout.height - 2 * c_border) / len.y;
        float scaleFactor = (scaleFactorX > scaleFactorY) ? scaleFactorY : scaleFactorX;
        Vector3 scale = new Vector2(scaleFactor, scaleFactor);

        //invert y if configured
        if (c_invertYPosition)
        {
            translate.y = -m_mapMax.y;
            scale.y *= -1;
        }

        //build actual world to screen matrix
        Matrix4x4 matrix = Matrix4x4.Translate(layoutTranslate) * Matrix4x4.Scale(scale) * Matrix4x4.Translate(translate);
        return matrix;
    }

    private Vector2 Min()
    {
        Vector2 min;
        min.x = m_mapData.Vertices.Min(x => x.WorldPosition.x);
        min.y = m_mapData.Vertices.Min(x => x.WorldPosition.y);
        return min;
    }

    private Vector2 Max()
    {
        Vector2 max;
        max.x = m_mapData.Vertices.Max(x => x.WorldPosition.x);
        max.y = m_mapData.Vertices.Max(x => x.WorldPosition.y);
        return max;
    }

    private Foldout CreateFoldOut()
    {
        Foldout foldout = new Foldout();
        foldout.text = "Preview";
        foldout.RegisterValueChangedCallback(x =>
        {
            visible = x.newValue;
            UpdateRect();
        });
        visible = foldout.value;

        foldout.Add(this);
        foldout.Add(new Label("")); //required for map to unfold properly

        return foldout;
    }

}