using System;
using UnityEngine;
using System.Collections.Generic;
public class BossMoveManager : CharaBase
{
    enum BossState
    {
        Idle = 1,
        Attack = 2,
        Die = 3,
        Invincible = 4,
    }

    BossState s_BossState = BossState.Idle;
    Dictionary<BossState, Action> d_Lottery;
    BossAttackManager c_BossAttackManager;
    //でバック
    public GameObject Player;
    public SpriteRenderer sprite;
    public GameObject pos;
    public override void Start()
    {
        base.Start();
        SortOrderManager.Instance.SetList(Player.transform.parent.GetComponent<SpriteRenderer>());
        Init();
    }
    private void Init()
    {
        c_BossAttackManager = GetComponent<BossAttackManager>();
        m_hp = 100;
        d_Lottery = new Dictionary<BossState, Action>
        {
            {BossState.Idle, null},
            {BossState.Attack,c_BossAttackManager.AttackEnter},
            {BossState.Die,Die },
            {BossState.Invincible,null}
        };
    }
    public override void Update()
    {
        CheckGround(-8,-2);
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SetStatus(CharaState.Boss,Animator.StringToHash("Move"));
        }
        if (GetIsAttackFlag()) return;
        if(Input.GetKey(KeyCode.A))
        {
            a_Animator.SetFloat("Move", 3f * Time.deltaTime, 0.05f, Time.deltaTime);
            transform.Translate(Vector3.left * 8f*Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            a_Animator.SetFloat("Move",0, 0.05f, Time.deltaTime);
            transform.Translate(Vector3.right * 8f * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.W))
        {
            a_Animator.SetFloat("Move", 3f * Time.deltaTime, 0.05f, Time.deltaTime);
            transform.Translate(Vector3.up * 8f * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            a_Animator.SetFloat("Move", 0, 0.05f, Time.deltaTime);
            transform.Translate(Vector3.down * 8f * Time.deltaTime);
        }

            if (Input.GetKeyDown(KeyCode.H)) { TakeDamage(5); }

        if (Input.GetKeyDown(KeyCode.E))
        {
            GetStatus();
            Debug.Log($"hp{m_hp}/pos{transform.position}/rotete{transform.rotation}");
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetIsAttackFlag(true);
            ChengeStatus(BossState.Attack);
            if(d_Lottery.TryGetValue(s_BossState,out var action))
            {
                action.Invoke();
            }
        }
    }
    public override void FixedUpdate()
    {
        CheckCollision(1f,1.2f,transform.position, Player.transform.position);
    }
    public override void SetStatus(CharaState state,int AnimeName)//画面切り替え時点何をしているのかhp,flagや（アニメーション）を保存
    {
        foreach (var list in c_BossAttackManager.l_BulletList)
        {
            list.StopClock();
        }
        AnimatorStateInfo status = a_Animator.GetCurrentAnimatorStateInfo(0);
        float animetime = status.normalizedTime;
        int animehash = status.fullPathHash;
        float animevalue = a_Animator.GetFloat(AnimeName);
        SetAnimetion(animetime, animevalue, animehash);

        base.SetStatus(state, AnimeName);
    }

    public override void GetStatus()//前回のステータスをセット        
    {
        a_Animator.SetFloat(GetAnimeHashCode(), c_SaveState.m_AnimeStateValue);
        a_Animator.Play(c_SaveState.m_AnimeHash, 0, c_SaveState.m_AnimeTime);

        base.GetStatus();
    }

    public override void SetIsAttackFlag(bool active)//攻撃開始時のフラグ
    {
        base.SetIsAttackFlag(active);
    }
    public void ResetAttackFlag() { SetIsAttackFlag(false); }//リセットフラグ

    private void ChengeStatus(BossState state) { s_BossState = state; }//ステータスを変える
}
