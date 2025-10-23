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

using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Oculus.Avatar2
{
    public class SampleAvatarHeadsetInputSimulator : OvrAvatarInputTrackingDelegate
    {
        private const string LOG_SCOPE = "SampleAvatarHeadsetInputSimulator";
        private const float MOVEMENT_SPEED = 1.4f;
        private const float ROTATION_SPEED = 60.0f;

        private enum SimulatedHeadsetEvents
        {
            MoveForward,
            MoveBackward,
            MoveLeft,
            MoveRight,
            MoveUp,
            MoveDown,
            TurnLeft,
            TurnRight,
            LookUp,
            LookDown,
            Reset,
        }

        private struct HeadsetState
        {
            public Vector3 HeadsetPosition;
            public Quaternion HeadsetRotation;
        }

        private HeadsetState _currentState;
        private bool _isActive = true;

        private readonly Vector3 _positionOffset = new(0f, 1.5f, 0f);
        private readonly Quaternion _rotationOffset = Quaternion.Euler(0f, 180f, 0f);
        private const float RESET_DELAY = 1f;

        #region Keyboard Assignments
        private readonly Dictionary<SimulatedHeadsetEvents, string> _keyMappings = new Dictionary<SimulatedHeadsetEvents, string>
        {
            { SimulatedHeadsetEvents.MoveForward, "y" },
            { SimulatedHeadsetEvents.MoveBackward, "h" },
            { SimulatedHeadsetEvents.MoveLeft, "g" },
            { SimulatedHeadsetEvents.MoveRight, "j" },
            { SimulatedHeadsetEvents.MoveUp, "t" },
            { SimulatedHeadsetEvents.MoveDown, "u" },
            { SimulatedHeadsetEvents.TurnLeft, "k" },
            { SimulatedHeadsetEvents.TurnRight, ";" },
            { SimulatedHeadsetEvents.LookUp, "o" },
            { SimulatedHeadsetEvents.LookDown, "l" },
            { SimulatedHeadsetEvents.Reset, "r" },
        };
        #endregion

        private float _startResetTime;

        public SampleAvatarHeadsetInputSimulator()
        {
            ResetCurrentState();
        }

        private void ResetCurrentState()
        {
            _currentState.HeadsetPosition = Vector3.zero;
            _currentState.HeadsetRotation = Quaternion.identity;
        }

        private void EmulateHeadPositionWithKeyboardInput()
        {
            Vector3 position = _currentState.HeadsetPosition;
            Quaternion rotation = _currentState.HeadsetRotation;

            if (CheckHeadsetEvents(SimulatedHeadsetEvents.MoveForward)) // Forward
            {
                position.z += MOVEMENT_SPEED * Time.deltaTime;
            }

            if (CheckHeadsetEvents(SimulatedHeadsetEvents.MoveBackward)) // Backward
            {
                position.z -= MOVEMENT_SPEED * Time.deltaTime;
            }

            if (CheckHeadsetEvents(SimulatedHeadsetEvents.MoveLeft)) // Left
            {
                position.x -= MOVEMENT_SPEED * Time.deltaTime;
            }

            if (CheckHeadsetEvents(SimulatedHeadsetEvents.MoveRight)) // Right
            {
                position.x += MOVEMENT_SPEED * Time.deltaTime;
            }

            if (CheckHeadsetEvents(SimulatedHeadsetEvents.MoveUp)) // Up
            {
                position.y += MOVEMENT_SPEED * Time.deltaTime;
            }

            if (CheckHeadsetEvents(SimulatedHeadsetEvents.MoveDown)) // Down
            {
                position.y -= MOVEMENT_SPEED * Time.deltaTime;
            }

            if (CheckHeadsetEvents(SimulatedHeadsetEvents.TurnLeft)) // Turn left
            {
                rotation *= Quaternion.Euler(0, -ROTATION_SPEED * Time.deltaTime, 0);
            }

            if (CheckHeadsetEvents(SimulatedHeadsetEvents.TurnRight)) // Turn right
            {
                rotation *= Quaternion.Euler(0, ROTATION_SPEED * Time.deltaTime, 0);
            }

            if (CheckHeadsetEvents(SimulatedHeadsetEvents.LookUp)) // Look up
            {
                rotation *= Quaternion.Euler(-ROTATION_SPEED * Time.deltaTime, 0, 0);
            }

            if (CheckHeadsetEvents(SimulatedHeadsetEvents.LookDown)) // Look down
            {
                rotation *= Quaternion.Euler(ROTATION_SPEED * Time.deltaTime, 0, 0);
            }

            if (CheckHeadsetEvents(SimulatedHeadsetEvents.Reset))
            {
                ResetPosition();
            }

            _currentState.HeadsetPosition = position;
            _currentState.HeadsetRotation = rotation;
        }

        private bool CheckHeadsetEvents(SimulatedHeadsetEvents headsetEvent)
        {
            if (!_keyMappings.TryGetValue(headsetEvent, out var keyMapping))
            {
                return false;
            }

#if ENABLE_INPUT_SYSTEM
            var keyControl = Keyboard.current.FindKeyOnCurrentKeyboardLayout(keyMapping);
            if (keyControl == null)
            {
                return false;
            }

            return keyControl?.isPressed == true;
#else
            return Input.GetKey(keyMapping);
#endif
        }

        private void ResetPosition()
        {
            _isActive = false;
            _startResetTime = Time.time;
        }


        public override bool GetRawInputTrackingState(out OvrAvatarInputTrackingState inputTrackingState)
        {
            inputTrackingState = default;

            inputTrackingState.leftControllerActive = false;
            inputTrackingState.rightControllerActive = false;
            inputTrackingState.leftControllerVisible = false;
            inputTrackingState.rightControllerVisible = false;

            if (_isActive)
            {
                EmulateHeadPositionWithKeyboardInput();
            }
            else
            {
                if (Time.time - _startResetTime > RESET_DELAY)
                {
                    ResetCurrentState();
                    _isActive = true;
                }
                else
                {
                    inputTrackingState.headsetActive = false;
                    return false;
                }
            }


            inputTrackingState.headsetActive = true;
            inputTrackingState.headset.position = _currentState.HeadsetPosition + _positionOffset;
            inputTrackingState.headset.orientation = _currentState.HeadsetRotation * _rotationOffset;
            inputTrackingState.headset.scale = Vector3.one;

            return true;
        }
    }
}
