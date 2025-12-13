using UnityEngine;

public class OVRSimpleClimbing : MonoBehaviour
{
    [Header("References")]
    public OVRCameraRig cameraRig;
    public CharacterController characterController;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Climbing Settings")]
    public LayerMask climbableLayer;
    public float grabRadius = 0.15f;
    public float climbStrength = 1.0f;

    [Header("Momentum Jump Settings")]
    public float jumpMultiplier = 1.2f;
    public float maxJumpVelocity = 4.0f;
    public float upwardBoost = 0.6f;

    private bool leftGrabbing;
    private bool rightGrabbing;

    private Vector3 lastLeftPos;
    private Vector3 lastRightPos;

    private Vector3 leftVelocity;
    private Vector3 rightVelocity;

    private Vector3 externalVelocity; // momentum + gravity

    void Start()
    {
        if (!cameraRig)
            cameraRig = FindObjectOfType<OVRCameraRig>();

        if (!characterController)
            characterController = cameraRig.GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleHand(
            OVRInput.Controller.LTouch,
            OVRInput.Button.PrimaryHandTrigger,
            leftHand,
            ref leftGrabbing,
            ref lastLeftPos,
            ref leftVelocity
        );

        HandleHand(
            OVRInput.Controller.RTouch,
            OVRInput.Button.PrimaryHandTrigger,
            rightHand,
            ref rightGrabbing,
            ref lastRightPos,
            ref rightVelocity
        );

        // Apply momentum & gravity
        if (!leftGrabbing && !rightGrabbing)
        {
            externalVelocity.y += Physics.gravity.y * Time.deltaTime;
            characterController.Move(externalVelocity * Time.deltaTime);
        }

        if (characterController.isGrounded)
        {
            externalVelocity = Vector3.zero;
        }
    }

    void HandleHand(
        OVRInput.Controller controller,
        OVRInput.Button grabButton,
        Transform hand,
        ref bool grabbing,
        ref Vector3 lastHandPos,
        ref Vector3 handVelocity)
    {
        bool gripPressed = OVRInput.Get(grabButton, controller);

        if (gripPressed && IsClimbable(hand.position))
        {
            if (!grabbing)
            {
                grabbing = true;
                lastHandPos = hand.position;
                handVelocity = Vector3.zero;
                externalVelocity = Vector3.zero; // stop falling
            }
            else
            {
                Vector3 delta = hand.position - lastHandPos;
                handVelocity = delta / Mathf.Max(Time.deltaTime, 0.0001f);

                Vector3 move = -delta * climbStrength;
                characterController.Move(move);

                lastHandPos = hand.position;
            }
        }
        else
        {
            if (grabbing)
            {
                ApplyMomentumJump(handVelocity);
            }

            grabbing = false;
            handVelocity = Vector3.zero;
        }
    }

    void ApplyMomentumJump(Vector3 handVelocity)
    {
        Vector3 jumpVelocity = -handVelocity * jumpMultiplier;
        jumpVelocity.y += upwardBoost;

        if (jumpVelocity.magnitude > maxJumpVelocity)
            jumpVelocity = jumpVelocity.normalized * maxJumpVelocity;

        externalVelocity = jumpVelocity;
    }

    bool IsClimbable(Vector3 position)
    {
        return Physics.CheckSphere(position, grabRadius, climbableLayer);
    }
}
