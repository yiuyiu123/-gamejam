using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemExchangeSystem : MonoBehaviour
{
    [Header("������������")]
    public ExchangeZone player1Zone;
    public ExchangeZone player2Zone;

    [Header("����Ч��")]
    public ParticleSystem exchangeEffect;
    public AudioClip exchangeSound;

    [Header("ϵͳ����")]
    public float globalCooldown = 1f; // ȫ����ȴʱ��

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

        Debug.Log("��Ʒ����ϵͳ��ʼ�����");
    }
    IEnumerator PerformSafeExchange(GameObject item, ExchangeZone fromZone, ExchangeZone toZone)
    {
        // ����ȫ����ȴ
        isGlobalCooldown = true;

        Debug.Log($"��ʼ��ȫ����: {item.name} �� {fromZone.zoneID} �� {toZone.zoneID}");

        // ���Ž���Ч��
        if (exchangeEffect != null)
        {
            Instantiate(exchangeEffect, item.transform.position, Quaternion.identity);
        }

        if (exchangeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(exchangeSound);
        }

        // �����Ʒ���ڽ���
        InteractableItem itemComponent = item.GetComponent<InteractableItem>();
        if (itemComponent != null)
        {
            itemComponent.isInExchangeProcess = true;
        }

        // �����ӳ�
        yield return new WaitForSeconds(0.3f);

        // ������Ʒ
        Vector3 targetPosition = toZone.transform.position;
        item.transform.position = targetPosition;

        // ������Ʒ״̬
        if (itemComponent != null)
        {
            itemComponent.ResetPhysics();
        }


        // ��������״̬
        fromZone.hasItem = false;
        fromZone.currentItem = null;

        // �ȴ���ȡ����Ʒ����״̬
        yield return new WaitForSeconds(0.5f);
        if (itemComponent != null)
        {
            itemComponent.isInExchangeProcess = false;
        }

        Debug.Log($"��ȫ�������: {item.name} �ѵ��� {toZone.zoneID}");

        // ȫ����ȴ����
        yield return new WaitForSeconds(globalCooldown);
        isGlobalCooldown = false;
    }

    // ǿ�ƽ�����������ȴ�����ڲ��ԣ�
    public void ForceExchange(ExchangeZone fromZone)
    {
        if (fromZone.currentItem != null)
        {
            StartCoroutine(PerformExchange(fromZone.currentItem, fromZone, fromZone.targetZone));
        }
    }

    IEnumerator PerformExchange(GameObject item, ExchangeZone fromZone, ExchangeZone toZone)
    {
        Debug.Log($"ǿ�ƽ���: {item.name} �� {fromZone.zoneID} �� {toZone.zoneID}");

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

        Debug.Log($"ǿ�ƽ������: {item.name} �ѵ��� {toZone.zoneID}");
    }

}
