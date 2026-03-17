using System.Linq;
using UnityEngine;

public class TestPlayerMovess :CharaBase
{
    public Vector3 v_scale;
    private GameObject g_Boss;

    public override void Start()
    {
        e_CharaState = CharaState.Player;
        m_MaxHp = 50;
        c_SaveState.g_Character = this.gameObject;
        base.Start();
        SetStatus(e_CharaState);
        //どこかでどっちから始めるかを設定
        EventEnter();
        SetHp();
        TatuGameManager.Instance.ActiveHpbar(CharaState.Player, true);
        NextFrame.Run(this, 0.5f, () =>
        {
            g_Boss = SaveManager.Instance.c_CurrentData.GetCharacter(CharaState.Boss);
        });
    }
    private void EventEnter()
    {
        Debug.Log("イベント登録");
        BattleManager.Instance.OnSetStageInfo += ChangePlayer;
        BattleManager.Instance.OnGetStageInfo += GetStatus;
    }
    public override void Update()
    {
        base.Update();
        if (TatuGameManager.Instance == null) return;

        if (Input.GetKeyDown(KeyCode.H))
        { TakeDamage(6); }
        // ReverseSprite(CharaState.Boss, v_scale);
        if (g_Boss != null) CheckCollisionBox(2, 0.7f, transform.position, g_Boss.transform.position, 7);
      CheckGround(TatuGameManager.Instance.m_StageScaleMinY, TatuGameManager.Instance.m_StageScaleMaxY);

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
    public override void SetStatus(CharaState state, int animeName = 0)
    {
        AnimatorStateInfo status = a_Animator.GetCurrentAnimatorStateInfo(0);
        float animetime = status.normalizedTime;
        int animehash = status.fullPathHash;
        float animevalue = animetime;
        SetAnimetion(animetime, animevalue, animeName);
        base.SetStatus(state, animeName);
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
    public void PlaySe(string name)
    {
        AudioManager.Instance.PlaySeAudio(name);
    }
    public void StopSe()
    {
        AudioManager.Instance.StopSe();
    }
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        if(m_hp <= 0)
        {
            Die();
        }
    }
    public override void Die()
    {
        base.Die();
        TatuGameManager.Instance.SetMoveFlag(false);
        TatuGameManager.Instance.ActiveHpbar(CharaState.Boss, false);
        BattleManager.Instance.m_CleaStage++;
        NextFrame.Run(this, 1, () =>
        {
            TatuGameManager.Instance.ChangePanel(TatuGameManager.UiPanelState.Score, true);
        });
    }
}
