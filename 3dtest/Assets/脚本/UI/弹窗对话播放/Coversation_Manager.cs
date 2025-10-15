using UnityEngine;
using UnityEngine.UI;

public class Coversation_Manager : MonoBehaviour
{
    [Header("图片列表")]
    public GameObject[] comicImages;

    [Header("切换按钮")]
    public Button nextButton;
    public scene1UI_Manager Begin_Manager;

    private int currentIndex = 0;

    private void Start()
    {
        if (comicImages.Length == 0)
        {
            Debug.LogError("没有绑定任何漫画图片！");
            return;
        }

        for (int i = 0; i < comicImages.Length; i++)
        {
            if (comicImages[i] == null)
            {
                Debug.LogError($"comicImages[{i}] 未绑定！");
            }
            comicImages[i].SetActive(false);
        }

        // 显示第 0 张
        comicImages[0].SetActive(true);

        nextButton.onClick.RemoveAllListeners(); // 确保只绑定一次
        nextButton.onClick.AddListener(OnNextClicked);
    }

    private void OnNextClicked()
    {
        // 播放 "点击" 组第0个元素点击音效
        AudioManager.Instance.PlayClick("翻书", volume: 1f, index: 0);

        currentIndex++;

        // 还在漫画数组范围内
        if (currentIndex < comicImages.Length)
        {
            comicImages[currentIndex].SetActive(true);
            Debug.Log($"显示第 {currentIndex + 1} 张漫画");
        }
        else
        {
            Debug.Log("所有漫画播放完毕，调用 OverComic()");
            nextButton.gameObject.SetActive(false);
            if (Begin_Manager != null)
            {
                Begin_Manager.OverConversation();
            }
            else
            {
                Debug.LogError("Begin_Manager 未正确绑定！");
            }
        }
    }
}
