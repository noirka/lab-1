using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class IndexPointPokeGate : MonoBehaviour
{
  [SerializeField] private XRHandTrackingEvents handTrackingEvents;
  [SerializeField] private XRPokeInteractor pokeInteractor;
  [SerializeField] private GameObject optionalPokeVisual;

  [Header("Extension Thresholds (meters)")]
  [Tooltip("Мінімальна відстань від IndexTip до Wrist, коли палець вважається витягнутим")]
  [SerializeField] private float indexExtendedMinDist = 0.16f;

  [Tooltip("Гістерезис для вимкнення, щоб не миготіло при залипанні клавіші T")]
  [SerializeField] private float indexRetractedMaxDist = 0.135f;

  [Tooltip("Максимальна відстань кінчиків середніх/безіменних/мізинця до Wrist (пальці підігнуті)")]
  [SerializeField] private float otherFingersCurledMaxDist = 0.125f;

  [Header("Anti-Jitter")]
  [SerializeField] private float stateChangeCooldown = 0.08f;

  private bool isPointingActive;
  private float lastToggleTime;

  private void Awake()
  {
    if (pokeInteractor == null)
      pokeInteractor = GetComponent<XRPokeInteractor>();

    ApplyPokeState(false, force: true);
  }

  private void OnEnable()
  {
    if (handTrackingEvents != null)
      handTrackingEvents.jointsUpdated.AddListener(OnJointsUpdated);
  }

  private void OnDisable()
  {
    if (handTrackingEvents != null)
      handTrackingEvents.jointsUpdated.RemoveListener(OnJointsUpdated);

    ApplyPokeState(false, force: true);
  }

  private void OnJointsUpdated(XRHandJointsUpdatedEventArgs args)
  {
    if (handTrackingEvents == null || !handTrackingEvents.handIsTracked)
    {
      ApplyPokeState(false);
      return;
    }

    if (!TryGetJointPos(args.hand, XRHandJointID.Wrist, out Vector3 wristPos) ||
        !TryGetJointPos(args.hand, XRHandJointID.IndexTip, out Vector3 indexTipPos) ||
        !TryGetJointPos(args.hand, XRHandJointID.MiddleTip, out Vector3 middleTipPos) ||
        !TryGetJointPos(args.hand, XRHandJointID.RingTip, out Vector3 ringTipPos) ||
        !TryGetJointPos(args.hand, XRHandJointID.LittleTip, out Vector3 littleTipPos))
    {
      return;
    }

    float indexDist = Vector3.Distance(indexTipPos, wristPos);
    float avgOtherDist = (
        Vector3.Distance(middleTipPos, wristPos) +
        Vector3.Distance(ringTipPos, wristPos) +
        Vector3.Distance(littleTipPos, wristPos)
    ) / 3f;

    bool othersCurled = avgOtherDist <= otherFingersCurledMaxDist;

    if (!isPointingActive)
    {
      if (indexDist >= indexExtendedMinDist && othersCurled)
        ApplyPokeState(true);
    }
    else
    {
      if (indexDist <= indexRetractedMaxDist || !othersCurled)
        ApplyPokeState(false);
    }

    Debug.Log(
    $"[PokeGate] tracked={handTrackingEvents?.handIsTracked} " +
    $"idx={indexDist:F3} (min={indexExtendedMinDist}) " +
    $"othersAvg={avgOtherDist:F3} (max={otherFingersCurledMaxDist}) " +
    $"pointing={isPointingActive}"
);
  }

  private bool TryGetJointPos(XRHand hand, XRHandJointID jointId, out Vector3 pos)
  {
    pos = Vector3.zero;
    XRHandJoint joint = hand.GetJoint(jointId);
    if (joint.TryGetPose(out Pose pose))
    {
      pos = pose.position;
      return true;
    }
    return false;
  }

  private void ApplyPokeState(bool active, bool force = false)
  {
    if (!force && active == isPointingActive)
      return;

    if (!force && Time.unscaledTime - lastToggleTime < stateChangeCooldown)
      return;

    isPointingActive = active;
    lastToggleTime = Time.unscaledTime;

    if (pokeInteractor != null)
      pokeInteractor.enabled = active;

    if (optionalPokeVisual != null)
      optionalPokeVisual.SetActive(active);
  }
}