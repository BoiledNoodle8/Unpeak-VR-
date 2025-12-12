using UnityEngine;

public class OVRSimpleClimbing : MonoBehaviour
{
    [Header("References")]
    public OVRCameraRig cameraRig;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Climbing Settings")]
    public LayerMask climbableLayer;
    public float grabRadius = 0.15f;
    public float climbStrength = 1.0f;
    public bool allowTwoHandedClimb = true;

    private bool leftGrabbing;
    private bool rightGrabbing;

    private Vector3 lastLeftPos;
    private Vector3 lastRightPos;

    void Start()
    {
        if (!cameraRig)
            cameraRig = FindObjectOfType<OVRCameraRig>();
    }

    void Update()
    {
        HandleHand(
            OVRInput.Controller.LTouch,
            OVRInput.Button.PrimaryHandTrigger,
            leftHand,
            ref leftGrabbing,
            ref lastLeftPos
        );

        HandleHand(
            OVRInput.Controller.RTouch,
            OVRInput.Button.PrimaryHandTrigger,
            rightHand,
            ref rightGrabbing,
            ref lastRightPos
        );
    }

    void HandleHand(
        OVRInput.Controller controller,
        OVRInput.Button grabButton,
        Transform hand,
        ref bool grabbing,
        ref Vector3 lastHandPos)
    {
        bool gripPressed = OVRInput.Get(grabButton, controller);

        if (gripPressed && IsClimbable(hand.position))
        {
            if (!grabbing)
            {
                grabbing = true;
                lastHandPos = hand.position;
            }
            else
            {
                Vector3 handDelta = hand.position - lastHandPos;
                Vector3 climbMove = -handDelta * climbStrength;

                cameraRig.transform.position += climbMove;
                lastHandPos = hand.position;
            }
        }
        else
        {
            grabbing = false;
        }
    }

    bool IsClimbable(Vector3 position)
    {
        return Physics.CheckSphere(position, grabRadius, climbableLayer);
    }
}
