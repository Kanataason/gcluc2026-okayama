using System;
using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
[RequireComponent(typeof(SpriteRenderer),typeof(Animator))]
public class CharaBase : MonoBehaviour
{
    public virtual void OnDisable()
    {
        Debug.Log("イベント解除");
        BattleManager.Instance.OnSetStageInfo -= ChangePlayer;
        BattleManager.Instance.OnGetStageInfo -= GetStatus;
    }
    public virtual void Start()
    {

        s_Sprite = GetComponent<SpriteRenderer>();
        a_Animator = GetComponent<Animator>();

        SortOrderManager.Instance.SetList(s_Sprite);
    }
    public virtual void Update() { }//キーの取得だけ

    public virtual void FixedUpdate() { }//主な処理はこっち
    public virtual void SetPos(Vector3 Pos) { transform.position = Pos; }//セット
    public virtual Vector3 GetPos() { return transform.position; }//保存

    public virtual void ReverseSprite(CharaState targetstate,Vector3 CharaScale)//向きを変える 1左 0右
    {
        var target = SaveManager.Instance.c_CurrentData.GetCharacter(targetstate);
        if (target == null) return;
        int dir = transform.position.x > target.transform.position.x ? 1 : 0;
        Func<int, bool> func = targetstate == CharaState.Boss
            ? (v => v == 0)
            : (v => v == 1);

        if (dir != CurrentDirection)
        {
            Debug.Log($"向き変更{this.name}");
            CurrentDirection = dir;
            transform.localScale = func(dir)
                ? CharaScale
                : new Vector3(-CharaScale.x, CharaScale.y, CharaScale.z);
        }
    }
    public virtual void SetStatus(CharaState state, int animeName =0) //画面切り替え時点何をしているのかhp,flagや（アニメーション）を保存
                                   //アニメーションが現在何秒まで進んでいるのかを
    {
        c_SaveState.m_Inihp = m_hp;
        c_SaveState.v_IniPosition = GetPos();
        c_SaveState.q_IniRotate = transform.rotation;
        c_SaveState.b_IsAttack = GetIsAttackFlag();
        c_SaveState.m_AnimeHashName = animeName;
        if(a_Animator != null) a_Animator.speed = 1;
        SaveManager.Instance.SetSaveData(state, c_SaveState);
    }
    public virtual void GetStatus(StageSaveData data) //前回のステータスをセット
    {
        SaveState save = null;
        if(e_CharaState == CharaState.Player) { save = SaveManager.Instance.c_CurrentData.GetPlayerState(data); }
        else if (e_CharaState == CharaState.Boss) { save = SaveManager.Instance.c_CurrentData.GetBossState(data); }

        if (save == null) return;

        //if (GetAnimeHashCode() != 0)
        //     a_Animator.SetFloat(GetAnimeHashCode(), save.m_AnimeStateValue);
        Debug.Log(save.m_AnimeHash);
        a_Animator.Play(save.m_AnimeHash, 0,save.m_AnimeTime);

        m_hp = save.m_Inihp;
        SetPos(save.v_IniPosition);
        transform.rotation = save.q_IniRotate;
        m_IsAttack = save.b_IsAttack;
    }
    public virtual void SetAnimetion(float CurrentAnime, float animeValue, int animeName)//現在のアニメーションの進行度、アニメのステートの値、名前
    {
        c_SaveState.m_AnimeTime = CurrentAnime;
        c_SaveState.m_AnimeHash = animeName;
        c_SaveState.m_AnimeStateValue = animeValue;
    }
    public virtual void CheckGround(float Min,float Max)//移動制限
    {
        Vector3 pos = transform.position;
        float ClampY = Mathf.Clamp(pos.y, Min, Max);
        float ClampX = Mathf.Clamp(pos.x, -23, 23);
        transform.position = new Vector3(ClampX,ClampY,ClampY);
    }
    public virtual void CheckCollision(float ScaleX,float ScaleY,Vector3 MyPos,Vector3 OppPos)//当たり判定 奥行きはｚで判定
    {
        if (GetIsHitFlag()) return;

        float dx = Mathf.Abs(MyPos.x - OppPos.x);
        float dz = Mathf.Abs(MyPos.z - OppPos.z);


        if (dx < ScaleX && dz < ScaleY)
        {
            SetIsHitFlag(true);
        }
    }

    public virtual void TakeDamage(int damage) { m_hp -= damage; }//攻撃を食らったときの関数
    public virtual void Die() { Debug.Log("死んだ"); }//死んだとき

    public virtual void SetIsAttackFlag(bool active) { m_IsAttack = active; }//攻撃初めのフラグ
    public virtual void SetIsHitFlag(bool active) { m_IsHit = active; }//攻撃が当たったときのフラグ
    public virtual bool GetIsAttackFlag() { return m_IsAttack; }
    public virtual bool GetIsHitFlag() { return m_IsHit; }
    public virtual int GetAnimeHashCode() { return c_SaveState.m_AnimeHashName; }
    public virtual void SetAnimaType(int type) { m_AnimeHashType = type; }//アニメーションタイプを設定

    public virtual void ChangePlayer() { }

    protected SpriteRenderer s_Sprite;
    protected Animator a_Animator;
    protected int m_hp;
    protected int m_AnimeHashType;
    protected SaveState c_SaveState = new SaveState();//自分にあったSaveManagerにあるものに書き込む
    protected CharaState e_CharaState;//自分が何のキャラクターかしまう変数
    private bool m_IsAttack = false;//攻撃をしているか
    private bool m_IsHit = false;//攻撃が当たっているか

    private int CurrentDirection;
}
[System.Serializable]
public class SaveState
{

    public GameObject g_Character;//playerを設定
    //アニメの処理
    public float m_AnimeTime;//現在のアニメ進行時間
    public int m_AnimeHash;//どこでアニメーションをするか
    public int m_AnimeHashName;//アニメーション単体の名前
    public float m_AnimeStateValue;//現在のアニメのステートの値

    public int m_Inihp;//現在のhp
    public Vector3 v_IniPosition;//現在の自分の場所
    public Quaternion q_IniRotate;//現在の回転値
    public bool b_IsAttack;//攻撃フラグ

    //ボス戦用
    public float m_ActionTime;//現在のアニメーションの時間
    public BossBehaviorManager.BossAwake e_BossAwake;
    public bool b_IsMove;
    
    public List<BossBulletManager> l_ObjList = new List<BossBulletManager>();

    public void Init()
    {
        m_AnimeHash = 0;
        m_AnimeStateValue = 0;
        m_AnimeTime = 0;
        m_AnimeHashName = 0;

        m_Inihp = 0;
        v_IniPosition = Vector3.zero;
        q_IniRotate = Quaternion.identity;
        b_IsAttack = false;
        b_IsMove = false;

    }
}