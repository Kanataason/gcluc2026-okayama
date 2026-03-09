using UnityEngine;

/// <summary>
/// プレイヤー移動処理
/// </summary>
public class PlayerMoveManager : CharaBase
{
    //移動速度
    const float MOVE_SPEED = 8f;

    //地面制限
    const float GROUND_MIN = -8f;
    const float GROUND_MAX = -2f;

    //アニメーション値
    const float MOVE_ANIME = 3f;
    const float DAMP_TIME = 0.05f;

    public override void Update()
    {
        //移動範囲制限
        CheckGround(GROUND_MIN, GROUND_MAX);

        //攻撃中は移動しない
        if (GetIsAttackFlag()) return;

        //左移動
        if (Input.GetKey(KeyCode.A))
        {
            a_Animator.SetFloat("Move", MOVE_ANIME * Time.deltaTime, DAMP_TIME, Time.deltaTime);

            transform.Translate(Vector3.left * MOVE_SPEED * Time.deltaTime);
        }

        //右移動
        else if (Input.GetKey(KeyCode.D))
        {
            a_Animator.SetFloat("Move", 0, DAMP_TIME, Time.deltaTime);

            transform.Translate(Vector3.right * MOVE_SPEED * Time.deltaTime);
        }

        //上移動（奥）
        else if (Input.GetKey(KeyCode.W))
        {
            a_Animator.SetFloat("Move", MOVE_ANIME * Time.deltaTime, DAMP_TIME, Time.deltaTime);

            transform.Translate(Vector3.up * MOVE_SPEED * Time.deltaTime);
        }

        //下移動（手前）
        else if (Input.GetKey(KeyCode.S))
        {
            a_Animator.SetFloat("Move", 0, DAMP_TIME, Time.deltaTime);

            transform.Translate(Vector3.down * MOVE_SPEED * Time.deltaTime);
        }

        //ダメージテスト
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(5);
        }

        //ステータス確認
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"hp{m_hp}/pos{transform.position}/rotete{transform.rotation}");
        }
    }
}