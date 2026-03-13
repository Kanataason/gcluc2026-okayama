using UnityEngine;

// プレイヤー移動管理クラス
public class PlayerMoveManager : CharaBase
{
    PlayerInputManager c_PlayerInput;

    //移動速度
    const float M_Speed = 8f;

    //ジャンプ
    const float J_Power = 7f;
    const float GRAVITY = -25f;

    //前後移動範囲
    const float g_Min = -6f;
    const float g_Max = 4f;

    //ジャンプ速度
    float j_Velocity = 0f;

    //地面にいるか
    bool is_Ground = true;

    //地面の高さ（自動取得）
    float groundY;

    public override void Start()
    {
        base.Start();

        c_PlayerInput = GetComponent<PlayerInputManager>();

        //現在の高さを地面として保存
        groundY = transform.position.y;
    }

    public virtual void MoveUpdate()
    {
        Move();
        Jump();
        UpdateAnimation();
    }

    //移動処理
    void Move()
    {
        CheckGround(g_Min, g_Max);

        if (GetIsAttackFlag()) return;

        float h = c_PlayerInput.GetHorizontal();
        float v = c_PlayerInput.GetVertical();

        Vector3 move = new Vector3(h, v, 0);

        transform.Translate(move * M_Speed * Time.deltaTime);

        //向き変更
        Vector3 scale = transform.localScale;

        if (h < 0)
        {
            scale.x = -Mathf.Abs(scale.x);
        }

        if (h > 0)
        {
            scale.x = Mathf.Abs(scale.x);
        }

        transform.localScale = scale;
    }

    //ジャンプ処理
    void Jump()
    {
        //ジャンプ開始
        if (c_PlayerInput.GetJump() && is_Ground)
        {
            is_Ground = false;
            j_Velocity = J_Power;

            if (a_Animator != null)
            {
                a_Animator.SetTrigger("Jump");
            }
        }

        //空中処理
        if (!is_Ground)
        {
            j_Velocity += GRAVITY * Time.deltaTime;

            Vector3 pos = transform.position;
            pos.y += j_Velocity * Time.deltaTime;

            transform.position = pos;

            //着地
            if (pos.y <= groundY)
            {
                pos.y = groundY;
                transform.position = pos;

                is_Ground = true;
                j_Velocity = 0;
            }
        }
    }

    //アニメーション更新
    void UpdateAnimation()
    {
        float h = Mathf.Abs(c_PlayerInput.GetHorizontal());
        float v = Mathf.Abs(c_PlayerInput.GetVertical());

        float speed = h + v;

        if (a_Animator != null)
        {
            a_Animator.SetFloat("MoveSpeed", speed);
            a_Animator.SetBool("Ground", is_Ground);
        }
    }

    //地面判定取得
    public bool IsGround()
    {
        return is_Ground;
    }
}