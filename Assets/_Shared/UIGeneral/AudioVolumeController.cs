using Sirenix.OdinInspector;
using UnityEngine;

public sealed class AudioVolumeController : BaseBehaviour
{
    [Header("Sliders")]
    [SerializeField, Required] private CustomSlider masterSlider;
    [SerializeField, Required] private CustomSlider bgmSlider;
    [SerializeField, Required] private CustomSlider sfxSlider;

    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.Instance;

        SynchronizeSliderValues();

        masterSlider.OnValueChanged.AddListener(SetMasterVolume);
        bgmSlider.OnValueChanged.AddListener(SetBgmVolume);
        sfxSlider.OnValueChanged.AddListener(SetSfxVolume);
    }

    private void OnDestroy()
    {
        masterSlider.OnValueChanged.RemoveListener(SetMasterVolume);
        bgmSlider.OnValueChanged.RemoveListener(SetBgmVolume);
        sfxSlider.OnValueChanged.RemoveListener(SetSfxVolume);
    }

    public void SynchronizeSliderValues()
    {
        masterSlider.Value = audioManager.MasterVolumeDb;
        bgmSlider.Value = audioManager.BGMVolumeDb;
        sfxSlider.Value = audioManager.SFXVolumeDb;
    }

    public void ResetVolumes()
    {
        audioManager.ResetVolumesToDefault();
        SynchronizeSliderValues();
    }

    private void SetMasterVolume(float value) => audioManager.MasterVolumeDb = value;

    private void SetBgmVolume(float value) => audioManager.BGMVolumeDb = value;

    private void SetSfxVolume(float value) => audioManager.SFXVolumeDb = value;
}