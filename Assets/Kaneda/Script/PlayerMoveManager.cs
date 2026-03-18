using UnityEngine;

// プレイヤー移動管理クラス
// 移動処理・ジャンプ処理・移動アニメーションを担当する
public class PlayerMoveManager : MonoBehaviour
{
    // 入力管理
    PlayerInputManager c_PlayerInputManager;

    // プレイヤー管理
    PlayerManager c_PlayerManager;

    // Animator
    Animator a_Animator;

    // 通常スケール
    Vector3 v_DefaultScale;

    // 移動速度
    const float MOVE_SPEED = 8f;

    // ジャンプ初速
    const float JUMP_POWER = 10f;

    // 重力
    const float GRAVITY = -25f;

    // Moveパラメータ最大値
    const float MOVE_ANIM_MAX = 1f;

    // ジャンプ速度
    float f_JumpVelocity;

    // ジャンプ開始位置
    float f_JumpStartY;

    // 地面にいるか
    bool b_IsGround = true;

    // ジャンプ中か
    bool b_IsJumping = false;

    // Animator パラメータ
    static readonly int MOVE_HASH = Animator.StringToHash("Move");
    static readonly int JUMP_HASH = Animator.StringToHash("Jump");
    static readonly int GROUND_HASH = Animator.StringToHash("Ground");

    void Start()
    {
        // 必要なコンポーネント取得
        c_PlayerInputManager = GetComponent<PlayerInputManager>();
        c_PlayerManager = GetComponent<PlayerManager>();
        a_Animator = GetComponent<Animator>();

        // 初期スケール保存
        v_DefaultScale = transform.localScale;

        // 取得確認
        Debug.Log($"InputManager : {c_PlayerInputManager}");
        Debug.Log($"PlayerManager : {c_PlayerManager}");
        Debug.Log($"Animator : {a_Animator}");
    }

    // 影用の地面Y位置を返す
    public float GetShadowGroundY()
    {
        // ジャンプ中はジャンプ開始位置を返す
        if (b_IsJumping)
        {
            return f_JumpStartY;
        }

        // 地面にいる時は現在のYを返す
        return transform.position.y;
    }

    // 待機中更新
    public void IdleUpdate()
    {
        UpdateMoveAnimation(0f);
        UpdateGroundAnimation();
    }

    // 移動更新
    public void MoveUpdate()
    {
        Move();
        JumpUpdate();
        UpdateMoveAnimationFromInput();
        UpdateGroundAnimation();
    }

    // ジャンプ開始
    public void JumpEnter()
    {
        // 地面にいないならジャンプしない
        if (!b_IsGround) return;

        b_IsGround = false;
        b_IsJumping = true;

        // 今いる位置を着地点にする
        f_JumpStartY = transform.position.y;

        // ジャンプ初速セット
        f_JumpVelocity = JUMP_POWER;

        // ジャンプアニメーション開始
        if (a_Animator != null)
        {
            a_Animator.SetTrigger(JUMP_HASH);
            a_Animator.SetBool(GROUND_HASH, false);
        }
    }

    // 移動処理
    void Move()
    {
        // null対策
        if (c_PlayerManager == null) return;
        if (c_PlayerInputManager == null) return;

        // 攻撃中は移動しない
        if (c_PlayerManager.GetIsAttackFlag()) return;

        float horizontal = c_PlayerInputManager.GetHorizontal();
        float vertical = c_PlayerInputManager.GetVertical();

        Vector3 move;

        // ジャンプ中は上下移動を切る
        // 今の仕様ではY軸を地面移動とジャンプで共用しているため
        // 空中中は左右移動だけにする
        if (b_IsJumping)
        {
            move = new Vector3(horizontal, 0f, 0f);
        }
        else
        {
            move = new Vector3(horizontal, vertical, 0f);
        }

        transform.Translate(move * MOVE_SPEED * Time.deltaTime);

        // 地面にいる時だけY範囲制限
        if (b_IsGround)
        {
            Vector3 position = transform.position;

            // ステージ管理がある時だけ制限する
            if (TatuGameManager.Instance != null)
            {
                position.y = Mathf.Clamp(
                    position.y,
                    TatuGameManager.Instance.m_StageScaleMinY,
                    TatuGameManager.Instance.m_StageScaleMaxY
                );

                transform.position = position;
            }
        }

        // 向き変更
        UpdateDirection(horizontal);
    }

    // 向き更新
    void UpdateDirection(float horizontal)
    {
        if (c_PlayerManager == null) return;

        if (horizontal > 0f)
        {
            c_PlayerManager.CurrentDirection = 1;
        }
        else if (horizontal < 0f)
        {
            c_PlayerManager.CurrentDirection = -1;
        }

        Vector3 scale = transform.localScale;

        scale.x = Mathf.Abs(v_DefaultScale.x) * c_PlayerManager.CurrentDirection;
        scale.y = v_DefaultScale.y;
        scale.z = v_DefaultScale.z;

        transform.localScale = scale;
    }

    // ジャンプ更新
    void JumpUpdate()
    {
        if (!b_IsJumping) return;

        // 重力計算
        f_JumpVelocity += GRAVITY * Time.deltaTime;

        Vector3 position = transform.position;
        position.y += f_JumpVelocity * Time.deltaTime;
        transform.position = position;

        // 着地判定
        if (position.y <= f_JumpStartY)
        {
            position.y = f_JumpStartY;
            transform.position = position;

            b_IsGround = true;
            b_IsJumping = false;
            f_JumpVelocity = 0f;
        }
    }

    // 入力から移動アニメ更新
    void UpdateMoveAnimationFromInput()
    {
        if (c_PlayerInputManager == null) return;

        float horizontal = Mathf.Abs(c_PlayerInputManager.GetHorizontal());
        float vertical = Mathf.Abs(c_PlayerInputManager.GetVertical());

        // ジャンプ中は上下移動を見ない
        if (b_IsJumping)
        {
            vertical = 0f;
        }

        float moveValue = horizontal + vertical;

        if (moveValue > MOVE_ANIM_MAX)
        {
            moveValue = MOVE_ANIM_MAX;
        }

        UpdateMoveAnimation(moveValue);
    }

    // Moveパラメータ更新
    void UpdateMoveAnimation(float moveValue)
    {
        if (a_Animator == null) return;
        if (c_PlayerManager == null) return;
        if (c_PlayerManager.GetIsAttackFlag()) return;

        a_Animator.SetFloat(MOVE_HASH, moveValue);
    }

    // Groundパラメータ更新
    void UpdateGroundAnimation()
    {
        if (a_Animator == null) return;

        a_Animator.SetBool(GROUND_HASH, b_IsGround);
    }

    // 地面判定取得
    public bool GetIsGround()
    {
        return b_IsGround;
    }

    // ジャンプ判定取得
    public bool GetIsJumping()
    {
        return b_IsJumping;
    }

    // 現在のジャンプ速度を取得
    public float JumpParame()
    {
        return f_JumpVelocity;
    }

    // ジャンプ速度を設定
    public void SetJump(float value)
    {
        f_JumpVelocity = value;
    }

    // ジャンプ中フラグを設定
    public void SetJumpFlag(bool isJump)
    {
        b_IsJumping = isJump;
    }
}