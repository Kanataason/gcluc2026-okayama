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
    private void Init()//初期化
    {
        c_BossBehaviorManager = GetComponent<BossBehaviorManager>();
        c_BossAttackManager = GetComponent<BossAttackManager>();

        SetStatus(e_CharaState,c_BossAttackManager.m_CurrentAnime);
    }
    public override void Update()
    {
        base.Update();
        //デバックをするために書いている
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log($"ob{SaveManager.Instance.c_CurrentData.c_PlayerData}/save{SaveManager.Instance.c_CurrentData.c_BossData.b_IsMove}" +
                $"att{c_BossAttackManager.m_IsBossCoroutine1}");
            Debug.Log($"load{BattleManager.Instance.b_IsLoading}cehavi{c_BossBehaviorManager.m_CurrentActionTime}");
        }


        if (Input.GetKeyDown(KeyCode.E))
        {
            c_BossAttackManager.SpawnEfect(5);
        }
    }
    public override void FixedUpdate()
    {
        if (TatuGameManager.Instance == null || !TatuGameManager.Instance.m_BossTeleport) return;
        CheckGround(TatuGameManager.Instance.m_StageScaleMinY, TatuGameManager.Instance.m_StageScaleMaxY);

        if (GetIsAttackFlag() != true) 
            ReverseSprite(CharaState.Player,v_Scale);
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

        c_SaveState.b_IsTransparent = a_Animator.GetBool(c_BossAttackManager.BossTransparent);

        if (c_BossAttackManager.m_IsBossCoroutine1)//ボス専用
        {
            c_SaveState.b_IsMove = c_BossAttackManager.m_IsBossCoroutine1;
            c_BossAttackManager.ReserAnima();
        }

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
        var BossData = data.c_BossData;
        c_BossBehaviorManager.e_AwakeHp = BossData.e_BossAwake;
        c_BossBehaviorManager.m_CurrentActionTime = BossData.m_ActionTime;

        c_BossAttackManager.m_IsBossCoroutine1 = BossData.b_IsMove;
        c_SaveState.b_IsMove = false;
        a_Animator.SetBool(c_BossAttackManager.BossTransparent, BossData.b_IsTransparent);


        if (BossData.l_ObjList != null && BossData.l_ObjList.Count > 0)
        {
            Debug.Log("再現");
            foreach (var obj in BossData.l_ObjList)
            {
                obj.RestartClock();
            }
            c_BossAttackManager.l_BulletList = BossData.l_ObjList.ToList();
            BossData.l_ObjList.Clear();
            c_SaveState.l_ObjList?.Clear();

            SaveManager.Instance.RemoveList(e_CharaState, BattleManager.Instance.m_CurrentRound);
        }

        NextFrame.OneFrame(this, () =>
        {
            data.InitState();
        });
    }
    public override void TakeDamage(float damage)//ダメージ
    {
        if (GetDieFlag() == true) return;
        base.TakeDamage(damage);
        m_hp = c_BossBehaviorManager.CheckBossAwakening(m_hp);
        if (m_hp == 0)
        {
            Die();
        }
    }
    public override void Die()
    {
        base.Die();
        c_BossAttackManager.PlayorStopTransparent(false, true);
        TatuGameManager.Instance.SetMoveFlag(false);
        TatuGameManager.Instance.ActiveHpbar(CharaState.Boss, false);
        SaveManager.Instance.c_CurrentData.c_BossData.b_DieFlag = true;
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
