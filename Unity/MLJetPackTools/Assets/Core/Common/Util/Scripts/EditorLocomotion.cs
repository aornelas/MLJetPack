//-----------------------------------------------------------------------
// <copyright file="EditorLocomotion.cs" company="Across Realities">
//
// Copyright 2018 Across Realities LLC. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorLocomotion : MonoBehaviour
{
    public float moveSpeedMultiplier = 3.0f;

    // Set this to modify position using WASD + QZ controls
    [SerializeField]
    public Transform objectToPosition;

    // Set this to modify inside-out orientation using arrow keys
    [SerializeField]
    Transform objectToOrient;

    // Set this to true to only work when shift is held down.
    // Example: put this script on a camera and a controller,
    // set shift on the controller and now they can be moved
    // independently
    //
    public bool usingShift = false;
    public bool usingAlt = false;

    public bool isModifyingCamera = false;

    void Awake()
    {
#if !UNITY_EDITOR
        Destroy(this);
        return;
#endif

        if (objectToPosition == null)
        {
            if (isModifyingCamera)
            {
                if (GetComponentInChildren<Camera>() != null)
                {
                    objectToPosition = GetComponentInChildren<Camera>().transform;
                }
                else if (Camera.main != null)
                {
                    objectToPosition = Camera.main.transform;
                }
                else
                {
                    Debug.LogError(name + ": Need to assign cameraRoot to the EditorLocomotion script or attach it to a camera.");
                    return;
                }

            }
            else
            {
                objectToPosition = transform;
            }
        }

        if (objectToOrient == null)
        {
            objectToOrient = objectToPosition;
        }

    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (!usingShift)
            {
                return;
            }
        }
        else if (usingShift)
        {
            return;
        }

        if (Input.GetKey(KeyCode.LeftAlt)) {
            if (!usingAlt) {
                return;
            }

        } else if (usingAlt) {
            return;
        }

        Vector3 localPosition = objectToOrient.localPosition;
        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.A))
        {
            direction = -objectToOrient.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction = objectToOrient.right;
        }
        if (Input.GetKey(KeyCode.W))
        {
            direction = objectToOrient.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction = -objectToOrient.forward;
        }
        direction.y = 0;
        if (Input.GetKey(KeyCode.Q))
        {
            direction = Vector3.up;
        }
        if (Input.GetKey(KeyCode.Z))
        {
            direction = -Vector3.up;
        }
        direction = direction.normalized;

        localPosition += direction * .01f * moveSpeedMultiplier;
        objectToOrient.localPosition = localPosition;


        Vector3 localAngles = objectToPosition.localEulerAngles;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            localAngles.y -= 1;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            localAngles.y += 1;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            localAngles.x -= 1;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            localAngles.x += 1;
        }

        if (localAngles.x > 90 && localAngles.x < 269)
        {
            localAngles.x = 90;
        }
        else if (localAngles.x < -90)
        {
            localAngles.x = -90;
        }
        objectToPosition.localEulerAngles = localAngles;
    }
}
