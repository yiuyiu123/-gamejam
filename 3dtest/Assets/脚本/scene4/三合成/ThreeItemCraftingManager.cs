using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ThreeItemRecipe
{
    public string recipeName;
    public List<string> requiredItems; // �������3����Ʒ
    public GameObject resultItemPrefab1;
    public GameObject resultItemPrefab2;

    [Header("�ϳ�λ������")]
    public Transform spawnPoint1; // ��һ�������Ʒ����λ��
    public Vector3 spawnOffset1 = Vector3.up * 2f;
    public Transform spawnPoint2; // �ڶ��������Ʒ����λ��
    public Vector3 spawnOffset2 = Vector3.up * 2f;
}

public class ThreeItemCraftingManager : MonoBehaviour
{
    public static ThreeItemCraftingManager Instance;

    [Header("���ϳ��䷽�б�")]
    public List<ThreeItemRecipe> threeItemRecipes = new List<ThreeItemRecipe>();

    [Header("���ϳ�Ч��")]
    public ParticleSystem synthesisEffect;
    public AudioClip synthesisSound;
    public float synthesisDelay = 1f;

    [Header("���ϳ�ʧ��Ч��")]
    public ParticleSystem failEffect;
    public AudioClip failSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ����Ƿ�������ϳ�
    public bool CanThreeItemCraft(List<InteractableItem> items)
    {
        if (items.Count != 3) return false;

        List<string> itemNames = new List<string>();
        foreach (var item in items)
        {
            if (item != null)
            {
                itemNames.Add(item.itemName);
            }
        }

        return FindMatchingRecipe(itemNames) != null;
    }

    // ִ�����ϳ�
    public ThreeItemRecipe CombineThreeItems(List<InteractableItem> items)
    {
        if (items.Count != 3)
        {
            Debug.LogError("���ϳ���Ҫǡ��3����Ʒ");
            return null;
        }

        List<string> itemNames = new List<string>();
        foreach (var item in items)
        {
            if (item != null)
            {
                itemNames.Add(item.itemName);
                Debug.Log($"���ϳ���Ʒ: {item.itemName}");
            }
        }

        Debug.Log($"�������ϳ����: {string.Join(" + ", itemNames)}");

        ThreeItemRecipe matchedRecipe = FindMatchingRecipe(itemNames);

        if (matchedRecipe != null && matchedRecipe.resultItemPrefab1 != null && matchedRecipe.resultItemPrefab2 != null)
        {
            Debug.Log($"���ϳɳɹ����䷽: {matchedRecipe.recipeName}");
            return matchedRecipe;
        }
        else
        {
            Debug.Log("���ϳ�ʧ�ܣ�û��ƥ����䷽");
            PlayFailEffect();
            return null;
        }
    }

    ThreeItemRecipe FindMatchingRecipe(List<string> itemNames)
    {
        foreach (var recipe in threeItemRecipes)
        {
            if (IsRecipeMatch(recipe, itemNames))
            {
                return recipe;
            }
        }
        return null;
    }

    bool IsRecipeMatch(ThreeItemRecipe recipe, List<string> itemNames)
    {
        if (recipe.requiredItems.Count != 3 || itemNames.Count != 3)
            return false;

        List<string> tempRequired = new List<string>(recipe.requiredItems);
        List<string> tempProvided = new List<string>(itemNames);

        // ������˳���ƥ��
        foreach (string requiredItem in tempRequired)
        {
            if (!tempProvided.Contains(requiredItem))
                return false;
            tempProvided.Remove(requiredItem);
        }
        return tempProvided.Count == 0;
    }

    void PlayFailEffect()
    {
        if (failEffect != null)
        {
            Instantiate(failEffect, Vector3.zero, Quaternion.identity);
        }
        if (failSound != null)
        {
            AudioSource.PlayClipAtPoint(failSound, Vector3.zero);
        }
    }

    // ��ȡ��һ����Ʒ�ĺϳ�λ��
    public Vector3 GetSpawnPosition1(ThreeItemRecipe recipe)
    {
        if (recipe.spawnPoint1 != null)
        {
            return recipe.spawnPoint1.position + recipe.spawnOffset1;
        }
        Debug.LogWarning($"�䷽ {recipe.recipeName} �� spawnPoint1 δ���ã�ʹ��Ĭ��λ��");
        return Vector3.zero;
    }

    // ��ȡ�ڶ�����Ʒ�ĺϳ�λ�� - �޸����
    public Vector3 GetSpawnPosition2(ThreeItemRecipe recipe)
    {
        if (recipe.spawnPoint2 != null)
        {
            return recipe.spawnPoint2.position + recipe.spawnOffset2; // ����Ӧ���� spawnOffset2
        }
        Debug.LogWarning($"�䷽ {recipe.recipeName} �� spawnPoint2 δ���ã�ʹ��Ĭ��λ��");
        return Vector3.zero + Vector3.right * 2f; // ���ڶ�����Ʒһ��ƫ�ƣ������ص�
    }
}