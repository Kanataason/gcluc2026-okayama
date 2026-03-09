using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        e_CharaState = CharaState.Boss;
        EventEnter();
        base.Start();
        SortOrderManager.Instance.SetList(Player.transform.parent.GetComponent<SpriteRenderer>());
        Init();
    }
    private void EventEnter()
    {
        Debug.Log("イベント登録");
        BattleManager.Instance.OnSetStageInfo += ChangePlayer;
        BattleManager.Instance.OnGetStageInfo += GetStatus;
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
        SetStatus(e_CharaState,c_BossAttackManager.CurrentAnime);
    }
    public override void Update()
    {
        CheckGround(-8,-2);
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SetStatus(e_CharaState, c_BossAttackManager.CurrentAnime);
        }


            if (Input.GetKeyDown(KeyCode.H)) { TakeDamage(5); }

        if (Input.GetKeyDown(KeyCode.E))
        {
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
        if (c_BossAttackManager.l_BulletList != null)
        {
            c_SaveState.l_ObjList = c_BossAttackManager.l_BulletList.ToList();
            foreach (var list in c_BossAttackManager.l_BulletList)
            {
                list.StopClock();
            }
        }
        
        AnimatorStateInfo status = a_Animator.GetCurrentAnimatorStateInfo(0);
        float animetime = status.normalizedTime;
        int animehash = status.fullPathHash;
        float animevalue = AnimeName;
        switch (m_AnimeHashType)
        {
            case 0: if (AnimeName == 0) break; animevalue = a_Animator.GetFloat(AnimeName); break;
            default:Debug.Log("取る必要ない"); break;
        }
        SetAnimetion(animetime, animevalue, animehash);
        base.SetStatus(state, AnimeName);
    }
    public override void ChangePlayer()//切り替え処理
    {
        SetStatus(e_CharaState, c_BossAttackManager.CurrentAnime);
    }
    public override void GetStatus(StageSaveData data)//前回のステータスをセット        
    {
        if (c_SaveState.l_ObjList != null)
        {
            Debug.Log("再現");
            foreach (var obj in c_SaveState.l_ObjList)
            {
                obj.RestartClock();
            }
            c_SaveState.l_ObjList.Clear();
        }
        base.GetStatus(data);
    }

    public override void SetIsAttackFlag(bool active)//攻撃開始時のフラグ
    {
        base.SetIsAttackFlag(active);
    }

    private void ChengeStatus(BossState state) { s_BossState = state; }//ステータスを変える
}
