using System.Linq;
using UnityEngine;

public class PlayerMove :CharaBase
{
    public Vector3 v_scale;
    private GameObject g_Boss;
    private PlayerMoveManager c_PlayerMoveManager;
    private PlayerManager c_PlayerManager;

    public override void Start()
    {
        Init();
        base.Start();
        //どこかでどっちから始めるかを設定
        EventEnter();
        SetStatus(e_CharaState);
        NextFrame.Run(this, 0.5f, () =>
        {
            SetHp();
            TatuGameManager.Instance.ActiveHpbar(CharaState.Player, true);
            g_Boss = SaveManager.Instance.c_CurrentData.GetCharacter(CharaState.Boss);
        });
    }
    private void Init()
    {
        c_PlayerMoveManager = GetComponent<PlayerMoveManager>();
        c_PlayerManager = GetComponent<PlayerManager>();

        e_CharaState = CharaState.Player;
        m_MaxHp = 50;
        c_SaveState.g_Character = this.gameObject;
    }
    private void EventEnter()//イベントを登録
    {
        Debug.Log("イベント登録");
        BattleManager.Instance.OnSetStageInfo += ChangePlayer;
        BattleManager.Instance.OnGetStageInfo += GetStatus;
    }
    public override void Update()
    {
        base.Update();
        if (TatuGameManager.Instance == null) return;

        if (g_Boss != null) CheckCollisionBox(2, 0.7f, transform.position, g_Boss.transform.position, 7);

         CheckGround(0, 0);
    }
    public override void CheckGround(float Min, float Max)
    {
        float height = c_Camera.orthographicSize;
        float width = height * c_Camera.aspect;

        float offset = 1f;

        Vector3 pos = transform.position;

        float camX = c_Camera.transform.position.x;
        float ClampX = Mathf.Clamp(pos.x,
                                   camX - width + offset,
                                   camX + width - offset);
        pos.x = ClampX;
        pos.z = c_PlayerMoveManager.GetShadowGroundY();
        transform.position = pos;
    }
    public override void ChangePlayer()//切り替え処理
    {
        if (c_SaveState.b_IsNextFrame)
        {
            NextFrame.OneFrame(this, () => { a_Animator.speed = 0; });
        }
        else
        {
            a_Animator.speed = 0;
        }
        SetStatus(e_CharaState, c_PlayerMoveManager.GetAnimaHashName());
    }
    public override void CheckCollisionBox(float ScaleX, float ScaleY, Vector3 MyPos, Vector3 OppPos, float damage = 0, bool IsFly = false)
    {
        if (IsFly)//ジャンプが必要な当たり判定
        {
            if (GetIsHitFlag() == true) return;

            var jump = c_PlayerMoveManager.GetJumpParame();
            var dx = Mathf.Abs(MyPos.x - OppPos.x);

            if (dx < ScaleX && (c_PlayerMoveManager.GetIsGround() || Mathf.Abs(jump) > 8.5f))
            {
                TakeDamage(damage);
            }
            return;
        }
        base.CheckCollisionBox(ScaleX, ScaleY, MyPos, OppPos, damage);
    }
    public override void SetStatus(CharaState state, int animeName = 0)
    {
        c_SaveState.b_IsJumpFlag = c_PlayerMoveManager.GetIsJumping();
        c_SaveState.e_PlayerState = c_PlayerManager.GetState();
        c_SaveState.m_GroundY = c_PlayerMoveManager.GetShadowGroundY();
        c_SaveState.b_IsGround = c_PlayerMoveManager.GetIsGround();
        c_SaveState.m_JumpHeightValue = c_PlayerMoveManager.GetJumpParame();
        c_SaveState.m_JumpVelocity = c_PlayerMoveManager.GetVelocity();

        InitState();

        base.SetStatus(state, animeName);
    }
    private void InitState()
    {
        c_PlayerMoveManager.SetJump(0);
        c_PlayerMoveManager.SetVelocity(0);
    }
    public override void GetStatus(StageSaveData data)//前回のステータスをセット        
    {
        base.GetStatus(data);
        var PlayerData = data.c_PlayerData;

        TryReproduction(PlayerData);//リストにある物を再現

        ReverseSprite(CharaState.Boss, v_scale);//向きを調整

        SetPlayerInfo(PlayerData);

        //アタックしていたらアニメーションを再生
        if (GetIsAttackFlag()== false)
            a_Animator.Play("Idle", 0, 0);

        //if (c_PlayerMoveManager.GetIsJumping() == true)
        //{
        //    c_PlayerMoveManager.SetJump(data.c_PlayerData.m_JumpHeightValue);
        //}

      //  NextFrame.OneFrame(this, () => { data.InitState(); });//データを初期化
    }
    private void SetPlayerInfo(SaveState data)
    {
        //状態処理
        c_PlayerManager.SetPlayerState(data.e_PlayerState);
        //ジャンプの処理
        c_PlayerMoveManager.SetShadowGroundY(data.v_IniPosition.y);
        c_PlayerMoveManager.SetJump(data.m_JumpHeightValue);
        c_PlayerMoveManager.SetVelocity(data.m_JumpVelocity);
        c_PlayerMoveManager.SetGround(data.b_IsGround);
        c_PlayerMoveManager.SetJumpFlag(data.b_IsJumpFlag);
    }
    private void TryReproduction(SaveState PlayerData)
    {
        if (PlayerData.l_ObjList != null && PlayerData.l_ObjList.Count > 0)
        {
            Debug.Log("再現");
            foreach (var obj in PlayerData.l_ObjList)
                obj.RestartClock();

            PlayerData.l_ObjList.Clear();
            c_SaveState.l_ObjList?.Clear();
            SaveManager.Instance.RemoveList(e_CharaState, BattleManager.Instance.m_CurrentRound);
        }
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
        SetDieFlag(true);
        base.Die();

        NextFrame.Run(this, 0.5f, () =>
        {
            TatuGameManager.Instance.ChangePanel(TatuGameManager.UiPanelState.Score, true);
        });
    }
    //アニメーションイベントから呼ばれる
    public void PlaySe(string name)
    {
        AudioManager.Instance.PlaySeAudio(name);
    }
    public void StopSe()
    {
        AudioManager.Instance.StopSe();
    }
}
