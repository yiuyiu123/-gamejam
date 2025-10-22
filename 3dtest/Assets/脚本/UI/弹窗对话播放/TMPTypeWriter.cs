using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPTypewriter : MonoBehaviour
{
    public float typingSpeed = 0.05f;

    public bool IsTyping { get; private set; }
    public string OriginalText { get; private set; }

    private TextMeshProUGUI textMeshPro;
    private Coroutine typingCoroutine;

    // �����ţ�������Чͣ��
    private char[] punctuation = new char[] { '��', '��', '��', '��', ',', '.', '!', '?' };

    /// <summary>
    /// ��ʼ���ֲ���
    /// </summary>
    /// <param name="isPlayer1">�Ƿ����1����������</param>
    public void StartTypewriter(bool isPlayer1 = true)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeRoutine(isPlayer1));
    }

    public void SetTextInstant()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (textMeshPro == null)
            textMeshPro = GetComponent<TextMeshProUGUI>();

        textMeshPro.text = OriginalText;
        IsTyping = false;

        // ֹͣ��Ч
        AudioManager.Instance.Pause("���ֹ���", isFadeOut: false);
        AudioManager.Instance.Pause("���ֹ���", isFadeOut: false);
    }

    private IEnumerator TypeRoutine(bool isPlayer1)
    {
        if (textMeshPro == null)
            textMeshPro = GetComponent<TextMeshProUGUI>();

        // �ȴ� GameObject ��ȫ����
        while (!gameObject.activeInHierarchy)
            yield return null;

        // �ȴ� UI ��Ⱦ
        yield return null;

        // ��ȡԭʼ����
        OriginalText = textMeshPro.text;
        textMeshPro.text = "";

        IsTyping = true;

        // �������ֹ�����Ч��ѭ�����ţ�
        AudioManager.Instance.PlayLoop("���ֹ���", 0, isFadeIn: false, isPlayer1: isPlayer1);

        foreach (char c in OriginalText)
        {
            textMeshPro.text += c;

            // ���������ţ���ͣ��Ч��ͣ��
            if (System.Array.IndexOf(punctuation, c) >= 0)
            {
                AudioManager.Instance.Pause("���ֹ���",isFadeOut: false);
                yield return new WaitForSeconds(typingSpeed * 2); // ���ͣ��
                AudioManager.Instance.PlayLoop("���ֹ���", 0, isFadeIn: false,isPlayer1: isPlayer1);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        // ������ɣ�ֹͣ��Ч
        AudioManager.Instance.Pause("���ֹ���", isFadeOut: false);

        IsTyping = false;
    }

    //scene3�¼�
    public void SetText(string text)
    {
        OriginalText = text;
        if (textMeshPro == null)
            textMeshPro = GetComponent<TextMeshProUGUI>();
        textMeshPro.text = text; // ��ѡ��ֱ����ʾ��ʼ�ı�
    }

}
