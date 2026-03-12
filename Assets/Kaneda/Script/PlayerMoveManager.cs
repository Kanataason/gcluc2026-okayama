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
    const float J_Power = -7f;

    //重力
    const float GRAVITY = -25f;

    //地面の前後移動範囲
    const float g_Min = -6f;
    const float g_Max = -4f;

    //ジャンプ中の速度
    float j_Velocity = 0f;

    //地面にいるかどうか
    bool is_Ground = true;

    //地面の高さ
    const float GROUND_Y = 0f;

    public override void Start()
    {
        base.Start();

        //入力スクリプト取得
        c_PlayerInput = GetComponent<PlayerInputManager>();
    }

    //PlayerManagerから呼ばれる更新処理
    public virtual void MoveUpdate()
    {
        Move();             //移動処理
        Jump();             //ジャンプ処理
        UpdateAnimation();  //アニメーション更新
    }

    //移動処理
    void Move()
    {
        CheckGround(g_Min, g_Max);

        //攻撃中は移動しない
        if (GetIsAttackFlag()) return;

        //入力取得
        float h = c_PlayerInput.GetHorizontal();
        float v = c_PlayerInput.GetVertical();

        //移動ベクトル
        Vector3 move = new Vector3(h, v, 0);

        //移動
        transform.Translate(move * M_Speed * Time.deltaTime);

        // 向き変更処理
        Vector3 scale = transform.localScale;

        //左向き
        if (h < 0)
        {
            scale.x = -Mathf.Abs(scale.x);
        }

        //右向き
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
            //地面状態を解除
            is_Ground = false;

            //ジャンプ初速設定
            j_Velocity = J_Power;

            //ジャンプアニメーション再生
            if (a_Animator != null)
            {
                a_Animator.SetTrigger("Jump");
            }
        }

        //空中処理
        if (!is_Ground)
        {
            //重力を加算
            j_Velocity += GRAVITY * Time.deltaTime;

            //現在位置取得
            Vector3 pos = transform.position;

            //高さ更新
            pos.y += j_Velocity * Time.deltaTime;

            //位置反映
            transform.position = pos;

            //地面に着地した場合
            if (pos.y <= GROUND_Y)
            {
                //地面位置に戻す
                pos.y = GROUND_Y;
                transform.position = pos;

                //地面状態に戻す
                is_Ground = true;

                //ジャンプ速度リセット
                j_Velocity = 0;
            }
        }
    }

    //アニメーション更新
    void UpdateAnimation()
    {
        //移動入力取得
        float h = Mathf.Abs(c_PlayerInput.GetHorizontal());
        float v = Mathf.Abs(c_PlayerInput.GetVertical());

        //移動速度計算
        float speed = h + v;

        if (a_Animator != null)
        {
            //Idle / Run アニメーション切り替え
            a_Animator.SetFloat("MoveSpeed", speed);

            //地面判定アニメーション
            a_Animator.SetBool("Ground", is_Ground);
        }
    }

    //地面判定取得（PlayerManagerから呼ばれる）
    public bool IsGround()
    {
        return is_Ground;
    }
}