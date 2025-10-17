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

    private AudioSource hoverAudioSource;
    private AudioSource clickAudioSource;

    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
    }

    // ▶ 播放一次（OneShot）支持左右声道和2D/3D
    public void PlayOneShot(string groupID, int index = -1, bool isFadeIn = false, float fadeInTime = 0.5f,
                            bool isFadeOut = false, float fadeOutTime = 0.5f, bool isPlayer1 = true, bool is3D = false)
    {
        if (!groupClipDict.TryGetValue(groupID, out var clips) || clips.Length == 0)
            return;

        AudioClip clip = GetClipByIndex(clips, index);
        StartCoroutine(PlayTempSound(clip, sfxVolume, isFadeIn, fadeInTime, isFadeOut, fadeOutTime, isPlayer1, is3D));
    }

    // 🔁 循环播放（Loop）支持左右声道和2D/3D
    public void PlayLoop(string groupID, int index = -1, bool isFadeIn = true, float fadeInTime = 1f, bool isPlayer1 = true, bool is3D = false)
    {
        if (!groupClipDict.TryGetValue(groupID, out var clips) || clips.Length == 0)
            return;

        AudioClip clip = GetClipByIndex(clips, index);
        var source = groupAudioSources[groupID];
        source.spatialBlend = is3D ? 1f : 0f;
        source.panStereo = !is3D ? (isPlayer1 ? -1f : 1f) : 0f;

        StartCoroutine(PlayClip(source, clip, sfxVolume, true, isFadeIn, false, 0f, fadeInTime, 0f));
    }

    // ⏸ 暂停播放，支持淡出
    public void Pause(string groupID, int index = -1, bool isFadeOut = true, float fadeOutTime = 1f)
    {
        if (groupAudioSources.TryGetValue(groupID, out AudioSource source))
        {
            if (isFadeOut)
                StartCoroutine(FadeOutAndPause(source, fadeOutTime));
            else
                source.Pause();
        }
    }

    // 🖱 悬浮播放一次
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
    }

    public void StopHover(bool fadeOut = true, float fadeOutDuration = 0.3f)
    {
        if (hoverAudioSource.isPlaying)
        {
            if (fadeOut)
                StartCoroutine(FadeOutAndStop(hoverAudioSource, fadeOutDuration));
            else
                hoverAudioSource.Stop();
        }
    }

    // 🖱 点击播放
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
            musicSource.volume = musicVolume;
    }

    // 设置音效音量
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
    }

    public bool IsPlaying(string name)
    {
        if (groupAudioSources.ContainsKey(name))
            return groupAudioSources[name].isPlaying;
        return false;
    }

    private AudioClip GetClipByIndex(AudioClip[] clips, int index)
    {
        if (index >= 0 && index < clips.Length)
            return clips[index];
        return clips[Random.Range(0, clips.Length)];
    }

    private IEnumerator PlayTempSound(AudioClip clip, float volume, bool isFadeIn, float fadeInTime,
                                      bool isFadeOut, float fadeOutTime, bool isPlayer1, bool is3D)
    {
        GameObject temp = new GameObject($"TempAudio_{clip.name}");
        DontDestroyOnLoad(temp);

        AudioSource src = temp.AddComponent<AudioSource>();
        src.clip = clip;
        src.loop = false;
        src.spatialBlend = is3D ? 1f : 0f;
        src.panStereo = !is3D ? (isPlayer1 ? -1f : 1f) : 0f;
        src.volume = 0f;
        src.Play();

        if (isFadeIn)
        {
            float t = 0f;
            while (t < fadeInTime)
            {
                src.volume = Mathf.Lerp(0f, volume, t / fadeInTime);
                t += Time.deltaTime;
                yield return null;
            }
        }
        else src.volume = volume;

        yield return new WaitForSeconds(clip.length - (isFadeOut ? fadeOutTime : 0f));

        if (isFadeOut)
        {
            float t = 0f;
            float start = src.volume;
            while (t < fadeOutTime)
            {
                src.volume = Mathf.Lerp(start, 0f, t / fadeOutTime);
                t += Time.deltaTime;
                yield return null;
            }
        }

        Destroy(temp);
    }

    private IEnumerator PlayClip(AudioSource source, AudioClip clip, float targetVolume, bool loop,
                                 bool fadeIn, bool fadeOut, float clipDuration, float fadeInDuration, float fadeOutDuration)
    {
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

    private IEnumerator FadeOutAndPause(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float t = 0f;
        while (t < duration)
        {
            source.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        source.Pause();
        source.volume = startVolume;
    }

    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float t = 0f;
        while (t < duration)
        {
            source.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        source.Stop();
        source.volume = startVolume;
    }

    public void ResetAllAudio()
    {
        foreach (var source in groupAudioSources.Values)
            if (source.isPlaying) source.Stop();

        if (hoverAudioSource.isPlaying) hoverAudioSource.Stop();
        if (clickAudioSource.isPlaying) clickAudioSource.Stop();
    }
}
