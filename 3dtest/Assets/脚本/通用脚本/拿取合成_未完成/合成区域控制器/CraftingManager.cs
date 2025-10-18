using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static GlobalSynthesisManager;

[System.Serializable]
public class CraftingRecipe
{
    public string recipeName;
    public List<string> requiredItems;
    public GameObject resultItemPrefab;
    public bool exactOrder = false;

    [Header("合成物品出生位置")]
    public SynthesisResultSpawnMode spawnMode = SynthesisResultSpawnMode.FirstZone;
    public SynthesisZone specificSpawnZone; // 当选择特定区域时使用
    public Transform customSpawnPoint; // 当选择自定义位置时使用
}

public class CraftingManager : MonoBehaviour
{
    //public static CraftingManager Instance;

    //[Header("合成配方列表")]
    //public List<CraftingRecipe> craftingRecipes = new List<CraftingRecipe>();

    //[Header("默认合成失败效果")]
    //public ParticleSystem failEffect;
    //public AudioClip failSound;

    //void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        DontDestroyOnLoad(gameObject);
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    //// 修改返回类型为 CraftingRecipe，这样我们可以获取配方的出生位置设置
    //public CraftingRecipe CombineItems(List<InteractableItem> items)
    //{
    //    // 获取物品名称列表
    //    List<string> itemNames = new List<string>();
    //    foreach (var item in items)
    //    {
    //        if (item != null)
    //        {
    //            itemNames.Add(item.itemName);
    //            Debug.Log($"合成物品: {item.itemName}");
    //        }
    //    }

    //    Debug.Log($"尝试合成物品组合: {string.Join(" + ", itemNames)}");

    //    // 显示所有可用配方
    //    Debug.Log($"可用配方数量: {craftingRecipes.Count}");
    //    foreach (var recipe in craftingRecipes)
    //    {
    //        Debug.Log($"配方: {recipe.recipeName}, 所需: {string.Join(" + ", recipe.requiredItems)}, 结果预制体: {recipe.resultItemPrefab != null}");
    //    }

    //    // 尝试匹配配方
    //    CraftingRecipe matchedRecipe = FindMatchingRecipe(itemNames);

    //    if (matchedRecipe != null && matchedRecipe.resultItemPrefab != null)
    //    {
    //        Debug.Log($"合成成功！配方: {matchedRecipe.recipeName}");
    //        return matchedRecipe; // 返回整个配方对象，包含出生位置设置
    //    }
    //    else if (matchedRecipe != null && matchedRecipe.resultItemPrefab == null)
    //    {
    //        Debug.LogError($"配方 {matchedRecipe.recipeName} 的 resultItemPrefab 为 null！");
    //        return null;
    //    }
    //    else
    //    {
    //        Debug.Log("合成失败：没有匹配的配方");
    //        PlayFailEffect();
    //        return null;
    //    }
    //}

    //CraftingRecipe FindMatchingRecipe(List<string> itemNames)
    //{
    //    foreach (var recipe in craftingRecipes)
    //    {
    //        if (IsRecipeMatch(recipe, itemNames))
    //        {
    //            return recipe;
    //        }
    //    }
    //    return null;
    //}

    //bool IsRecipeMatch(CraftingRecipe recipe, List<string> itemNames)
    //{
    //    if (recipe.requiredItems.Count != itemNames.Count)
    //        return false;

    //    List<string> tempRequired = new List<string>(recipe.requiredItems);
    //    List<string> tempProvided = new List<string>(itemNames);

    //    if (recipe.exactOrder)
    //    {
    //        for (int i = 0; i < tempRequired.Count; i++)
    //        {
    //            if (tempRequired[i] != tempProvided[i])
    //                return false;
    //        }
    //        return true;
    //    }
    //    else
    //    {
    //        foreach (string requiredItem in tempRequired)
    //        {
    //            if (!tempProvided.Contains(requiredItem))
    //                return false;
    //            tempProvided.Remove(requiredItem);
    //        }
    //        return tempProvided.Count == 0;
    //    }
    //}

    //void PlayFailEffect()
    //{
    //    if (failEffect != null)
    //    {
    //        Instantiate(failEffect, Vector3.zero, Quaternion.identity);
    //    }
    //}

    //// 添加示例配方
    //[ContextMenu("添加示例配方")]
    //void AddExampleRecipes()
    //{
    //    craftingRecipes.Add(new CraftingRecipe()
    //    {
    //        recipeName = "钥匙+宝石=魔法钥匙",
    //        requiredItems = new List<string> { "钥匙", "宝石" },
    //        resultItemPrefab = null, // 需要在Inspector中设置
    //        exactOrder = false
    //    });

    //    craftingRecipes.Add(new CraftingRecipe()
    //    {
    //        recipeName = "矿泉水+空喷壶=花洒",
    //        requiredItems = new List<string> { "矿泉水", "空喷壶" },
    //        resultItemPrefab = null, // 需要在Inspector中设置
    //        exactOrder = false
    //    });
    //}

    //[ContextMenu("添加测试配方")]
    //void AddTestRecipe()
    //{
    //    craftingRecipes.Add(new CraftingRecipe()
    //    {
    //        recipeName = "测试合成",
    //        requiredItems = new List<string> { "苹果", "香蕉" }, // 确保与您的物品名称匹配
    //        resultItemPrefab = null, // 需要在 Inspector 中设置
    //        exactOrder = false
    //    });
    //    Debug.Log("已添加测试配方");
    //}
    public static CraftingManager Instance;

    [Header("合成配方列表")]
    public List<CraftingRecipe> craftingRecipes = new List<CraftingRecipe>();

    [Header("默认合成失败效果")]
    public ParticleSystem failEffect;
    public AudioClip failSound;

    [Header("合成事件")]
    public UnityEvent<CraftingRecipe> OnCraftingSuccess;
    public UnityEvent OnCraftingFail;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 修改返回类型为 CraftingRecipe，这样我们可以获取配方的出生位置设置
    public CraftingRecipe CombineItems(List<InteractableItem> items)
    {
        // 获取物品名称列表
        List<string> itemNames = new List<string>();
        foreach (var item in items)
        {
            if (item != null)
            {
                itemNames.Add(item.itemName);
                Debug.Log($"合成物品: {item.itemName}");
            }
        }

        Debug.Log($"尝试合成物品组合: {string.Join(" + ", itemNames)}");

        // 显示所有可用配方
        Debug.Log($"可用配方数量: {craftingRecipes.Count}");
        foreach (var recipe in craftingRecipes)
        {
            Debug.Log($"配方: {recipe.recipeName}, 所需: {string.Join(" + ", recipe.requiredItems)}, 结果预制体: {recipe.resultItemPrefab != null}");
        }

        // 尝试匹配配方
        CraftingRecipe matchedRecipe = FindMatchingRecipe(itemNames);

 
        if (matchedRecipe != null && matchedRecipe.resultItemPrefab != null)
        {
            Debug.Log($"合成成功！配方: {matchedRecipe.recipeName}");

            // 触发合成成功事件
            OnCraftingSuccess?.Invoke(matchedRecipe);

            return matchedRecipe;
        }
        else
        {
            Debug.Log("合成失败：没有匹配的配方");
            PlayFailEffect();

            // 触发合成失败事件
            OnCraftingFail?.Invoke();

            return null;
        }

    }

    CraftingRecipe FindMatchingRecipe(List<string> itemNames)
    {
        foreach (var recipe in craftingRecipes)
        {
            if (IsRecipeMatch(recipe, itemNames))
            {
                return recipe;
            }
        }
        return null;
    }

    bool IsRecipeMatch(CraftingRecipe recipe, List<string> itemNames)
    {
        if (recipe.requiredItems.Count != itemNames.Count)
            return false;

        List<string> tempRequired = new List<string>(recipe.requiredItems);
        List<string> tempProvided = new List<string>(itemNames);

        if (recipe.exactOrder)
        {
            for (int i = 0; i < tempRequired.Count; i++)
            {
                if (tempRequired[i] != tempProvided[i])
                    return false;
            }
            return true;
        }
        else
        {
            foreach (string requiredItem in tempRequired)
            {
                if (!tempProvided.Contains(requiredItem))
                    return false;
                tempProvided.Remove(requiredItem);
            }
            return tempProvided.Count == 0;
        }
    }

    void PlayFailEffect()
    {
        if (failEffect != null)
        {
            Instantiate(failEffect, Vector3.zero, Quaternion.identity);
        }
    }
}