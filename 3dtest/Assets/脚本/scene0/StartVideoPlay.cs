using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartVideoPlay : MonoBehaviour
{
    public RawImage rawImage;           
    public VideoPlayer videoPlayer;    
    public GameObject skipButton;       
    public float skipHoldTime = 1.0f;
    public float fadeTime = 2.0f;

    private bool skipPressed = false;

    private void Start()
    {
        // �ж��Ƿ��һ�ν�����Ϸ
        bool hasSeenVideo = PlayerPrefs.GetInt("HasSeenIntroVideo", 0) == 1;
        // ��һ�ν�����Ϸ���ذ�ť��֮����ʾ
        //skipButton.SetActive(hasSeenVideo);
        videoPlayer.Play();
        // ������Ƶ�������
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            skipPressed = true;
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
                SkipVideo();
            }
        }
        else
        {
            skipHoldTime = 1.0f; // ���õ���ʱ
        }
    }

    public void OnClickSkipButton()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
        OnVideoFinished(videoPlayer);
    }

    private void SkipVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            print("�ɹ�����");
        }
        OnVideoFinished(videoPlayer);
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // ����Ѿ��������洢�ڻ�����
        PlayerPrefs.SetInt("HasSeenIntroVideo", 1);
        PlayerPrefs.Save();

        // ��֡Э����������������������س���1
        StartCoroutine(LoadScene1());
    }

    private IEnumerator LoadScene1()
    {
        // ���䵭��
        CanvasGroup cg = rawImage.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = rawImage.gameObject.AddComponent<CanvasGroup>();
        }
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            cg.alpha = 1 - (t / fadeTime);
            yield return null;
        }
        // ȷ��ȫ��
        cg.alpha = 0;

        // ���س���1
        SceneManager.LoadScene("scene1");
    }
}
