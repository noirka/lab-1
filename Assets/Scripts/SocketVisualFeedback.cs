using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SocketVisualFeedback : MonoBehaviour
{
  [SerializeField] private XRSocketInteractor socketInteractor;
  [SerializeField] private Renderer indicatorRenderer;
  [SerializeField] private GameObject indicatorObject;
  [SerializeField] private Color emptyColor = Color.yellow;
  [SerializeField] private Color snappedColor = Color.green;

  private Material indicatorMaterial;

  private void Awake()
  {
    if (socketInteractor == null)
      socketInteractor = GetComponent<XRSocketInteractor>();

    if (indicatorRenderer != null)
      indicatorMaterial = indicatorRenderer.material;

    SetSnappedState(false);
  }

  private void OnEnable()
  {
    if (socketInteractor != null)
    {
      socketInteractor.selectEntered.AddListener(OnStickSnapped);
      socketInteractor.selectExited.AddListener(OnStickRemoved);
    }
  }

  private void OnDisable()
  {
    if (socketInteractor != null)
    {
      socketInteractor.selectEntered.RemoveListener(OnStickSnapped);
      socketInteractor.selectExited.RemoveListener(OnStickRemoved);
    }
  }

  private void OnStickSnapped(SelectEnterEventArgs args)
  {
    SetSnappedState(true);
  }

  private void OnStickRemoved(SelectExitEventArgs args)
  {
    SetSnappedState(false);
  }

  private void SetSnappedState(bool isSnapped)
  {
    if (indicatorMaterial != null)
    {
      Color targetColor = isSnapped ? snappedColor : emptyColor;

      if (indicatorMaterial.HasProperty("_BaseColor"))
        indicatorMaterial.SetColor("_BaseColor", targetColor);
      else if (indicatorMaterial.HasProperty("_Color"))
        indicatorMaterial.SetColor("_Color", targetColor);
    }

    if (indicatorObject != null)
      indicatorObject.SetActive(isSnapped);
  }
}