using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    [Header("Звук кроків")]
    public AudioSource footstepAudio;
    public float stepInterval = 0.45f;
    public float minMoveSpeed = 0.1f;

    [Header("Легка варіативність (необов'язково)")]
    public bool randomizePitch = true;
    public float minPitch = 0.92f;
    public float maxPitch = 1.08f;

    private Vector3 _lastPosition;
    private float _stepTimer;

    private void Awake()
    {
        _lastPosition = transform.position;
        if (footstepAudio == null)
        {
            footstepAudio = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        Vector3 currentPos = transform.position;
        Vector3 horizontalDelta = new Vector3(currentPos.x - _lastPosition.x, 0f, currentPos.z - _lastPosition.z);
        float speed = horizontalDelta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        _lastPosition = currentPos;

        if (speed >= minMoveSpeed)
        {
            _stepTimer -= Time.deltaTime;
            if (_stepTimer <= 0f)
            {
                PlayStep();
                _stepTimer = stepInterval;
            }
        }
        else
        {
            _stepTimer = Mathf.Min(_stepTimer, 0.08f);
        }
    }

    private void PlayStep()
    {
        if (footstepAudio == null || footstepAudio.clip == null) return;

        if (randomizePitch)
        {
            footstepAudio.pitch = Random.Range(minPitch, maxPitch);
        }

        footstepAudio.PlayOneShot(footstepAudio.clip);
    }
}