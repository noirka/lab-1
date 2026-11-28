using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GameSettingsMenu : MonoBehaviour
{
  [Header("Панель та поява")]
  [SerializeField] private GameObject settingsPanel;
  [SerializeField] private Transform spawnReference;
  [SerializeField] private float distanceFromReference = 1.2f;
  [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.05f, 0f);

  [Header("Ліхтарик (Spot Light) та він'єтка (Global Volume)")]
  [SerializeField] private Light flashlightLight;
  [SerializeField] private Volume globalVolume;

  [Header("Режими Ray / Poke")]
  [SerializeField] private GameObject rayInteractorObject;
  [SerializeField] private GameObject pokeInteractorObject;

  [Header("Елементи форми")]
  [SerializeField] private Toggle flashlightToggle;
  [SerializeField] private Toggle pokeModeToggle;
  [SerializeField] private Slider flashlightBrightnessSlider;
  [SerializeField] private Slider vignetteSlider;
  [SerializeField] private TMP_InputField notesInputField;
  [SerializeField] private TMP_Text statusText;

  [Header("Стандартні значення")]
  [SerializeField] private bool defaultFlashlightOn = true;
  [SerializeField] private bool defaultPokeMode = true;
  [SerializeField] private float defaultFlashlightBrightness = 2.5f;
  [SerializeField] private float defaultVignette = 0.25f;

  [Header("Телеметрія та погода")]
  [SerializeField] private TelemetryAndApi telemetryApi;

  private const string KeyFlashlightOn = "Settings_FlashlightOn";
  private const string KeyPokeMode = "Settings_PokeMode";
  private const string KeyBrightness = "Settings_Brightness";
  private const string KeyVignette = "Settings_Vignette";
  private const string KeyNotes = "Settings_Notes";

  private void Awake()
  {
    if (settingsPanel == null)
      settingsPanel = gameObject;

    if (spawnReference == null && Camera.main != null)
      spawnReference = Camera.main.transform;

    LoadOrInitValues();
    ApplyAllSettings();
    settingsPanel.SetActive(false);
  }

  private void Update()
  {
    bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    if (shiftHeld && Input.GetKeyDown(KeyCode.N))
    {
      TogglePanel();
    }
  }

  public void TogglePanel()
  {
    Debug.Log("[Lab5] TogglePanel викликано");
    bool nextState = !settingsPanel.activeSelf;
    settingsPanel.SetActive(nextState);

    if (nextState && spawnReference != null)
    {
      Vector3 targetPos = spawnReference.position
                        + spawnReference.forward * distanceFromReference
                        + spawnReference.TransformDirection(localOffset);

      settingsPanel.transform.position = targetPos;

      Vector3 lookDir = settingsPanel.transform.position - spawnReference.position;
      if (lookDir.sqrMagnitude > 0.0001f)
        settingsPanel.transform.rotation = Quaternion.LookRotation(lookDir);
    }

    if (telemetryApi != null)
    {
      if (nextState)
        telemetryApi.ShowWeatherPanelAndFetch();
      else
        telemetryApi.HideWeatherPanel();
    }
  }

  public void OnSettingsChanged()
  {
    ApplyAllSettings();
  }

  public void SaveSettings()
  {
    PlayerPrefs.SetInt(KeyFlashlightOn, flashlightToggle != null && flashlightToggle.isOn ? 1 : 0);
    PlayerPrefs.SetInt(KeyPokeMode, pokeModeToggle != null && pokeModeToggle.isOn ? 1 : 0);
    PlayerPrefs.SetFloat(KeyBrightness, flashlightBrightnessSlider != null ? flashlightBrightnessSlider.value : defaultFlashlightBrightness);
    PlayerPrefs.SetFloat(KeyVignette, vignetteSlider != null ? vignetteSlider.value : defaultVignette);
    PlayerPrefs.SetString(KeyNotes, notesInputField != null ? notesInputField.text : string.Empty);
    PlayerPrefs.Save();

    ApplyAllSettings();
    SetStatus("Налаштування та замітки збережено");
  }

  public void ResetSettings()
  {
    if (flashlightToggle != null) flashlightToggle.isOn = defaultFlashlightOn;
    if (pokeModeToggle != null) pokeModeToggle.isOn = defaultPokeMode;
    if (flashlightBrightnessSlider != null) flashlightBrightnessSlider.value = defaultFlashlightBrightness;
    if (vignetteSlider != null) vignetteSlider.value = defaultVignette;
    if (notesInputField != null) notesInputField.text = string.Empty;

    ApplyAllSettings();
    SetStatus("Повернено стандартні значення");
  }

  public void ClearNotes()
  {
    if (notesInputField != null)
    {
      notesInputField.text = string.Empty;
    }

    PlayerPrefs.SetString(KeyNotes, string.Empty);
    PlayerPrefs.Save();
    SetStatus("Замітки очищено");
  }

  private void LoadOrInitValues()
  {
    bool flashOn = PlayerPrefs.GetInt(KeyFlashlightOn, defaultFlashlightOn ? 1 : 0) == 1;
    bool pokeOn = PlayerPrefs.GetInt(KeyPokeMode, defaultPokeMode ? 1 : 0) == 1;
    float brightness = PlayerPrefs.GetFloat(KeyBrightness, defaultFlashlightBrightness);
    float vignette = PlayerPrefs.GetFloat(KeyVignette, defaultVignette);
    string notes = PlayerPrefs.GetString(KeyNotes, string.Empty);

    if (flashlightToggle != null) flashlightToggle.SetIsOnWithoutNotify(flashOn);
    if (pokeModeToggle != null) pokeModeToggle.SetIsOnWithoutNotify(pokeOn);
    if (flashlightBrightnessSlider != null) flashlightBrightnessSlider.SetValueWithoutNotify(brightness);
    if (vignetteSlider != null) vignetteSlider.SetValueWithoutNotify(vignette);
    if (notesInputField != null) notesInputField.SetTextWithoutNotify(notes);
  }

  private void ApplyAllSettings()
  {
    bool flashOn = flashlightToggle == null || flashlightToggle.isOn;
    float brightness = flashlightBrightnessSlider != null ? flashlightBrightnessSlider.value : defaultFlashlightBrightness;
    float vignette = vignetteSlider != null ? vignetteSlider.value : defaultVignette;
    bool pokeOn = pokeModeToggle != null && pokeModeToggle.isOn;

    if (flashlightLight != null)
    {
      flashlightLight.enabled = flashOn;
      flashlightLight.intensity = brightness;
    }

    if (globalVolume != null && globalVolume.profile != null)
    {
      if (globalVolume.profile.TryGet<Vignette>(out var vignetteEffect))
      {
        vignetteEffect.intensity.overrideState = true;
        vignetteEffect.intensity.value = Mathf.Clamp01(vignette);
      }
    }

    if (pokeModeToggle != null)
    {
      if (pokeInteractorObject != null) pokeInteractorObject.SetActive(pokeOn);
      if (rayInteractorObject != null) rayInteractorObject.SetActive(!pokeOn);
    }
  }

  private void SetStatus(string message)
  {
    if (statusText != null)
      statusText.text = message;
  }
}