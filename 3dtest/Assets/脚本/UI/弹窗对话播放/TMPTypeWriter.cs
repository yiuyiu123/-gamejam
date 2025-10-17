using UnityEngine;
using TMPro;
using System.Collections;

public class TMPTypewriter : MonoBehaviour
{
    public float charInterval = 0.05f; // ÿ���ּ��

    private TMP_Text tmpText;
    private string fullText;

    private readonly char[] punctuation = new char[] { '��', '��', '��', '��', ',', '.', '!', '?' };

    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    public void StartTyping(string text)
    {
        StopAllCoroutines();
        fullText = text;
        tmpText.text = "";
        StartCoroutine(TypeRoutine());
    }

    private IEnumerator TypeRoutine()
    {
        // ��ʼѭ�����Ź�����Ч
        AudioManager.Instance.PlayLoop("���ֹ���");

        for (int i = 0; i < fullText.Length; i++)
        {
            tmpText.text += fullText[i];

            // �������ţ���ͣ��Ч
            if (System.Array.IndexOf(punctuation, fullText[i]) >= 0)
            {
                AudioManager.Instance.Pause("���ֹ���", fadeOut: false); // ��������������ͣ
                yield return new WaitForSeconds(charInterval * 2); // ���ͣ��
                AudioManager.Instance.PlayLoop("���ֹ���"); // ����ѭ������
            }

            yield return new WaitForSeconds(charInterval);
        }

        // ������ɣ�ֹͣ��Ч
        AudioManager.Instance.Pause("���ֹ���", fadeOut: false);
    }
}
