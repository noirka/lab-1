using UnityEngine;

public class SlidingWallBarrier : MonoBehaviour
{
  [SerializeField] private Vector3 closedLocalPosition;
  [SerializeField] private Vector3 openLocalPosition = new Vector3(0f, 3.5f, 0f);
  [SerializeField] private float moveSpeed = 3f;
  [SerializeField] private bool startsOpen;

  private bool isOpen;

  private void Awake()
  {
    isOpen = startsOpen;
    transform.localPosition = isOpen ? openLocalPosition : closedLocalPosition;
  }

  private void Update()
  {
    Vector3 target = isOpen ? openLocalPosition : closedLocalPosition;
    transform.localPosition = Vector3.MoveTowards(
        transform.localPosition,
        target,
        moveSpeed * Time.deltaTime
    );
  }

  public void SetOpen(bool open)
  {
    isOpen = open;
  }

  public void Toggle()
  {
    isOpen = !isOpen;
  }
}