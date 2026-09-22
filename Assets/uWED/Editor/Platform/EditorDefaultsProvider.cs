using UnityEngine;

public class EditorDefaultsProvider : IDefaultsProvider
{
    private readonly Material m_wireMaterial;
    private readonly Material m_polyMaterial;

    public Material GetWireMaterial() => m_wireMaterial;
    public Material GetPolyMaterial() => m_polyMaterial;

    public EditorDefaultsProvider()
    {
        m_polyMaterial = Resources.Load<Material>("testmaterial"); //TEMP
        /*PropertyInfo matProperty =
            typeof(HandleUtility).GetProperty("handleWireMaterial", BindingFlags.NonPublic | BindingFlags.Static);
        m_editorMaterial = (Material)matProperty.GetValue(null);*/
        var shader = Shader.Find("Hidden/Internal-Colored");
        m_wireMaterial = new Material(shader);
        m_wireMaterial.hideFlags = HideFlags.HideAndDontSave;
        m_wireMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m_wireMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m_wireMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        m_wireMaterial.SetInt("_ZWrite", 0);
        //m_wireMaterial=(Material) EditorGUIUtility.LoadRequired("SceneView/2DHandleLines.mat");
    }
}