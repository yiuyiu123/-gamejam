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
    public SynthesisZone specificSpawnZone;
    public Transform customSpawnPoint;
}

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    [Header("合成配方列表")]
    public List<CraftingRecipe> craftingRecipes = new List<CraftingRecipe>();

    [Header("默认合成失败效果")]
    public ParticleSystem failEffect;
    public string failSoundGroupID = "合成失败";

    [Header("合成成功音效")]
    public string successSoundGroupID = "合成成功";

    [Header("合成特效设置")]
    public bool enableSynthesisEffects = true;

    [Header("双区域特效设置")]
    public bool enableDualZoneEffects = true;
    public List<SynthesisZone> allSynthesisZones = new List<SynthesisZone>();

    [Header("合成事件")]
    public UnityEvent<CraftingRecipe> OnCraftingSuccess;
    public UnityEvent OnCraftingFail;

    private SynthesisZone lastUsedZone;

    public void SetLastUsedZone(SynthesisZone zone)
    {
        lastUsedZone = zone;
    }

    // 新增：注册合成区域
    public void RegisterSynthesisZone(SynthesisZone zone)
    {
        if (zone != null && !allSynthesisZones.Contains(zone))
        {
            allSynthesisZones.Add(zone);
            Debug.Log($"注册合成区域: {zone.zoneID}");
        }
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

    void Start()
    {
        // 自动查找场景中的合成区域
        FindAllSynthesisZones();
    }

    // 自动查找所有合成区域
    void FindAllSynthesisZones()
    {
        SynthesisZone[] zonesInScene = FindObjectsOfType<SynthesisZone>();
        allSynthesisZones.Clear();

        foreach (SynthesisZone zone in zonesInScene)
        {
            if (zone != null)
            {
                RegisterSynthesisZone(zone);
            }
        }

        Debug.Log($"总共找到 {allSynthesisZones.Count} 个合成区域");
    }

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

            // 播放合成特效 - 只在成功时播放
            if (enableSynthesisEffects)
            {
                PlaySynthesisEffects();
            }

            // 触发合成成功事件
            OnCraftingSuccess?.Invoke(matchedRecipe);

            return matchedRecipe;
        }
        else
        {
            Debug.Log("合成失败：没有匹配的配方");

            // 注意：合成失败时不播放合成成功特效！
            // 只播放失败音效和效果
            PlayFailEffect();

            // 触发合成失败事件
            OnCraftingFail?.Invoke();

            return null;
        }
    }

    // 修改：确保只在合成成功时播放特效
    void PlaySynthesisEffects()
    {
        if (!enableSynthesisEffects) return;

        int effectsPlayed = 0;

        // 播放所有合成区域的特效
        foreach (SynthesisZone zone in allSynthesisZones)
        {
            if (zone != null && zone.synthesisEffect != null)
            {
                zone.PlaySynthesisEffect();
                effectsPlayed++;
                Debug.Log($"播放区域 {zone.zoneID} 的合成成功特效");
            }
        }

        Debug.Log($"合成成功！总共播放了 {effectsPlayed} 个区域的合成特效");

        // 如果没有找到任何区域，使用备用方法
        if (effectsPlayed == 0 && lastUsedZone != null)
        {
            lastUsedZone.PlaySynthesisEffect();
            Debug.Log($"使用备用方法播放区域 {lastUsedZone.zoneID} 的合成成功特效");
        }
    }

    void PlayCraftingSuccessSound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(successSoundGroupID))
        {
            bool isPlayer1 = DetermineSoundChannel();

            AudioManager.Instance.PlayOneShot(
                successSoundGroupID,
                -1,
                false,
                0f,
                false,
                0f,
                isPlayer1,
                false
            );

            Debug.Log($"播放合成成功音效 - 声道: {(isPlayer1 ? "左(玩家1)" : "右(玩家2)")}");
        }
    }

    void PlayFailEffect()
    {
        // 播放全局失败效果
        if (failEffect != null)
        {
            Instantiate(failEffect, Vector3.zero, Quaternion.identity);
        }

        // 播放失败音效
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(failSoundGroupID))
        {
            bool isPlayer1 = DetermineSoundChannel();

            AudioManager.Instance.PlayOneShot(
                failSoundGroupID,
                -1,
                false,
                0f,
                false,
                0f,
                isPlayer1,
                false
            );
        }
    }

    bool DetermineSoundChannel()
    {
        if (lastUsedZone != null)
        {
            GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
            GameObject player2 = GameObject.FindGameObjectWithTag("Player2");

            if (player1 != null && player2 != null)
            {
                float distanceToPlayer1 = Vector3.Distance(lastUsedZone.transform.position, player1.transform.position);
                float distanceToPlayer2 = Vector3.Distance(lastUsedZone.transform.position, player2.transform.position);

                return distanceToPlayer1 <= distanceToPlayer2;
            }
        }

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

    [ContextMenu("测试所有区域特效")]
    public void TestAllZoneEffects()
    {
        PlaySynthesisEffects();
    }

    [ContextMenu("重新查找合成区域")]
    public void RefreshSynthesisZones()
    {
        FindAllSynthesisZones();
        Debug.Log($"重新查找完成，找到 {allSynthesisZones.Count} 个合成区域");
    }

    [ContextMenu("显示区域状态")]
    public void ShowZoneStatus()
    {
        Debug.Log("=== 合成区域状态 ===");
        foreach (SynthesisZone zone in allSynthesisZones)
        {
            if (zone != null)
            {
                Debug.Log($"区域: {zone.zoneID}, 成功特效: {zone.synthesisEffect != null}, 失败特效: {zone.failEffect != null}");
            }
        }
    }
}