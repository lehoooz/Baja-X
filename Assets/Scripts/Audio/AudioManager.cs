using UnityEngine;

/// <summary>
/// Mengatur semua suara: BGM (musik latar) dan SFX (efek suara).
/// Satu AudioManager untuk seluruh game (DontDestroyOnLoad), jadi musik tidak putus pindah scene.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource; // untuk BGM (loop)
    [SerializeField] private AudioSource sfxSource;   // untuk SFX (sekali main)

    [Header("Volume (0 - 1)")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.8f;

    [Header("BGM")]
    [SerializeField] private AudioClip stageBGM;
    [SerializeField] private AudioClip bossBGM;

    [Header("SFX Pemain")]
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip dashClip;
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private AudioClip deathClip;

    [Header("SFX Musuh & Combat")]
    [SerializeField] private AudioClip hitClip;        // peluru kena musuh
    [SerializeField] private AudioClip enemyDeathClip;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioClip bossShootClip;
    [SerializeField] private AudioClip bossSlamClip;
    [SerializeField] private AudioClip bossDeathClip;

    [Header("SFX UI & Dunia")]
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip checkpointClip;
    [SerializeField] private AudioClip stageClearClip;
    [SerializeField] private AudioClip gameOverClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    // ---------- BGM ----------
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlayStageMusic() => PlayMusic(stageBGM);
    public void PlayBossMusic() => PlayMusic(bossBGM);
    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    // ---------- SFX ----------
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayShoot() => PlaySFX(shootClip);
    public void PlayJump() => PlaySFX(jumpClip);
    public void PlayDash() => PlaySFX(dashClip);
    public void PlayHurt() => PlaySFX(hurtClip);
    public void PlayDeath() => PlaySFX(deathClip);
    public void PlayHit() => PlaySFX(hitClip);
    public void PlayEnemyDeath() => PlaySFX(enemyDeathClip);
    public void PlayExplosion() => PlaySFX(explosionClip);
    public void PlayBossShoot() => PlaySFX(bossShootClip);
    public void PlayBossSlam() => PlaySFX(bossSlamClip);
    public void PlayBossDeath() => PlaySFX(bossDeathClip);
    public void PlayButtonClick() => PlaySFX(buttonClickClip);
    public void PlayCheckpoint() => PlaySFX(checkpointClip);
    public void PlayStageClear() => PlaySFX(stageClearClip);
    public void PlayGameOver() => PlaySFX(gameOverClip);

    // ---------- Volume (untuk slider Settings) ----------
    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }
}