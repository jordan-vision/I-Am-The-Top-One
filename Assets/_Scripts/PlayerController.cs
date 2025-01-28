using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] KeyCode left, right, jump, attack;

    public bool GetLeft()
    {
        return Input.GetKey(left);
    }

    public bool GetLeftDown()
    {
        return Input.GetKeyDown(left);
    }

    public bool GetRight()
    {
        return Input.GetKey(right);
    }

    public bool GetRightDown()
    {
        return Input.GetKeyDown(right);
    }

    public bool GetJump()
    {
        return Input.GetKey(jump);
    }

    public bool GetJumpDown()
    {
        return Input.GetKeyDown(jump);
    }
}
