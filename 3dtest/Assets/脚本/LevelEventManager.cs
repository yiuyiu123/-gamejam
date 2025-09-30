using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEventManager : MonoBehaviour
{
    [Header("关卡事件")]
    public List<ItemTriggerZone> triggerZones = new List<ItemTriggerZone>();
    public List<DoorController> doors = new List<DoorController>();

    [Header("关卡状态")]
    public int keysCollected = 0;
    public int doorsOpened = 0;
    public bool levelCompleted = false;

    void Start()
    {
        // 自动查找场景中的触发区域和门
        if (triggerZones.Count == 0)
        {
            triggerZones.AddRange(FindObjectsOfType<ItemTriggerZone>());
        }

        if (doors.Count == 0)
        {
            doors.AddRange(FindObjectsOfType<DoorController>());
        }

        // 设置事件监听
        SetupEventListeners();

        Debug.Log($"关卡事件管理器初始化: {triggerZones.Count} 个触发区域, {doors.Count} 扇门");
    }

    void SetupEventListeners()
    {
        foreach (ItemTriggerZone zone in triggerZones)
        {
            // 清空现有事件，避免重复绑定
            zone.onTriggerSuccess.RemoveAllListeners();

            // 根据区域名称自动绑定对应的事件
            if (zone.zoneName.Contains("门") || zone.requiredItemName.Contains("钥匙"))
            {
                zone.onTriggerSuccess.AddListener(() => OnDoorUnlocked(zone));
            }
            else if (zone.zoneName.Contains("机关") || zone.zoneName.Contains("陷阱"))
            {
                zone.onTriggerSuccess.AddListener(() => OnMechanismTriggered(zone));
            }

            // 通用事件
            zone.onTriggerSuccess.AddListener(() => OnAnyTriggerSuccess(zone));
        }
    }

    void OnDoorUnlocked(ItemTriggerZone zone)
    {
        Debug.Log($"门锁触发: {zone.zoneName}");

        // 查找对应的门并解锁
        DoorController correspondingDoor = FindCorrespondingDoor(zone);
        if (correspondingDoor != null)
        {
            correspondingDoor.OnKeyUsed();
            doorsOpened++;
        }

        keysCollected++;
        CheckLevelCompletion();
    }

    void OnMechanismTriggered(ItemTriggerZone zone)
    {
        Debug.Log($"机关触发: {zone.zoneName}");
        // 这里可以添加机关触发的具体逻辑
    }

    void OnAnyTriggerSuccess(ItemTriggerZone zone)
    {
        Debug.Log($"触发区域激活: {zone.zoneName}");

        // 更新UI或播放全局效果
        UpdateLevelProgress();
    }

    DoorController FindCorrespondingDoor(ItemTriggerZone zone)
    {
        // 简单的匹配逻辑：通过名称或位置接近度
        foreach (DoorController door in doors)
        {
            if (Vector3.Distance(zone.transform.position, door.transform.position) < 5f)
            {
                return door;
            }
        }
        return null;
    }

    void UpdateLevelProgress()
    {
        // 更新UI显示
        Debug.Log($"进度: {doorsOpened}/{doors.Count} 扇门已开启");
    }

    void CheckLevelCompletion()
    {
        if (doorsOpened >= doors.Count && !levelCompleted)
        {
            levelCompleted = true;
            OnLevelComplete();
        }
    }

    void OnLevelComplete()
    {
        Debug.Log("关卡完成！");
        // 这里可以触发通关动画、显示胜利UI等
    }

    // 公共方法：重置关卡
    public void ResetLevel()
    {
        keysCollected = 0;
        doorsOpened = 0;
        levelCompleted = false;

        foreach (ItemTriggerZone zone in triggerZones)
        {
            zone.ResetTrigger();
        }

        foreach (DoorController door in doors)
        {
            door.LockDoor();
        }

        Debug.Log("关卡已重置");
    }
}
