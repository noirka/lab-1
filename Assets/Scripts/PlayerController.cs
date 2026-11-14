using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform playerCamera;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        if (playerCamera != null)
        {
            Vector3 camForward = playerCamera.forward;
            Vector3 camRight = playerCamera.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camForward * moveVertical + camRight * moveHorizontal).normalized;

            Vector3 targetVelocity = moveDir * moveSpeed;
            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = targetVelocity;
        }
    }
}