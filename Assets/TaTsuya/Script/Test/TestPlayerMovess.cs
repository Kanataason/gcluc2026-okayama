using UnityEngine;
using static BossAttackManager;

public class TestPlayerMovess :CharaBase
{
    public Vector3 v_scale;
   public override void Start()
    {
        e_CharaState = CharaState.Player;
        m_hp = 10;
        c_SaveState.g_Character = this.gameObject;
        SetStatus(e_CharaState);
        //‚Ç‚±‚©‚Å‚Ç‚Á‚¿‚©‚çn‚ß‚é‚©‚ğİ’è
        SortOrderManager.Instance.SetList(this.gameObject.GetComponent<SpriteRenderer>());
    }
    public override void Update()
    {
        if (TatuGameManager.Instance == null) return;
       // ReverseSprite(CharaState.Boss, v_scale);
        CheckGround(TatuGameManager.Instance.m_StageScaleMinY, TatuGameManager.Instance.m_StageScaleMaxY);

    }
}
