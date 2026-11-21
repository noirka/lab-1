using TMPro;
using UnityEngine;

public class NotePopupUI : MonoBehaviour
{
  [SerializeField] private TMP_Text titleText;
  [SerializeField] private TMP_Text bodyText;
  [SerializeField] private bool faceCamera = true;

  private Camera targetCamera;

  private void Awake()
  {
    targetCamera = Camera.main;
    gameObject.SetActive(false);
  }

  public void ShowNote(string title, string body, Vector3 worldPosition)
  {
    if (titleText != null)
      titleText.text = title;

    if (bodyText != null)
      bodyText.text = body;

    transform.position = worldPosition;

    if (faceCamera)
    {
      if (targetCamera == null)
        targetCamera = Camera.main;

      if (targetCamera != null)
      {
        Vector3 lookDir = transform.position - targetCamera.transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.0001f)
          transform.rotation = Quaternion.LookRotation(lookDir);
      }
    }

    gameObject.SetActive(true);
  }

  public void HideNote()
  {
    gameObject.SetActive(false);
  }
}