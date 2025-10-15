using UnityEngine;
using System.Collections;

public class PasswordLock : MonoBehaviour
{
    [Header("��������")]
    public string correctPassword = "1234"; // ��ȷ���� 
    public int passwordLength = 4; // ���볤��

    [Header("UI����")]
    public GameObject passwordUI; // �����������
    public UnityEngine.UI.InputField passwordInputField; // ���������
    public UnityEngine.UI.Text hintText; // ��ʾ�ı�

    [Header("��������")]
    public Animator chestAnimator; // �����䶯�������� 
    public string defaultAnimation = "Idle"; // Ĭ�϶���״̬��
    public string openAnimation = "Open"; // �򿪶���״̬��

    [Header("��Ʒ����")]
    public GameObject itemPrefab; // Ҫ���ɵ���ƷԤ����
    public Transform spawnPoint; // ��Ʒ����λ��

    [Header("��ʾͼ��")]
    public GameObject interactionHint; // ��������ͷ����ʾͼ�꣨��Sprite��3D���壩
    public Camera playerCamera;       // ����������Ĭ��Ϊ���������

    private bool isPlayerInRange = false;
    private bool isOpened = false;

   

    void Start()
    {
        // ��ʼ��״̬
        InitializeChest();

        // ��ʼ����ʾͼ��״̬ 

        interactionHint.SetActive(false);

        // ���δָ���������Ĭ��ʹ��������� 
        if (playerCamera == null)
            playerCamera = Camera.main;

        StartCoroutine(DisableHintAfterFrame());
    }

    IEnumerator DisableHintAfterFrame()
    {
        yield return null; // �ȴ�һ֡
        if (interactionHint != null)
            interactionHint.SetActive(false);
    }

    void Update()
    {
        // �������Ƿ��ڷ�Χ���Ұ���F��
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.H) && !isOpened)
        {
            Debug.Log("��Ҵ򿪽���ҳ��");
            ShowPasswordUI();
        }

        // ÿ֡������ʾͼ�곯������� 
        if (interactionHint != null && interactionHint.activeSelf)
        {
            // ���㳯��������ķ��� 
            Vector3 directionToCamera = interactionHint.transform.position - playerCamera.transform.position;
            // ������ת��Ĭ���Ǳ��泯���������
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
            // ���180��Y����ת��ʹ���泯������� 
            interactionHint.transform.rotation = targetRotation * Quaternion.Euler(0, 180f, 0);
        }
    }

    void InitializeChest()
    {
        // ���������������
        if (passwordUI != null)
            passwordUI.SetActive(false);

        // ����Ĭ�϶���
        if (chestAnimator != null)
            chestAnimator.Play(defaultAnimation);

        // ������ʾ�ı�
        if (hintText != null)
            hintText.text = "������" + passwordLength + "λ����";

       
    }

    void ShowPasswordUI()
    {
        if (passwordUI != null)
        {
            passwordUI.SetActive(true);

            // ��������
            if (passwordInputField != null)
            {
                passwordInputField.text = "";
                passwordInputField.Select();
                passwordInputField.ActivateInputField();
            }

            // ��ʾ��ʾ
            if (hintText != null)
                hintText.text = "������" + passwordLength + "λ����";
        }
    }

    // ��֤���밴ť���õķ���
    public void CheckPassword()
    {
        string inputPassword = passwordInputField.text;

        // ������볤��
        if (inputPassword.Length != passwordLength)
        {
            if (hintText != null)
                hintText.text = "�������Ϊ" + passwordLength + "λ��";
            return;
        }

        // ��������Ƿ���ȷ
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

        // �����������
        if (passwordUI != null)
            passwordUI.SetActive(false);

        // ���Ŵ򿪶���
        if (chestAnimator != null)
            chestAnimator.Play(openAnimation);

        // ������Ʒ
        StartCoroutine(SpawnItemAndDestroy());

        if (hintText != null)
            hintText.text = "������ȷ�������Ѵ�";
    }

    void PasswordIncorrect()
    {
        if (hintText != null)
            hintText.text = "�����������������";

        // ��������
        if (passwordInputField != null)
        {
            passwordInputField.text = "";
            passwordInputField.Select();
            passwordInputField.ActivateInputField();
        }
    }

    // �ر�������水ť���õķ���
    public void ClosePasswordUI()
    {
        if (passwordUI != null)
            passwordUI.SetActive(false);
    }

    IEnumerator SpawnItemAndDestroy()
    {
        // ������Ʒ
        if (itemPrefab != null && spawnPoint != null)
        {
            Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        // �ȴ�1�� 
        yield return new WaitForSeconds(1f);

        // ���ٽű��������� 
        Destroy(gameObject);
    }

    // �����������ҽ��뷶Χ
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player2"))
        {
            isPlayerInRange = true;
            Debug.Log("��ҽ��뷶Χ");
        }

        if (interactionHint != null)
            interactionHint.SetActive(true);
    }

    // �������������뿪��Χ
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player2"))
        {
            isPlayerInRange = false;
            Debug.Log("��ҽ��뷶Χ��");
            // ����뿪ʱ�����������
            if (passwordUI != null)
                passwordUI.SetActive(false);
            // ������ʾͼ����������
            if (interactionHint != null)
                interactionHint.SetActive(false);
            
        }
    }

}