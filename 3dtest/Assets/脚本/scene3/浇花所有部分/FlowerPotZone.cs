using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPotZone : MonoBehaviour
{
    [Header("花盆设置")]
    public string zoneID = "FlowerPot";
    public float detectionRadius = 3f;

    [Header("浇花效果")]
    public ParticleSystem wateringEffect;    // 浇水粒子效果
    public GameObject flowerPrefab;          // 花的预制体（动画）
    public Transform flowerSpawnPoint;       // 花生长的位置
    public GameObject keyPrefab;             // 生成的钥匙预制体
    public Transform keySpawnPoint;          // 钥匙生成位置

    [Header("浇花时间设置")]
    public float wateringDuration = 2f;      // 浇水持续时间
    public float flowerGrowthDelay = 1f;     // 花生长延迟
    public float keySpawnDelay = 2f;         // 钥匙生成延迟
    public Vector3 customFlowerRotation = Vector3.zero;

    [Header("水壶设置")]
    public bool destroyWateringCanAfterUse = true; // 使用后删除水壶

    [Header("状态")]
    public bool hasBeenWatered = false;      // 是否已经浇过水
    public bool isWatering = false;          // 是否正在浇水

    [Header("调试选项")]
    public bool showDebugInfo = true; // 默认开启调试

    [Header("张奕忻：是否弹出钥匙")]
    public bool isKeyAppear=false;

    private GameObject currentFlower;        // 当前的花
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

        Debug.Log($"花盆区域 {zoneID} 已初始化，检测半径: {detectionRadius}");
    }

    void EnsureColliderSize()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = detectionRadius;
            Debug.Log($"为花盆区域 {zoneID} 添加了球形碰撞体，半径: {detectionRadius}");
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
                    Debug.Log($"玩家 {other.name} 进入花盆区域 {zoneID}，区域内玩家数: {playersInZone.Count}");
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
                    Debug.Log($"玩家 {other.name} 离开花盆区域 {zoneID}，区域内玩家数: {playersInZone.Count}");
            }
        }
    }

    // 检查玩家是否在花盆区域内
    public bool IsPlayerInZone(GameObject player)
    {
        bool isInZone = playersInZone.Contains(player);
        if (showDebugInfo)
            Debug.Log($"检查玩家 {player.name} 是否在花盆区域: {isInZone}");
        return isInZone;
    }

    // 开始浇花过程
    public bool StartWatering(GameObject player, GameObject wateringCan)
    {
        if (hasBeenWatered)
        {
            Debug.Log($"花盆 {zoneID} 已经浇过水，无法再次浇水");
            return false;
        }

        if (isWatering)
        {
            Debug.Log($"花盆 {zoneID} 正在浇水过程中，请等待");
            return false;
        }

        isWatering = true;

        if (showDebugInfo)
            Debug.Log($"花盆 {zoneID} 开始浇水过程，玩家: {player.name}, 水壶: {wateringCan.name}");

        StartCoroutine(WateringProcess(player, wateringCan));
        return true;
    }

    // 浇花过程协程
    IEnumerator WateringProcess(GameObject player, GameObject wateringCan)
    {
        Debug.Log($"开始浇花过程协程，玩家: {player.name}");

        // 锁定玩家和水壶状态
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

        // 1. 播放浇水粒子效果
        if (wateringEffect != null)
        {
            wateringEffect.Play();
            Debug.Log("播放浇水粒子效果");

            // 播放音效
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }
        }

        // 等待浇水完成
        yield return new WaitForSeconds(wateringDuration);

        // 2. 停止粒子效果
        if (wateringEffect != null)
        {
            wateringEffect.Stop();
        }

        // 3. 等待花生长延迟
        yield return new WaitForSeconds(flowerGrowthDelay);


        if (flowerPrefab != null && flowerSpawnPoint != null)
        {
            // 使用自定义方向创建旋转
            Quaternion customRotation = Quaternion.Euler(customFlowerRotation);
            currentFlower = Instantiate(flowerPrefab, flowerSpawnPoint.position, customRotation);
            Debug.Log("花已生长");

            // 播放花的生长动画（如果有）
            Animator flowerAnimator = currentFlower.GetComponent<Animator>();
            if (flowerAnimator != null)
            {
                flowerAnimator.SetTrigger("Grow");
            }
        }

        // 5. 等待钥匙生成延迟
        yield return new WaitForSeconds(keySpawnDelay);

        // 6. 生成钥匙
        if (keyPrefab != null)
        {
            Vector3 spawnPosition = keySpawnPoint != null ? keySpawnPoint.position : transform.position + Vector3.up * 2f;
            GameObject newKey = Instantiate(keyPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("钥匙已生成");

            // 给钥匙添加弹出效果
            Rigidbody keyRb = newKey.GetComponent<Rigidbody>();
            if (keyRb != null)
            {
                Vector3 popDirection = new Vector3(Random.Range(-0.5f, 0.5f), 1f, Random.Range(-0.5f, 0.5f)).normalized;
                keyRb.AddForce(popDirection * 5f, ForceMode.Impulse);
                //张奕忻
                isKeyAppear = true;
            }
        }

        // 7. 删除水壶（如果设置了）
        if (destroyWateringCanAfterUse && wateringCan != null)
        {
            Debug.Log("准备删除水壶");

            // 先强制释放水壶
            if (playerController != null)
            {
                playerController.ForceReleaseItem();
            }

            // 然后销毁水壶对象
            Destroy(wateringCan);
            Debug.Log("水壶已被删除");
        }

        // 标记为已浇水
        hasBeenWatered = true;
        isWatering = false;

        // 解锁玩家（如果水壶没有被删除）
        if (playerController != null && !destroyWateringCanAfterUse)
        {
            playerController.SetTemporaryLock(false);
        }

        if (wateringCanItem != null && !destroyWateringCanAfterUse)
        {
            wateringCanItem.isInExchangeProcess = false;
        }

        Debug.Log("浇花过程完成");
    }

    // 重置花盆状态（用于调试或重新开始）
    [ContextMenu("重置花盆状态")]
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

        Debug.Log($"花盆 {zoneID} 已重置");
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