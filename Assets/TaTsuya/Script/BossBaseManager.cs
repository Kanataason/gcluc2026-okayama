using UnityEngine;
public class BossBaseManager : CharaBase
{
    BossAttackManager c_BossAttackManager;
    BossBehaviorManager c_BossBehaviorManager;

    public Vector3 v_Scale;

    public void Init()//初期化
    {
        m_AttackParame = 1;
        m_MaxHp = 250;
        m_hp = m_MaxHp;
        e_CharaState = CharaState.Boss;

        GetComponents();
    }
    private void GetComponents()
    {
        c_BossBehaviorManager = GetComponent<BossBehaviorManager>();
        c_BossAttackManager = GetComponent<BossAttackManager>();
    }
    public override void FixedUpdate()
    {
        if (c_Stage == null || !c_Stage.b_InBattle) return;
        CheckGround(TatuGameManager.Instance.m_StageScaleMinY, TatuGameManager.Instance.m_StageScaleMaxY);

        if (!GetIsAttackFlag())
            ReverseSprite(CharaState.Player, v_Scale);
    }
    public override void TakeDamage(float damage)//ダメージ
    {
        //死亡中・戦闘外(覚醒演出中など)はダメージを受けない
        if (GetDieFlag() || c_Stage == null || !c_Stage.b_InBattle) return;
        base.TakeDamage(damage);

        //ボス専用 覚醒判定
        if (c_BossBehaviorManager.TryAwakening(m_hp, out float awakeningHp))
        {
            m_hp = awakeningHp;
            SetHp();
        }
        if (m_hp <= 0)
        {
            m_hp = 0;
            Die();
        }
    }
    public override void Die()
    {
        SetDieFlag(true);
        base.Die();//結果の表示はBattleManagerが行う
        //ボス専用　テレポートフラグ
        c_BossAttackManager.SetTransparent(false, true);
    }
    public void PowerUp(float rate) { m_AttackParame += rate; }//攻撃力の倍率を上げる(弾のダメージにかかる)

    //アニメーションイベント(Attack-NoEffect.anim, Skil.anim)が残っているので受け口だけ残す
    //交代の仕組みで使っていたもので今は何もしない アニメーションからイベントを消したらこの関数も消してよい
    public void SetNextFrameActionEvent(int isNextFrame) { }
}
