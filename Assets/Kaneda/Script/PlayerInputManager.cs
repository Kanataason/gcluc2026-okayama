using UnityEngine;
using UnityEngine.InputSystem;

// プレイヤー入力管理クラス
// Input System の入力取得だけを担当する
public class PlayerInputManager : MonoBehaviour
{
    // Input Actions
    PlayerControls c_PlayerControls;

    // 移動入力
    Vector2 v_MoveInput;

    // ジャンプ入力バッファ時間
    const float JUMP_BUFFER_TIME = 0.15f;

    // 攻撃入力バッファ時間
    const float ATTACK_BUFFER_TIME = 0.15f;

    // ジャンプ入力タイマー
    float f_JumpBufferTimer;

    // 攻撃入力タイマー
    float f_AttackBufferTimer;

    void Awake()
    {
        c_PlayerControls = new PlayerControls();

        // 移動入力
        c_PlayerControls.Player.Move.performed += context =>
        {
            v_MoveInput = context.ReadValue<Vector2>();
        };

        // 移動解除
        c_PlayerControls.Player.Move.canceled += context =>
        {
            v_MoveInput = Vector2.zero;
        };

        // ジャンプ入力
        c_PlayerControls.Player.Jump.performed += context =>
        {
            f_JumpBufferTimer = JUMP_BUFFER_TIME;
        };

        // 攻撃入力
        c_PlayerControls.Player.Attack.performed += context =>
        {
            f_AttackBufferTimer = ATTACK_BUFFER_TIME;
        };
    }

    void OnEnable()
    {
        c_PlayerControls.Enable();
    }

    void OnDisable()
    {
        c_PlayerControls.Disable();
    }

    void Update()
    {
        // ジャンプ入力バッファ減少
        if (f_JumpBufferTimer > 0f)
        {
            f_JumpBufferTimer -= Time.deltaTime;
        }

        // 攻撃入力バッファ減少
        if (f_AttackBufferTimer > 0f)
        {
            f_AttackBufferTimer -= Time.deltaTime;
        }
    }

    // 横入力取得
    public float GetHorizontal()
    {
        return v_MoveInput.x;
    }

    // 縦入力取得
    public float GetVertical()
    {
        return v_MoveInput.y;
    }

    // ジャンプ入力取得
    public bool GetJump()
    {
        if (f_JumpBufferTimer > 0f)
        {
            f_JumpBufferTimer = 0f;
            return true;
        }

        return false;
    }

    // 攻撃入力取得
    public bool GetAttack()
    {
        if (f_AttackBufferTimer > 0f)
        {
            f_AttackBufferTimer = 0f;
            return true;
        }

        return false;
    }
}