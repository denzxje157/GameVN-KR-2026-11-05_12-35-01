using UnityEngine;

/// <summary>
/// Quản lý âm thanh toàn diện cho Lễ Hội Linh Vật (Mythic Bumper).
/// Điều phối 3 kênh độc lập: Nhạc Nền (BGM), Hiệu Ứng (SFX), Âm Thanh Cổ Vũ (Crowd).
/// Tự động khởi tạo (Self-instantiating Singleton) để bảo đảm luôn hoạt động 100%.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<AudioManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        if (_instance == null)
        {
            var inst = Instance;
            Debug.Log("<color=green>[AUDIO]</color> AudioManager tự động kích hoạt trước khi tải Scene!");
        }
    }

    [Header("--- AUDIO SOURCES ---")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource crowdSource;

    [Header("--- AUDIO CLIPS ---")]
    [SerializeField] private AudioClip bgmFestival;
    [SerializeField] private AudioClip sfxButtonTap;
    [SerializeField] private AudioClip sfxBumperHit;
    [SerializeField] private AudioClip sfxSkillBurst;
    [SerializeField] private AudioClip sfxRingOut;
    [SerializeField] private AudioClip sfxCrowdCheer;
    [SerializeField] private AudioClip sfxVictory;

    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    private float crowdVolume = 1f;

    public float BGMVolume => bgmVolume;
    public float SFXVolume => sfxVolume;
    public float CrowdVolume => crowdVolume;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureAudioSources();
            LoadAudioClips();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBGM();
        PlayCrowdAmbient();
    }

    private void EnsureAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        if (crowdSource == null)
        {
            crowdSource = gameObject.AddComponent<AudioSource>();
            crowdSource.loop = true;
            crowdSource.playOnAwake = false;
        }
    }

    public void LoadAudioClips()
    {
#if UNITY_EDITOR
        if (bgmFestival == null) bgmFestival = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/bgm_festival_loop.wav");
        if (sfxButtonTap == null) sfxButtonTap = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_button_tap.wav");
        if (sfxBumperHit == null) sfxBumperHit = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_bumper_hit.wav");
        if (sfxSkillBurst == null) sfxSkillBurst = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_skill_burst.wav");
        if (sfxRingOut == null) sfxRingOut = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_ring_out.wav");
        if (sfxCrowdCheer == null) sfxCrowdCheer = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_crowd_cheer.wav");
        if (sfxVictory == null) sfxVictory = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_victory.wav");
#endif
    }

    public void PlayBGM()
    {
        EnsureAudioSources();
        LoadAudioClips();
        if (bgmSource != null && bgmFestival != null && !bgmSource.isPlaying)
        {
            bgmSource.clip = bgmFestival;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
            Debug.Log("<color=green>[AUDIO]</color> Đang phát Nhạc Nền Lễ Hội Hữu Nghị (BGM Loop)!");
        }
    }

    public void PlayCrowdAmbient()
    {
        EnsureAudioSources();
        LoadAudioClips();
        if (crowdSource != null && sfxCrowdCheer != null && !crowdSource.isPlaying)
        {
            crowdSource.clip = sfxCrowdCheer;
            crowdSource.volume = crowdVolume * 0.6f;
            crowdSource.Play();
        }
    }

    public void PlayButtonClick()
    {
        PlaySFX(sfxButtonTap, 0.9f);
    }

    public void PlayCollisionHit()
    {
        PlaySFX(sfxBumperHit, 1.0f);
    }

    public void PlaySkillBurst()
    {
        PlaySFX(sfxSkillBurst, 1.0f);
    }

    public void PlayRingOut()
    {
        PlaySFX(sfxRingOut, 1.0f);
    }

    public void PlayVictoryFanfare()
    {
        PlaySFX(sfxVictory, 1.0f);
    }

    public void PlayCrowdCheerBurst()
    {
        PlaySFX(sfxCrowdCheer, 1.0f);
    }

    public void PlaySFX(AudioClip clip, float pitch = 1.0f)
    {
        EnsureAudioSources();
        if (clip == null) LoadAudioClips();
        if (sfxSource != null && clip != null && sfxVolume > 0.01f)
        {
            sfxSource.pitch = pitch * Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null) bgmSource.volume = bgmVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    public void SetCrowdVolume(float volume)
    {
        crowdVolume = Mathf.Clamp01(volume);
        if (crowdSource != null) crowdSource.volume = crowdVolume * 0.6f;
    }

    public void ToggleMute()
    {
        AudioListener.pause = !AudioListener.pause;
    }
}
