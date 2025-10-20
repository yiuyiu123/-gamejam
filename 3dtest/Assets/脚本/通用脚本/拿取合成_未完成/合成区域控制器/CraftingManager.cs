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

    [Header("�ϳ���Ʒ����λ��")]
    public SynthesisResultSpawnMode spawnMode = SynthesisResultSpawnMode.FirstZone;
    public SynthesisZone specificSpawnZone; // ��ѡ���ض�����ʱʹ��
    public Transform customSpawnPoint; // ��ѡ���Զ���λ��ʱʹ��
}

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    [Header("�ϳ��䷽�б�")]
    public List<CraftingRecipe> craftingRecipes = new List<CraftingRecipe>();

    [Header("Ĭ�Ϻϳ�ʧ��Ч��")]
    public ParticleSystem failEffect;
    public AudioClip failSound;

    [Header("�ϳ��¼�")]
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

    // �޸ķ�������Ϊ CraftingRecipe���������ǿ��Ի�ȡ�䷽�ĳ���λ������
    public CraftingRecipe CombineItems(List<InteractableItem> items)
    {
        // ��ȡ��Ʒ�����б�
        List<string> itemNames = new List<string>();
        foreach (var item in items)
        {
            if (item != null)
            {
                itemNames.Add(item.itemName);
                Debug.Log($"�ϳ���Ʒ: {item.itemName}");
            }
        }

        Debug.Log($"���Ժϳ���Ʒ���: {string.Join(" + ", itemNames)}");

        // ��ʾ���п����䷽
        Debug.Log($"�����䷽����: {craftingRecipes.Count}");
        foreach (var recipe in craftingRecipes)
        {
            Debug.Log($"�䷽: {recipe.recipeName}, ����: {string.Join(" + ", recipe.requiredItems)}, ���Ԥ����: {recipe.resultItemPrefab != null}");
        }

        // ����ƥ���䷽
        CraftingRecipe matchedRecipe = FindMatchingRecipe(itemNames);


        if (matchedRecipe != null && matchedRecipe.resultItemPrefab != null)
        {
            Debug.Log($"�ϳɳɹ����䷽: {matchedRecipe.recipeName}");

            // �����ϳɳɹ��¼�
            OnCraftingSuccess?.Invoke(matchedRecipe);

            return matchedRecipe;
        }
        else
        {
            Debug.Log("�ϳ�ʧ�ܣ�û��ƥ����䷽");
            PlayFailEffect();

            // �����ϳ�ʧ���¼�
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