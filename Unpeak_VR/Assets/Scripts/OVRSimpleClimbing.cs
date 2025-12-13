using UnityEngine;

public class OVRSimpleClimbing : MonoBehaviour
{
    [Header("References")]
    public Transform playerRoot;          // THIS moves
    public CharacterController controller;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Climb Detection")]
    public LayerMask climbableLayer;
    public float grabRadius = 0.15f;

    [Header("Movement")]
    public float climbStrength = 1.0f;
    public float smoothing = 10f;
    public float deadzone = 0.0025f;

    [Header("Momentum Jump")]
    public float jumpMultiplier = 1.2f;
    public float upwardBoost = 0.6f;
    public float maxJumpVelocity = 4f;

    private bool leftGrab;
    private bool rightGrab;

    private Vector3 lastLeftPos;
    private Vector3 lastRightPos;

    private Vector3 leftVel;
    private Vector3 rightVel;

    private Vector3 smoothedMove;
    private Vector3 externalVelocity;

    void FixedUpdate()
    {
        Vector3 climbMove = Vector3.zero;

        if (leftGrab)
            climbMove += -leftVel * Time.fixedDeltaTime;

        if (rightGrab)
            climbMove += -rightVel * Time.fixedDeltaTime;

        if (leftGrab && rightGrab)
            climbMove *= 0.5f;

        if (climbMove.sqrMagnitude > 0f)
        {
            smoothedMove = Vector3.Lerp(
                smoothedMove,
                climbMove * climbStrength,
                smoothing * Time.fixedDeltaTime
            );

            controller.Move(smoothedMove);
            externalVelocity = Vector3.zero;
        }
        else
        {
            smoothedMove = Vector3.zero;

            if (!controller.isGrounded)
            {
                externalVelocity.y += Physics.gravity.y * Time.fixedDeltaTime;
                controller.Move(externalVelocity * Time.fixedDeltaTime);
            }
            else
            {
                externalVelocity = Vector3.zero;
            }
        }
    }

    void Update()
    {
        UpdateHand(
            OVRInput.Controller.LTouch,
            OVRInput.Button.PrimaryHandTrigger,
            leftHand,
            ref leftGrab,
            ref lastLeftPos,
            ref leftVel
        );

        UpdateHand(
            OVRInput.Controller.RTouch,
            OVRInput.Button.PrimaryHandTrigger,
            rightHand,
            ref rightGrab,
            ref lastRightPos,
            ref rightVel
        );
    }

    void UpdateHand(
        OVRInput.Controller controller,
        OVRInput.Button grabButton,
        Transform hand,
        ref bool grabbing,
        ref Vector3 lastPos,
        ref Vector3 velocity)
    {
        bool grip = OVRInput.Get(grabButton, controller);

        if (grip && IsClimbable(hand.position))
        {
            if (!grabbing)
            {
                grabbing = true;
                lastPos = hand.position;
                velocity = Vector3.zero;
                externalVelocity = Vector3.zero;
            }
            else
            {
                Vector3 delta = hand.position - lastPos;

                if (delta.magnitude < deadzone)
                    velocity = Vector3.zero;
                else
                    velocity = Vector3.Lerp(
                        velocity,
                        delta / Time.deltaTime,
                        smoothing * Time.deltaTime
                    );

                lastPos = hand.position;
            }
        }
        else
        {
            if (grabbing)
                ApplyMomentumJump(velocity);

            grabbing = false;
            velocity = Vector3.zero;
        }
    }

    void ApplyMomentumJump(Vector3 handVelocity)
    {
        Vector3 jump = -handVelocity * jumpMultiplier;
        jump.y += upwardBoost;

        if (jump.magnitude > maxJumpVelocity)
            jump = jump.normalized * maxJumpVelocity;

        externalVelocity = jump;
    }

    bool IsClimbable(Vector3 pos)
    {
        return Physics.CheckSphere(pos, grabRadius, climbableLayer, QueryTriggerInteraction.Ignore);
    }
}
