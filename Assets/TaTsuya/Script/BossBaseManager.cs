using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
public class BossBaseManager : CharaBase
{

    BossAttackManager c_BossAttackManager;
    BossBehaviorManager c_BossBehaviorManager;

    public GameObject g_Player;
    public Vector3 v_Scale;

    public override void Start()
    {
        m_MaxHp = 250;
        e_CharaState = CharaState.Boss;
        c_SaveState.g_Character = this.gameObject;
        EventEnter();
        base.Start();
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
        SetStatus(e_CharaState,c_BossAttackManager.m_CurrentAnime);
    }
    public override void Update()
    {
        base.Update();
        if (TatuGameManager.Instance == null || !TatuGameManager.Instance.m_BossTeleport) return;
        CheckGround(TatuGameManager.Instance.m_StageScaleMinY,TatuGameManager.Instance.m_StageScaleMaxY);
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log($"ob{SaveManager.Instance.c_CurrentData.c_PlayerData}/save{SaveManager.Instance.c_CurrentData.c_BossData.b_IsMove}" +
                $"att{c_BossAttackManager.m_IsBossCoroutine1}");
            Debug.Log($"load{BattleManager.Instance.b_IsLoading}cehavi{c_BossBehaviorManager.m_CurrentActionTime}");
        }


        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"{a_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime}");
        }
    }
    public override void FixedUpdate()
    {
        if (TatuGameManager.Instance == null || !TatuGameManager.Instance.m_BossTeleport) return;
     // if (g_Player != null) CheckCollisionBox(1f,1.2f,transform.position, g_Player.transform.position,2);
      if(GetIsAttackFlag() != true) ReverseSprite(CharaState.Player,v_Scale);
    }
    public override void SetStatus(CharaState state, int AnimeName)//画面切り替え時点何をしているのかhp,flagや（アニメーション）を保存
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
        SetDieFlag(false);
        
        AnimatorStateInfo status = a_Animator.GetCurrentAnimatorStateInfo(0);
        float animetime = status.normalizedTime;
        int animehash = status.fullPathHash;
        float animevalue = AnimeName;
        c_SaveState.b_IsTransparent = a_Animator.GetBool(c_BossAttackManager.BossTransparent);


        if (c_BossAttackManager.m_IsBossCoroutine1)//ボス専用
        {
            c_SaveState.b_IsMove = c_BossAttackManager.m_IsBossCoroutine1;
            c_BossAttackManager.ReserAnima();
        }

        switch (m_AnimeHashType)
        {
            case 0: if (AnimeName != 0)  animevalue = a_Animator.GetFloat(AnimeName); break;
            default:Debug.Log("取る必要ない"); break;
        }
        SetAnimetion(animetime, animevalue, animehash);
        base.SetStatus(state, AnimeName);
    }
    public override void ChangePlayer()//切り替え処理
    {
        if (c_SaveState.b_IsNextFrame)
        {
            NextFrame.OneFrame(this, () =>
            {
                a_Animator.speed = 0;
                SetNextFrameActionEvent(1);
            });
        }
        else
        {
            a_Animator.speed = 0;
        }
            SetStatus(e_CharaState, c_BossAttackManager.m_CurrentAnime);
    }
    public override void GetStatus(StageSaveData data)//前回のステータスをセット        
    {
        base.GetStatus(data);
        c_BossBehaviorManager.e_AwakeHp = data.c_BossData.e_BossAwake;
        c_BossBehaviorManager.m_CurrentActionTime = data.c_BossData.m_ActionTime;

        c_BossAttackManager.m_IsBossCoroutine1 = data.c_BossData.b_IsMove;
        c_SaveState.b_IsMove = false;
        a_Animator.SetBool(c_BossAttackManager.BossTransparent, data.c_BossData.b_IsTransparent);

        if (e_CharaState == CharaState.Player)
        {
            if (data.c_PlayerData.l_ObjList != null&&data.c_PlayerData.l_ObjList.Count>0)
            {
                Debug.Log("再現");
                foreach (var obj in data.c_BossData.l_ObjList)
                {
                    obj.RestartClock();
                }

                c_BossAttackManager.l_BulletList = data.c_PlayerData.l_ObjList.ToList();
                data.c_PlayerData.l_ObjList.Clear();
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
                c_BossAttackManager.l_BulletList = data.c_BossData.l_ObjList.ToList();
                data.c_BossData.l_ObjList.Clear();
                c_SaveState.l_ObjList?.Clear();
                SaveManager.Instance.RemoveList(e_CharaState, BattleManager.Instance.m_CurrentRound);
            }
        }
        NextFrame.OneFrame(this, () =>
        {
            data.InitState();
        });
    }
    public override void TakeDamage(float damage)//ダメージ
    {
        base.TakeDamage(damage);
        m_hp = c_BossBehaviorManager.CheckBossAwakening(m_hp);
        if(m_hp == 0)
        {
            Die();
        }
    }
    public override void Die()
    {
        base.Die();
        Debug.Log(SaveManager.Instance.c_CurrentData.m_TimeScore + BattleManager.Instance.m_TimeScore);
        c_BossAttackManager.PlayorStopTransparent(false, true);
        TatuGameManager.Instance.SetMoveFlag(false);
        TatuGameManager.Instance.ActiveHpbar(CharaState.Boss, false);
        BattleManager.Instance.m_CleaStage++;
        NextFrame.Run(this, 3, () =>
        {
            TatuGameManager.Instance.ChangePanel(TatuGameManager.UiPanelState.Score, true);
        });
    }

    public override void SetIsAttackFlag(bool active)//攻撃開始時のフラグ
    {
        base.SetIsAttackFlag(active);
    }
    public float GetHp() { return m_hp; }

    public void SetNextFrameActionEvent(int NextFrame) => c_SaveState.b_IsNextFrame = NextFrame == 0?true:false;

}
