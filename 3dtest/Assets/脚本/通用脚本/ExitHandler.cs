using UnityEngine;

public class ExitHandler : MonoBehaviour
{
    void Update()
    {
        // 检测ESC键按下
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        // 在编辑器中停止播放
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 在构建的应用中退出
            Application.Quit();
#endif
    }
}