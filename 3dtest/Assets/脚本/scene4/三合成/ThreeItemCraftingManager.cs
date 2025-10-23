using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ThreeItemRecipe
{
    public string recipeName;
    public List<string> requiredItems; // 必须包含3个物品
    public GameObject resultItemPrefab1;
    public GameObject resultItemPrefab2;

    [Header("合成位置设置")]
    public Transform spawnPoint1; // 第一个结果物品生成位置
    public Vector3 spawnOffset1 = Vector3.up * 2f;
    public Transform spawnPoint2; // 第二个结果物品生成位置
    public Vector3 spawnOffset2 = Vector3.up * 2f;
}

public class ThreeItemCraftingManager : MonoBehaviour
{
    public static ThreeItemCraftingManager Instance;

    [Header("三合成配方列表")]
    public List<ThreeItemRecipe> threeItemRecipes = new List<ThreeItemRecipe>();

    [Header("三合成效果")]
    public ParticleSystem synthesisEffect;
    public AudioClip synthesisSound;
    public float synthesisDelay = 1f;

    [Header("三合成失败效果")]
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

    // 检查是否可以三合成
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

    // 执行三合成
    public ThreeItemRecipe CombineThreeItems(List<InteractableItem> items)
    {
        if (items.Count != 3)
        {
            Debug.LogError("三合成需要恰好3个物品");
            return null;
        }

        List<string> itemNames = new List<string>();
        foreach (var item in items)
        {
            if (item != null)
            {
                itemNames.Add(item.itemName);
                Debug.Log($"三合成物品: {item.itemName}");
            }
        }

        Debug.Log($"尝试三合成组合: {string.Join(" + ", itemNames)}");

        ThreeItemRecipe matchedRecipe = FindMatchingRecipe(itemNames);

        if (matchedRecipe != null && matchedRecipe.resultItemPrefab1 != null && matchedRecipe.resultItemPrefab2 != null)
        {
            Debug.Log($"三合成成功！配方: {matchedRecipe.recipeName}");
            return matchedRecipe;
        }
        else
        {
            Debug.Log("三合成失败：没有匹配的配方");
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

        // 不考虑顺序的匹配
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

    // 获取第一个物品的合成位置
    public Vector3 GetSpawnPosition1(ThreeItemRecipe recipe)
    {
        if (recipe.spawnPoint1 != null)
        {
            return recipe.spawnPoint1.position + recipe.spawnOffset1;
        }
        Debug.LogWarning($"配方 {recipe.recipeName} 的 spawnPoint1 未设置，使用默认位置");
        return Vector3.zero;
    }

    // 获取第二个物品的合成位置 - 修复这里！
    public Vector3 GetSpawnPosition2(ThreeItemRecipe recipe)
    {
        if (recipe.spawnPoint2 != null)
        {
            return recipe.spawnPoint2.position + recipe.spawnOffset2; // 这里应该是 spawnOffset2
        }
        Debug.LogWarning($"配方 {recipe.recipeName} 的 spawnPoint2 未设置，使用默认位置");
        return Vector3.zero + Vector3.right * 2f; // 给第二个物品一个偏移，避免重叠
    }
}