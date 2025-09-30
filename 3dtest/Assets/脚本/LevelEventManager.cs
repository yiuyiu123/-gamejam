using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEventManager : MonoBehaviour
{
    [Header("�ؿ��¼�")]
    public List<ItemTriggerZone> triggerZones = new List<ItemTriggerZone>();
    public List<DoorController> doors = new List<DoorController>();

    [Header("�ؿ�״̬")]
    public int keysCollected = 0;
    public int doorsOpened = 0;
    public bool levelCompleted = false;

    void Start()
    {
        // �Զ����ҳ����еĴ����������
        if (triggerZones.Count == 0)
        {
            triggerZones.AddRange(FindObjectsOfType<ItemTriggerZone>());
        }

        if (doors.Count == 0)
        {
            doors.AddRange(FindObjectsOfType<DoorController>());
        }

        // �����¼�����
        SetupEventListeners();

        Debug.Log($"�ؿ��¼���������ʼ��: {triggerZones.Count} ����������, {doors.Count} ����");
    }

    void SetupEventListeners()
    {
        foreach (ItemTriggerZone zone in triggerZones)
        {
            // ��������¼��������ظ���
            zone.onTriggerSuccess.RemoveAllListeners();

            // �������������Զ��󶨶�Ӧ���¼�
            if (zone.zoneName.Contains("��") || zone.requiredItemName.Contains("Կ��"))
            {
                zone.onTriggerSuccess.AddListener(() => OnDoorUnlocked(zone));
            }
            else if (zone.zoneName.Contains("����") || zone.zoneName.Contains("����"))
            {
                zone.onTriggerSuccess.AddListener(() => OnMechanismTriggered(zone));
            }

            // ͨ���¼�
            zone.onTriggerSuccess.AddListener(() => OnAnyTriggerSuccess(zone));
        }
    }

    void OnDoorUnlocked(ItemTriggerZone zone)
    {
        Debug.Log($"��������: {zone.zoneName}");

        // ���Ҷ�Ӧ���Ų�����
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
        Debug.Log($"���ش���: {zone.zoneName}");
        // ���������ӻ��ش����ľ����߼�
    }

    void OnAnyTriggerSuccess(ItemTriggerZone zone)
    {
        Debug.Log($"�������򼤻�: {zone.zoneName}");

        // ����UI�򲥷�ȫ��Ч��
        UpdateLevelProgress();
    }

    DoorController FindCorrespondingDoor(ItemTriggerZone zone)
    {
        // �򵥵�ƥ���߼���ͨ�����ƻ�λ�ýӽ���
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
        // ����UI��ʾ
        Debug.Log($"����: {doorsOpened}/{doors.Count} �����ѿ���");
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
        Debug.Log("�ؿ���ɣ�");
        // ������Դ���ͨ�ض�������ʾʤ��UI��
    }

    // �������������ùؿ�
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

        Debug.Log("�ؿ�������");
    }
}
