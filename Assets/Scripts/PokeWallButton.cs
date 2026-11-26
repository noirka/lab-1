using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class PokeWallButton : MonoBehaviour
{
  [SerializeField] private SlidingWallBarrier[] targetWalls;
  [SerializeField] private Transform buttonVisual;
  [SerializeField] private float pressDepth = 0.03f;
  [SerializeField] private bool toggleMode = true;

  private XRSimpleInteractable interactable;
  private Vector3 initialVisualLocalPos;
  private bool isActivated;

  private void Awake()
  {
    interactable = GetComponent<XRSimpleInteractable>();
    if (buttonVisual == null)
      buttonVisual = transform;

    initialVisualLocalPos = buttonVisual.localPosition;
  }

  private void OnEnable()
  {
    interactable.selectEntered.AddListener(OnPokeEnter);
    interactable.selectExited.AddListener(OnPokeExit);
  }

  private void OnDisable()
  {
    interactable.selectEntered.RemoveListener(OnPokeEnter);
    interactable.selectExited.RemoveListener(OnPokeExit);
  }

  private void OnPokeEnter(SelectEnterEventArgs args)
  {
    buttonVisual.localPosition = initialVisualLocalPos - new Vector3(0f, pressDepth, 0f);

    if (toggleMode)
    {
      isActivated = !isActivated;
      for (int i = 0; i < targetWalls.Length; i++)
      {
        if (targetWalls[i] != null)
          targetWalls[i].SetOpen(isActivated);
      }
    }
    else
    {
      for (int i = 0; i < targetWalls.Length; i++)
      {
        if (targetWalls[i] != null)
          targetWalls[i].SetOpen(true);
      }
    }
  }

  private void OnPokeExit(SelectExitEventArgs args)
  {
    buttonVisual.localPosition = initialVisualLocalPos;
  }
}