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

    private bool isPlayerInRange = false;
    private bool isOpened = false;

    void Start()
    {
        // ��ʼ��״̬
        InitializeChest();
    }

    void Update()
    {
        // �������Ƿ��ڷ�Χ���Ұ���F��
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F) && !isOpened)
        {
            Debug.Log("��Ҵ򿪽���ҳ��");
            ShowPasswordUI();
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
        if (other.CompareTag("Player1"))
        {
            isPlayerInRange = true;
            Debug.Log("��ҽ��뷶Χ");
        }
    }

    // �������������뿪��Χ
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player1"))
        {
            isPlayerInRange = false;
            Debug.Log("��ҽ��뷶Χ��");
            // ����뿪ʱ�����������
            if (passwordUI != null)
                passwordUI.SetActive(false);
        }
    }

}