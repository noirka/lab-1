using UnityEngine;

public class ItemCollector : MonoBehaviour
{
  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("GhostItem"))
    {
      Debug.Log("Знайдено містичний предмет: " + other.gameObject.name);
      Destroy(other.gameObject);
    }
  }
}