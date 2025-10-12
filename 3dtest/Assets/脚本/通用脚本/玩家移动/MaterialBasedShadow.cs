using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialBasedShadow : MonoBehaviour
{
    [Header("��Ӱ��������")]
    public Material shadowMaterial; // ��ק�㴴����ShadowMaterial������

    [Header("��Ӱ����")]
    public bool enableShadow = true;
    public float shadowSize = 1f;
    public float heightOffset = 0.01f;

    [Header("���ʲ���")]
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
            Debug.LogError("��Ӱ����δ���䣡");
            return;
        }

        // ������Ӱ����
        shadowObject = new GameObject("MaterialShadow");
        shadowObject.transform.SetParent(transform);

        // ���SpriteRenderer
        shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();

        // ��������ʵ���������޸�ԭʼ���ʣ�
        shadowMaterialInstance = new Material(shadowMaterial);
        shadowRenderer.material = shadowMaterialInstance;

        // ��������Sprite��Shader�ᴦ��Բ�Σ�
        CreateBaseSprite();

        // ���²��ʲ���
        UpdateMaterialProperties();

        // ���ó�ʼλ�úʹ�С
        UpdateShadowTransform();

        // ������Ⱦ˳��
        shadowRenderer.sortingOrder = -5;

        Debug.Log("������Ӱ�������");
    }

    void CreateBaseSprite()
    {
        // ����һ���򵥵�������Sprite
        int textureSize = 16;
        Texture2D texture = new Texture2D(textureSize, textureSize);

        // ����ɫ������״��Shader����
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

        // ���ô�С
        shadowObject.transform.localScale = Vector3.one * shadowSize;

        // ����ˮƽ�����߸������������Ƕȵ�����
        shadowObject.transform.rotation = Quaternion.identity;
    }

    // ����������������Ӱ��ɫ
    public void SetShadowColors(Color newCenter, Color newEdge)
    {
        centerColor = newCenter;
        edgeColor = newEdge;
        UpdateMaterialProperties();
    }

    // ����������������Ӱ��Ͷ�
    public void SetShadowSoftness(float newSoftness)
    {
        softness = newSoftness;
        UpdateMaterialProperties();
    }

    // ����������������Ӱ�뾶
    public void SetShadowRadius(float newRadius)
    {
        radius = newRadius;
        UpdateMaterialProperties();
    }

    // �������������¿�߱ȣ�����͸�Ӳ�����
    public void SetAspectRatio(float ratio)
    {
        aspectRatio = ratio;
        UpdateMaterialProperties();
    }

    // ����������������Ӱ�ɼ���
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
