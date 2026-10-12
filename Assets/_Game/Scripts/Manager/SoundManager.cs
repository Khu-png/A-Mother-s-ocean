using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [System.Serializable]
    private struct MusicEntry
    {
        public string id;
        public AudioClip clip;
    }

    [System.Serializable]
    private struct SoundEntry
    {
        public string id;
        public AudioClip clip;
    }

    private const string MUSIC_KEY = "MusicEnabled";
    private const string SOUND_KEY = "SoundEnabled";

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource soundSource;
    private readonly Dictionary<string, AudioClip> musicLookup = new Dictionary<string, AudioClip>();
    private readonly Dictionary<string, AudioClip> soundLookup = new Dictionary<string, AudioClip>();

    [SerializeField] private MusicEntry[] musicEntries = new MusicEntry[0];
    [SerializeField] private SoundEntry[] soundEntries = new SoundEntry[0];

    private bool isMusicEnabled = true;
    private bool isSoundEnabled = true;
    private bool isLoaded;
    private bool isMusicSuspended;

    public bool IsMusicEnabled => isMusicEnabled;
    public bool IsSoundEnabled => isSoundEnabled;

    void Awake()
    {
        RegisterSingleton(this);

        LoadSettings();
        BuildClipLookup();
        isLoaded = true;
    }

    void OnValidate()
    {
        BuildClipLookup();
    }

    public void PlayMusic(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || !isLoaded || !musicLookup.TryGetValue(id, out AudioClip clip) || clip == null) return;
        if (musicSource == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        if (isMusicEnabled && !isMusicSuspended) musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void SetMusicSuspended(bool suspended)
    {
        isMusicSuspended = suspended;
        if (suspended) StopMusic();
    }

    public void PlaySound(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || !isLoaded || !isSoundEnabled) return;
        if (!soundLookup.TryGetValue(id, out AudioClip clip) || clip == null) return;

        if (soundSource == null) return;
        soundSource.PlayOneShot(clip);
    }

    public void PlayButtonSound(string id = GameConstants.BUTTON_SOUND)
    {
        if (!isLoaded || !isSoundEnabled) return;

        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        PlaySound(id);
    }

    public void SetMusicEnabled(bool enabled)
    {
        isMusicEnabled = enabled;
        PlayerPrefs.SetInt(MUSIC_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (musicSource == null) return;
        if (enabled && !isMusicSuspended && musicSource.clip != null) musicSource.Play();
        if (!enabled) musicSource.Stop();
    }

    public void SetSoundEnabled(bool enabled)
    {
        isSoundEnabled = enabled;
        PlayerPrefs.SetInt(SOUND_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
        if (!enabled && soundSource != null) soundSource.Stop();
    }

    public void ToggleMusic()
    {
        SetMusicEnabled(!isMusicEnabled);
    }

    public void ToggleSound()
    {
        SetSoundEnabled(!isSoundEnabled);
    }

    private void LoadSettings()
    {
        isMusicEnabled = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        isSoundEnabled = PlayerPrefs.GetInt(SOUND_KEY, 1) == 1;
    }

    private void BuildClipLookup()
    {
        musicLookup.Clear();
        soundLookup.Clear();
        foreach (MusicEntry entry in musicEntries)
        {
            if (string.IsNullOrWhiteSpace(entry.id)) continue;
            if (!musicLookup.TryAdd(entry.id, entry.clip))
                GameDiagnostics.LogWarning($"Music ID bị trùng: {entry.id}. Giữ clip đầu tiên.", this);
        }
        foreach (SoundEntry entry in soundEntries)
        {
            if (string.IsNullOrWhiteSpace(entry.id)) continue;
            if (!soundLookup.TryAdd(entry.id, entry.clip))
                GameDiagnostics.LogWarning($"Sound ID bị trùng: {entry.id}. Giữ clip đầu tiên.", this);
        }
    }

}
