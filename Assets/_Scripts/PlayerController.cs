using UnityEngine;

public class PlayerController : MonoBehaviour
{
    static int settingUp = 1;
    string horizontal;
    KeyCode jump, attack;

    public static int SettingUp => settingUp;

    public void Setup()
    {
        jump = settingUp == 1 ? KeyCode.Joystick1Button0 : KeyCode.Joystick2Button0;
        attack = settingUp == 1 ? KeyCode.Joystick1Button2 : KeyCode.Joystick2Button2;
        horizontal = "X" + settingUp;
        settingUp++;
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
        return Input.GetKeyDown(attack);
    }
}
