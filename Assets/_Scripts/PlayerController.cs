using UnityEngine;

public class PlayerController : MonoBehaviour
{
    static int settingUp = 1;
    string horizontal;
    KeyCode jump, attack1, attack2;

    public static int SettingUp => settingUp;

    public void Setup()
    {
        jump = settingUp == 1 ? KeyCode.Joystick1Button0 : KeyCode.Joystick2Button0;
        attack1 = settingUp == 1 ? KeyCode.Joystick1Button1 : KeyCode.Joystick2Button1;
        attack2 = settingUp == 1 ? KeyCode.Joystick1Button2 : KeyCode.Joystick2Button2;
        horizontal = "X" + settingUp;
        settingUp++;
    }

    public static void Reset()
    {
        settingUp = 1;
    }

    public bool GetLeft()
    {
        return Input.GetAxis(horizontal) <= -0.5f;
    }

    public bool GetRight()
    {
        return Input.GetAxis(horizontal) >= 0.5f;
    }

    public bool GetJump()
    {
        return Input.GetKey(jump);
    }

    public bool GetJumpDown()
    {
        return Input.GetKeyDown(jump);
    }

    public bool GetAttackDown()
    {
        return Input.GetKeyDown(attack1) || Input.GetKeyDown(attack2);
    }
}
