using System;
using UnityEngine;

public class BossBulletManager : MonoBehaviour
{
    public enum BulletState
    {
        Move,
        Stop,
        Destroy,
        Attack,
        None
    }
    private static readonly int m_AttackHash = Animator.StringToHash("Attack");
    private const float MoveSpeed = 15f;

    //攻撃をする際の当たり判定とダメージ
    public float m_MyScaleX;
    public float m_MyScaleY;
    public int m_Damage;
    public bool b_IsFlyAttack;

    //オブジェクトの情報
    public float m_DestroyTime = 0.5f;
    public float m_StartAnima = 1.5f;
    private float m_Time = 0;
    private float m_AnimaTime = 0;
    private Vector3 v_MoveDirection;
    //エフェクトのタイプ
    public int m_CharaType;
    public int m_EfectType;
    public BossBehaviorManager.BossAttackType e_Attacktype;
    //フラグ
    private bool b_IsCollider = false;
    private BulletState e_BulletState;

    private CharaBase c_Player = null;
    private CharaBase c_Boss = null;

    //オブジェクトによって変える
    public Animator a_Anima;
    public SpriteRenderer s_Sprite;

    public event Action<BossBulletManager,int,int> OnDestroyObjEvent;

    private void OnDestroy()
    {
        OnDestroyObjEvent = null;
    }
    public void Init(float startAnimaTime, BulletState state, CharaBase player, CharaBase boss, bool isAttack = true)
    {
        e_BulletState = state;
        c_Player = player;
        c_Boss = boss;
        m_StartAnima = startAnimaTime;
        m_Time = 0;
        m_AnimaTime = 0;
        SetCollider(isAttack);
        if (s_Sprite != null) SortOrderManager.Instance.SetSpriteOrder(s_Sprite);
    }

    void Update()
    {
        if (BattleManager.Instance.b_IsLoading) return;

        switch (e_BulletState)
        {
            case BulletState.Destroy:
                UpDateDestroyTimer(); break;
            case BulletState.Move:
                UpDateMoveTimer(); break;
            case BulletState.Attack:
                PlayAttackAnima(); break;
            case BulletState.Stop:
                UpDateAnimaTime(); break;
        }
    }
    void PlayAttackAnima()
    {
        if (a_Anima != null)
            a_Anima.SetTrigger(m_AttackHash);

        e_BulletState = BulletState.Destroy;
    }
    void UpDateAnimaTime()
    {
        m_AnimaTime += Time.deltaTime;
        if (m_AnimaTime >= m_StartAnima)
        {
            m_AnimaTime = 0;
            e_BulletState = BulletState.Attack;
        }
    }
    void UpDateMoveTimer()
    {
        if (UpDateDestroyTimer()) return;
        transform.Translate(v_MoveDirection * MoveSpeed * Time.deltaTime);
    }
    bool UpDateDestroyTimer()//消えたらtrue
    {
        m_Time += Time.deltaTime;
        if (m_Time <= m_DestroyTime) return false;

        DestroyInfo();
        return true;
    }
    private void FixedUpdate()
    {
        if (c_Player == null || !b_IsCollider) return;

        float attackRate = (c_Boss != null && c_Boss.m_AttackParame != 0) ? c_Boss.m_AttackParame : 1;
        c_Player.CheckCollisionBox(m_MyScaleX, m_MyScaleY, transform.position,
            c_Player.transform.position, m_Damage * attackRate, b_IsFlyAttack);
    }
    private void DestroyInfo()//消えるとき
    {
        e_BulletState = BulletState.None;
        c_Player = null;
        SetCollider(false);
        OnDestroyObjEvent?.Invoke(this, m_CharaType, m_EfectType);
    }
    public void Move()//動く攻撃用 ボスの向いている方向へ飛ばす
    {
        v_MoveDirection = c_Boss != null && c_Boss.CurrentDirection == 1 ? Vector3.left : Vector3.right;
    }
    public void PlaySe(string name)
    {
        AudioManager.Instance.StopSe();
        AudioManager.Instance.PlaySeAudio(name);
    }
    public void StopSe()
    {
        AudioManager.Instance.StopSe();
    }
    public void ActiveCollider(int IsTrue) => SetCollider(IsTrue == 1);//アニメーションイベント(名前変更不可) 1:有効
    private void SetCollider(bool isActive) => b_IsCollider = isActive;
}
