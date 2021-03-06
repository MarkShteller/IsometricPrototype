﻿using System;
using UnityEngine;

public static class ControllerInputDevice
{
    private static bool isLeftTriggerPressed = false;
    private static bool isRightTriggerPressed = false;
    private static bool isDashPressed = false;
    private static bool isSpecialPressed = false;
    private static bool isConfirmPressed = false;
    private static bool isChargePressed = false;

    public static bool GetLeftTriggerDown()
    {
        if (Input.GetAxis("LeftTrigger") == 1)// && !isLeftTriggerPressed)
        {
            isLeftTriggerPressed = true;
            return true;
        }

        if (Input.GetAxis("LeftTrigger") == 0)
            isLeftTriggerPressed = false;

        return false;
    }

    public static bool GetLeftTriggerUp()
    {
        if (isLeftTriggerPressed && Input.GetAxis("LeftTrigger") == 0)
        {
            isLeftTriggerPressed = false;
            return true;
        }
        return false;
    }

    public static bool GetRightTriggerDown()
    {
        if (Input.GetAxis("RightTrigger") == 1 && !isRightTriggerPressed)
        {
            isRightTriggerPressed = true;
            return true;
        }

        if (Input.GetAxis("RightTrigger") == 0)
            isRightTriggerPressed = false;

        return false;
    }

    public static bool GetConfirmButtonDown()
    {
        if (Input.GetButtonDown("Submit") && !isConfirmPressed)
        {
            //Debug.Log("ConfirmButtonDown");
            isConfirmPressed = true;
            return true;
        }

        if (Input.GetButtonDown("Submit"))
            isConfirmPressed = false;

        return false;
    }

    public static bool GetDashButtonDown()
    {
        if (Input.GetAxis("Dash") == 1 && !isDashPressed)
        {
            isDashPressed = true;
            return true;
        }

        if (Input.GetAxis("Dash") == 0)
            isDashPressed = false;

        return false;
    }

    public static bool GetSpecialButtonDown()
    {
        if (Input.GetAxis("Special") == 1 && !isSpecialPressed)
        {
            isSpecialPressed = true;
            return true;
        }

        if (Input.GetAxis("Special") == 0)
            isSpecialPressed = false;

        return false;
    }

    public static bool GetChargeButtonDown()
    {
        if (Input.GetAxis("Charge") == 1)
        {
            isChargePressed = true;
            return true;
        }

        if (Input.GetAxis("Charge") == 0)
            isChargePressed = false;

        return false;
    }

    public static bool GetHeavyButtonDown()
    {
        if (Input.GetAxis("Charge") == 1 && !isChargePressed)
        {
            isChargePressed = true;
            return true;
        }

        if (Input.GetAxis("Charge") == 0)
            isChargePressed = false;

        return false;
    }
}
