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
    //public AudioClip collaborationSound;
    public Light collaborationLight;

    [Header("Э���ɹ���Ч")]
    public string collaborationSoundGroupID = "Э���ɹ�"; // ��Ч��ID

    [Header("UI��ʾ")]
    public GameObject waitingPrompt; // ��ʾ"�ȴ���һλ���"��UI
    public GameObject successPrompt; // ��ʾ"Э���ɹ�"��UI

    [Header("������scene1��UI_Play��ʼ��Ϸ")]
    public GameObject UI_Play;
    public bool isNotScene1 = true;
    public float skipHoldTime = 5.0f;
    private bool skipPressed = false;

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
        //������
        if (UI_Play != null) UI_Play.SetActive(false);

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

    // ����Э���ɹ���Ч��˫������
    void PlayCollaborationSound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(collaborationSoundGroupID))
        {
            // ��������������е�λ������������
            Vector3 midpoint = CalculateMidpoint();
            bool isPlayer1 = DetermineSoundChannel(midpoint);

            AudioManager.Instance.PlayOneShot(
                collaborationSoundGroupID,
                -1,           // ���ѡ����Ч
                false,        // ������
                0f,
                false,        // ������
                0f,
                isPlayer1,    // ��������
                false         // 2D��Ч
            );

            Debug.Log($"����Э���ɹ���Ч - ����: {(isPlayer1 ? "��(���1)" : "��(���2)")}");
        }
        else
        {
            //// ���ã�ʹ��ԭ������Ч
            //if (collaborationSound != null)
            //{
            //    AudioSource.PlayClipAtPoint(collaborationSound, Vector3.zero);
            //}
        }
    }

    // ��������������е�
    Vector3 CalculateMidpoint()
    {
        if (player1Zone != null && player2Zone != null)
        {
            return (player1Zone.transform.position + player2Zone.transform.position) / 2f;
        }
        else if (player1Zone != null)
        {
            return player1Zone.transform.position;
        }
        else if (player2Zone != null)
        {
            return player2Zone.transform.position;
        }

        return Vector3.zero;
    }

    // �����е�λ�þ�������
    bool DetermineSoundChannel(Vector3 midpoint)
    {
        // ������Ҷ���
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");

        if (player1 != null && player2 != null)
        {
            float distanceToPlayer1 = Vector3.Distance(midpoint, player1.transform.position);
            float distanceToPlayer2 = Vector3.Distance(midpoint, player2.transform.position);

            // �������1������ʹ��������������ʹ��������
            return distanceToPlayer1 <= distanceToPlayer2;
        }

        // Ĭ��ʹ��������
        return true;
    }

    //�����øĶ�
    IEnumerator CompleteCollaboration()
    {
        isTransitioning = true;
        Debug.Log("=== ˫��Э����ɣ� ===");

        // ����Э�����Ч��
        PlayCollaborationEffects();

        // ����Э���ɹ���Ч
        PlayCollaborationSound();

        Debug.Log($"˫��Э���ɹ������� {sceneTransitionDelay} �����ת������: {targetSceneName}");

        //�����scene1���ȴ����뵯��Play�������ҳ����������Play�����л��ڶ���
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
                        Debug.Log("���¿ո��");
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
                            Debug.Log("�ո񳤴�����");
                            StartCoroutine(LoadNextSceneWithFade());
                            yield break; // ������ǰЭ��
                        }
                    }
                    else
                    {
                        skipHoldTime = 5.0f; // ���õ���ʱ
                    }

                    yield return null; // ÿ֡�ȴ�һ��
                }
            }
        }
        //�������scene1��������ĳ���ϳɺ�ȴ������Զ���ת
        else
        {
            // �ȴ�һ��ʱ��
            yield return new WaitForSeconds(sceneTransitionDelay);

            // ʹ�ú�ĻЧ����ת����
            yield return StartCoroutine(LoadNextSceneWithFade());
        }
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

        if (isNotScene1 && collaborationLight != null)
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