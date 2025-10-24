using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("��״̬")]
    public bool isLocked = true;
    public bool isOpen = false;

    [Header("��λ������ - ��ק���嵽����")]
    public Transform closedStateTarget; // ����ʱ��Ŀ������
    public Transform openStateTarget;   // ����ʱ��Ŀ������

    [Header("��������")]
    public float openSpeed = 2f;
    public bool useAnimation = false;
    public Animator doorAnimator;
    public string openAnimationName = "Open";
    public string closeAnimationName = "Close";

    [Header("����Ч��")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockSound;
    public AudioClip unlockSound;

    [Header("��������")]
    public bool autoClose = false; // �Ƿ��Զ�����
    public float autoCloseDelay = 3f; // �Զ������ӳ�

    // �ڲ��洢��ʵ��λ�ú���ת
    private Vector3 closedPosition;
    private Quaternion closedRotation;
    private Vector3 openPosition;
    private Quaternion openRotation;

    private AudioSource audioSource;
    private Coroutine autoCloseCoroutine;
    private bool isInitialized = false;

    void Awake()
    {
        InitializeDoor();
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        isInitialized = true;
    }

    void InitializeDoor()
    {
        // ��Ŀ�������ȡλ�ú���ת������ʹ��Ĭ��ֵ
        UpdatePositionsFromTargets();

        // ���ݳ�ʼ״̬�����ŵ�λ��
        if (isOpen)
        {
            transform.position = openPosition;
            transform.rotation = openRotation;
        }
        else
        {
            transform.position = closedPosition;
            transform.rotation = closedRotation;
        }
    }

    void Update()
    {
        if (!isLocked && !useAnimation && isInitialized)
        {
            Vector3 targetPosition = isOpen ? openPosition : closedPosition;
            Quaternion targetRotation = isOpen ? openRotation : closedRotation;

            // ƽ���ƶ���Ŀ��״̬
            transform.position = Vector3.Lerp(transform.position, targetPosition, openSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, openSpeed * Time.deltaTime);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // �ڱ༭��ģʽ�£���Ŀ���������ʱ����λ��
        if (!Application.isPlaying)
        {
            UpdatePositionsFromTargets();

            // �����ǰû�����У�����ͬ������ȷλ��
            if (isOpen)
            {
                transform.position = openPosition;
                transform.rotation = openRotation;
            }
            else
            {
                transform.position = closedPosition;
                transform.rotation = closedRotation;
            }
        }
    }
#endif

    void UpdatePositionsFromTargets()
    {
        // ��Ŀ�������ȡλ�ú���ת
        if (closedStateTarget != null)
        {
            closedPosition = closedStateTarget.position;
            closedRotation = closedStateTarget.rotation;
        }
        else
        {
            // ���û������Ŀ�꣬ʹ�õ�ǰλ��
            closedPosition = transform.position;
            closedRotation = transform.rotation;
        }

        if (openStateTarget != null)
        {
            openPosition = openStateTarget.position;
            openRotation = openStateTarget.rotation;
        }
        else
        {
            // ���û������Ŀ�꣬ʹ�õ�ǰλ��
            openPosition = transform.position;
            openRotation = transform.rotation;
        }
    }

    [ContextMenu("����")]
    public void OpenDoor()
    {
        if (isLocked)
        {
            //Debug.Log("�ű���ס�ˣ�");
            //PlaySound(lockSound);
            //return;
            isLocked = false;
        }

        if (!isOpen)
        {
            isOpen = true;
            Debug.Log("����");

            if (useAnimation && doorAnimator != null)
            {
                doorAnimator.Play(openAnimationName);
            }

            PlaySound(openSound);
            //������
            StartCoroutine(Scene3UI_Manager.Instance.DelayedSceneTransition(1f)); // �ӳ�1��������

            // �����Զ�����
            if (autoClose)
            {
                if (autoCloseCoroutine != null)
                    StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = StartCoroutine(AutoCloseAfterDelay());
            }
        }
    }
    #region ����
    //[ContextMenu("����")]
    //public void CloseDoor()
    //{
    //    if (isOpen)
    //    {
    //        isOpen = false;
    //        Debug.Log("����");

    //        if (useAnimation && doorAnimator != null)
    //        {
    //            doorAnimator.Play(closeAnimationName);
    //        }

    //        PlaySound(closeSound);

    //        // ֹͣ�Զ�����Э��
    //        if (autoCloseCoroutine != null)
    //        {
    //            StopCoroutine(autoCloseCoroutine);
    //            autoCloseCoroutine = null;
    //        }
    //    }
    //}

    //[ContextMenu("�л���״̬")]
    //public void ToggleDoor()
    //{
    //    //if (isOpen)
    //        //CloseDoor();
    //    else
    //        OpenDoor();
    //}
    #endregion
    [ContextMenu("������")]
    public void UnlockDoor()
    {
        if (isLocked)
        {
            isLocked = false;
            Debug.Log("���ѽ���");
            PlaySound(unlockSound);
            OpenDoor();

        }
    }
    #region ������
    //[ContextMenu("������")]
    //public void LockDoor()
    //{
    //    if (!isLocked)
    //    {
    //        isLocked = true;
    //        // ������ǿ��ŵģ��ȹ�����
    //        if (isOpen)
    //        {
    //            CloseDoor();
    //        }
    //        Debug.Log("��������");
    //        PlaySound(lockSound);
    //    }
    //}
    #endregion
    IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        //CloseDoor();
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    //// ���������������������
    //public void OnPasswordUnlock()
    //{
    //    UnlockDoor();
    //}

    // ��������������Կ�׽���
    public void OnKeyUsed()
    {
        UnlockDoor();
    }

    // ��������������������Ϊ����״̬���޶�����
    public void SetDoorOpenImmediate()
    {
        isOpen = true;
        transform.position = openPosition;
        transform.rotation = openRotation;
    }

    // ��������������������Ϊ�ر�״̬���޶�����
    public void SetDoorClosedImmediate()
    {
        isOpen = false;
        transform.position = closedPosition;
        transform.rotation = closedRotation;
    }

    // �������������浱ǰλ��Ϊ����λ��
    [ContextMenu("��������Ŀ������")]
    public void CreateClosedTarget()
    {
        GameObject target = new GameObject("Door_ClosedTarget");
        target.transform.position = transform.position;
        target.transform.rotation = transform.rotation;
        closedStateTarget = target.transform;
        UpdatePositionsFromTargets();
        Debug.Log("�Ѵ�������Ŀ������");
    }

    // �������������浱ǰλ��Ϊ����λ��
    [ContextMenu("��������Ŀ������")]
    public void CreateOpenTarget()
    {
        GameObject target = new GameObject("Door_OpenTarget");
        target.transform.position = transform.position;
        target.transform.rotation = transform.rotation;
        openStateTarget = target.transform;
        UpdatePositionsFromTargets();
        Debug.Log("�Ѵ�������Ŀ������");
    }

    // �����������ӵ�ǰTransform���¹���λ��
    [ContextMenu("�ӵ�ǰ���¹���λ��")]
    public void UpdateClosedFromCurrent()
    {
        if (closedStateTarget != null)
        {
            closedStateTarget.position = transform.position;
            closedStateTarget.rotation = transform.rotation;
            UpdatePositionsFromTargets();
            Debug.Log("�Ѵӵ�ǰλ�ø��¹���λ��");
        }
        else
        {
            Debug.LogWarning("���ȴ�������Ŀ������");
        }
    }

    // �����������ӵ�ǰTransform���¿���λ��
    [ContextMenu("�ӵ�ǰ���¿���λ��")]
    public void UpdateOpenFromCurrent()
    {
        if (openStateTarget != null)
        {
            openStateTarget.position = transform.position;
            openStateTarget.rotation = transform.rotation;
            UpdatePositionsFromTargets();
            Debug.Log("�Ѵӵ�ǰλ�ø��¿���λ��");
        }
        else
        {
            Debug.LogWarning("���ȴ�������Ŀ������");
        }
    }

    // �ڱ༭���п��ӻ��ŵĿ���״̬
    void OnDrawGizmosSelected()
    {
        //// ���ƹر�״̬
        //Gizmos.color = Color.green;
        //if (closedStateTarget != null)
        //{
        //    Gizmos.matrix = Matrix4x4.TRS(closedStateTarget.position, closedStateTarget.rotation, transform.lossyScale);
        //}
        //else
        //{
        //    Gizmos.matrix = Matrix4x4.TRS(closedPosition, closedRotation, transform.lossyScale);
        //}
        //Gizmos.DrawWireCube(Vector3.zero, new Vector3(1f, 2f, 0.1f));

        //// ���ƿ���״̬
        //Gizmos.color = Color.blue;
        //if (openStateTarget != null)
        //{
        //    Gizmos.matrix = Matrix4x4.TRS(openStateTarget.position, openStateTarget.rotation, transform.lossyScale);
        //}
        //else
        //{
        //    Gizmos.matrix = Matrix4x4.TRS(openPosition, openRotation, transform.lossyScale);
        //}
        //Gizmos.DrawWireCube(Vector3.zero, new Vector3(1f, 2f, 0.1f));

        // ���ƴӹص���������
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.identity;
        Vector3 closedPos = closedStateTarget != null ? closedStateTarget.position : closedPosition;
        Vector3 openPos = openStateTarget != null ? openStateTarget.position : openPosition;
        Gizmos.DrawLine(closedPos, openPos);
    }

    // ������Ϣ
    [ContextMenu("��ʾ��״̬")]
    public void DisplayDoorStatus()
    {
        Debug.Log($"=== ��״̬ ===");
        Debug.Log($"����: {isLocked}");
        Debug.Log($"����: {isOpen}");
        Debug.Log($"��ǰλ��: {transform.position}");
        Debug.Log($"��ǰ��ת: {transform.rotation.eulerAngles}");
        Debug.Log($"����Ŀ��: {(closedStateTarget != null ? closedStateTarget.name : "��")}");
        Debug.Log($"����Ŀ��: {(openStateTarget != null ? openStateTarget.name : "��")}");
        Debug.Log($"�Զ�����: {autoClose}");
        Debug.Log($"=================");
    }
}