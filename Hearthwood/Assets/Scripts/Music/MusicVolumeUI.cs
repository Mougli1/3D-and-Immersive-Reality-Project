using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private AudioSource musicSource;

    [Header("Config")]
    [SerializeField] private float maxMusicVolume = 0.2f;

    private const string PREF_KEY = "MusicVolume01"; // on stocke 0..1

    private void Awake()
    {
        if (!musicSlider) return;

        float saved01 = PlayerPrefs.GetFloat(PREF_KEY, 1f); // par défaut 100%
        musicSlider.SetValueWithoutNotify(saved01);
        Apply(saved01);

        musicSlider.onValueChanged.AddListener(Apply);
    }

    private void OnDestroy()
    {
        if (musicSlider) musicSlider.onValueChanged.RemoveListener(Apply);
    }

    private void Apply(float value01)
    {
        value01 = Mathf.Clamp01(value01);

        if (musicSource)
            musicSource.volume = value01 * maxMusicVolume;

        PlayerPrefs.SetFloat(PREF_KEY, value01);
        PlayerPrefs.Save();
    }
}
