using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class BossBaseManager : CharaBase
{

    BossAttackManager c_BossAttackManager;
    BossBehaviorManager c_BossBehaviorManager;
    //でバック
    public GameObject Player;

    public bool m_IsAwake1 = false;
    public bool m_IsAwake2 = false;
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
        c_BossBehaviorManager = GetComponent<BossBehaviorManager>();
        c_BossAttackManager = GetComponent<BossAttackManager>();
        m_hp = 150;
        SetStatus(e_CharaState,c_BossAttackManager.CurrentAnime);
    }
    public override void Update()
    {
        CheckGround(-8,-2);
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SetStatus(e_CharaState, c_BossAttackManager.CurrentAnime);
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log($"ob{c_SaveState.l_ObjList.Count}/save{SaveManager.Instance.CurrentData.c_BossData.l_ObjList.Count}" +
            $"/bu{c_BossAttackManager.l_BulletList.Count}");
        }

        if (Input.GetKeyDown(KeyCode.H)) { TakeDamage(5); }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"hp{m_hp}/pos{transform.position}/rotete{transform.rotation}");
        }
    }
    public override void FixedUpdate()
    {
        CheckCollision(1f,1.2f,transform.position, Player.transform.position);
    }
    public override void SetStatus(CharaState state,int AnimeName)//画面切り替え時点何をしているのかhp,flagや（アニメーション）を保存
    {
        if (c_BossAttackManager.l_BulletList != null&& c_BossAttackManager.l_BulletList.Count >0)
        {
            c_SaveState.l_ObjList = c_BossAttackManager.l_BulletList.ToList();
            foreach (var list in c_BossAttackManager.l_BulletList)
            {
                list.StopClock();
            }
            c_BossAttackManager.l_BulletList.Clear();
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
        base.GetStatus(data);
        c_BossBehaviorManager.e_AwakeHp = data.c_BossData.e_BossAwake;
        c_BossBehaviorManager.m_CurrentActionTime = data.c_BossData.m_ActionTime;

        if (e_CharaState == CharaState.Player)
        {
            if (data.c_PlayerData.l_ObjList != null&&data.c_PlayerData.l_ObjList.Count>0)
            {
                Debug.Log("再現");
                foreach (var obj in data.c_BossData.l_ObjList)
                {
                    obj.RestartClock();
                }

                data.c_BossData.l_ObjList.Clear();
                c_SaveState.l_ObjList?.Clear();
                SaveManager.Instance.RemoveList(e_CharaState, BattleManager.Instance.m_CurrentRound);
            }  
        }
        else if(e_CharaState == CharaState.Boss)
        {
            if(data.c_BossData.l_ObjList != null && data.c_BossData.l_ObjList.Count > 0)
            {
                Debug.Log("再現");
                foreach (var obj in data.c_BossData.l_ObjList)
                {
                    obj.RestartClock();
                }

                data.c_BossData.l_ObjList.Clear();
                c_SaveState.l_ObjList?.Clear();
                SaveManager.Instance.RemoveList(e_CharaState, BattleManager.Instance.m_CurrentRound);
            }
        }
        NextFrame.Run(this, 0.1f, () =>
        {
            BattleManager.Instance.b_IsLoading = false;
        });
    }
    public override void TakeDamage(int damage)//ダメージ
    {
        base.TakeDamage(damage);
        m_hp = c_BossBehaviorManager.CheckBossAwakening(m_hp);
    }

    public override void SetIsAttackFlag(bool active)//攻撃開始時のフラグ
    {
        base.SetIsAttackFlag(active);
    }
    public int GetHp() { return m_hp; }

}
