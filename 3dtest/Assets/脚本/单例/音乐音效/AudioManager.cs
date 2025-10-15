using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AudioGroup
{
    public string groupID;
    public AudioClip[] clips;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioGroup[] audioGroups;
    public AudioSource defaultAudioSourcePrefab;

    private Dictionary<string, AudioClip[]> groupClipDict = new Dictionary<string, AudioClip[]>();
    private Dictionary<string, AudioSource> groupAudioSources = new Dictionary<string, AudioSource>();
    private Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();


    private AudioSource hoverAudioSource;
    private AudioSource clickAudioSource;

    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 可选，跨场景保留
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (var group in audioGroups)
        {
            if (!groupClipDict.ContainsKey(group.groupID))
            {
                groupClipDict.Add(group.groupID, group.clips);
                AudioSource source = Instantiate(defaultAudioSourcePrefab, transform);
                source.playOnAwake = false;
                groupAudioSources.Add(group.groupID, source);
            }
        }

        hoverAudioSource = Instantiate(defaultAudioSourcePrefab, transform);
        hoverAudioSource.playOnAwake = false;

        clickAudioSource = Instantiate(defaultAudioSourcePrefab, transform);
        clickAudioSource.playOnAwake = false;
        // 🪵 打印调试信息
        Debug.Log("[AudioManager] hoverAudioSource created");
        Debug.Log($"HoverAudioSource Settings - Volume: {hoverAudioSource.volume}, Mute: {hoverAudioSource.mute}, Blend: {hoverAudioSource.spatialBlend}, Output: {hoverAudioSource.outputAudioMixerGroup}");
    }

    // ▶ 播放一次（可指定 index）
    public void PlayOneShot(string groupID, int index = -1, float volume = 1f, bool fadeIn = false, float fadeInDuration = 0.5f)
    {
        if (groupClipDict.TryGetValue(groupID, out AudioClip[] clips) && clips.Length > 0)
        {
            AudioClip clip = GetClipByIndex(clips, index);
            // 使用临时播放方式，避免场景切换被打断
            StartCoroutine(PlayOneShotWithTempSource(clip, volume * sfxVolume, fadeIn, fadeInDuration));
        }
    }

    // 🔁 循环播放（可指定 index）
    public void PlayLoop(string groupID, int index = -1, float volume = 1f, bool fadeIn = true, float fadeInDuration = 1f)
    {
        if (groupClipDict.TryGetValue(groupID, out AudioClip[] clips) && clips.Length > 0)
        {
            AudioClip clip = GetClipByIndex(clips, index);
            var source = groupAudioSources[groupID];
            StartCoroutine(PlayClip(source, clip, volume * sfxVolume, true, fadeIn, false, 0f, fadeInDuration, 0f));
        }
    }

    // ⏸ 暂停播放，支持淡出
    public void Pause(string groupID,int index=-1, bool fadeOut = true, float fadeOutDuration = 1f)
    {
        if (groupAudioSources.TryGetValue(groupID, out AudioSource source))
        {
            if (fadeOut)
                StartCoroutine(FadeOutAndPause(source, fadeOutDuration));
            else
                source.Pause();
        }
    }

    // 🖱 悬浮播放一次（可指定 index）
    // 🖱 悬浮播放一次（使用固定音效）
    public void PlayHover()
    {
        const string hoverGroupID = "点击";
        const int hoverIndex = 1;
        const float hoverVolume = 1f;

        if (groupClipDict.TryGetValue(hoverGroupID, out AudioClip[] clips) && clips.Length > hoverIndex)
        {
            AudioClip clip = clips[hoverIndex];
            StartCoroutine(PlayClip(hoverAudioSource, clip, hoverVolume * sfxVolume,
                                  false, false, false, 0f, 0f, 0f));
        }
        else
        {
            Debug.LogError($"悬浮音效加载失败！GroupID: {hoverGroupID}, Index: {hoverIndex}");
        }
    }

    // ⛔ 停止悬浮音效播放，支持淡出
    public void StopHover(bool fadeOut = true, float fadeOutDuration = 0.3f)
    {
        if (hoverAudioSource.isPlaying)
        {
            if (fadeOut)
                StartCoroutine(FadeOutAndStop(hoverAudioSource, fadeOutDuration));
            else
                hoverAudioSource.Stop();
        }
        //测试
        Debug.Log("StopHover() called");

    }

    // 🖱 点击播放（可指定 index）
    public void PlayClick(string groupID, int index = -1, float volume = 1f)
    {
        if (groupClipDict.TryGetValue(groupID, out AudioClip[] clips) && clips.Length > 0)
        {
            AudioClip clip = GetClipByIndex(clips, index);
            clickAudioSource.volume = volume * sfxVolume;
            clickAudioSource.PlayOneShot(clip);
        }
    }

    // 设置音乐音量
    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (groupAudioSources.TryGetValue("战斗背景音", out var musicSource))
        {
            musicSource.volume = musicVolume;
        }
    }

    // 设置音效音量
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
    }

    public bool IsPlaying(string name)
    {
        if (audioSources.ContainsKey(name))
        {
            return audioSources[name].isPlaying;
        }
        return false;
    }


    // 辅助：根据index获取音效，-1为随机
    private AudioClip GetClipByIndex(AudioClip[] clips, int index)
    {
        if (index >= 0 && index < clips.Length)
            return clips[index];
        return clips[Random.Range(0, clips.Length)];
    }

    // 🌊 播放协程，控制淡入、淡出、循环与停止
    private IEnumerator PlayClip(AudioSource source, AudioClip clip, float targetVolume, bool loop, bool fadeIn, bool fadeOut, float clipDuration, float fadeInDuration, float fadeOutDuration)
    {
       // Debug.Log($"StartCoroutine PlayClip: playing {clip.name} with loop={loop}, fadeIn={fadeIn}");


        source.clip = clip;
        source.loop = loop;

        if (fadeIn)
        {
            source.volume = 0f;
            source.Play();
            float time = 0f;
            while (time < fadeInDuration)
            {
                source.volume = Mathf.Lerp(0f, targetVolume, time / fadeInDuration);
                time += Time.deltaTime;
                yield return null;
            }
            source.volume = targetVolume;
        }
        else
        {
            source.volume = targetVolume;
            source.Play();
        }

        if (!loop && fadeOut)
        {
            yield return new WaitForSeconds(clip.length - fadeOutDuration);
            float time = 0f;
            float startVolume = source.volume;
            while (time < fadeOutDuration)
            {
                source.volume = Mathf.Lerp(startVolume, 0f, time / fadeOutDuration);
                time += Time.deltaTime;
                yield return null;
            }
            source.Stop();
            source.volume = targetVolume;
        }
    }

    // 🌙 淡出后暂停
    private IEnumerator FadeOutAndPause(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float time = 0f;
        while (time < duration)
        {
            source.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        source.Pause();
        source.volume = startVolume;
    }

    // 🛑 淡出后停止
    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float time = 0f;
        while (time < duration)
        {
            source.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        source.Stop();
        source.volume = startVolume;
    }

    private IEnumerator PlayOneShotWithTempSource(AudioClip clip, float volume, bool fadeIn, float fadeInDuration)
    {
        GameObject tempGO = new GameObject("OneShotAudio");
        DontDestroyOnLoad(tempGO);
        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = 0f;
        audioSource.loop = false;
        audioSource.playOnAwake = false;

        audioSource.Play();

        if (fadeIn)
        {
            float time = 0f;
            while (time < fadeInDuration)
            {
                audioSource.volume = Mathf.Lerp(0f, volume, time / fadeInDuration);
                time += Time.deltaTime;
                yield return null;
            }
            audioSource.volume = volume;
        }
        else
        {
            audioSource.volume = volume;
        }

        // 等待音频播放完毕
        yield return new WaitForSeconds(clip.length);

        // 淡出音量（可选）
        float fadeOutDuration = 0.5f;
        float fadeOutTime = 0f;
        float startVolume = audioSource.volume;
        while (fadeOutTime < fadeOutDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, fadeOutTime / fadeOutDuration);
            fadeOutTime += Time.deltaTime;
            yield return null;
        }

        Destroy(tempGO);
    }

    public void ResetAllAudio()
    {
    // 重置所有音频源
    foreach (var source in groupAudioSources.Values)
    {
        if (source.isPlaying)
        {
            source.Stop();
        }
    }
    
    // 重置悬停和点击音效
    if (hoverAudioSource.isPlaying) hoverAudioSource.Stop();
    if (clickAudioSource.isPlaying) clickAudioSource.Stop();
    
    /*// 重置背景音乐
    SceneBGM bgm = FindObjectOfType<SceneBGM>();
    if (bgm != null)
    {
        bgm.ResetBGM();
    }*/
    }
}
