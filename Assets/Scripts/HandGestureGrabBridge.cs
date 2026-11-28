using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class HandGestureGrabBridge : MonoBehaviour
{
  [SerializeField] private XRDirectInteractor directInteractor;
  [SerializeField] private XRInteractionManager interactionManager;

  private IXRSelectInteractable currentGrabbed;

  private void Awake()
  {
    if (directInteractor == null)
      directInteractor = GetComponent<XRDirectInteractor>();

    if (interactionManager == null && directInteractor != null)
      interactionManager = directInteractor.interactionManager;
  }

  public void StartGrab()
  {
    if (directInteractor == null || interactionManager == null)
    {
      // Debug.LogWarning("[HandGrabBridge] StartGrab: відсутній directInteractor або interactionManager");
      return;
    }

    if (currentGrabbed != null)
      return;

    IXRSelectInteractable target = FindBestTarget();
    if (target == null)
    {
      // Debug.Log("[HandGrabBridge] StartGrab: target is null (нічого не в Hover)");
      return;
    }

    // Debug.Log($"[HandGrabBridge] StartGrab: беру {target.transform.name}");
    interactionManager.SelectEnter((IXRSelectInteractor)directInteractor, target);
    currentGrabbed = target;
  }

  public void ReleaseGrab()
  {
    if (currentGrabbed == null || directInteractor == null || interactionManager == null)
      return;

    // Debug.Log($"[HandGrabBridge] ReleaseGrab: відпускаю {currentGrabbed.transform.name}");
    interactionManager.SelectExit((IXRSelectInteractor)directInteractor, currentGrabbed);
    currentGrabbed = null;
  }

  private IXRSelectInteractable FindBestTarget()
  {
    if (directInteractor == null)
      return null;

    List<IXRHoverInteractable> hovered = directInteractor.interactablesHovered;
    if (hovered != null)
    {
      for (int i = 0; i < hovered.Count; i++)
      {
        if (hovered[i] is IXRSelectInteractable selectInteractable)
        {
          if (directInteractor.CanSelect(selectInteractable))
            return selectInteractable;
        }
      }
    }

    return null;
  }

}