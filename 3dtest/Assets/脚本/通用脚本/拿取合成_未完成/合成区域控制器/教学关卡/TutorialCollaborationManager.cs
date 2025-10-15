using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialCollaborationManager : MonoBehaviour
{
    public static TutorialCollaborationManager Instance;

    [Header("�����������")]
    public TutorialSynthesisZone player1Zone;
    public TutorialSynthesisZone player2Zone;

    [Header("������ת����")]
    public string targetSceneName = "NextScene";
    public float sceneTransitionDelay = 2f;

    [Header("Э�����Ч��")]
    public ParticleSystem collaborationEffect;
    public AudioClip collaborationSound;
    public Light collaborationLight;

    [Header("UI��ʾ")]
    public GameObject waitingPrompt; // ��ʾ"�ȴ���һλ���"��UI
    public GameObject successPrompt; // ��ʾ"Э���ɹ�"��UI

    private bool player1Ready = false;
    private bool player2Ready = false;
    private bool isTransitioning = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ע������ص�
        if (player1Zone != null)
            player1Zone.OnItemStateChanged += OnPlayer1ZoneUpdated;

        if (player2Zone != null)
            player2Zone.OnItemStateChanged += OnPlayer2ZoneUpdated;

        // ��ʼ��UI
        if (waitingPrompt != null) waitingPrompt.SetActive(false);
        if (successPrompt != null) successPrompt.SetActive(false);

        Debug.Log("Э����ѧ�ؿ�������������");
    }

    void OnPlayer1ZoneUpdated(bool hasRequiredItem)
    {
        player1Ready = hasRequiredItem;
        Debug.Log($"���1����״̬: {(hasRequiredItem ? "����" : "δ����")}");
        CheckCollaborationStatus();
    }

    void OnPlayer2ZoneUpdated(bool hasRequiredItem)
    {
        player2Ready = hasRequiredItem;
        Debug.Log($"���2����״̬: {(hasRequiredItem ? "����" : "δ����")}");
        CheckCollaborationStatus();
    }

    void CheckCollaborationStatus()
    {
        if (isTransitioning) return;

        bool bothReady = player1Ready && player2Ready;
        bool oneReady = player1Ready || player2Ready;

        // ����UI��ʾ
        if (waitingPrompt != null)
            waitingPrompt.SetActive(oneReady && !bothReady);

        if (successPrompt != null)
            successPrompt.SetActive(bothReady);

        // ���˫����׼�����ˣ���ʼ�������
        if (bothReady)
        {
            StartCoroutine(CompleteCollaboration());
        }
    }

    IEnumerator CompleteCollaboration()
    {
        isTransitioning = true;
        Debug.Log("=== ˫��Э����ɣ� ===");

        // ����Э�����Ч��
        PlayCollaborationEffects();

        Debug.Log($"˫��Э���ɹ������� {sceneTransitionDelay} �����ת������: {targetSceneName}");

        // �ȴ�һ��ʱ��
        yield return new WaitForSeconds(sceneTransitionDelay);

        // ʹ�ú�ĻЧ����ת����
        yield return StartCoroutine(LoadNextSceneWithFade());
    }

    IEnumerator LoadNextSceneWithFade()
    {
        Debug.Log("��ʼ���س��������뵭��Ч��");

        // ʹ�ü򻯰汾
        if (SimpleSceneTransitionManager.Instance != null)
        {
            yield return StartCoroutine(SimpleSceneTransitionManager.Instance.TransitionToScene(targetSceneName));
        }
        else if (SimpleSceneTransitionManager.Instance != null)
        {
            yield return StartCoroutine(SimpleSceneTransitionManager.Instance.TransitionToScene(targetSceneName));
        }
        else
        {
            Debug.LogError("û���ҵ��������ɹ�������");
            LoadNextScene();
        }
    }

    void PlayCollaborationEffects()
    {
        if (collaborationEffect != null)
        {
            collaborationEffect.Play();
        }

        if (collaborationSound != null)
        {
            AudioSource.PlayClipAtPoint(collaborationSound, Vector3.zero);
        }

        if (collaborationLight != null)
        {
            StartCoroutine(FlashCollaborationLight());
        }
    }

    IEnumerator FlashCollaborationLight()
    {
        collaborationLight.enabled = true;
        float originalIntensity = collaborationLight.intensity;

        // ����������˸Ч����ʾЭ���ɹ�
        for (int i = 0; i < 5; i++)
        {
            collaborationLight.intensity = originalIntensity * 3f;
            collaborationLight.color = i % 2 == 0 ? Color.blue : Color.green;
            yield return new WaitForSeconds(0.15f);
            collaborationLight.intensity = originalIntensity;
            yield return new WaitForSeconds(0.15f);
        }

        collaborationLight.intensity = originalIntensity;
        collaborationLight.color = Color.white;
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"��ת������: {targetSceneName}");
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("Ŀ�곡������δ���ã�");
        }
    }

    // ���Է���
    [ContextMenu("ǿ�����Э��")]
    public void ForceCompleteCollaboration()
    {
        if (!isTransitioning)
        {
            player1Ready = true;
            player2Ready = true;
            StartCoroutine(CompleteCollaboration());
        }
    }

    [ContextMenu("����Э��״̬")]
    public void ResetCollaboration()
    {
        player1Ready = false;
        player2Ready = false;
        isTransitioning = false;

        if (waitingPrompt != null) waitingPrompt.SetActive(false);
        if (successPrompt != null) successPrompt.SetActive(false);

        Debug.Log("Э��״̬������");
    }

    void OnDestroy()
    {
        // ȡ��ע��ص�
        if (player1Zone != null)
            player1Zone.OnItemStateChanged -= OnPlayer1ZoneUpdated;

        if (player2Zone != null)
            player2Zone.OnItemStateChanged -= OnPlayer2ZoneUpdated;
    }
}