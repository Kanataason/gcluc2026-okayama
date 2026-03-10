using UnityEngine;

// プレイヤー移動管理クラス
// 入力情報を受け取りキャラクターを移動させる
public class PlayerMoveManager : CharaBase
{
    //入力管理クラス
    PlayerInputManager c_PlayerInput;

    //移動速度
    const float move_Speed = 8f;

    //地面移動範囲
    const float ground_Min = -8f;
    const float ground_Max = -2f;

    //アニメーション値
    const float move_Anime = 3f;
    const float damp_Time = 0.05f;

    // 初期化
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
    }

    // プレイヤー移動処理
    void Move()
    {
        //移動範囲制限
        CheckGround(ground_Min, ground_Max);

        //攻撃中は移動しない
        if (GetIsAttackFlag()) return;

        //入力取得
        float h = c_PlayerInput.GetHorizontal();
        float v = c_PlayerInput.GetVertical();

        //移動ベクトル
        Vector3 move = new Vector3(h, v, v);

        //移動処理
        transform.Translate(move * move_Speed * Time.deltaTime);

        //アニメーション更新
        if (a_Animator != null)
        {
            a_Animator.SetFloat("Move", move_Anime * Mathf.Abs(h + v), damp_Time, Time.deltaTime);
        }
    }
}