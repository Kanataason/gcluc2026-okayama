using UnityEngine;
using UnityEngine.InputSystem;

//入力管理
public class PlayerInputManager : MonoBehaviour
{
    PlayerControls controls;

    Vector2 moveInput;
    bool jumpPressed;

    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Enable();

        //移動入力
        controls.Player.Move.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
        };

        controls.Player.Move.canceled += ctx =>
        {
            moveInput = Vector2.zero;
        };

        //ジャンプ入力
        controls.Player.Jump.performed += ctx =>
        {
            jumpPressed = true;
        };
    }

    void LateUpdate()
    {
        //1フレームでリセット
        jumpPressed = false;
    }

    public float GetHorizontal()
    {
        return moveInput.x;
    }

    public float GetVertical()
    {
        return moveInput.y;
    }

    public bool GetJump()
    {
        return jumpPressed;
    }
}