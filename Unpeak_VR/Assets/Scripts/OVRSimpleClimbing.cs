using UnityEngine;

public class OVRClimbingController : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Climbing")]
    public LayerMask climbableLayer;
    public float grabRadius = 0.15f;
    public float climbStrength = 1.0f;
    public float handDeadzone = 0.003f;
    public float smoothing = 12f;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float groundedForce = -2f;

    [Header("Momentum Jump")]
    public float jumpMultiplier = 1.2f;
    public float upwardBoost = 0.6f;
    public float maxJumpVelocity = 4f;

    private bool leftGrabbing;
    private bool rightGrabbing;

    private Vector3 lastLeftPos;
    private Vector3 lastRightPos;

    private Vector3 leftVelocity;
    private Vector3 rightVelocity;

    private Vector3 smoothClimbMove;
    private Vector3 velocity;

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
    }

    void FixedUpdate()
    {
        Vector3 climbMove = Vector3.zero;

        if (leftGrabbing)
            climbMove += -leftVelocity * Time.fixedDeltaTime;

        if (rightGrabbing)
            climbMove += -rightVelocity * Time.fixedDeltaTime;

        if (leftGrabbing && rightGrabbing)
            climbMove *= 0.5f;

        // Apply climbing
        if (climbMove.sqrMagnitude > 0.0001f)
        {
            smoothClimbMove = Vector3.Lerp(
                smoothClimbMove,
                climbMove * climbStrength,
                smoothing * Time.fixedDeltaTime
            );

            velocity = Vector3.zero;
            controller.Move(smoothClimbMove);
        }
        else
        {
            smoothClimbMove = Vector3.zero;

            // Gravity
            if (controller.isGrounded)
            {
                velocity.y = groundedForce;
            }
            else
            {
                velocity.y += gravity * Time.fixedDeltaTime;
            }

            controller.Move(velocity * Time.fixedDeltaTime);
        }
    }

    void HandleHand(
        OVRInput.Controller controllerInput,
        OVRInput.Button grabButton,
        Transform hand,
        ref bool grabbing,
        ref Vector3 lastPos,
        ref Vector3 handVelocity
    )
    {
        bool grip = OVRInput.Get(grabButton, controllerInput);

        if (grip && IsClimbable(hand.position))
        {
            if (!grabbing)
            {
                grabbing = true;
                lastPos = hand.position;
                handVelocity = Vector3.zero;
                velocity = Vector3.zero;
            }
            else
            {
                Vector3 delta = hand.position - lastPos;

                if (delta.magnitude < handDeadzone)
                {
                    handVelocity = Vector3.zero;
                }
                else
                {
                    handVelocity = Vector3.Lerp(
                        handVelocity,
                        delta / Time.deltaTime,
                        smoothing * Time.deltaTime
                    );
                }

                lastPos = hand.position;
            }
        }
        else
        {
            if (grabbing)
                ApplyMomentumJump(handVelocity);

            grabbing = false;
            handVelocity = Vector3.zero;
        }
    }

    void ApplyMomentumJump(Vector3 handVelocity)
    {
        Vector3 jump = -handVelocity * jumpMultiplier;
        jump.y += upwardBoost;

        if (jump.magnitude > maxJumpVelocity)
            jump = jump.normalized * maxJumpVelocity;

        velocity = jump;
    }

    bool IsClimbable(Vector3 position)
    {
        return Physics.CheckSphere(
            position,
            grabRadius,
            climbableLayer,
            QueryTriggerInteraction.Ignore
        );
    }
}
