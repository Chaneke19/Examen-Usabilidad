using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jsgaona
{

    // Este script se puede usar para controlar la accesibilidad de color en aplicaciones XR.
    // Permite a los usuarios seleccionar diferentes modos de daltonismo y ajustar la intensidad del efecto.
    public class XRColorAccessibilityController : MonoBehaviour
    {

        // Lista de modos de daltonismo disponibles.
        // Puedes agregar más modos si tu shader los soporta.
        public enum ColorBlindMode
        {
            Off = 0,
            Protanopia = 1,
            Deuteranopia = 2,
            Tritanopia = 3,
            HighContrast = 4
        }
        [SerializeField] private TMP_Dropdown Drop;

        [Header("Subtitulos y voz")]
        [SerializeField] private Toggle subtitlesToggle;
        [SerializeField] private Slider subtitleVolumeSlider;
        [SerializeField] private TMP_Text subtitleVolumeText;

        private void OnEnable()
        {
            if (subtitlesToggle) subtitlesToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("SubtitlesEnabled", 1) == 1);
            float volume = PlayerPrefs.GetFloat("SubtitleVolume", 1f);
            if (subtitleVolumeSlider) subtitleVolumeSlider.SetValueWithoutNotify(volume);
            if (subtitleVolumeText) subtitleVolumeText.text = Mathf.RoundToInt(volume * 100f) + "%";
            if (subtitlesToggle) subtitlesToggle.onValueChanged.AddListener(SetSubtitlesEnabled);
            if (subtitleVolumeSlider) subtitleVolumeSlider.onValueChanged.AddListener(SetSubtitleVolume);
        }

        private void OnDisable()
        {
            if (subtitlesToggle) subtitlesToggle.onValueChanged.RemoveListener(SetSubtitlesEnabled);
            if (subtitleVolumeSlider) subtitleVolumeSlider.onValueChanged.RemoveListener(SetSubtitleVolume);
            PlayerPrefs.Save();
        }

        public void SetSubtitlesEnabled(bool enabled)
        {
            PlayerPrefs.SetInt("SubtitlesEnabled", enabled ? 1 : 0);
        }

        public void SetSubtitleVolume(float value)
        {
            PlayerPrefs.SetFloat("SubtitleVolume", value);
            if (subtitleVolumeText) subtitleVolumeText.text = Mathf.RoundToInt(value * 100f) + "%";
        }


        // Material que se usará para aplicar el efecto de accesibilidad de color.
        [SerializeField] private Material accessibilityMaterial;

        // Modo de daltonismo seleccionado por el usuario.
        [SerializeField] private ColorBlindMode currentMode = ColorBlindMode.Off;

        // Intensidad del efecto de accesibilidad de color.
        [SerializeField][Range(0f, 1f)] private float intensity = 1f;



        // Aplica la configuración inicial al iniciar la aplicación.
        private void Start()
        {
            int Index = PlayerPrefs.GetInt("Daltonismo");
            SetMode(Index);
            Drop.value = Index;
            ApplySettings();
        }


        // Aplica los cambios en tiempo real cuando se modifican los valores en el Inspector.
        private void OnValidate()
        {
            ApplySettings();
        }


        // Métodos públicos para cambiar el modo y la intensidad desde otros scripts o UI.
        public void SetMode(int modeIndex)
        {
            currentMode = (ColorBlindMode)modeIndex;
            ApplySettings();
            PlayerPrefs.SetInt("Daltonismo", modeIndex);

        }


        // Permite ajustar la intensidad del efecto desde otros scripts o UI.
        public void SetIntensity(float value)
        {
            intensity = Mathf.Clamp01(value);
            ApplySettings();
        }
        // 


        // Aplica los ajustes al material de accesibilidad.
        private void ApplySettings()
        {
            if (accessibilityMaterial == null) return;
            accessibilityMaterial.SetFloat("_Mode", (float)currentMode);
            accessibilityMaterial.SetFloat("_Intensity", intensity);
        }
        private void SaveSettings(int value)
        {
            PlayerPrefs.SetInt("Daltonismo", value);
        }
        private void ReadSettings(float value)
        {

        }
    }
}