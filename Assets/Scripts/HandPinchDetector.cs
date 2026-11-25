using UnityEngine;
using UnityEngine.XR.Hands;

public class HandPinchDetector : MonoBehaviour
{
  [SerializeField] private XRHandTrackingEvents handTrackingEvents;
  [SerializeField] private HandGestureGrabBridge grabBridge;

  [Header("Pinch Distance Thresholds (meters)")]
  [SerializeField] private float pinchStartDistance = 0.035f;
  [SerializeField] private float pinchReleaseDistance = 0.055f;

  private bool isPinching;

  private void OnEnable()
  {
    if (handTrackingEvents != null)
      handTrackingEvents.jointsUpdated.AddListener(OnJointsUpdated);
  }

  private void OnDisable()
  {
    if (handTrackingEvents != null)
      handTrackingEvents.jointsUpdated.RemoveListener(OnJointsUpdated);

    if (isPinching)
    {
      isPinching = false;
      grabBridge?.ReleaseGrab();
    }
  }

  private void OnJointsUpdated(XRHandJointsUpdatedEventArgs args)
  {
    if (grabBridge == null || handTrackingEvents == null || !handTrackingEvents.handIsTracked)
    {
      if (isPinching)
      {
        isPinching = false;
        grabBridge?.ReleaseGrab();
      }
      return;
    }

    XRHandJoint thumbTip = args.hand.GetJoint(XRHandJointID.ThumbTip);
    XRHandJoint indexTip = args.hand.GetJoint(XRHandJointID.IndexTip);

    if (!thumbTip.TryGetPose(out Pose thumbPose) || !indexTip.TryGetPose(out Pose indexPose))
      return;

    float distance = Vector3.Distance(thumbPose.position, indexPose.position);

    if (!isPinching && distance <= pinchStartDistance)
    {
      isPinching = true;
      grabBridge.StartGrab();
    }
    else if (isPinching && distance >= pinchReleaseDistance)
    {
      isPinching = false;
      grabBridge.ReleaseGrab();
    }
  }
}