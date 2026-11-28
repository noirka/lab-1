using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ItemCollector : MonoBehaviour
{
  [Header("Налаштування скримеру")]
  public GameObject screamerImageObj;
  public AudioSource screamerAudio;
  public float displayDuration = 2.0f;

  [Header("Телеметрія")]
  public TelemetryAndApi telemetryApi;

  private bool isTriggered = false;

  private void Awake()
  {
    if (screamerImageObj != null)
    {
      screamerImageObj.SetActive(false);
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (isTriggered) return;

    if (other.CompareTag("GhostItem"))
    {
      Debug.Log("Знайдено містичний предмет: " + other.gameObject.name);
      isTriggered = true;

      if (telemetryApi != null)
        telemetryApi.RecordGhostItemCollected();

      StartCoroutine(TriggerScreamer(other.gameObject));
    }
  }

  private IEnumerator TriggerScreamer(GameObject ghostToDestroy)
  {
    Renderer ghostRenderer = ghostToDestroy.GetComponent<Renderer>();
    if (ghostRenderer != null) ghostRenderer.enabled = false;

    Collider ghostCollider = ghostToDestroy.GetComponent<Collider>();
    if (ghostCollider != null) ghostCollider.enabled = false;

    if (screamerImageObj != null)
    {
      screamerImageObj.SetActive(true);
    }

    if (screamerAudio != null)
    {
      screamerAudio.Play();
    }

    yield return new WaitForSeconds(displayDuration);

    if (screamerImageObj != null)
    {
      screamerImageObj.SetActive(false);
    }

    Destroy(ghostToDestroy);
    isTriggered = false;
  }
}