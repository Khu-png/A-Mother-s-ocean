using UnityEngine;

public enum MusicID
{
    BG_1 = 0,
}

public enum SoundID
{
    Button = 0,
    SummonItem = 1,
}

public class SoundManager : Singleton<SoundManager>
{
    private const string MusicKey = "MusicEnabled";
    private const string SoundKey = "SoundEnabled";

    private AudioSource musicSource;
    private AudioSource[] soundSources = new AudioSource[System.Enum.GetValues(typeof(SoundID)).Length];

    [SerializeField] private AudioClip[] musicClips;
    [SerializeField] private AudioClip[] soundClips;

    private bool isMusicEnabled = true;
    private bool isSoundEnabled = true;
    private bool isLoaded;

    public bool IsMusicEnabled => isMusicEnabled;
    public bool IsSoundEnabled => isSoundEnabled;

    private void Awake()
    {
        RegisterSingleton(this);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        LoadSettings();
    }

    private void Start()
    {
        OnLoad();
    }

    private void OnLoad()
    {
        isLoaded = true;
        if (musicClips.Length > 0)
        {
            PlayMusic(MusicID.BG_1);
        }
    }

    public void PlayMusic(MusicID id)
    {
        if (!CanPlay(id, musicClips)) return;

        musicSource.clip = musicClips[(int)id];
        if (isMusicEnabled) musicSource.Play();
    }

    public void PlaySound(SoundID id)
    {
        if (!isSoundEnabled || !CanPlay(id, soundClips)) return;

        AudioSource source = GetSoundSource(id);
        source.PlayOneShot(soundClips[(int)id]);
    }

    public void SetMusicEnabled(bool enabled)
    {
        isMusicEnabled = enabled;
        PlayerPrefs.SetInt(MusicKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (enabled && musicSource.clip != null) musicSource.Play();
        if (!enabled) musicSource.Stop();
    }

    public void SetSoundEnabled(bool enabled)
    {
        isSoundEnabled = enabled;
        PlayerPrefs.SetInt(SoundKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
        if (!enabled)
        {
            foreach (AudioSource source in soundSources)
                if (source != null) source.Stop();
        }
    }

    public void ToggleMusic()
    {
        SetMusicEnabled(!isMusicEnabled);
    }

    public void ToggleSound()
    {
        SetSoundEnabled(!isSoundEnabled);
        PlaySound(SoundID.Button);
    }

    private AudioSource GetSoundSource(SoundID id)
    {
        int index = (int)id;
        if (soundSources[index] != null) return soundSources[index];

        AudioSource source = new GameObject(id.ToString()).AddComponent<AudioSource>();
        source.loop = false;
        source.transform.SetParent(transform);
        soundSources[index] = source;
        return source;
    }

    private void LoadSettings()
    {
        isMusicEnabled = PlayerPrefs.GetInt(MusicKey, 1) == 1;
        isSoundEnabled = PlayerPrefs.GetInt(SoundKey, 1) == 1;
    }

    private bool CanPlay(System.Enum id, AudioClip[] clips)
    {
        int index = System.Convert.ToInt32(id);
        return isLoaded && index >= 0 && index < clips.Length && clips[index] != null;
    }
}
