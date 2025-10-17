using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class InspectSceneController : MonoBehaviour
{
    [Header("UIԪ��")]
    public Button returnButton;
    public Text itemNameText;

    void Start()
    {
        // ���÷��ذ�ť�¼�
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(ReturnToGame);
        }

        // ��ʾ������Ϣ
        DisplayItemInfo();
    }

    void DisplayItemInfo()
    {
        // �ӳ�����������ȡ������Ϣ
        SceneTransitionManager sceneManager = SceneTransitionManager.Instance;
        if (sceneManager != null)
        {
            // ���������ʾ���ߵ���ϸ��Ϣ
            // ���磺itemNameText.text  = sceneManager.GetCurrentItemName();
        }
    }

    public void ReturnToGame()
    {
        // ͨ����������������������
        SceneTransitionManager sceneManager = SceneTransitionManager.Instance;
        if (sceneManager != null)
        {
            sceneManager.ReturnToMainScene();
        }
        else
        {
            // ���÷�����ֱ�Ӽ���������
            SceneManager.LoadScene("MainScene");
        }
    }

    // ��Ӽ���֧�֣���ESCҲ���Է��أ�
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToGame();
        }
    }
}