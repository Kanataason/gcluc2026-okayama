using System.Linq;
using UnityEngine;
public class BossBaseManager : CharaBase
{

    BossAttackManager c_BossAttackManager;
    BossBehaviorManager c_BossBehaviorManager;

    public Vector3 v_Scale;
    public override void Start()
    {
        Init();
        EventEnter();

        base.Start();
        SetStatus(e_CharaState, c_BossAttackManager.m_CurrentAnime);
    }
    private void EventEnter()//イベント登録
    {
        Debug.Log("イベント登録");
        BattleManager.Instance.OnSetStageInfo += ChangePlayer;
        BattleManager.Instance.OnGetStageInfo += GetStatus;
    }
    private void Init()//初期化
    {
        m_MaxHp = 250;
        e_CharaState = CharaState.Boss;
        c_SaveState.g_Character = this.gameObject;

        c_BossBehaviorManager = GetComponent<BossBehaviorManager>();
        c_BossAttackManager = GetComponent<BossAttackManager>();

    }
    public override void Update()
    {
        base.Update();
        //デバックをするために書いている
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log($"ob{SaveManager.Instance.c_CurrentData.c_PlayerData}/save{SaveManager.Instance.c_CurrentData.c_BossData.b_IsMove}" +
                $"att{c_BossAttackManager.b_IsBossCoroutine}");
            Debug.Log($"load{BattleManager.Instance.b_IsLoading}cehavi{c_BossBehaviorManager.m_CurrentActionTime}");
        }

    }
    public override void FixedUpdate()
    {
        if (TatuGameManager.Instance == null || !TatuGameManager.Instance.b_BossTeleport) return;
        CheckGround(TatuGameManager.Instance.m_StageScaleMinY, TatuGameManager.Instance.m_StageScaleMaxY);

        if (GetIsAttackFlag() != true) 
            ReverseSprite(CharaState.Player,v_Scale);
    }
    public override void SetStatus(CharaState state, int AnimeName)//画面切り替え時点何をしているのかhp,flagや（アニメーション）を保存
    {
        TryStopObject();

        if (c_BossAttackManager.b_IsBossCoroutine)//ボス専用
        {
            c_SaveState.b_IsMove = c_BossAttackManager.b_IsBossCoroutine;
            c_BossAttackManager.ReserAnima();
        }

        //アニメーションの設定
        SetDieFlag(false);
        if(a_Animator != null)
        c_SaveState.b_IsTransparent = a_Animator.GetBool(c_BossAttackManager.BossTransparent);

        base.SetStatus(state, AnimeName);
    }
    private void TryStopObject()
    {
        //生成した処理
        if (c_BossAttackManager.l_BulletList != null && c_BossAttackManager.l_BulletList.Count > 0)
        {
            c_SaveState.l_ObjList = c_BossAttackManager.l_BulletList.ToList();
            c_BossAttackManager.l_BulletList.Clear();

            foreach (var list in c_SaveState.l_ObjList)
                list.StopClock();
        }

    }
    public override void ChangePlayer()//切り替え処理
    {
        if (c_SaveState.b_IsNextFrame)
        {
            NextFrame.OneFrame(this, () => { 
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

        SetBossInfo(BossData);
        TryReproductionObject(BossData);//生成したものを再現
        //アニメーションをセットする
        a_Animator.SetBool(c_BossAttackManager.BossTransparent, BossData.b_IsTransparent);

      //  NextFrame.OneFrame(this, () => { data.InitState(); });//初期化
    }
    private void SetBossInfo(SaveState BossData)
    {
        c_BossBehaviorManager.e_AwakeHp = BossData.e_BossAwake;
        c_BossBehaviorManager.m_CurrentActionTime = BossData.m_ActionTime;
        c_BossAttackManager.b_IsBossCoroutine = BossData.b_IsMove;
        c_SaveState.b_IsMove = false;

    }
    private void TryReproductionObject(SaveState BossData)
    {
        if (BossData.l_ObjList != null && BossData.l_ObjList.Count > 0)
        {
            Debug.Log("再現");
            foreach (var obj in BossData.l_ObjList)
                obj.RestartClock();

            c_BossAttackManager.l_BulletList = BossData.l_ObjList.ToList();
            BossData.l_ObjList.Clear();
            c_SaveState.l_ObjList?.Clear();

            SaveManager.Instance.RemoveList(e_CharaState, BattleManager.Instance.m_CurrentRound);
        }

    }
    public override void TakeDamage(float damage)//ダメージ
    {
        if (GetDieFlag() == true) return;
        base.TakeDamage(damage);
        //ボス専用 覚醒判定
        m_hp = c_BossBehaviorManager.CheckBossAwakening(m_hp);
        if (m_hp == 0)
        {
            Die();
        }
    }
    public override void Die()
    {
        base.Die();
        //ボス戦用　テレポートフラグ
        c_BossAttackManager.PlayorStopTransparent(false, true);

        NextFrame.Run(this, 3, () =>
        {
            TatuGameManager.Instance.ChangePanel(TatuGameManager.UiPanelState.Score, true);
        });
    }

    public override void SetIsAttackFlag(bool active)//攻撃開始時のフラグ
    {
        base.SetIsAttackFlag(active);
    }
    public float GetHp() { return m_hp; }//現在のHpをとる

    public void SetNextFrameActionEvent(int NextFrame) 
        => c_SaveState.b_IsNextFrame = NextFrame == 0?true:false;

}
