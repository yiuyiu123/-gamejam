using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemExchangeSystem : MonoBehaviour
{
    [Header("交换区域设置")]
    public ExchangeZone player1Zone;
    public ExchangeZone player2Zone;

    [Header("交换效果")]
    public ParticleSystem exchangeEffect;
    public AudioClip exchangeSound;

    [Header("系统设置")]
    public float globalCooldown = 1f; // 全局冷却时间

    private AudioSource audioSource;
    private bool isGlobalCooldown = false;

    void Start()
    {
        if (player1Zone != null && player2Zone != null)
        {
            player1Zone.targetZone = player2Zone;
            player2Zone.targetZone = player1Zone;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        Debug.Log("物品交换系统初始化完成");
    }
    IEnumerator PerformSafeExchange(GameObject item, ExchangeZone fromZone, ExchangeZone toZone)
    {
        // 设置全局冷却
        isGlobalCooldown = true;

        Debug.Log($"开始安全交换: {item.name} 从 {fromZone.zoneID} 到 {toZone.zoneID}");

        // 播放交换效果
        if (exchangeEffect != null)
        {
            Instantiate(exchangeEffect, item.transform.position, Quaternion.identity);
        }

        if (exchangeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(exchangeSound);
        }

        // 标记物品正在交换
        InteractableItem itemComponent = item.GetComponent<InteractableItem>();
        if (itemComponent != null)
        {
            itemComponent.isInExchangeProcess = true;
        }

        // 短暂延迟
        yield return new WaitForSeconds(0.3f);

        // 传送物品
        Vector3 targetPosition = toZone.transform.position;
        item.transform.position = targetPosition;

        // 重置物品状态
        if (itemComponent != null)
        {
            itemComponent.ResetPhysics();
        }


        // 重置区域状态
        fromZone.hasItem = false;
        fromZone.currentItem = null;

        // 等待后取消物品交换状态
        yield return new WaitForSeconds(0.5f);
        if (itemComponent != null)
        {
            itemComponent.isInExchangeProcess = false;
        }

        Debug.Log($"安全交换完成: {item.name} 已到达 {toZone.zoneID}");

        // 全局冷却结束
        yield return new WaitForSeconds(globalCooldown);
        isGlobalCooldown = false;
    }

    // 强制交换（忽略冷却，用于测试）
    public void ForceExchange(ExchangeZone fromZone)
    {
        if (fromZone.currentItem != null)
        {
            StartCoroutine(PerformExchange(fromZone.currentItem, fromZone, fromZone.targetZone));
        }
    }

    IEnumerator PerformExchange(GameObject item, ExchangeZone fromZone, ExchangeZone toZone)
    {
        Debug.Log($"强制交换: {item.name} 从 {fromZone.zoneID} 到 {toZone.zoneID}");

        if (exchangeEffect != null)
        {
            Instantiate(exchangeEffect, item.transform.position, Quaternion.identity);
        }

        if (exchangeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(exchangeSound);
        }

        yield return new WaitForSeconds(0.3f);

        Vector3 targetPosition = toZone.transform.position;
        item.transform.position = targetPosition;

        InteractableItem itemComponent = item.GetComponent<InteractableItem>();
        if (itemComponent != null)
        {
            itemComponent.ResetPhysics();
        }

        fromZone.hasItem = false;
        fromZone.currentItem = null;

        Debug.Log($"强制交换完成: {item.name} 已到达 {toZone.zoneID}");
    }

}
