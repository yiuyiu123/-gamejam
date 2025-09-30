using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialBasedShadow : MonoBehaviour
{
    [Header("阴影材质引用")]
    public Material shadowMaterial; // 拖拽你创建的ShadowMaterial到这里

    [Header("阴影设置")]
    public bool enableShadow = true;
    public float shadowSize = 1f;
    public float heightOffset = 0.01f;

    [Header("材质参数")]
    public Color centerColor = new Color(0, 0, 0, 0.7f);
    public Color edgeColor = new Color(0, 0, 0, 0f);
    public float softness = 2f;
    public float radius = 1f;
    public float aspectRatio = 1f;

    private GameObject shadowObject;
    private SpriteRenderer shadowRenderer;
    private Material shadowMaterialInstance;

    void Start()
    {
        CreateShadow();
    }

    void LateUpdate()
    {
        if (enableShadow && shadowObject != null)
        {
            UpdateShadowTransform();
        }
    }

    void CreateShadow()
    {
        if (!enableShadow || shadowMaterial == null)
        {
            Debug.LogError("阴影材质未分配！");
            return;
        }

        // 创建阴影对象
        shadowObject = new GameObject("MaterialShadow");
        shadowObject.transform.SetParent(transform);

        // 添加SpriteRenderer
        shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();

        // 创建材质实例（避免修改原始材质）
        shadowMaterialInstance = new Material(shadowMaterial);
        shadowRenderer.material = shadowMaterialInstance;

        // 创建基础Sprite（Shader会处理圆形）
        CreateBaseSprite();

        // 更新材质参数
        UpdateMaterialProperties();

        // 设置初始位置和大小
        UpdateShadowTransform();

        // 设置渲染顺序
        shadowRenderer.sortingOrder = -5;

        Debug.Log("材质阴影创建完成");
    }

    void CreateBaseSprite()
    {
        // 创建一个简单的正方形Sprite
        int textureSize = 16;
        Texture2D texture = new Texture2D(textureSize, textureSize);

        // 纯白色纹理，形状由Shader控制
        Color[] pixels = new Color[textureSize * textureSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, textureSize, textureSize),
            new Vector2(0.5f, 0.5f),
            textureSize
        );

        shadowRenderer.sprite = sprite;
    }

    void UpdateMaterialProperties()
    {
        if (shadowMaterialInstance != null)
        {
            shadowMaterialInstance.SetColor("_CenterColor", centerColor);
            shadowMaterialInstance.SetColor("_EdgeColor", edgeColor);
            shadowMaterialInstance.SetFloat("_Softness", softness);
            shadowMaterialInstance.SetFloat("_Radius", radius);
            shadowMaterialInstance.SetFloat("_AspectRatio", aspectRatio);
        }
    }

    void UpdateShadowTransform()
    {
        Vector3 shadowPos = transform.position;
        shadowPos.y -= heightOffset;
        shadowObject.transform.position = shadowPos;

        // 设置大小
        shadowObject.transform.localScale = Vector3.one * shadowSize;

        // 保持水平（或者根据你的摄像机角度调整）
        shadowObject.transform.rotation = Quaternion.identity;
    }

    // 公共方法：更新阴影颜色
    public void SetShadowColors(Color newCenter, Color newEdge)
    {
        centerColor = newCenter;
        edgeColor = newEdge;
        UpdateMaterialProperties();
    }

    // 公共方法：更新阴影柔和度
    public void SetShadowSoftness(float newSoftness)
    {
        softness = newSoftness;
        UpdateMaterialProperties();
    }

    // 公共方法：更新阴影半径
    public void SetShadowRadius(float newRadius)
    {
        radius = newRadius;
        UpdateMaterialProperties();
    }

    // 公共方法：更新宽高比（用于透视补偿）
    public void SetAspectRatio(float ratio)
    {
        aspectRatio = ratio;
        UpdateMaterialProperties();
    }

    // 公共方法：设置阴影可见性
    public void SetShadowVisible(bool visible)
    {
        if (shadowRenderer != null)
        {
            shadowRenderer.enabled = visible;
        }
    }

    void OnDestroy()
    {
        if (shadowObject != null)
        {
            Destroy(shadowObject);
        }
        if (shadowMaterialInstance != null)
        {
            Destroy(shadowMaterialInstance);
        }
    }
}
