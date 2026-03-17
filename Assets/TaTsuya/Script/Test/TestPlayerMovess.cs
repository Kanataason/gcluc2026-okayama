using System.Linq;
using UnityEngine;
using static BossAttackManager;

public class TestPlayerMovess :CharaBase
{
    public Vector3 v_scale;

    public override void Start()
    {
        e_CharaState = CharaState.Player;
        m_MaxHp = 50;
        c_SaveState.g_Character = this.gameObject;
        base.Start();
        SetStatus(e_CharaState);
        //どこかでどっちから始めるかを設定
        EventEnter();
        NextFrame.Run(this, 0.2f, () => { TakeDamage(0); });
    }
    private void EventEnter()
    {
        Debug.Log("イベント登録");
        BattleManager.Instance.OnSetStageInfo += ChangePlayer;
        BattleManager.Instance.OnGetStageInfo += GetStatus;
    }
    public override void Update()
    {
        if (TatuGameManager.Instance == null) return;
       // ReverseSprite(CharaState.Boss, v_scale);
    //   CheckGround(TatuGameManager.Instance.m_StageScaleMinY, TatuGameManager.Instance.m_StageScaleMaxY);

    }
    public override void ChangePlayer()//切り替え処理
    {
        if (c_SaveState.b_IsNextFrame)
        {
            NextFrame.OneFrame(this, () =>
            {
                a_Animator.speed = 0;
            });
        }
        else
        {
            a_Animator.speed = 0;
        }
        SetStatus(e_CharaState, 0);
    }
         public override void GetStatus(StageSaveData data)//前回のステータスをセット        
    {
        base.GetStatus(data);
        ReverseSprite(CharaState.Boss, v_scale);
        if (e_CharaState == CharaState.Player)
        {
            if (data.c_PlayerData.l_ObjList != null && data.c_PlayerData.l_ObjList.Count > 0)
            {
                Debug.Log("再現");
                foreach (var obj in data.c_BossData.l_ObjList)
                {
                    obj.RestartClock();
                }

                data.c_PlayerData.l_ObjList.Clear();
                c_SaveState.l_ObjList?.Clear();
                SaveManager.Instance.RemoveList(e_CharaState, BattleManager.Instance.m_CurrentRound);
            }
        }
        NextFrame.OneFrame(this, () =>
        {
            data.InitState();
        });
    }

}
