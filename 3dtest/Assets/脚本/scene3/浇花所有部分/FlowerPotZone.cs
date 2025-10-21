using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPotZone : MonoBehaviour
{
    [Header("��������")]
    public string zoneID = "FlowerPot";
    public float detectionRadius = 3f;

    [Header("����Ч��")]
    public ParticleSystem wateringEffect;    // ��ˮ����Ч��
    public GameObject flowerPrefab;          // ����Ԥ���壨������
    public Transform flowerSpawnPoint;       // ��������λ��
    public GameObject keyPrefab;             // ���ɵ�Կ��Ԥ����
    public Transform keySpawnPoint;          // Կ������λ��

    [Header("����ʱ������")]
    public float wateringDuration = 2f;      // ��ˮ����ʱ��
    public float flowerGrowthDelay = 1f;     // �������ӳ�
    public float keySpawnDelay = 2f;         // Կ�������ӳ�
    public Vector3 customFlowerRotation = Vector3.zero;

    [Header("ˮ������")]
    public bool destroyWateringCanAfterUse = true; // ʹ�ú�ɾ��ˮ��

    [Header("״̬")]
    public bool hasBeenWatered = false;      // �Ƿ��Ѿ�����ˮ
    public bool isWatering = false;          // �Ƿ����ڽ�ˮ

    [Header("����ѡ��")]
    public bool showDebugInfo = true; // Ĭ�Ͽ�������

    [Header("�����ã��Ƿ񵯳�Կ��")]
    public bool isKeyAppear=false;

    private GameObject currentFlower;        // ��ǰ�Ļ�
    private AudioSource audioSource;
    private List<GameObject> playersInZone = new List<GameObject>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        EnsureColliderSize();

        Debug.Log($"�������� {zoneID} �ѳ�ʼ�������뾶: {detectionRadius}");
    }

    void EnsureColliderSize()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = detectionRadius;
            Debug.Log($"Ϊ�������� {zoneID} �����������ײ�壬�뾶: {detectionRadius}");
        }
        else if (collider is SphereCollider sphereCollider)
        {
            sphereCollider.radius = detectionRadius;
            sphereCollider.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1")|| other.CompareTag("Player2"))
        {
            if (!playersInZone.Contains(other.gameObject))
            {
                playersInZone.Add(other.gameObject);
                if (showDebugInfo)
                    Debug.Log($"��� {other.name} ���뻨������ {zoneID}�������������: {playersInZone.Count}");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playersInZone.Contains(other.gameObject))
            {
                playersInZone.Remove(other.gameObject);
                if (showDebugInfo)
                    Debug.Log($"��� {other.name} �뿪�������� {zoneID}�������������: {playersInZone.Count}");
            }
        }
    }

    // �������Ƿ��ڻ���������
    public bool IsPlayerInZone(GameObject player)
    {
        bool isInZone = playersInZone.Contains(player);
        if (showDebugInfo)
            Debug.Log($"������ {player.name} �Ƿ��ڻ�������: {isInZone}");
        return isInZone;
    }

    // ��ʼ��������
    public bool StartWatering(GameObject player, GameObject wateringCan)
    {
        if (hasBeenWatered)
        {
            Debug.Log($"���� {zoneID} �Ѿ�����ˮ���޷��ٴν�ˮ");
            return false;
        }

        if (isWatering)
        {
            Debug.Log($"���� {zoneID} ���ڽ�ˮ�����У���ȴ�");
            return false;
        }

        isWatering = true;

        if (showDebugInfo)
            Debug.Log($"���� {zoneID} ��ʼ��ˮ���̣����: {player.name}, ˮ��: {wateringCan.name}");

        StartCoroutine(WateringProcess(player, wateringCan));
        return true;
    }

    // ��������Э��
    IEnumerator WateringProcess(GameObject player, GameObject wateringCan)
    {
        Debug.Log($"��ʼ��������Э�̣����: {player.name}");

        // ������Һ�ˮ��״̬
        PlayerController playerController = player.GetComponent<PlayerController>();
        InteractableItem wateringCanItem = wateringCan.GetComponent<InteractableItem>();

        if (playerController != null)
        {
            playerController.SetTemporaryLock(true);
        }

        if (wateringCanItem != null)
        {
            wateringCanItem.isInExchangeProcess = true;
        }

        // 1. ���Ž�ˮ����Ч��
        if (wateringEffect != null)
        {
            wateringEffect.Play();
            Debug.Log("���Ž�ˮ����Ч��");

            // ������Ч
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }
        }

        // �ȴ���ˮ���
        yield return new WaitForSeconds(wateringDuration);

        // 2. ֹͣ����Ч��
        if (wateringEffect != null)
        {
            wateringEffect.Stop();
        }

        // 3. �ȴ��������ӳ�
        yield return new WaitForSeconds(flowerGrowthDelay);


        if (flowerPrefab != null && flowerSpawnPoint != null)
        {
            // ʹ���Զ��巽�򴴽���ת
            Quaternion customRotation = Quaternion.Euler(customFlowerRotation);
            currentFlower = Instantiate(flowerPrefab, flowerSpawnPoint.position, customRotation);
            Debug.Log("��������");

            // ���Ż�����������������У�
            Animator flowerAnimator = currentFlower.GetComponent<Animator>();
            if (flowerAnimator != null)
            {
                flowerAnimator.SetTrigger("Grow");
            }
        }

        // 5. �ȴ�Կ�������ӳ�
        yield return new WaitForSeconds(keySpawnDelay);

        // 6. ����Կ��
        if (keyPrefab != null)
        {
            Vector3 spawnPosition = keySpawnPoint != null ? keySpawnPoint.position : transform.position + Vector3.up * 2f;
            GameObject newKey = Instantiate(keyPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("Կ��������");

            // ��Կ����ӵ���Ч��
            Rigidbody keyRb = newKey.GetComponent<Rigidbody>();
            if (keyRb != null)
            {
                Vector3 popDirection = new Vector3(Random.Range(-0.5f, 0.5f), 1f, Random.Range(-0.5f, 0.5f)).normalized;
                keyRb.AddForce(popDirection * 5f, ForceMode.Impulse);
                //������
                isKeyAppear = true;
            }
        }

        // 7. ɾ��ˮ������������ˣ�
        if (destroyWateringCanAfterUse && wateringCan != null)
        {
            Debug.Log("׼��ɾ��ˮ��");

            // ��ǿ���ͷ�ˮ��
            if (playerController != null)
            {
                playerController.ForceReleaseItem();
            }

            // Ȼ������ˮ������
            Destroy(wateringCan);
            Debug.Log("ˮ���ѱ�ɾ��");
        }

        // ���Ϊ�ѽ�ˮ
        hasBeenWatered = true;
        isWatering = false;

        // ������ң����ˮ��û�б�ɾ����
        if (playerController != null && !destroyWateringCanAfterUse)
        {
            playerController.SetTemporaryLock(false);
        }

        if (wateringCanItem != null && !destroyWateringCanAfterUse)
        {
            wateringCanItem.isInExchangeProcess = false;
        }

        Debug.Log("�����������");
    }

    // ���û���״̬�����ڵ��Ի����¿�ʼ��
    [ContextMenu("���û���״̬")]
    public void ResetFlowerPot()
    {
        hasBeenWatered = false;
        isWatering = false;

        if (currentFlower != null)
        {
            Destroy(currentFlower);
            currentFlower = null;
        }

        if (wateringEffect != null)
        {
            wateringEffect.Stop();
        }

        playersInZone.Clear();

        Debug.Log($"���� {zoneID} ������");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (flowerSpawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(flowerSpawnPoint.position, 0.3f);
        }

        if (keySpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(keySpawnPoint.position, 0.2f);
        }
    }
}