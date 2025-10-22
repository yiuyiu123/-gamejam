using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("灯光设置")]
    public Light spotlight; // 手电筒的聚光灯组件
    public float maxIntensity = 3f; // 最大亮度
    public float fadeInTime = 1.5f; // 渐亮时间
    public float fadeOutTime = 0.8f; // 渐灭时间

    [Header("状态")]
    public bool isOn = false;
    public bool isTurningOn = false;
    public bool isTurningOff = false;

    private InteractableItem interactableItem;
    private Coroutine lightCoroutine;

    void Start()
    {
        interactableItem = GetComponent<InteractableItem>();

        // 确保灯光组件存在
        if (spotlight == null)
        {
            // 尝试在子物体中查找聚光灯
            spotlight = GetComponentInChildren<Light>();
        }

        // 初始化灯光状态
        if (spotlight != null)
        {
            spotlight.intensity = 0f;
            spotlight.enabled = false;
        }
        else
        {
            Debug.LogWarning($"手电筒 {gameObject.name} 没有找到聚光灯组件!");
        }
    }

    void Update()
    {
        // 检查手电筒是否被持有
        CheckFlashlightState();
    }

    void CheckFlashlightState()
    {
        if (interactableItem == null) return;

        // 如果手电筒被player2持有且灯没开，自动开灯
        if (interactableItem.isBeingHeld &&
            interactableItem.currentHolder != null &&
            interactableItem.currentHolder.CompareTag("Player2") &&
            !isOn && !isTurningOn)
        {
            TurnOn();
        }
        // 如果手电筒没有被持有且灯还亮着，关灯
        else if (!interactableItem.isBeingHeld && isOn && !isTurningOff)
        {
            TurnOff();
        }
    }

    public void TurnOn()
    {
        if (isOn || isTurningOn || spotlight == null) return;

        if (lightCoroutine != null)
            StopCoroutine(lightCoroutine);

        lightCoroutine = StartCoroutine(FadeLight(true));

        // 通知PlayerController
        PlayerController playerController = interactableItem.currentHolder?.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.isHoldFlashLight = true;
        }
    }

    public void TurnOff()
    {
        if (!isOn || isTurningOff || spotlight == null) return;

        if (lightCoroutine != null)
            StopCoroutine(lightCoroutine);

        lightCoroutine = StartCoroutine(FadeLight(false));

        // 通知PlayerController
        PlayerController playerController = interactableItem.currentHolder?.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.isHoldFlashLight = false;
        }
    }

    IEnumerator FadeLight(bool turnOn)
    {
        if (turnOn)
        {
            isTurningOn = true;
            isTurningOff = false;
            spotlight.enabled = true;

            float elapsedTime = 0f;
            float startIntensity = spotlight.intensity;

            while (elapsedTime < fadeInTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeInTime;
                spotlight.intensity = Mathf.Lerp(startIntensity, maxIntensity, t);
                yield return null;
            }

            spotlight.intensity = maxIntensity;
            isOn = true;
            isTurningOn = false;

            Debug.Log($"手电筒渐亮完成，当前亮度: {spotlight.intensity}");
        }
        else
        {
            isTurningOff = true;
            isTurningOn = false;

            float elapsedTime = 0f;
            float startIntensity = spotlight.intensity;

            while (elapsedTime < fadeOutTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeOutTime;
                spotlight.intensity = Mathf.Lerp(startIntensity, 0f, t);
                yield return null;
            }

            spotlight.intensity = 0f;
            spotlight.enabled = false;
            isOn = false;
            isTurningOff = false;

            Debug.Log("手电筒渐灭完成");
        }

        lightCoroutine = null;
    }

    // 强制立即开关灯（用于特殊情况）
    public void SetLightImmediate(bool on)
    {
        if (lightCoroutine != null)
        {
            StopCoroutine(lightCoroutine);
            lightCoroutine = null;
        }

        if (spotlight != null)
        {
            if (on)
            {
                spotlight.intensity = maxIntensity;
                spotlight.enabled = true;
                isOn = true;
            }
            else
            {
                spotlight.intensity = 0f;
                spotlight.enabled = false;
                isOn = false;
            }
        }

        isTurningOn = false;
        isTurningOff = false;
    }

    void OnDestroy()
    {
        if (lightCoroutine != null)
        {
            StopCoroutine(lightCoroutine);
        }
    }
}