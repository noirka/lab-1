using UnityEngine;
using UnityEngine.InputSystem;

public class SpatialMenuToggle : MonoBehaviour
{
  [Header("UI Panel")]
  [SerializeField] private GameObject menuPanel;
  [SerializeField] private Transform viewerCamera;

  [Header("Placement")]
  [SerializeField] private float distanceFromCamera = 0.9f;
  [SerializeField] private float heightOffset = -0.12f;
  [SerializeField] private bool followCameraWhileOpen = true;
  [SerializeField] private float followSpeed = 8f;

  [Header("Left Controller Input")]
  [SerializeField] private InputActionProperty leftControllerMenuAction;

  [Header("Keyboard Shortcut (for testing without headset)")]
  [SerializeField] private bool requireModifierKey = true;
  [SerializeField] private Key modifierKey = Key.LeftCtrl;
  [SerializeField] private Key toggleKey = Key.M;

  private void OnEnable()
  {
    if (leftControllerMenuAction.action != null)
    {
      leftControllerMenuAction.action.Enable();
      leftControllerMenuAction.action.performed += OnControllerTogglePerformed;
    }
  }

  private void OnDisable()
  {
    if (leftControllerMenuAction.action != null)
    {
      leftControllerMenuAction.action.performed -= OnControllerTogglePerformed;
      leftControllerMenuAction.action.Disable();
    }
  }

  private void Start()
  {
    if (viewerCamera == null && Camera.main != null)
      viewerCamera = Camera.main.transform;

    if (menuPanel != null)
      menuPanel.SetActive(false);
  }

  private void Update()
  {
    if (IsKeyboardShortcutPressed())
    {
      ToggleMenu();
    }

    if (followCameraWhileOpen && menuPanel != null && menuPanel.activeSelf)
    {
      UpdatePlacement(instant: false);
    }
  }

  private bool IsKeyboardShortcutPressed()
  {
    var kb = Keyboard.current;
    if (kb == null) return false;

    bool keyJustPressed = kb[toggleKey].wasPressedThisFrame;
    if (!keyJustPressed) return false;

    if (!requireModifierKey) return true;

    return kb[modifierKey].isPressed;
  }

  private void OnControllerTogglePerformed(InputAction.CallbackContext ctx)
  {
    ToggleMenu();
  }

  public void ToggleMenu()
  {
    if (menuPanel == null) return;

    bool nextState = !menuPanel.activeSelf;
    if (nextState)
    {
      UpdatePlacement(instant: true);
    }
    menuPanel.SetActive(nextState);
  }

  private void UpdatePlacement(bool instant)
  {
    if (viewerCamera == null) return;

    Vector3 forwardFlat = Vector3.ProjectOnPlane(viewerCamera.forward, Vector3.up).normalized;
    if (forwardFlat.sqrMagnitude < 0.001f)
      forwardFlat = viewerCamera.forward;

    Vector3 targetPos = viewerCamera.position
                      + forwardFlat * distanceFromCamera
                      + Vector3.up * heightOffset;

    Vector3 lookDir = viewerCamera.position - targetPos;
    lookDir.y = 0f;
    Quaternion targetRot = lookDir.sqrMagnitude > 0.0001f
        ? Quaternion.LookRotation(-lookDir.normalized, Vector3.up)
        : menuPanel.transform.rotation;

    if (instant)
    {
      menuPanel.transform.position = targetPos;
      menuPanel.transform.rotation = targetRot;
    }
    else
    {
      float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
      menuPanel.transform.position = Vector3.Lerp(menuPanel.transform.position, targetPos, t);
      menuPanel.transform.rotation = Quaternion.Slerp(menuPanel.transform.rotation, targetRot, t);
    }
  }
}