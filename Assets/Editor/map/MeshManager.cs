using UnityEngine;
using UnityEngine.Rendering;

public class MeshManager
{
    private Mesh m_mesh;
    private MeshTopology m_topology;
    private Vector3[] m_vertices;
    private int[] m_indices;
    private Color[] m_colors;
    private int m_count;

    public Vector3[] Vertices { get => m_vertices; }
    public int[] Indices { get => m_indices; }
    public Color[] Colors { get => m_colors; }
    public int Count { get => m_count; }

    public MeshManager(MeshTopology topology)
    {
        m_mesh = new Mesh() { indexFormat = IndexFormat.UInt32 };
        m_topology = topology;
    }

    public void PrepareBuffers(int count)
    {
        if (m_count != count)
        {
            m_vertices = new Vector3[count];
            m_indices = new int[count];
            m_colors = new Color[count];
            m_count = count;
        }
    }

    public void DrawMesh()
    {
        m_mesh.Clear();
        m_mesh.vertices = m_vertices;
        m_mesh.colors = m_colors;
        m_mesh.SetIndices(m_indices, m_topology, 0);
        Graphics.DrawMeshNow(m_mesh, Matrix4x4.identity);
    }
}