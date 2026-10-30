using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public sealed class AudioService : MonoBehaviour
{
    private static AudioService instance;

    [Header("Fuentes")]
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("UI universal")]
    [Tooltip("Único sonido utilizado por todos los botones del juego.")]
    [SerializeField] private AudioClip universalButtonClip;
    [Range(0f, 1f)] [SerializeField] private float uiVolume = 1f;

    [Header("Efectos")]
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    [Header("Combate")]
    [SerializeField] private AudioClip playerHitClip;
    [SerializeField] private AudioClip playerDeathClip;
    [SerializeField] private AudioClip enemyDeathClip;
    [Range(0f, 1f)] [SerializeField] private float combatFeedbackVolume = 1f;

    [Header("Gacha")]
    [SerializeField] private AudioClip upgradeClip;
    [Range(0f, 1f)] [SerializeField] private float upgradeVolume = 1f;

    private readonly Dictionary<AudioClip, float> nextAllowedPlayTime =
        new Dictionary<AudioClip, float>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        ServiceLocator.Instance.Register(this);
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        if (ServiceLocator.HasInstance) ServiceLocator.Instance.Unregister(this);
        instance = null;
    }

    public void PlayUniversalButton() => Play(uiSource, universalButtonClip, uiVolume);

    public void PlayUi(AudioClip clip, float volumeScale = 1f) =>
        Play(uiSource, clip, uiVolume * Mathf.Clamp01(volumeScale));

    public void PlaySfx(AudioClip clip, float volumeScale = 1f, float minimumInterval = 0f)
    {
        if (clip == null || sfxSource == null) return;
        float now = Time.unscaledTime;
        if (minimumInterval > 0f && nextAllowedPlayTime.TryGetValue(clip, out float nextTime) && now < nextTime)
            return;

        if (minimumInterval > 0f) nextAllowedPlayTime[clip] = now + minimumInterval;
        Play(sfxSource, clip, sfxVolume * Mathf.Clamp01(volumeScale));
    }

    public void PlayDamageFeedback(Faction faction, bool died)
    {
        AudioClip clip = faction == Faction.Player
            ? (died ? playerDeathClip : playerHitClip)
            : (died ? enemyDeathClip : null);
        PlaySfx(clip, combatFeedbackVolume);
    }

    public void PlayUpgradeFeedback() => PlaySfx(upgradeClip, upgradeVolume);

    private static void Play(AudioSource source, AudioClip clip, float volume)
    {
        if (source == null || clip == null || volume <= 0f) return;
        source.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}
