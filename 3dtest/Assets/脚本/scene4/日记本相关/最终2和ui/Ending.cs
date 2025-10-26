using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ending : MonoBehaviour
{
    [Header("引用脚本")]
    public newmanage newManager;

    [Header("UI组件")]
    public GameObject I_Yes;
    public GameObject I_No;
    public GameObject I_mask1;
    public GameObject I_mask2;

    [Header("选定结局音效")]
    private AudioSource audioSource;             // 音频源
    public AudioClip sureSound;

    [Header("选定结局操作")]
    public float skipHoldTime = 5.0f;
    private bool skipPressed = false;


    private void Start()
    {
        if (I_Yes != null)I_Yes.SetActive(false);
        if (I_No != null)I_No.SetActive(false);
        if (I_mask1 != null)I_mask1.SetActive(false);
        if (I_mask2 != null)I_mask2.SetActive(false);
        else
        {
            Debug.Log("结局中UI组件有空");
        }
    }
    void Update()
    {
        if(newManager!=null)newManager.OnStartEnding += OnChoose;
        else
        {
            FindObjectOfType<newmanage>();
        }
    }

    void OnChoose()
    {
        newManager.OnStartEnding -= OnChoose;
        StartCoroutine(StartChooseMask());
    }

    IEnumerator StartChooseMask()
    {
        yield return new WaitForSeconds(1f);//等待玩家输入禁用
        if (Input.GetKeyDown(KeyCode.F))
        {
            I_mask1.SetActive(true);
            I_mask2.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            I_mask1.SetActive(false);
            I_mask2.SetActive(true);
        }
        else yield return null;
    }

    IEnumerator SureTheChoice()
    {
        // 播放协作成功音效 
        if (sureSound != null)
            audioSource.PlayOneShot(sureSound);

        Debug.Log($"");

        //如果是scene1，等待数秒弹出Play，如果玩家长按五秒或点击Play，则切换第二关 
        if (!isNotScene1)
        {
            yield return new WaitForSeconds(2f);
            if (UI_Play != null)
            {
                UI_Play.SetActive(true);
                while (true)
                {
                    if (Input.GetKey(KeyCode.Space))
                    {
                        skipPressed = true;
                        Debug.Log("按下空格键");
                    }
                    else
                    {
                        skipPressed = false;
                    }

                    if (skipPressed)
                    {
                        skipHoldTime -= Time.deltaTime;

                        if (skipHoldTime <= 0)
                        {
                            Debug.Log("空格长达五秒");
                            ShowCollaborationUI(); // 显示协作完成UI 
                            yield break; // 跳出当前协程 
                        }
                    }
                    else
                    {
                        skipHoldTime = 5.0f; // 重置倒计时 
                    }

                    yield return null; // 每帧等待一次 
                }
            }
        }
        //如果不是scene1，按唤洋的程序合成后等待两秒显示UI 
        else
        {
            // 等待一段时间 
            yield return new WaitForSeconds(uiDisplayDelay);

            // 显示协作完成UI 
            ShowCollaborationUI();
        }
    }
}
