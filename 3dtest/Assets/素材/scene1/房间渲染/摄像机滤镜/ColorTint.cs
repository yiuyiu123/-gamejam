using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Custom Post Effects/ColorTint")]
public class ColorTint : MonoBehaviour
{
    public Shader colorTintShader;
    private Material colorTintMaterial;

    [Header(" RGB通道偏移 (范围更大)")]
    [Range(-50f, 50f)] public float redOffset = 0f;
    [Range(-50f, 50f)] public float greenOffset = 0f;
    [Range(-50f, 50f)] public float blueOffset = 0f;

    [Header(" RGB颜色乘因子")]
    public Color tintColor = Color.white;

    void Start()
    {
        if (!colorTintShader)
            colorTintShader = Shader.Find("Custom/ColorTint");

        if (!SystemInfo.supportsImageEffects || colorTintShader == null || !colorTintShader.isSupported)
            enabled = false;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (colorTintMaterial == null)
        {
            colorTintMaterial = new Material(colorTintShader);
            colorTintMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        colorTintMaterial.SetFloat("_RedOffset", redOffset);
        colorTintMaterial.SetFloat("_GreenOffset", greenOffset);
        colorTintMaterial.SetFloat("_BlueOffset", blueOffset);
        colorTintMaterial.SetColor("_TintColor", tintColor);

        Graphics.Blit(source, destination, colorTintMaterial);
    }

    void OnDisable()
    {
        if (colorTintMaterial)
            DestroyImmediate(colorTintMaterial);
    }
}
