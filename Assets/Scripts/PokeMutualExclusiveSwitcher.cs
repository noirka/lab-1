using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PokeMutualExclusiveSwitcher : MonoBehaviour
{
  [Header("Hands Poke")]
  [SerializeField] private XRPokeInteractor handPokeInteractor;
  [SerializeField] private IndexPointPokeGate handPokeGate;
  [SerializeField] private bool useGateWhenHandActive = true;

  [Header("Controller Poke")]
  [SerializeField] private XRPokeInteractor controllerPokeInteractor;

  [Header("Start State")]
  [SerializeField] private bool startWithHands = false;
  [SerializeField] private int ignoreFirstPresses = 1;

  private bool isHandsMode;
  private int ignoredPressCounter;

  private void Awake()
  {
    ignoredPressCounter = 0;
    ResolveHandPokeGate();
    ApplyMode(startWithHands, logChange: true);
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.LeftBracket) || Input.GetKeyDown(KeyCode.RightBracket))
    {
      if (ignoredPressCounter < ignoreFirstPresses)
      {
        ignoredPressCounter++;
        Debug.Log($"[PokeSwitcher] Ignored key press #{ignoredPressCounter}");
        return;
      }

      ApplyMode(!isHandsMode, logChange: true);
    }
  }

  private void LateUpdate()
  {
    EnforceCurrentMode();
  }

  private void ResolveHandPokeGate()
  {
    if (handPokeGate != null)
      return;

    if (handPokeInteractor != null)
    {
      handPokeGate = handPokeInteractor.GetComponent<IndexPointPokeGate>();
      if (handPokeGate == null)
        handPokeGate = handPokeInteractor.GetComponentInChildren<IndexPointPokeGate>(true);
    }
  }

  public void ApplyMode(bool useHands, bool logChange = false)
  {
    isHandsMode = useHands;
    EnforceCurrentMode();

    if (logChange)
    {
      Debug.Log($"[PokeSwitcher] Mode switched -> isHandsMode={isHandsMode}");
    }
  }

  private void EnforceCurrentMode()
  {
    ResolveHandPokeGate();

    if (isHandsMode)
    {
      SetControllerPokeState(false);
      SetHandPokeState(true);
    }
    else
    {
      SetHandPokeState(false);
      SetControllerPokeState(true);
    }
  }

  private void SetHandPokeState(bool active)
  {
    if (handPokeInteractor == null)
      return;

    if (active)
    {
      if (!handPokeInteractor.gameObject.activeSelf)
        handPokeInteractor.gameObject.SetActive(true);

      if (useGateWhenHandActive && handPokeGate != null)
      {
        if (!handPokeGate.enabled)
          handPokeGate.enabled = true;
      }
      else
      {
        if (handPokeGate != null && handPokeGate.enabled)
          handPokeGate.enabled = false;

        if (!handPokeInteractor.enabled)
          handPokeInteractor.enabled = true;
      }
    }
    else
    {
      if (handPokeGate != null && handPokeGate.enabled)
        handPokeGate.enabled = false;

      if (handPokeInteractor.enabled)
        handPokeInteractor.enabled = false;

      if (handPokeInteractor.gameObject.activeSelf)
        handPokeInteractor.gameObject.SetActive(false);
    }
  }

  private void SetControllerPokeState(bool active)
  {
    if (controllerPokeInteractor == null)
      return;

    if (active)
    {
      if (!controllerPokeInteractor.gameObject.activeSelf)
        controllerPokeInteractor.gameObject.SetActive(true);

      if (!controllerPokeInteractor.enabled)
        controllerPokeInteractor.enabled = true;
    }
    else
    {
      if (controllerPokeInteractor.enabled)
        controllerPokeInteractor.enabled = false;

      if (controllerPokeInteractor.gameObject.activeSelf)
        controllerPokeInteractor.gameObject.SetActive(false);
    }
  }
}