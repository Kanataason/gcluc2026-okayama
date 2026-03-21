using UnityEngine;

// プレイヤー攻撃管理クラス
// 攻撃処理と攻撃アニメーションだけを担当する
public class PlayerAttackManager : MonoBehaviour
{
    // 入力管理
    PlayerInputManager c_PlayerInputManager;

    // プレイヤー管理
    PlayerManager c_PlayerManager;

    //行動管理
    PlayerMoveManager c_PlayerMoveManager;

    // Animator
    Animator a_Animator;

    // 攻撃時間
    const float ATTACK_TIME = 0.5f;

    // 攻撃タイマー
    float f_AttackTimer;

    // Animator パラメータ
    static readonly int ATTACK_HASH = Animator.StringToHash("Attack");

    void Start()
    {
        // 必要なコンポーネント取得
        c_PlayerInputManager = GetComponent<PlayerInputManager>();
        c_PlayerMoveManager = GetComponent<PlayerMoveManager>();
        c_PlayerManager = GetComponent<PlayerManager>();
        a_Animator = GetComponent<Animator>();
    }

    // 攻撃入力確認
    public bool HasAttackInput()
    {
        return c_PlayerInputManager.GetAttack();
    }

    // 攻撃開始
    public void AttackEnter()
    {
        // すでに攻撃中なら開始しない
        if (c_PlayerManager.GetIsAttackFlag()) return;

        // 攻撃開始
        c_PlayerManager.SetIsAttackFlag(true);
        f_AttackTimer = ATTACK_TIME;

        // 攻撃アニメーション再生
        if (a_Animator != null)
        {
            a_Animator.SetTrigger(ATTACK_HASH);
            c_PlayerMoveManager.SetAnimaHashName(ATTACK_HASH);
        }
    }

    // 攻撃中更新
    public void AttackUpdate()
    {
        if (!c_PlayerManager.GetIsAttackFlag()) return;

        f_AttackTimer -= Time.deltaTime;

        if (f_AttackTimer <= 0f)
        {
            f_AttackTimer = 0f;
            c_PlayerManager.SetIsAttackFlag(false);
        }
    }
}