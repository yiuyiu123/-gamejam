using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("������")]
    public bool isLocked = true;
    public float openSpeed = 2f;
    public float openHeight = 3f; // ���ź������ĸ߶�

    [Header("��������")]
    public bool useAnimation = false;
    public Animator doorAnimator;
    public string openAnimationName = "Open";
    public string closeAnimationName = "Close";

    [Header("����Ч��")]
    public AudioClip openSound;
    public AudioClip closeSound;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;
    private AudioSource audioSource;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * openHeight;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (!isLocked && !isOpen)
        {
            OpenDoor();
        }
    }

    public void UnlockDoor()
    {
        if (isLocked)
        {
            isLocked = false;
            Debug.Log("���ѽ���");

            if (openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }
        }
    }

    public void LockDoor()
    {
        if (!isLocked)
        {
            isLocked = true;
            isOpen = false;
            Debug.Log("��������");

            if (closeSound != null)
            {
                audioSource.PlayOneShot(closeSound);
            }
        }
    }

    void OpenDoor()
    {
        if (useAnimation && doorAnimator != null)
        {
            doorAnimator.Play(openAnimationName);
            isOpen = true;
        }
        else
        {
            // �򵥵�λ���ƶ�
            transform.position = Vector3.Lerp(transform.position, openPosition, openSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, openPosition) < 0.1f)
            {
                isOpen = true;
            }
        }
    }

    void CloseDoor()
    {
        if (useAnimation && doorAnimator != null)
        {
            doorAnimator.Play(closeAnimationName);
            isOpen = false;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, closedPosition, openSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, closedPosition) < 0.1f)
            {
                isOpen = false;
            }
        }
    }

    // �����������������ţ�������Ʒ����������¼���
    public void OnKeyUsed()
    {
        UnlockDoor();
    }
}
