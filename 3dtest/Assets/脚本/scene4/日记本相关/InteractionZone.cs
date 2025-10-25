using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    [Header("���������")]
    public Camera playerCamera;

    [Header("UI��ʾԤ����")]
    public GameObject uiHintPrefab;

    [Header("������ƷԤ����")]
    public GameObject interactableItemPrefab;

    [Header("UI��ʾƫ��")]
    public Vector3 uiOffset = new Vector3(0, 2f, 0);

    private GameObject currentUIHint;
    private bool playerInZone = false;
    private GameObject spawnedItem;

    void Start()
    {
        // ȷ������ײ�� 
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
            GetComponent<Collider>().isTrigger = true;
        }
    }

    void Update()
    {
        // ����UI���������
        if (currentUIHint != null && playerCamera != null)
        {
            currentUIHint.transform.LookAt(currentUIHint.transform.position +
                playerCamera.transform.rotation * Vector3.forward,
                playerCamera.transform.rotation * Vector3.up);
        }

        // ���H������ 
        if (playerInZone && Input.GetKeyDown(KeyCode.H))
        {
            SpawnInteractableItem();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player2"))
        {
            playerInZone = true;
            ShowUIHint();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player2"))
        {
            playerInZone = false;
            HideUIHint();
            // �Ƴ���DestroySpawnedItem()���ã���Ʒ������ʧ
        }
    }

    void ShowUIHint()
    {
        if (uiHintPrefab != null && currentUIHint == null)
        {
            currentUIHint = Instantiate(uiHintPrefab,
                transform.position + uiOffset,
                Quaternion.identity);
        }
    }

    void HideUIHint()
    {
        if (currentUIHint != null)
        {
            Destroy(currentUIHint);
            currentUIHint = null;
        }
    }

    void SpawnInteractableItem()
    {
        // ����Ѿ�������Ʒ�������� 
        DestroySpawnedItem();

        if (interactableItemPrefab != null)
        {
            // ����������������Ʒ
            spawnedItem = Instantiate(interactableItemPrefab,
                transform.position,
                Quaternion.identity);

            Debug.Log("������Ʒ�����ɣ�");
        }
    }

    void DestroySpawnedItem()
    {
        if (spawnedItem != null)
        {
            Destroy(spawnedItem);
            spawnedItem = null;
        }
    }
}