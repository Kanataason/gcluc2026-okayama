using UnityEngine;

public class PlayerMove :CharaBase
{
    private GameObject g_Boss;
    private PlayerMoveManager c_PlayerMoveManager;

    public override void Start()
    {
        Init();
        base.Start();
        NextFrame.Run(this, 0.5f, () =>
        {
            SetHp();
            if (c_Stage == null) return;
            c_Stage.c_Hud.SetActive(CharaState.Player, true);
            g_Boss = c_Stage.GetCharacter(CharaState.Boss);
        });
    }
    private void Init()
    {
        c_PlayerMoveManager = GetComponent<PlayerMoveManager>();

        e_CharaState = CharaState.Player;
        m_MaxHp = 50;
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
        base.Die();//結果の表示はBattleManagerが行う
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
