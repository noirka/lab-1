using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterMove : MonoBehaviour
{
  private CharacterController controller;
  public float speed = 1.2f;
  public float rotationSpeed = 2.0f;

  public Transform trackingSpace;
  private float verticalRotation = 0f;

  [Header("Налаштування кроків")]
  public AudioSource footstepsAudioSource;
  public AudioClip[] stepAudioClips;
  public float stepInterval = 0.5f;
  private float stepTimer;

  void Start()
  {
    controller = GetComponent<CharacterController>();

    if (trackingSpace == null)
    {
      Transform found = transform.Find("TrackingSpace");
      if (found != null) trackingSpace = found;
    }
  }

  void Update()
  {
    float moveX = Input.GetAxis("Horizontal");
    float moveZ = Input.GetAxis("Vertical");

    Vector3 move = transform.right * moveX + transform.forward * moveZ;
    move.y = 0f;

    controller.Move(move * speed * Time.deltaTime);

    bool isMoving = (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f);

    if (isMoving)
    {
      stepTimer -= Time.deltaTime;
      if (stepTimer <= 0f)
      {
        PlayFootstep();
        stepTimer = stepInterval;
      }
    }
    else
    {
      stepTimer = 0f;
    }

    if (Input.GetMouseButton(1))
    {
      float rotX = Input.GetAxis("Mouse X") * rotationSpeed;
      float rotY = Input.GetAxis("Mouse Y") * rotationSpeed;

      transform.Rotate(0, rotX, 0);

      if (trackingSpace != null)
      {
        verticalRotation -= rotY;
        verticalRotation = Mathf.Clamp(verticalRotation, -75f, 75f);

        Vector3 currentRot = trackingSpace.localEulerAngles;
        currentRot.x = verticalRotation;
        trackingSpace.localEulerAngles = currentRot;
      }
    }
  }

  void PlayFootstep()
  {
    if (footstepsAudioSource == null || stepAudioClips == null || stepAudioClips.Length == 0) return;

    AudioClip clip = stepAudioClips[Random.Range(0, stepAudioClips.Length)];

    footstepsAudioSource.PlayOneShot(clip);
  }
}