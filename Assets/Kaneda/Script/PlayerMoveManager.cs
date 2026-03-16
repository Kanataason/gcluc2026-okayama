using UnityEngine;

// プレイヤー移動管理クラス
// 移動処理とジャンプ処理を担当する
public class PlayerMoveManager : CharaBase
{
    // 入力管理クラス
    PlayerInputManager c_PlayerInputManager;

    // 移動速度
    const float MOVE_SPEED = 8f;

    // 地面移動範囲
    const float GROUND_MIN_Y = -6f;
    const float GROUND_MAX_Y = -4f;

    // ジャンプ初速
    const float JUMP_POWER = 7f;

    // 重力
    const float GRAVITY = -25f;

    // 移動アニメーション値の最大
    const float MOVE_ANIM_MAX = 1f;

    // ジャンプ速度
    float f_JumpVelocity;

    // ジャンプ開始位置
    float f_JumpStartY;

    // 地面にいるか
    bool b_IsGround = true;

    // ジャンプ中か
    bool b_IsJumping = false;

    // 向き
    int m_CurrentDirection = 1;

    // アニメーション用ハッシュ
    static readonly int MOVE_HASH = Animator.StringToHash("Move");

    // ジャンプステート名
    const string JUMP_STATE_NAME = "Jump";

    public override void Start()
    {
        base.Start();

        c_PlayerInputManager = GetComponent<PlayerInputManager>();
    }

    // 移動更新
    public void MoveUpdate()
    {
        Move();
        JumpUpdate();
        UpdateMoveAnimation();
    }

    // 移動処理
    void Move()
    {
        if (GetIsAttackFlag()) return;

        float horizontal = c_PlayerInputManager.GetHorizontal();
        float vertical = c_PlayerInputManager.GetVertical();

        Vector3 move;

        // ジャンプ中は上下移動させない
        if (b_IsJumping)
        {
            move = new Vector3(horizontal, 0f, 0f);
        }
        else
        {
            move = new Vector3(horizontal, vertical, 0f);
        }

        transform.Translate(move * MOVE_SPEED * Time.deltaTime);

        // 地面にいる時だけ歩ける範囲を制限
        if (b_IsGround)
        {
            Vector3 position = transform.position;
            position.y = Mathf.Clamp(position.y, GROUND_MIN_Y, GROUND_MAX_Y);
            transform.position = position;
        }

        UpdateDirection(horizontal);
    }

    // 向き変更
    void UpdateDirection(float horizontal)
    {
        if (horizontal > 0f)
        {
            m_CurrentDirection = 1;
        }
        else if (horizontal < 0f)
        {
            m_CurrentDirection = -1;
        }

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * m_CurrentDirection;
        transform.localScale = scale;
    }

    // ジャンプ開始
    public void JumpEnter()
    {
        if (!b_IsGround) return;

        b_IsGround = false;
        b_IsJumping = true;
        f_JumpVelocity = JUMP_POWER;
        f_JumpStartY = transform.position.y;

        if (a_Animator != null)
        {
            a_Animator.Play(JUMP_STATE_NAME, 0, 0f);
        }
    }

    // ジャンプ更新
    void JumpUpdate()
    {
        if (!b_IsJumping) return;

        f_JumpVelocity += GRAVITY * Time.deltaTime;

        Vector3 position = transform.position;
        position.y += f_JumpVelocity * Time.deltaTime;
        transform.position = position;

        // 着地
        if (position.y <= f_JumpStartY)
        {
            position.y = f_JumpStartY;
            transform.position = position;

            b_IsGround = true;
            b_IsJumping = false;
            f_JumpVelocity = 0f;
        }
    }

    // 移動アニメーション更新
    void UpdateMoveAnimation()
    {
        if (a_Animator == null) return;
        if (GetIsAttackFlag()) return;
        if (b_IsJumping) return;

        float horizontal = Mathf.Abs(c_PlayerInputManager.GetHorizontal());
        float vertical = Mathf.Abs(c_PlayerInputManager.GetVertical());

        float moveValue = horizontal + vertical;

        if (moveValue > MOVE_ANIM_MAX)
        {
            moveValue = MOVE_ANIM_MAX;
        }

        a_Animator.SetFloat(MOVE_HASH, moveValue);
    }

    // 地面判定取得
    public bool GetIsGround()
    {
        return b_IsGround;
    }

    // ジャンプ中判定取得
    public bool GetIsJumping()
    {
        return b_IsJumping;
    }

    // 向き取得
    public int GetCurrentDirection()
    {
        return m_CurrentDirection;
    }
}