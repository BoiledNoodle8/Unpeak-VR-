/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#nullable enable

#if USING_XR_MANAGEMENT && (USING_XR_SDK_OCULUS || USING_XR_SDK_OPENXR) && !OVRPLUGIN_UNSUPPORTED_PLATFORM
#define USING_XR_SDK
#endif

using System.Collections.Generic;
using Oculus.Avatar2;
using UnityEngine;

#if !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class SampleSceneLocomotion : MonoBehaviour, IUIControllerInterface
{
    [SerializeField]
    [Tooltip("Controls the speed of movement")]
    public float movementSpeed = 1.0f;

    [SerializeField]
    [Tooltip("Invert the horizontal movement direction. Useful for avatar mirroring")]
    public bool invertHorizontalMovement = false;

    [SerializeField]
    [Tooltip("Invert the vertical movement direction. Useful for avatar mirroring")]
    public bool invertVerticalMovement = false;

    [SerializeField]
    [Tooltip("Controls the speed of rotation")]
    private float rotationSpeed = 60f;

#if UNITY_EDITOR
    [SerializeField]
    [Tooltip("Use keyboard buttons in Editor/PCVR to move avatars.")]
    private bool _useKeyboardDebug = true;

    [SerializeField]
    [Tooltip("Requires you to click your mouse in the game window activate keyboard debug.")]
    private bool _clickForKeyboardDebug = true;

    private bool _keyboardDebugActivated = false;

#endif

    void Update()
    {
        if (UIManager.IsPaused)
        {
            return;
        }
        float rotationAngleY;
        Vector3 movement;
#if USING_XR_SDK
        Vector2 inputVectorL = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector2 inputVectorR = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        float xMove = inputVectorL.x;
        float yMove = inputVectorR.y;
        float zMove = inputVectorL.y;
        float yRotate = inputVectorR.x;
        rotationAngleY = yRotate * rotationSpeed * Time.deltaTime;

        if (invertHorizontalMovement)
        {
            xMove = -xMove;
        }
        if (invertVerticalMovement)
        {
            zMove = -zMove;
        }

        movement = new Vector3(xMove, yMove, zMove) * movementSpeed * Time.deltaTime;
        transform.Translate(movement);
        transform.Rotate(0f, rotationAngleY, 0f);
#endif
#if UNITY_EDITOR
        if (_useKeyboardDebug)
        {
            if (_keyboardDebugActivated || !_clickForKeyboardDebug)
            {
                Vector3 inputDirection = GetMovementInput();
                yRotate = GetRotationInput();
                rotationAngleY = yRotate * rotationSpeed * Time.deltaTime;
                if (invertHorizontalMovement)
                {
                    inputDirection.x = -inputDirection.x;
                }
                if (invertVerticalMovement)
                {
                    inputDirection.z = -inputDirection.z;
                }

                UpdateRotationForEditor(ref inputDirection);

                movement = inputDirection * movementSpeed * Time.deltaTime;
                transform.Translate(movement);
                transform.Rotate(0f, rotationAngleY, 0f);

#if ENABLE_LEGACY_INPUT_MANAGER
                if (Input.GetKey(KeyCode.Escape) || Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
#else
                if (Keyboard.current?.escapeKey?.wasReleasedThisFrame == true
                    || Mouse.current?.leftButton?.wasReleasedThisFrame == true
                    || Mouse.current?.rightButton?.wasReleasedThisFrame == true)
#endif
                {
                    _keyboardDebugActivated = false;
                }
            }
#if ENABLE_LEGACY_INPUT_MANAGER
            else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
#else
            else if (Mouse.current?.leftButton?.wasReleasedThisFrame == true
                || Mouse.current?.rightButton?.wasReleasedThisFrame == true)
#endif
            {
                _keyboardDebugActivated = true;
            }
        }
#endif
    }

#if UNITY_EDITOR
    private Vector3 GetMovementInput()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        var xMove = Input.GetAxis("Horizontal");
        var yMove = Input.GetAxis("Mouse ScrollWheel") * 100f;
        var zMove = Input.GetAxis("Vertical");

        return new Vector3(xMove, yMove, zMove);
#else
        var result = Vector3.zero;
        if (Keyboard.current?.leftArrowKey?.isPressed == true || Keyboard.current?.aKey?.isPressed == true)
        {
            result.x = -1;
        }
        else if (Keyboard.current?.rightArrowKey?.isPressed == true || Keyboard.current?.dKey?.isPressed == true)
        {
            result.x = 1;
        }

        if (Keyboard.current?.upArrowKey?.isPressed == true || Keyboard.current?.wKey?.isPressed == true)
        {
            result.z = 1;
        }
        else if (Keyboard.current?.downArrowKey?.isPressed == true || Keyboard.current?.sKey?.isPressed == true)
        {
            result.z = -1;
        }

        if (Mouse.current?.scroll?.IsActuated() == true)
        {
            result.y = Mouse.current!.scroll!.value.y * 0.1f;
        }

        return result;
#endif
    }

    private float GetRotationInput()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetAxis("Mouse X");
#else
        if (Mouse.current?.delta?.IsActuated() == true)
        {
            return Mouse.current!.delta!.x.value * 0.5f;
        }

        return 0;
#endif
    }

    // If SampleXplatformObjectPlacement is present and is applying a rotation offset to the Camera,
    // the movement of the locomotion script will then be out of sync with the Camera.
    // This function can be used to adjust that by applying the same rotation to the input vector.
    private void UpdateRotationForEditor(ref Vector3 inputDirection)
    {
        if (OvrAvatarUtility.IsHeadsetActive())
        {
            return;
        }
        SampleXplatformObjectPlacement placement = FindObjectOfType<SampleXplatformObjectPlacement>();
        if (placement)
        {
            if (placement.gameObject.GetComponent<Camera>())
            {
                Quaternion rotation = Quaternion.Euler(placement.GetXPlatformRotation());
                inputDirection = rotation * inputDirection;
            }
        }
    }
#endif // UNITY_EDITOR

#if USING_XR_SDK
    public List<UIInputControllerButton> GetControlSchema()
    {
        var primaryAxis2D = new UIInputControllerButton
        {
            axis2d = OVRInput.Axis2D.PrimaryThumbstick,
            controller = OVRInput.Controller.All,
            description = "Move in XZ plane",
            scope = "SampleSceneLocomotion"
        };
        var secondaryAxis2D = new UIInputControllerButton
        {
            axis2d = OVRInput.Axis2D.SecondaryThumbstick,
            controller = OVRInput.Controller.All,
            description = "Move in and Rotate around Y axis",
            scope = "SampleSceneLocomotion"
        };
        var buttons = new List<UIInputControllerButton>
        {
            primaryAxis2D,
            secondaryAxis2D,
        };
        return buttons;
    }
#endif
}
