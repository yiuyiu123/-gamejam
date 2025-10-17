using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class InspectSceneController : MonoBehaviour
{
    [Header("UI元素")]
    public Button returnButton;
    public Text itemNameText;

    void Start()
    {
        // 设置返回按钮事件
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(ReturnToGame);
        }

        // 显示道具信息
        DisplayItemInfo();
    }

    void DisplayItemInfo()
    {
        // 从场景管理器获取道具信息
        SceneTransitionManager sceneManager = SceneTransitionManager.Instance;
        if (sceneManager != null)
        {
            // 这里可以显示道具的详细信息
            // 例如：itemNameText.text  = sceneManager.GetCurrentItemName();
        }
    }

    public void ReturnToGame()
    {
        // 通过场景管理器返回主场景
        SceneTransitionManager sceneManager = SceneTransitionManager.Instance;
        if (sceneManager != null)
        {
            sceneManager.ReturnToMainScene();
        }
        else
        {
            // 备用方案：直接加载主场景
            SceneManager.LoadScene("MainScene");
        }
    }

    // 添加键盘支持（按ESC也可以返回）
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToGame();
        }
    }
}