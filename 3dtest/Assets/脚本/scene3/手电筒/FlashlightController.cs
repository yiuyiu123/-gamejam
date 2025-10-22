using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("�ƹ�����")]
    public Light spotlight; // �ֵ�Ͳ�ľ۹�����
    public float maxIntensity = 3f; // �������
    public float fadeInTime = 1.5f; // ����ʱ��
    public float fadeOutTime = 0.8f; // ����ʱ��

    [Header("״̬")]
    public bool isOn = false;
    public bool isTurningOn = false;
    public bool isTurningOff = false;

    private InteractableItem interactableItem;
    private Coroutine lightCoroutine;

    void Start()
    {
        interactableItem = GetComponent<InteractableItem>();

        // ȷ���ƹ��������
        if (spotlight == null)
        {
            // �������������в��Ҿ۹��
            spotlight = GetComponentInChildren<Light>();
        }

        // ��ʼ���ƹ�״̬
        if (spotlight != null)
        {
            spotlight.intensity = 0f;
            spotlight.enabled = false;
        }
        else
        {
            Debug.LogWarning($"�ֵ�Ͳ {gameObject.name} û���ҵ��۹�����!");
        }
    }

    void Update()
    {
        // ����ֵ�Ͳ�Ƿ񱻳���
        CheckFlashlightState();
    }

    void CheckFlashlightState()
    {
        if (interactableItem == null) return;

        // ����ֵ�Ͳ��player2�����ҵ�û�����Զ�����
        if (interactableItem.isBeingHeld &&
            interactableItem.currentHolder != null &&
            interactableItem.currentHolder.CompareTag("Player2") &&
            !isOn && !isTurningOn)
        {
            TurnOn();
        }
        // ����ֵ�Ͳû�б������ҵƻ����ţ��ص�
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

        // ֪ͨPlayerController
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

        // ֪ͨPlayerController
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

            Debug.Log($"�ֵ�Ͳ������ɣ���ǰ����: {spotlight.intensity}");
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

            Debug.Log("�ֵ�Ͳ�������");
        }

        lightCoroutine = null;
    }

    // ǿ���������صƣ��������������
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