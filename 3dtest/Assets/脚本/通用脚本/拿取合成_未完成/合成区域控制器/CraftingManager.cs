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
    public static CraftingManager Instance;

    [Header("合成配方列表")]
    public List<CraftingRecipe> craftingRecipes = new List<CraftingRecipe>();

    [Header("默认合成失败效果")]
    public ParticleSystem failEffect;
    public string failSoundGroupID = "合成失败"; // 改为使用音效组

    [Header("合成成功音效")]
    public string successSoundGroupID = "合成成功";

    [Header("合成事件")]
    public UnityEvent<CraftingRecipe> OnCraftingSuccess;
    public UnityEvent OnCraftingFail;

    // 存储最近的合成区域，用于确定声道
    private SynthesisZone lastUsedZone;

    // 设置最近使用的合成区域（在合成前调用）
    public void SetLastUsedZone(SynthesisZone zone)
    {
        lastUsedZone = zone;
    }

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
    public CraftingRecipe CombineItems(List<InteractableItem> items, SynthesisZone synthesisZone = null)
    {
        // 记录合成区域
        if (synthesisZone != null)
        {
            SetLastUsedZone(synthesisZone);
        }

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

            // 播放合成成功音效
            PlayCraftingSuccessSound();

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
    // 播放合成成功音效
    void PlayCraftingSuccessSound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(successSoundGroupID))
        {
            // 确定声道：如果合成区域靠近玩家1则左声道，靠近玩家2则右声道
            bool isPlayer1 = DetermineSoundChannel();

            AudioManager.Instance.PlayOneShot(
                successSoundGroupID,
                -1,           // 随机选择音效
                false,        // 不淡入
                0f,
                false,        // 不淡出
                0f,
                isPlayer1,    // 声道分配
                false         // 2D音效
            );

            Debug.Log($"播放合成成功音效 - 声道: {(isPlayer1 ? "左(玩家1)" : "右(玩家2)")}");
        }
    }

    // 确定音效声道
    bool DetermineSoundChannel()
    {
        // 如果有最近的合成区域，根据位置决定声道
        if (lastUsedZone != null)
        {
            // 获取玩家对象（假设场景中有玩家1和玩家2）
            GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
            GameObject player2 = GameObject.FindGameObjectWithTag("Player2");

            if (player1 != null && player2 != null)
            {
                float distanceToPlayer1 = Vector3.Distance(lastUsedZone.transform.position, player1.transform.position);
                float distanceToPlayer2 = Vector3.Distance(lastUsedZone.transform.position, player2.transform.position);

                // 距离玩家1更近则使用左声道，否则使用右声道
                return distanceToPlayer1 <= distanceToPlayer2;
            }
        }

        // 默认使用左声道
        return true;
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

        // 播放合成失败音效
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(failSoundGroupID))
        {
            bool isPlayer1 = DetermineSoundChannel();

            AudioManager.Instance.PlayOneShot(
                failSoundGroupID,
                -1,           // 随机选择音效
                false,        // 不淡入
                0f,
                false,        // 不淡出
                0f,
                isPlayer1,    // 声道分配
                false         // 2D音效
            );
        }
    }
}