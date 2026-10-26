using System;
using UnityEngine;
[RequireComponent(typeof(SpriteRenderer),typeof(Animator))]
public class CharaBase : MonoBehaviour
{
    public virtual void OnDisable()
    {
        SortOrderManager.Instance.RemoveList(s_Sprite);
        OnHpBar = null;
    }
    public virtual void Start()
    {
        c_Camera = c_Stage != null ? c_Stage.c_Camera : Camera.main;
        s_Sprite = GetComponent<SpriteRenderer>();
        a_Animator = GetComponent<Animator>();

        m_hp = m_MaxHp;

        SortOrderManager.Instance.SetList(s_Sprite);
    }
    public virtual void Update()
    {
        if (BattleManager.Instance.b_IsLoading) return;

        if (GetIsHitFlag() == false) return;

        HitTime += Time.deltaTime;

        Color col = s_Sprite.color;
        col.a = Mathf.PingPong(HitTime * 8f, 1f);
        s_Sprite.color = col;

        if (HitTime > Duraction)
        {
            EndHitEffect();
        }
    }//キーの取得だけ
    private void EndHitEffect()
    {
        HitTime = 0;

        var col = s_Sprite.color;
        col.a = 1f;
        s_Sprite.color = col;

        SetIsHitFlag(false);
    }

    public virtual void FixedUpdate() { }//主な処理はこっち

    public virtual void ReverseSprite(CharaState targetstate,Vector3 CharaScale)//向きを変える -1左1右
    {
        if (c_Stage == null) return;
        var target = c_Stage.GetCharacter(targetstate);
        if (target == null) return;

        int dir = transform.position.x > target.transform.position.x ? 1 : -1;
        Func<int, bool> func = targetstate == CharaState.Boss
            ? (v => v == -1)
            : (v => v == 1);

        if (dir != CurrentDirection)
        {
            CurrentDirection = dir;
            transform.localScale = func(dir)
                ? CharaScale
                : new Vector3(-CharaScale.x, CharaScale.y, CharaScale.z);
        }
    }
    public virtual void CheckGround(float Min, float Max)
    {
        float height = c_Camera.orthographicSize;
        float width = height * c_Camera.aspect;

        float offset = 1f;

        Vector3 pos = transform.position;

        float ClampY = Mathf.Clamp(pos.y, Min, Max);

        float camX = c_Camera.transform.position.x;
        float ClampX = Mathf.Clamp(pos.x,
                                   camX - width + offset,
                                   camX + width - offset);
        transform.position = new Vector3(ClampX, ClampY, ClampY);
    }
    public virtual void CheckCollisionBox(float ScaleX, float ScaleY, Vector3 MyPos, Vector3 OppPos, float damage = 0,bool IsFly = false)//当たり判定 奥行きはｚで判定
    {
        //ジャンプは別の変数で管理をしてそれを判定する
        if (GetIsHitFlag() == true) return;

        float dx = Mathf.Abs(MyPos.x - OppPos.x);
        float dz = Mathf.Abs(MyPos.z - OppPos.z);
        if (dx < ScaleX && dz < ScaleY)
        {
            TakeDamage(damage);
        }
    }
    public virtual void TakeDamage(float damage)//攻撃を食らったときの関数
    {
        SetIsHitFlag(true);

        m_hp -= damage;
        AudioManager.Instance.PlaySeAudio("Hit");
        SetHp();
    }
    public virtual void Die()
    {
        //戦闘を止めてボスのHPバーを非表示、勝敗はBattleManagerが決める
        if (c_Stage != null) c_Stage.OnCharacterDie(e_CharaState);
    }//死んだとき

    public virtual void SetDieFlag(bool IsDie) { b_IsDie = IsDie; }

    public virtual bool GetDieFlag() { return b_IsDie; }//死んだときのフラグ

    public virtual void SetIsAttackFlag(bool active) { m_IsAttack = active; }//攻撃初めのフラグ
    public virtual void SetIsHitFlag(bool active) { m_IsHit = active; }//攻撃が当たったときのフラグ
    public virtual bool GetIsAttackFlag() { return m_IsAttack; }//攻撃フラグを取る
    public virtual bool GetIsHitFlag() { return m_IsHit; }//ヒットフラグを取る

    public virtual void SetHp() //HpBarに反映させる処理
    {
        float value = m_hp / m_MaxHp;
        var clamp = Mathf.Clamp01(value);//０～１に収める
        OnHpBar?.Invoke(e_CharaState,clamp);
    }
    public void SetStage(StageContext stage) { c_Stage = stage; }//自分がいるステージ(生成時にBattleManagerが設定)

    protected SpriteRenderer s_Sprite;
    protected Animator a_Animator;
    protected float m_hp;//マックスhpを参照する
    protected float m_MaxHp;//自分の設定する
    protected CharaState e_CharaState;//自分が何のキャラクターかしまう変数

    private bool m_IsAttack = false;//攻撃をしているか
    private bool m_IsHit = false;//攻撃が当たっているか
    private bool b_IsDie = false;//死んだときのフラグ

    public int CurrentDirection;//現在の向き
    public float m_AttackParame = 1;

    public event Action<CharaState,float> OnHpBar;

    //時間の管理
    private float HitTime = 0;
    private float Duraction = 1f;
    protected Camera c_Camera;
    protected StageContext c_Stage;

}
