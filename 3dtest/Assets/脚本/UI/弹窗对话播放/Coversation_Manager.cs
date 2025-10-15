using UnityEngine;
using UnityEngine.UI;

public class Coversation_Manager : MonoBehaviour
{
    [Header("ͼƬ�б�")]
    public GameObject[] comicImages;

    [Header("�л���ť")]
    public Button nextButton;
    public scene1UI_Manager Begin_Manager;

    private int currentIndex = 0;

    private void Start()
    {
        if (comicImages.Length == 0)
        {
            Debug.LogError("û�а��κ�����ͼƬ��");
            return;
        }

        for (int i = 0; i < comicImages.Length; i++)
        {
            if (comicImages[i] == null)
            {
                Debug.LogError($"comicImages[{i}] δ�󶨣�");
            }
            comicImages[i].SetActive(false);
        }

        // ��ʾ�� 0 ��
        comicImages[0].SetActive(true);

        nextButton.onClick.RemoveAllListeners(); // ȷ��ֻ��һ��
        nextButton.onClick.AddListener(OnNextClicked);
    }

    private void OnNextClicked()
    {
        // ���� "���" ���0��Ԫ�ص����Ч
        AudioManager.Instance.PlayClick("����", volume: 1f, index: 0);

        currentIndex++;

        // �����������鷶Χ��
        if (currentIndex < comicImages.Length)
        {
            comicImages[currentIndex].SetActive(true);
            Debug.Log($"��ʾ�� {currentIndex + 1} ������");
        }
        else
        {
            Debug.Log("��������������ϣ����� OverComic()");
            nextButton.gameObject.SetActive(false);
            if (Begin_Manager != null)
            {
                Begin_Manager.OverConversation();
            }
            else
            {
                Debug.LogError("Begin_Manager δ��ȷ�󶨣�");
            }
        }
    }
}
