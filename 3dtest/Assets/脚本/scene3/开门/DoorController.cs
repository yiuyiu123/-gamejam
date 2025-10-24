using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("门状态")]
    public bool isLocked = true;
    public bool isOpen = false;

    [Header("门位置设置 - 拖拽物体到这里")]
    public Transform closedStateTarget; // 关门时的目标物体
    public Transform openStateTarget;   // 开门时的目标物体

    [Header("动画设置")]
    public float openSpeed = 2f;
    public bool useAnimation = false;
    public Animator doorAnimator;
    public string openAnimationName = "Open";
    public string closeAnimationName = "Close";

    [Header("声音效果")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockSound;
    public AudioClip unlockSound;

    [Header("交互设置")]
    public bool autoClose = false; // 是否自动关门
    public float autoCloseDelay = 3f; // 自动关门延迟

    // 内部存储的实际位置和旋转
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
        // 从目标物体获取位置和旋转，或者使用默认值
        UpdatePositionsFromTargets();

        // 根据初始状态设置门的位置
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

            // 平滑移动到目标状态
            transform.position = Vector3.Lerp(transform.position, targetPosition, openSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, openSpeed * Time.deltaTime);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // 在编辑器模式下，当目标物体更改时更新位置
        if (!Application.isPlaying)
        {
            UpdatePositionsFromTargets();

            // 如果当前没有运行，立即同步到正确位置
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
        // 从目标物体获取位置和旋转
        if (closedStateTarget != null)
        {
            closedPosition = closedStateTarget.position;
            closedRotation = closedStateTarget.rotation;
        }
        else
        {
            // 如果没有设置目标，使用当前位置
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
            // 如果没有设置目标，使用当前位置
            openPosition = transform.position;
            openRotation = transform.rotation;
        }
    }

    [ContextMenu("开门")]
    public void OpenDoor()
    {
        if (isLocked)
        {
            //Debug.Log("门被锁住了！");
            //PlaySound(lockSound);
            //return;
            isLocked = false;
        }

        if (!isOpen)
        {
            isOpen = true;
            Debug.Log("开门");

            if (useAnimation && doorAnimator != null)
            {
                doorAnimator.Play(openAnimationName);
            }

            PlaySound(openSound);
            //张奕忻
            StartCoroutine(Scene3UI_Manager.Instance.DelayedSceneTransition(1f)); // 延迟1秒跳场景

            // 处理自动关门
            if (autoClose)
            {
                if (autoCloseCoroutine != null)
                    StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = StartCoroutine(AutoCloseAfterDelay());
            }
        }
    }
    #region 关门
    //[ContextMenu("关门")]
    //public void CloseDoor()
    //{
    //    if (isOpen)
    //    {
    //        isOpen = false;
    //        Debug.Log("关门");

    //        if (useAnimation && doorAnimator != null)
    //        {
    //            doorAnimator.Play(closeAnimationName);
    //        }

    //        PlaySound(closeSound);

    //        // 停止自动关门协程
    //        if (autoCloseCoroutine != null)
    //        {
    //            StopCoroutine(autoCloseCoroutine);
    //            autoCloseCoroutine = null;
    //        }
    //    }
    //}

    //[ContextMenu("切换门状态")]
    //public void ToggleDoor()
    //{
    //    //if (isOpen)
    //        //CloseDoor();
    //    else
    //        OpenDoor();
    //}
    #endregion
    [ContextMenu("解锁门")]
    public void UnlockDoor()
    {
        if (isLocked)
        {
            isLocked = false;
            Debug.Log("门已解锁");
            PlaySound(unlockSound);
            OpenDoor();

        }
    }
    #region 锁定门
    //[ContextMenu("锁定门")]
    //public void LockDoor()
    //{
    //    if (!isLocked)
    //    {
    //        isLocked = true;
    //        // 如果门是开着的，先关上门
    //        if (isOpen)
    //        {
    //            CloseDoor();
    //        }
    //        Debug.Log("门已锁定");
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

    //// 公共方法：用于密码解锁
    //public void OnPasswordUnlock()
    //{
    //    UnlockDoor();
    //}

    // 公共方法：用于钥匙解锁
    public void OnKeyUsed()
    {
        UnlockDoor();
    }

    // 公共方法：立即设置门为开启状态（无动画）
    public void SetDoorOpenImmediate()
    {
        isOpen = true;
        transform.position = openPosition;
        transform.rotation = openRotation;
    }

    // 公共方法：立即设置门为关闭状态（无动画）
    public void SetDoorClosedImmediate()
    {
        isOpen = false;
        transform.position = closedPosition;
        transform.rotation = closedRotation;
    }

    // 公共方法：保存当前位置为关门位置
    [ContextMenu("创建关门目标物体")]
    public void CreateClosedTarget()
    {
        GameObject target = new GameObject("Door_ClosedTarget");
        target.transform.position = transform.position;
        target.transform.rotation = transform.rotation;
        closedStateTarget = target.transform;
        UpdatePositionsFromTargets();
        Debug.Log("已创建关门目标物体");
    }

    // 公共方法：保存当前位置为开门位置
    [ContextMenu("创建开门目标物体")]
    public void CreateOpenTarget()
    {
        GameObject target = new GameObject("Door_OpenTarget");
        target.transform.position = transform.position;
        target.transform.rotation = transform.rotation;
        openStateTarget = target.transform;
        UpdatePositionsFromTargets();
        Debug.Log("已创建开门目标物体");
    }

    // 公共方法：从当前Transform更新关门位置
    [ContextMenu("从当前更新关门位置")]
    public void UpdateClosedFromCurrent()
    {
        if (closedStateTarget != null)
        {
            closedStateTarget.position = transform.position;
            closedStateTarget.rotation = transform.rotation;
            UpdatePositionsFromTargets();
            Debug.Log("已从当前位置更新关门位置");
        }
        else
        {
            Debug.LogWarning("请先创建关门目标物体");
        }
    }

    // 公共方法：从当前Transform更新开门位置
    [ContextMenu("从当前更新开门位置")]
    public void UpdateOpenFromCurrent()
    {
        if (openStateTarget != null)
        {
            openStateTarget.position = transform.position;
            openStateTarget.rotation = transform.rotation;
            UpdatePositionsFromTargets();
            Debug.Log("已从当前位置更新开门位置");
        }
        else
        {
            Debug.LogWarning("请先创建开门目标物体");
        }
    }

    // 在编辑器中可视化门的开关状态
    void OnDrawGizmosSelected()
    {
        //// 绘制关闭状态
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

        //// 绘制开门状态
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

        // 绘制从关到开的连线
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.identity;
        Vector3 closedPos = closedStateTarget != null ? closedStateTarget.position : closedPosition;
        Vector3 openPos = openStateTarget != null ? openStateTarget.position : openPosition;
        Gizmos.DrawLine(closedPos, openPos);
    }

    // 调试信息
    [ContextMenu("显示门状态")]
    public void DisplayDoorStatus()
    {
        Debug.Log($"=== 门状态 ===");
        Debug.Log($"锁定: {isLocked}");
        Debug.Log($"开启: {isOpen}");
        Debug.Log($"当前位置: {transform.position}");
        Debug.Log($"当前旋转: {transform.rotation.eulerAngles}");
        Debug.Log($"关门目标: {(closedStateTarget != null ? closedStateTarget.name : "无")}");
        Debug.Log($"开门目标: {(openStateTarget != null ? openStateTarget.name : "无")}");
        Debug.Log($"自动关门: {autoClose}");
        Debug.Log($"=================");
    }
}