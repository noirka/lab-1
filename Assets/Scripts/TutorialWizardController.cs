using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialWizardController : MonoBehaviour
{
  [Serializable]
  public class WizardStep
  {
    public string title;
    [TextArea(3, 6)]
    public string description;
  }

  public enum WizardState
  {
    Inactive,
    StepInProgress,
    Completed
  }

  [Header("Панель та поява")]
  [SerializeField] private GameObject wizardPanel;
  [SerializeField] private Transform spawnReference;
  [SerializeField] private float distanceFromReference = 1.2f;
  [SerializeField] private Vector3 localOffset = new Vector3(0f, 0.02f, 0f);
  [SerializeField] private bool openOnStart = true;

  [Header("UI елементи візарду")]
  [SerializeField] private TMP_Text stepCounterText;
  [SerializeField] private TMP_Text titleText;
  [SerializeField] private TMP_Text descriptionText;
  [SerializeField] private Button backButton;
  [SerializeField] private Button nextButton;
  [SerializeField] private TMP_Text nextButtonLabel;

  [Header("Телеметрія")]
  [SerializeField] private TelemetryAndApi telemetryApi;

  [Header("Кроки інструктажу (State Machine)")]
  [SerializeField]
  private List<WizardStep> steps = new List<WizardStep>
    {
        new WizardStep
        {
            title = "Вітаємо в лабіринті",
            description = "Досліджуй локацію, шукай підказки та рухайся вперед. Цей покроковий гід допоможе розібратися з керуванням."
        },
        new WizardStep
        {
            title = "Ліхтарик і він'єтка",
            description = "Натисни Shift + N, щоб відкрити меню налаштувань: вмикай/вимикай ліхтарик, змінюй його яскравість та інтенсивність він'єтки."
        },
        new WizardStep
        {
            title = "Режими Ray/Poke та замітки",
            description = "У меню можна перемикати режими Ray та Poke, читати записки на стінах і записувати власні нотатки дослідника."
        },
        new WizardStep
        {
            title = "Готово до дослідження",
            description = "Ти готовий проходити лабіринт. Натисни «Завершити», щоб закрити гід. Повернутися до нього можна по Shift + T."
        }
    };

  public WizardState CurrentState { get; private set; } = WizardState.Inactive;
  private int currentStepIndex = 0;

  private void Start()
  {
    if (wizardPanel == null)
      wizardPanel = gameObject;

    if (spawnReference == null && Camera.main != null)
      spawnReference = Camera.main.transform;

    if (nextButtonLabel == null && nextButton != null)
      nextButtonLabel = nextButton.GetComponentInChildren<TMP_Text>();

    if (steps == null || steps.Count == 0)
    {
      wizardPanel.SetActive(false);
      CurrentState = WizardState.Inactive;
      return;
    }

    if (openOnStart)
    {
      OpenWizard(0);
    }
    else
    {
      wizardPanel.SetActive(false);
      CurrentState = WizardState.Inactive;
    }
  }

  private void Update()
  {
    bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    if (shiftHeld && Input.GetKeyDown(KeyCode.T))
    {
      ToggleWizard();
    }
  }

  public void ToggleWizard()
  {
    if (wizardPanel.activeSelf)
    {
      CloseWizard();
    }
    else
    {
      int startIdx = CurrentState == WizardState.Completed ? 0 : currentStepIndex;
      OpenWizard(startIdx);
    }
  }

  public void OpenWizard(int stepIndex = 0)
  {
    if (steps == null || steps.Count == 0) return;

    currentStepIndex = Mathf.Clamp(stepIndex, 0, steps.Count - 1);
    CurrentState = WizardState.StepInProgress;
    wizardPanel.SetActive(true);
    PositionPanelInFrontOfReference();
    RefreshStepUI();
  }

  public void NextStep()
  {
    if (steps == null || steps.Count == 0) return;

    if (currentStepIndex < steps.Count - 1)
    {
      currentStepIndex++;
      CurrentState = WizardState.StepInProgress;
      RefreshStepUI();
    }
    else
    {
      CompleteWizard();
    }
  }

  public void PreviousStep()
  {
    if (steps == null || steps.Count == 0) return;

    if (currentStepIndex > 0)
    {
      currentStepIndex--;
      CurrentState = WizardState.StepInProgress;
      RefreshStepUI();
    }
  }

  public void CompleteWizard()
  {
    CurrentState = WizardState.Completed;
    wizardPanel.SetActive(false);

    if (telemetryApi != null)
      telemetryApi.StartMazeRunTimer();
  }

  public void CloseWizard()
  {
    wizardPanel.SetActive(false);
    if (CurrentState == WizardState.StepInProgress)
    {
      CurrentState = WizardState.Inactive;
    }
  }

  private void RefreshStepUI()
  {
    WizardStep step = steps[currentStepIndex];

    if (stepCounterText != null)
      stepCounterText.text = $"Крок {currentStepIndex + 1} / {steps.Count}";

    if (titleText != null)
      titleText.text = step.title;

    if (descriptionText != null)
      descriptionText.text = step.description;

    if (backButton != null)
      backButton.interactable = currentStepIndex > 0;

    if (nextButtonLabel != null)
      nextButtonLabel.text = (currentStepIndex == steps.Count - 1) ? "Завершити" : "Далі";
  }

  private void PositionPanelInFrontOfReference()
  {
    if (spawnReference == null) return;

    Vector3 targetPos = spawnReference.position
                      + spawnReference.forward * distanceFromReference
                      + spawnReference.TransformDirection(localOffset);

    wizardPanel.transform.position = targetPos;

    Vector3 lookDir = wizardPanel.transform.position - spawnReference.position;
    if (lookDir.sqrMagnitude > 0.0001f)
      wizardPanel.transform.rotation = Quaternion.LookRotation(lookDir);
  }
}