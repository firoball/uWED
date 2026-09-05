using System.Linq;
using Editor.Assets;
using UnityEngine.UIElements;
using UnityEngine;

namespace Editor.Inspector
{

    public class MapAssetViewer : ImmediateModeElement
    {
        private Color c_lineColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
        private Color c_backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);

        private readonly MapAsset m_mapAsset;
        private readonly Material m_material;
        private readonly Foldout m_foldout;
        private MeshManager m_segmentMgr;
        private MapDataSet m_mapData;
        private Matrix4x4 m_matrix;
        private Vector2 m_mapMin;
        private Vector2 m_mapMax;
        private float m_aspect;

        private const bool c_invertYPosition = true;
        private const float c_border = 5f;

        public Foldout Foldout
        {
            get => m_foldout;
        }

        public MapAssetViewer(MapAsset mapAsset)
        {
            m_foldout = CreateFoldOut();
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            m_material = new Material(shader);
            m_mapAsset = mapAsset;
            m_aspect = 1f;
            UpdateMap();
        }

        public void UpdateMap()
        {
            m_mapData = m_mapAsset.Data;
            PrepareMesh();
            
            if (m_mapData.Segments.Count == 0)
                return;

            m_mapMin = Min();
            m_mapMax = Max();
            m_aspect = (m_mapMax.y - m_mapMin.y) / (m_mapMax.x - m_mapMin.x);
            UpdateRect();
        }

        public void UpdateRect()
        {
            if (parent == null) return;

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
                v1 = m_mapData.Segments[i].Vertex1;
                v2 = m_mapData.Segments[i].Vertex2;
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
            Matrix4x4 matrix = Matrix4x4.Translate(layoutTranslate) * Matrix4x4.Scale(scale) *
                               Matrix4x4.Translate(translate);
            return matrix;
        }

        private Vector2 Min()
        {
            Vector2 min;
            min.x = (float)m_mapData.Vertices.Min(v => v.X);
            min.y = (float)m_mapData.Vertices.Min(v => v.Y);
            return min;
        }

        private Vector2 Max()
        {
            Vector2 max;
            max.x = (float)m_mapData.Vertices.Max(v => v.X);
            max.y = (float)m_mapData.Vertices.Max(v => v.Y);
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
}