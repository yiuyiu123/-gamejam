using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItem : MonoBehaviour
{
   
    [Header("��Ʒ����")]
    public string itemName = "��Ʒ";
    public bool canBePickedUp = true;
    public Vector3 holdOffset = new Vector3(0, 1, 1);

    [Header("���սǶ�����")]
    public Vector3 holdRotationOffset = Vector3.zero; // ����ʱ�ĽǶ�ƫ��

    [Header("��������")]
    public bool canBeExchanged = true;
    public bool isExchangeLocked = false;
    public string lastExchangeZone = "";

    [Header("״̬")]
    public bool isBeingHeld = false;
    public bool isInExchangeProcess = false;
    public GameObject currentHolder = null;

    [Header("ʰȡ��������")]
    public float pickUpTransitionTime = 0.3f;
    public AnimationCurve pickUpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine transitionCoroutine;
    private bool isTransitioning = false;

    private Rigidbody rb;
    private Collider itemCollider;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private ExchangeZone currentZone;

    // ��ӹ�������������˽���ֶ�
    public Rigidbody Rb => rb;
    public Collider ItemCollider => itemCollider;
    public Vector3 OriginalPosition => originalPosition;
    public Quaternion OriginalRotation => originalRotation;
    public Vector3 OriginalScale => originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (isBeingHeld && currentHolder != null && !isInExchangeProcess)
        {
            FollowHolder();
        }

        CheckIfLeftZone();
    }

    void LateUpdate()
    {
        // ��ÿ֡�����ǿ�Ʊ������ţ�ȷ�������κ��������ܵ������޸�
        if (isBeingHeld)
        {
            transform.localScale = originalScale;
        }
    }

    // �� Interact ��������Ӽ��
    public void Interact(GameObject player)
    {
        if (!canBePickedUp || isInExchangeProcess) return; // ��� canBePickedUp ���

        if (!isBeingHeld)
        {
            PickUp(player);
        }
        else
        {
            PutDown();
        }
    }

    void PickUp(GameObject player)
    {
        if (!canBePickedUp || isTransitioning) return;

        // �������û���״̬�������߼�����
        isBeingHeld = true;
        currentHolder = player;
        originalScale = transform.localScale;

        ResetExchangeLock();

        // ��������Ч��
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }

        // ��ʼ���ɶ���
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(PickUpTransition(player));

        Debug.Log($"{player.name} ������ {itemName}");
    }

    IEnumerator PickUpTransition(GameObject player)
    {
        isTransitioning = true;

        // �����ʼ״̬
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < pickUpTransitionTime && currentHolder != null)
        {
            elapsedTime += Time.deltaTime;
            float t = pickUpCurve.Evaluate(elapsedTime / pickUpTransitionTime);

            // ���㵱ǰ֡��Ŀ��λ��
            Vector3 targetPosition = GetTargetHoldPosition();
            Quaternion targetRotation = GetTargetHoldRotation();

            // ƽ����ֵ
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        // ȷ������λ��׼ȷ
        if (currentHolder != null)
        {
            transform.position = GetTargetHoldPosition();
            transform.rotation = GetTargetHoldRotation();
        }

        isTransitioning = false;
    }

    public void ResetItemState()
    {
        isBeingHeld = false;
        isInExchangeProcess = false;
        canBePickedUp = true;
        currentHolder = null;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }
    }

    public void PutDown()
    {
        if (!isBeingHeld) return; // ����Ѿ����ǳ���״̬��ֱ�ӷ���

        isBeingHeld = false;

        // �ָ�����Ч��
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // �ָ���ײ��
        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }

        // ����ȷ������
        transform.localScale = originalScale;

        Debug.Log($"{currentHolder?.name} ������ {itemName}");
        currentHolder = null;
    }
    void FollowHolder()
    {
        if (currentHolder == null || isTransitioning) return;

        // ֱ������λ�ã�û���ӳ�
        transform.position = GetTargetHoldPosition();
        transform.rotation = GetTargetHoldRotation();
        transform.localScale = originalScale;
    }
    Vector3 GetTargetHoldPosition()
    {
        return currentHolder.transform.position +
               currentHolder.transform.forward * holdOffset.z +
               currentHolder.transform.up * holdOffset.y +
               currentHolder.transform.right * holdOffset.x;
    }

    Quaternion GetTargetHoldRotation()
    {
        return currentHolder.transform.rotation * Quaternion.Euler(holdRotationOffset);
    }

    [Header("����ѡ��")]
    public bool showDebugInfo = false;

    // ����Ƿ��뿪�˽�������
    void CheckIfLeftZone()
    {
        if (currentZone != null && !isBeingHeld)
        {
            float distance = Vector3.Distance(transform.position, currentZone.transform.position);
            if (distance > currentZone.detectionRadius * 1.2f)
            {
                OnLeftZone();
            }
        }
    }

    // ����Ʒ�뿪����ʱ����
    void OnLeftZone()
    {
        if (currentZone != null)
        {
            Debug.Log($"��Ʒ {itemName} �뿪������ {currentZone.zoneID}");
            currentZone.OnItemLeft(this.gameObject);
            currentZone = null;
            ResetExchangeLock();
        }
    }

    // �����Ʒ�ѱ�����
    public void MarkAsExchanged(string fromZoneID)
    {
        isExchangeLocked = true;
        lastExchangeZone = fromZoneID;
        Debug.Log($"��Ʒ {itemName} �����Ϊ�ѽ������������� {fromZoneID}");
    }

    // ���ý�������
    public void ResetExchangeLock()
    {
        if (isExchangeLocked)
        {
            isExchangeLocked = false;
            lastExchangeZone = "";
            Debug.Log($"��Ʒ {itemName} �Ľ�������������");
        }
    }

    // ����Ƿ���Խ�����ָ������
    public bool CanExchangeTo(string targetZoneID)
    {
        if (!canBeExchanged || isExchangeLocked || isBeingHeld || isInExchangeProcess)
            return false;

        if (lastExchangeZone == targetZoneID)
            return false;

        return true;
    }

    // ���õ�ǰ���ڵ�����
    public void SetCurrentZone(ExchangeZone zone)
    {
        currentZone = zone;
    }

    // ������Ʒ��ԭʼλ��
    public void ResetItem()
    {
        PutDown();
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;
        isInExchangeProcess = false;
        ResetExchangeLock();
    }

    // ��������״̬
    public void ResetPhysics()
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void ForceRelease()
    {
        if (isBeingHeld)
        {
            // ֹͣ���ɶ���
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                isTransitioning = false;
            }

            isBeingHeld = false;
            currentHolder = null;

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            if (itemCollider != null)
            {
                itemCollider.enabled = true;
            }

            Debug.Log($"ǿ���ͷ���Ʒ: {itemName}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (isBeingHeld && currentHolder != null)
        {
            Gizmos.color = isExchangeLocked ? Color.red : (isInExchangeProcess ? Color.yellow : Color.green);
            Vector3 holdPosition = currentHolder.transform.position +
                                  currentHolder.transform.forward * holdOffset.z +
                                  currentHolder.transform.up * holdOffset.y +
                                  currentHolder.transform.right * holdOffset.x;
            Gizmos.DrawWireSphere(holdPosition, 0.2f);
        }

        if (isExchangeLocked)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.2f);
        }
    }
}