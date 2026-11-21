using UnityEngine;

public class WallNote : MonoBehaviour
{
  [SerializeField] private NotePopupUI popupUI;
  [SerializeField] private string noteTitle = "Записка";
  [TextArea(3, 8)]
  [SerializeField] private string noteBody = "Текст записки...";
  [SerializeField] private Vector3 popupOffset = new Vector3(0f, 0.25f, -0.15f);

  public void OnHoverEntered()
  {
    if (popupUI == null)
      return;

    Vector3 spawnPos = transform.position + transform.TransformDirection(popupOffset);
    popupUI.ShowNote(noteTitle, noteBody, spawnPos);
  }

  public void OnHoverExited()
  {
    if (popupUI != null)
      popupUI.HideNote();
  }
}