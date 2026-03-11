using UnityEngine;

// プレイヤー移動管理クラス
// 入力情報を受け取りキャラクターを移動させる
public class PlayerMoveManager : CharaBase
{
    //入力管理クラス
    PlayerInputManager c_PlayerInput;

    //移動速度
    const float M_Speed = 8f;

    //ジャンプ初速
    const float J_Power = 6f;

    //重力
    const float GRAVITY = -20f;

    //地面移動範囲
    const float g_Min = -6f;
    const float g_Max = -4f;

    //ジャンプ速度
    float j_Velocity = 0f;

    //地面判定
    bool is_Ground = true;

    public override void Start()
    {
        base.Start();

        //入力スクリプト取得
        c_PlayerInput = GetComponent<PlayerInputManager>();
    }

    // PlayerManagerから呼ばれる移動更新
    public virtual void MoveUpdate()
    {
        Move();
        Jump();
        UpdateAnimation();
    }

    // プレイヤー移動処理
    void Move()
    {
        //移動範囲制限
        CheckGround(g_Min, g_Max);

        //攻撃中は移動しない
        if (GetIsAttackFlag()) return;

        //入力取得
        float h = c_PlayerInput.GetHorizontal();
        float v = c_PlayerInput.GetVertical();

        //ベルトスクロール移動
        Vector3 move = new Vector3(h, 0, v);

        //移動
        transform.Translate(move * M_Speed * Time.deltaTime);
    }

    // ジャンプ処理
    void Jump()
    {
        //ジャンプ開始
        if (c_PlayerInput.GetJump() && is_Ground)
        {
            is_Ground = false;

            //ジャンプ初速
            j_Velocity = J_Power;

            //ジャンプアニメーション
            if (a_Animator != null)
            {
                a_Animator.SetTrigger("Jump");
            }
        }

        //空中処理
        if (!is_Ground)
        {
            //重力
            j_Velocity += GRAVITY * Time.deltaTime;

            //ジャンプ移動
            transform.Translate(Vector3.up * j_Velocity * Time.deltaTime);

            //地面に戻る
            if (transform.position.y <= g_Min)
            {
                Vector3 pos = transform.position;
                pos.y = g_Min;
                transform.position = pos;

                is_Ground = true;
                j_Velocity = 0;
            }
        }
    }

    // アニメーション更新
    void UpdateAnimation()
    {
        float h = Mathf.Abs(c_PlayerInput.GetHorizontal());
        float v = Mathf.Abs(c_PlayerInput.GetVertical());

        float moveSpeed = h + v;

        if (a_Animator != null)
        {
            //Dashアニメーション
            a_Animator.SetFloat("MoveSpeed", moveSpeed);

            //地面状態
            a_Animator.SetBool("Ground", is_Ground);
        }
    }
}