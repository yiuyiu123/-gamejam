using UnityEngine;
using System.Collections;

public class PasswordLock : MonoBehaviour
{
    [Header("密码设置")]
    public string correctPassword = "1234"; // 正确密码 
    public int passwordLength = 4; // 密码长度

    [Header("UI引用")]
    public GameObject passwordUI; // 密码输入界面
    public UnityEngine.UI.InputField passwordInputField; // 密码输入框
    public UnityEngine.UI.Text hintText; // 提示文本

    [Header("动画引用")]
    public Animator chestAnimator; // 密码箱动画控制器 
    public string defaultAnimation = "Idle"; // 默认动画状态名
    public string openAnimation = "Open"; // 打开动画状态名

    [Header("物品生成")]
    public GameObject itemPrefab; // 要生成的物品预制体
    public Transform spawnPoint; // 物品生成位置

    [Header("提示图标")]
    public GameObject interactionHint; // 面向摄像头的提示图标（如Sprite或3D物体）
    public Camera playerCamera;       // 玩家摄像机（默认为主摄像机）

    private bool isPlayerInRange = false;
    private bool isOpened = false;

   

    void Start()
    {
        // 初始化状态
        InitializeChest();

        // 初始化提示图标状态 

        interactionHint.SetActive(false);

        // 如果未指定摄像机，默认使用主摄像机 
        if (playerCamera == null)
            playerCamera = Camera.main;

        StartCoroutine(DisableHintAfterFrame());
    }

    IEnumerator DisableHintAfterFrame()
    {
        yield return null; // 等待一帧
        if (interactionHint != null)
            interactionHint.SetActive(false);
    }

    void Update()
    {
        // 检测玩家是否在范围内且按下F键
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.H) && !isOpened)
        {
            Debug.Log("玩家打开解密页面");
            ShowPasswordUI();
        }

        // 每帧更新提示图标朝向摄像机 
        if (interactionHint != null && interactionHint.activeSelf)
        {
            // 计算朝向摄像机的方向 
            Vector3 directionToCamera = interactionHint.transform.position - playerCamera.transform.position;
            // 生成旋转（默认是背面朝向摄像机）
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
            // 添加180度Y轴旋转，使正面朝向摄像机 
            interactionHint.transform.rotation = targetRotation * Quaternion.Euler(0, 180f, 0);
        }
    }

    void InitializeChest()
    {
        // 隐藏密码输入界面
        if (passwordUI != null)
            passwordUI.SetActive(false);

        // 播放默认动画
        if (chestAnimator != null)
            chestAnimator.Play(defaultAnimation);

        // 重置提示文本
        if (hintText != null)
            hintText.text = "请输入" + passwordLength + "位密码";

       
    }

    void ShowPasswordUI()
    {
        if (passwordUI != null)
        {
            passwordUI.SetActive(true);

            // 清空输入框
            if (passwordInputField != null)
            {
                passwordInputField.text = "";
                passwordInputField.Select();
                passwordInputField.ActivateInputField();
            }

            // 显示提示
            if (hintText != null)
                hintText.text = "请输入" + passwordLength + "位密码";
        }
    }

    // 验证密码按钮调用的方法
    public void CheckPassword()
    {
        string inputPassword = passwordInputField.text;

        // 检查密码长度
        if (inputPassword.Length != passwordLength)
        {
            if (hintText != null)
                hintText.text = "密码必须为" + passwordLength + "位！";
            return;
        }

        // 检查密码是否正确
        if (inputPassword == correctPassword)
        {
            PasswordCorrect();
        }
        else
        {
            PasswordIncorrect();
        }
    }

    void PasswordCorrect()
    {
        isOpened = true;

        // 隐藏密码界面
        if (passwordUI != null)
            passwordUI.SetActive(false);

        // 播放打开动画
        if (chestAnimator != null)
            chestAnimator.Play(openAnimation);

        // 生成物品
        StartCoroutine(SpawnItemAndDestroy());

        if (hintText != null)
            hintText.text = "密码正确！宝箱已打开";
    }

    void PasswordIncorrect()
    {
        if (hintText != null)
            hintText.text = "密码错误！请重新输入";

        // 清空输入框
        if (passwordInputField != null)
        {
            passwordInputField.text = "";
            passwordInputField.Select();
            passwordInputField.ActivateInputField();
        }
    }

    // 关闭密码界面按钮调用的方法
    public void ClosePasswordUI()
    {
        if (passwordUI != null)
            passwordUI.SetActive(false);
    }

    IEnumerator SpawnItemAndDestroy()
    {
        // 生成物品
        if (itemPrefab != null && spawnPoint != null)
        {
            Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        // 等待1秒 
        yield return new WaitForSeconds(1f);

        // 销毁脚本所在物体 
        Destroy(gameObject);
    }

    // 触发器检测玩家进入范围
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player2"))
        {
            isPlayerInRange = true;
            Debug.Log("玩家进入范围");
        }

        if (interactionHint != null)
            interactionHint.SetActive(true);
    }

    // 触发器检测玩家离开范围
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player2"))
        {
            isPlayerInRange = false;
            Debug.Log("玩家进入范围不");
            // 玩家离开时隐藏密码界面
            if (passwordUI != null)
                passwordUI.SetActive(false);
            // 隐藏提示图标和密码界面
            if (interactionHint != null)
                interactionHint.SetActive(false);
            
        }
    }

}