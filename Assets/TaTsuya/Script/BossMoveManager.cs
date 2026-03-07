using System;
using UnityEngine;
using System.Collections.Generic;
public class BossMoveManager : CharaBase
{
    enum BossStatus
    {
        Idle = 1,
        Attack = 2,
        Die = 3,
        Invincible = 4,
    }

    BossStatus s_BossStatus = BossStatus.Idle;
    Dictionary<BossStatus, Action> d_Lottery;
    BossAttackManager c_BossAttackManager;
    public override void Start()
    {
        base.Start();
        Init();
    }
    private void Init()
    {
        c_BossAttackManager = GetComponent<BossAttackManager>();
        m_hp = 100;
        d_Lottery = new Dictionary<BossStatus, Action>
        {
            {BossStatus.Idle, null},
            {BossStatus.Attack,c_BossAttackManager.AttackEnter},
            {BossStatus.Die,Die },
            {BossStatus.Invincible,null}
        };
    }
    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SetStatus(Animator.StringToHash("Move"));
            Debug.Log($"hp{c_SaveState.m_Inihp}/pos{c_SaveState.v_IniPosition}/rotete" +
                $"{c_SaveState.q_IniRotate}/animetime{c_SaveState.m_AnimeTime}");
        }
        if (GetIsAttackFlag()) return;
        if(Input.GetKey(KeyCode.A))
        {
            a_Animator.SetFloat("Move", 3f * Time.deltaTime, 0.05f, Time.deltaTime);
            transform.Translate(Vector3.left * 3f*Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            a_Animator.SetFloat("Move",0, 0.05f, Time.deltaTime);
            transform.Translate(Vector3.right * 4f * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.W))
        {
            a_Animator.SetFloat("Move", 3f * Time.deltaTime, 0.05f, Time.deltaTime);
            transform.Translate(Vector3.up * 4f * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            a_Animator.SetFloat("Move", 0, 0.05f, Time.deltaTime);
            transform.Translate(Vector3.down * 4f * Time.deltaTime);
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
            ChengeStatus(BossStatus.Attack);
            if(d_Lottery.TryGetValue(s_BossStatus,out var action))
            {
                action.Invoke();
            }
        }
    }
    public override void SetStatus(int AnimeName)//画面切り替え時点何をしているのかhp,flagや（アニメーション）を保存
    {
        AnimatorStateInfo status = a_Animator.GetCurrentAnimatorStateInfo(0);
        float animetime = status.normalizedTime;
        int animehash = status.fullPathHash;
        float animevalue = a_Animator.GetFloat(AnimeName);
        SetAnimetion(animetime, animevalue, animehash);

        base.SetStatus(AnimeName);
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

    private void ChengeStatus(BossStatus state) { s_BossStatus = state; }//ステータスを変える
}
