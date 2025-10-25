using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractableItem : MonoBehaviour
{
    [Header("��Ʒ����")]
    public string itemName = "��Ʒ";
    [TextArea(3, 5)] // ��Ӷ����ı����򣬷����������������
    public string itemDescription = ""; // ��Ʒ����
    public bool canBePickedUp = true;
    public Vector3 holdOffset = new Vector3(0, 1, 1);

    [Header("�ؼ���������")]
    public bool isKeyItem = false; // �Ƿ�Ϊ�ؼ�����
    public bool showPickupMessage = true; // �Ƿ���ʾʰȡ��ʾ

    [Header("UI��ʾ����")]
    public GameObject pickupUIPrefab; // UIԤ����
    public float displayTime = 3f; // ��ʾʱ��
    public float fadeDuration = 0.5f; // ����ʱ��

    [Header("UIλ������")]
    [Range(0f, 1f)]
    public float uiVerticalPosition = 0.3f; // UI��ֱλ�ã���Ļ�߶ȵı�����0=�ײ���1=������
    public float uiHorizontalOffset = 0f; // ˮƽ΢��ƫ��

    [Header("��ʾ��������")]
    public string pickupMessageFormat = "�ҵ��� {0}"; // ��ʾ��Ϣ��ʽ��{0}�ᱻitemName�滻
    [TextArea(2, 4)]
    public string customPickupMessage = ""; // �Զ�����ʾ��Ϣ�����Ϊ����ʹ��Ĭ�ϸ�ʽ

    [Header("���սǶ�����")]
    public Vector3 holdRotationOffset = Vector3.zero; // ����ʱ�ĽǶ�ƫ��

    [Header("��������")]
    public bool canBeExchanged = true;
    public bool isExchangeLocked = false;
    public string lastExchangeZone = "";

    [Header("������������")]
    public bool limitExchangeTimes = true;  // �Ƿ����ƽ�������
    public int maxExchangeTimes = 1;        // ��󽻻�����
    public int currentExchangeTimes = 0;    // ��ǰ��������

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

    // UI��ر���
    private Dictionary<GameObject, GameObject> playerUIInstances = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, Coroutine> playerUICoroutines = new Dictionary<GameObject, Coroutine>();

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

        // ��ʼ����������
        currentExchangeTimes = 0;

        // ���û������UIԤ���壬���Զ�̬����һ���򵥵�UI
        if (pickupUIPrefab == null)
        {
            CreateDefaultUIPrefab();
        }
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
        if (!canBePickedUp || isTransitioning || player == null) return;

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

        // ��ʾUI��ʾ
        if (isKeyItem && showPickupMessage)
        {
            ShowPickupUI(player);
        }

        // ��ʼ���ɶ���
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(PickUpTransition(player));

        Debug.Log($"{player.name} ������ {itemName}");
    }

    // ��ʾʰȡUI��ʾ
    private void ShowPickupUI(GameObject player)
    {
        // ����Ѿ���UI����ʾ����ֹ֮ͣǰ��Э��
        if (playerUICoroutines.ContainsKey(player) && playerUICoroutines[player] != null)
        {
            StopCoroutine(playerUICoroutines[player]);
        }

        // �������ȡUIʵ��
        GameObject uiInstance = GetOrCreateUIInstance(player);

        if (uiInstance != null)
        {
            // ��ʼ��ʾUI��Э��
            playerUICoroutines[player] = StartCoroutine(ShowPickupMessageRoutine(player, uiInstance));
        }
    }

    // ��ȡ�򴴽�UIʵ��
    private GameObject GetOrCreateUIInstance(GameObject player)
    {
        // ���������ٵ�����
        if (playerUIInstances.ContainsKey(player) && playerUIInstances[player] == null)
        {
            playerUIInstances.Remove(player);
        }

        if (playerUICoroutines.ContainsKey(player) && playerUICoroutines[player] == null)
        {
            playerUICoroutines.Remove(player);
        }

        if (!playerUIInstances.ContainsKey(player) || playerUIInstances[player] == null)
        {
            if (pickupUIPrefab != null)
            {
                // �����µ�UIʵ��
                GameObject uiInstance = Instantiate(pickupUIPrefab);

                // ����UI�ĸ�����ΪCanvas
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    uiInstance.transform.SetParent(canvas.transform, false);
                }

                // ����UIλ�ú�ê��
                RectTransform rectTransform = uiInstance.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    int playerIndex = GetPlayerIndex(player);

                    if (playerIndex == 0) // ���1 - ����
                    {
                        // �����ײ����У�ˮƽ0-50%����ֱ�ײ�
                        rectTransform.anchorMin = new Vector2(0f, 0f);
                        rectTransform.anchorMax = new Vector2(0.5f, 0f);
                        rectTransform.pivot = new Vector2(0.5f, 0f);
                        rectTransform.anchoredPosition = new Vector2(0, uiVerticalPosition * 100);
                    }
                    else // ���2 - ����
                    {
                        // �����ײ����У�ˮƽ50%-100%����ֱ�ײ�
                        rectTransform.anchorMin = new Vector2(0.5f, 0f);
                        rectTransform.anchorMax = new Vector2(1f, 0f);
                        rectTransform.pivot = new Vector2(0.5f, 0f);
                        rectTransform.anchoredPosition = new Vector2(0, uiVerticalPosition * 100);
                    }
                }

                playerUIInstances[player] = uiInstance;
            }
            else
            {
                Debug.LogWarning("Pickup UI Prefab δ���ã�");
                return null;
            }
        }

        return playerUIInstances[player];
    }

    // ��ȡ�������
    private int GetPlayerIndex(GameObject player)
    {
        // ������Ҷ�������ƻ��ǩ�ж����ĸ����
        if (player.name.Contains("Player1") || player.CompareTag("Player1"))
            return 0;
        else if (player.name.Contains("Player2") || player.CompareTag("Player2"))
            return 1;
        else
        {
            Debug.LogWarning($"�޷�ȷ���������: {player.name}");
            return 0;
        }
    }

    // ������ʾ��Ϣ
    private string GeneratePickupMessage()
    {
        // ������Զ�����Ϣ������ʹ��
        if (!string.IsNullOrEmpty(customPickupMessage))
        {
            return customPickupMessage;
        }

        // ����ʹ�ø�ʽ����Ϣ
        if (!string.IsNullOrEmpty(pickupMessageFormat))
        {
            return string.Format(pickupMessageFormat, itemName);
        }

        // Ĭ����Ϣ
        return $"�ҵ��� {itemName}";
    }

    // ��ʾʰȡ��Ϣ��Э��
    private IEnumerator ShowPickupMessageRoutine(GameObject player, GameObject uiInstance)
    {
        // �����������Ƿ���Ч
        if (uiInstance == null || player == null)
            yield break;

        // ��ȡ�ı����
        TextMeshProUGUI textComponent = uiInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent == null)
        {
            Debug.LogWarning("UIʵ����δ�ҵ�TextMeshProUGUI�����");
            yield break;
        }

        // �����ı�����
        textComponent.text = GeneratePickupMessage();

        // ������ʾ����͸����
        textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 1f);
        uiInstance.SetActive(true);

        // �ȴ���ʾʱ��
        yield return new WaitForSeconds(displayTime);

        // ����Ч��
        float elapsedTime = 0f;
        Color startColor = textComponent.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsedTime < fadeDuration)
        {
            // ÿ֡���������Ƿ���Ȼ��Ч
            if (uiInstance == null || textComponent == null)
                yield break;

            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            textComponent.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // ��ȫ͸��������UI���������Ƿ���Ȼ��Ч
        if (uiInstance != null && textComponent != null)
        {
            textComponent.color = targetColor;
            uiInstance.SetActive(false);
        }

        // ����Э������
        if (playerUICoroutines.ContainsKey(player))
        {
            playerUICoroutines[player] = null;
        }
    }

    // ����Ĭ��UIԤ���壨���û�����õĻ���
    private void CreateDefaultUIPrefab()
    {
        // ������Դ���һ���򵥵�UIԤ����
        // ��ʵ����Ŀ�У������ڱ༭�������ú�UIԤ����
        Debug.Log("������Pickup UI Prefab���ڱ༭���д���UIԪ��");
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

        // ���ӽ�������
        if (limitExchangeTimes)
        {
            currentExchangeTimes++;
            Debug.Log($"��Ʒ {itemName} �����Ϊ�ѽ������������� {fromZoneID}����������: {currentExchangeTimes}/{maxExchangeTimes}");

            // ����Ƿ�ﵽ������������
            if (currentExchangeTimes >= maxExchangeTimes)
            {
                canBeExchanged = false;
                Debug.Log($"��Ʒ {itemName} �Ѵﵽ��󽻻����� ({maxExchangeTimes})����ֹ�ٴν���");
            }
        }
        else
        {
            Debug.Log($"��Ʒ {itemName} �����Ϊ�ѽ������������� {fromZoneID}");
        }
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
        // ��齻����������
        if (limitExchangeTimes && currentExchangeTimes >= maxExchangeTimes)
        {
            if (showDebugInfo) Debug.Log($"��Ʒ {itemName} �Ѵﵽ��󽻻��������޷�����");
            return false;
        }

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

    // ���ý����������������������������Ʒ״̬��
    public void ResetExchangeTimes()
    {
        currentExchangeTimes = 0;
        canBeExchanged = true;
        Debug.Log($"��Ʒ {itemName} �Ľ�������������");
    }

    // ���ý�����������
    public void SetExchangeLimit(int maxTimes)
    {
        maxExchangeTimes = maxTimes;
        limitExchangeTimes = true;
        Debug.Log($"��Ʒ {itemName} �Ľ���������������Ϊ: {maxTimes}");
    }

    // �Ƴ�������������
    public void RemoveExchangeLimit()
    {
        limitExchangeTimes = false;
        Debug.Log($"��Ʒ {itemName} �Ľ��������������Ƴ�");
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

    void OnDestroy()
    {
        // ֹͣ�������е�Э��
        foreach (var pair in playerUICoroutines)
        {
            if (pair.Value != null)
            {
                StopCoroutine(pair.Value);
            }
        }

        // ��������UIʵ��
        foreach (var uiInstance in playerUIInstances.Values)
        {
            if (uiInstance != null)
            {
                Destroy(uiInstance);
            }
        }
        playerUIInstances.Clear();
        playerUICoroutines.Clear();
    }

    // �����Ʒ������ʱ������
    void OnDisable()
    {
        // ֹͣ����Э�̵�������UI����Ϊ��Ʒ����ֻ����ʱ����
        foreach (var pair in playerUICoroutines)
        {
            if (pair.Value != null)
            {
                StopCoroutine(pair.Value);
                playerUICoroutines[pair.Key] = null;
            }
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

        // ��ʾ����������Ϣ
        if (limitExchangeTimes && showDebugInfo)
        {
#if UNITY_EDITOR
            string exchangeInfo = $"{itemName}\n����: {currentExchangeTimes}/{maxExchangeTimes}";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, exchangeInfo);
#endif
        }
    }
}