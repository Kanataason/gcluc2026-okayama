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

    // 地面Y位置
    float f_GroundY;

    // ジャンプの高さ
    float f_JumpHeight;

    // ジャンプ速度
    float f_JumpVelocity;

    // 地面にいるか
    bool b_IsGround = true;

    // ジャンプ中か
    bool b_IsJumping = false;

    // Animator パラメータ
    static readonly int MOVE_HASH = Animator.StringToHash("Move");
    static readonly int JUMP_HASH = Animator.StringToHash("Jump");
    static readonly int GROUND_HASH = Animator.StringToHash("Ground");

    int m_AnimaHashName;

    public void Init()
    {
        
        NextFrame.OneFrame(this, () => { a_Animator = c_PlayerManager.GetAnimator(); });

        transform.position = new Vector3(-12, -4, 0);
        // 初期スケール保存
        v_DefaultScale = transform.localScale;

        // 初期地面位置保存
        f_GroundY = transform.position.y;

        // 初期ジャンプ高さ
        f_JumpHeight = 0f;

        // 最初の位置を反映
        ApplyPosition();
        CacheComponents();
    }
    private void CacheComponents()
    {
        // 必要なコンポーネント取得
        c_PlayerInputManager = GetComponent<PlayerInputManager>();
        c_PlayerManager = GetComponent<PlayerManager>();
    }
    // 影用の地面Y位置を返す
    public float GetShadowGroundY()
    {
        return f_GroundY;
    }
    public void SetShadowGroundY(float value)
    {
        f_GroundY = value;
    }

    // 待機中更新
    public void IdleUpdate()
    {
        UpdateGroundMove();
        JumpUpdate();
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

        SetGround(false);
        SetJumpFlag(true);

        // ジャンプ高さを初期化
        f_JumpHeight = 0f;

        // ジャンプ初速
        f_JumpVelocity = JUMP_POWER;

        // ジャンプアニメーション再生
        if (a_Animator != null)
        {
            a_Animator.SetTrigger(JUMP_HASH);
            a_Animator.SetBool(GROUND_HASH, false);
            m_AnimaHashName = JUMP_HASH;
        }
    }

    // 移動処理
    void Move()
    {
        if (c_PlayerInputManager == null) return;
        if (c_PlayerManager == null) return;

        // 攻撃中は移動しない
        if (c_PlayerManager.GetIsAttackFlag()) return;

        float horizontal = c_PlayerInputManager.GetHorizontal();
        float vertical = c_PlayerInputManager.GetVertical();

        // 左右移動
        transform.Translate(new Vector3(horizontal, 0f, 0f) * MOVE_SPEED * Time.deltaTime);

        // 地面移動
        UpdateGroundMove();

        // 向き変更
        UpdateDirection(horizontal);

        // 最終位置反映
        ApplyPosition();
    }

    // 地面上の移動更新
    void UpdateGroundMove()
    {
        if (c_PlayerInputManager == null) return;
        if (TatuGameManager.Instance == null) return;

        float vertical = c_PlayerInputManager.GetVertical();

        // 地面Yだけ更新
        f_GroundY += vertical * MOVE_SPEED * Time.deltaTime;

        // 地面の移動範囲を制限
        f_GroundY = Mathf.Clamp(
            f_GroundY,
            TatuGameManager.Instance.m_StageScaleMinY,
            TatuGameManager.Instance.m_StageScaleMaxY
        );
    }

    // ジャンプ更新
    void JumpUpdate()
    {
        if (!b_IsJumping)
        {
            ApplyPosition();
            return;
        }

        // 重力加算
        f_JumpVelocity += GRAVITY * Time.deltaTime;

        // ジャンプ高さ更新
        f_JumpHeight += f_JumpVelocity * Time.deltaTime;

        // 着地判定
        if (f_JumpHeight <= 0f)
        {
            f_JumpHeight = 0f;
            f_JumpVelocity = 0f;
            SetJumpFlag(false);
            SetGround(true);
        }

        // 最終位置反映
        ApplyPosition();
    }

    // 最終位置反映
    void ApplyPosition()
    {
        Vector3 position = transform.position;
        position.y = f_GroundY + f_JumpHeight;
        transform.position = position;
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

    // 入力から移動アニメ更新
    void UpdateMoveAnimationFromInput()
    {
        if (c_PlayerInputManager == null) return;

        float horizontal = Mathf.Abs(c_PlayerInputManager.GetHorizontal());
        float vertical = Mathf.Abs(c_PlayerInputManager.GetVertical());

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
        m_AnimaHashName = MOVE_HASH;
        
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

    // 現在のジャンプ速度取得
    public float GetJumpParame()
    {
        return f_JumpHeight;
    }
    //現在のジャンプ速度取得
    public float GetVelocity()
    {
        return f_JumpVelocity;
    }

    // ジャンプ高さ設定
    public void SetJump(float value)
    {
        f_JumpHeight = value;
    }
    // ジャンプ速度設定
    public void SetVelocity(float value)
    {
        f_JumpVelocity = value;
    }

    //グラウンドフラグの設定
    public void SetGround(bool IsGround)
    {
        b_IsGround = IsGround;
    }

    // ジャンプ中フラグ設定
    public void SetJumpFlag(bool isJump)
    {
        b_IsJumping = isJump;
    }
    //アニメーションのハッシュの名前を取得
    public int GetAnimaHashName()
    {
        return m_AnimaHashName;
    }
    //アニメーションのハッシュをセット
    public void SetAnimaHashName(int HashName)
    {
        m_AnimaHashName = HashName;
        c_PlayerManager.SetAnimaType(m_AnimaHashName);
    }
}