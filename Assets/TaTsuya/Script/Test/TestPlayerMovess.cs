using System.Linq;
using UnityEngine;

public class TestPlayerMovess :CharaBase
{
    public Vector3 v_scale;
    private GameObject g_Boss;
    private PlayerMoveManager c_PlayerMoveManager;

    public override void Start()
    {
        c_PlayerMoveManager = GetComponent<PlayerMoveManager>();
        e_CharaState = CharaState.Player;
        m_MaxHp = 50;
        c_SaveState.g_Character = this.gameObject;
        base.Start();
        SetStatus(e_CharaState);
        //どこかでどっちから始めるかを設定
        EventEnter();
        SetHp();
        NextFrame.Run(this, 0.5f, () =>
        {
            TatuGameManager.Instance.ActiveHpbar(CharaState.Player, true);
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
        { Debug.Log(GetDieFlag()); }
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
    public override void CheckCollisionBox(float ScaleX, float ScaleY, Vector3 MyPos, Vector3 OppPos, float damage = 0, bool IsFly = false)
    {
        if (IsFly)
        {
            if (GetIsHitFlag() == true) return;
            var jump = c_PlayerMoveManager.JumpParame();
            var dx = Mathf.Abs(MyPos.x - OppPos.x);
            if (dx < ScaleX && (c_PlayerMoveManager.GetIsGround() || Mathf.Abs(jump) > 8.5f))
            {
                SetIsHitFlag(true);
                TakeDamage(damage);
            }
            return;
        }
        base.CheckCollisionBox(ScaleX, ScaleY, MyPos, OppPos, damage);
    }
    public override void SetStatus(CharaState state, int animeName = 0)
    {
        if (c_PlayerMoveManager.GetIsJumping())
        {
            c_SaveState.m_JumpHeightValue = c_PlayerMoveManager.JumpParame();
        }

        AnimatorStateInfo status = a_Animator.GetCurrentAnimatorStateInfo(0);
        float animetime = status.normalizedTime;
        int animehash = status.fullPathHash;
        float animevalue = animetime;
        c_SaveState.b_IsJumpFlag = c_PlayerMoveManager.GetIsJumping();
        
        SetAnimetion(animetime, animevalue, animeName);
        base.SetStatus(state, animeName);
    }
    public override void GetStatus(StageSaveData data)//前回のステータスをセット        
    {
        base.GetStatus(data);
        c_PlayerMoveManager.SetJumpFlag(data.c_PlayerData.b_IsJumpFlag);
        if (GetIsAttackFlag()== true)
        {
            Debug.Log("ssss");
            a_Animator.Play(data.c_PlayerData.m_AnimeHash, 0, data.c_PlayerData.m_AnimeTime);
        }
        else
        {
            a_Animator.Play("Idle", 0, 0);
        }
        if (c_PlayerMoveManager.GetIsJumping() == true)
        {
            c_PlayerMoveManager.SetJump(data.c_PlayerData.m_JumpHeightValue);
        }
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
        if (GetDieFlag() == true) return;
        base.TakeDamage(damage);
        if (m_hp <= 0)
        {
            Die();
        }
    }
    public override void Die()
    {
        base.Die();
        TatuGameManager.Instance.SetMoveFlag(false);
        TatuGameManager.Instance.ActiveHpbar(CharaState.Boss, false);
        SaveManager.Instance.c_CurrentData.c_PlayerData.b_DieFlag = true; 
        NextFrame.Run(this, 0.5f, () =>
        {
            TatuGameManager.Instance.ChangePanel(TatuGameManager.UiPanelState.Score, true);
        });
    }
}
