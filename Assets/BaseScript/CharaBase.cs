using System;
using System.Collections.Generic;
//using UnityEditor.Overlays;
using UnityEngine;
[RequireComponent(typeof(SpriteRenderer),typeof(Animator))]
public class CharaBase : MonoBehaviour
{
    public virtual void OnDisable()
    {
        Debug.Log("イベント解除");
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnSetStageInfo -= ChangePlayer;
            BattleManager.Instance.OnGetStageInfo -= GetStatus;
        }

        SortOrderManager.Instance.RemoveList(s_Sprite);
        OnHpBar = null;
    }
    public virtual void Start()
    {
        c_Camera = Camera.main;
        s_Sprite = GetComponent<SpriteRenderer>();
        a_Animator = GetComponent<Animator>();

        m_hp = m_MaxHp;

        SortOrderManager.Instance.SetList(s_Sprite);
    }
    public virtual void Update() 
    {
        if (BattleManager.Instance.b_IsLoading) return;

        if (GetIsHitFlag() == false) return;

        HitTime += Time.deltaTime;

        Color col = s_Sprite.color;
        col.a = Mathf.PingPong(HitTime * 8f, 1f);
        s_Sprite.color = col;

        if (HitTime > Duraction)
        {
            EndHitEffect();
        }
    }//キーの取得だけ
    private void EndHitEffect()
    {
        Debug.Log("終わり");

        HitTime = 0;

        var col = s_Sprite.color;
        col.a = 1f;
        s_Sprite.color = col;

        SetIsHitFlag(false);
    }

    public virtual void FixedUpdate() { }//主な処理はこっち
    public virtual void SetPos(Vector3 Pos) { transform.position = Pos; }//セット
    public virtual Vector3 GetPos() { return transform.position; }//保存

    public virtual void ReverseSprite(CharaState targetstate,Vector3 CharaScale)//向きを変える -1左1右
    {
        var target = SaveManager.Instance.c_CurrentData.GetCharacter(targetstate);
        if (target == null) return;

        int dir = transform.position.x > target.transform.position.x ? 1 : -1;
        Func<int, bool> func = targetstate == CharaState.Boss
            ? (v => v == -1)
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
        c_SaveState.m_HitTimer = HitTime;

        HitTime = 0;

        if (a_Animator != null)
        {
            AnimatorStateInfo status = a_Animator.GetCurrentAnimatorStateInfo(0);
            float animetime = status.normalizedTime;
            int animehash = status.fullPathHash;
            float animevalue = animetime;

            switch (m_AnimeHashType)
            {
                case 0: if (animetime != 0) animevalue = a_Animator.GetFloat(animeName); break;
                default: Debug.Log("取る必要ない"); break;
            }
            SetAnimetion(animetime, animevalue, animehash);
        }
        SaveManager.Instance.SetSaveData(state, c_SaveState);
    }
    public virtual void GetStatus(StageSaveData data) //前回のステータスをセット
    {
        SaveState save = null;

        if(e_CharaState == CharaState.Player) { save = SaveManager.Instance.c_CurrentData.GetPlayerState(data); }
        else if (e_CharaState == CharaState.Boss) { save = SaveManager.Instance.c_CurrentData.GetBossState(data); }

        if (save == null) return;

        if (a_Animator != null)
        {
            a_Animator.Play(save.m_AnimeHash, 0, save.m_AnimeTime);
            a_Animator.speed = 1;
            foreach (var param in a_Animator.parameters)
            {
                if (param.nameHash == save.m_AnimeHashName)
                {
                    switch (param.type)
                    {
                        case AnimatorControllerParameterType.Float:
                            float f = a_Animator.GetFloat(save.m_AnimeHashName);
                            Debug.Log($"Float: {f}");
                            break;

                        case AnimatorControllerParameterType.Bool:
                            bool b = a_Animator.GetBool(save.m_AnimeHashName);
                            Debug.Log($"Bool: {b}");
                            break;

                        case AnimatorControllerParameterType.Int:
                            int i = a_Animator.GetInteger(save.m_AnimeHashName);
                            Debug.Log($"Int: {i}");
                            break;

                        case AnimatorControllerParameterType.Trigger:
                            Debug.Log("Trigger（取得不可）");
                            break;
                    }
                }
            }
        }
        //ステータスをセット
        m_hp = save.m_Inihp;
        transform.rotation = save.q_IniRotate;
        SetPos(save.v_IniPosition);
        SetIsAttackFlag(save.b_IsAttack);
        HitTime = save.m_HitTimer;

        SetHp();//UiにHpを反映
    }
    public virtual void SetAnimetion(float CurrentAnime, float animeValue, int animeName)//現在のアニメーションの進行度、アニメのステートの値、名前
    {
        c_SaveState.m_AnimeTime = CurrentAnime;
        c_SaveState.m_AnimeHash = animeName;
        c_SaveState.m_AnimeStateValue = animeValue;
    }
    public virtual void CheckGround(float Min, float Max)
    {
        float height = c_Camera.orthographicSize;
        float width = height * c_Camera.aspect;

        float offset = 1f;

        Vector3 pos = transform.position;

        float ClampY = Mathf.Clamp(pos.y, Min, Max);

        float camX = c_Camera.transform.position.x;
        float ClampX = Mathf.Clamp(pos.x,
                                   camX - width + offset,
                                   camX + width - offset);
        transform.position = new Vector3(ClampX, ClampY, ClampY);
    }
    public virtual void CheckCollisionBox(float ScaleX, float ScaleY, Vector3 MyPos, Vector3 OppPos, float damage = 0,bool IsFly = false)//当たり判定 奥行きはｚで判定
    {
        //ジャンプは別の変数で管理をしてそれを判定する
        if (GetIsHitFlag() == true) return;

        float dx = Mathf.Abs(MyPos.x - OppPos.x);
        float dz = Mathf.Abs(MyPos.z - OppPos.z);
        if (dx < ScaleX && dz < ScaleY)
        {
            Debug.Log("当たった");
            TakeDamage(damage);
        }
    }
    public virtual void CheckCollision2DSphere(float Range, Vector3 MyPos, Vector3 OppPos)
    {
        Vector3 Distance = (MyPos - OppPos);
        Debug.Log(Distance.sqrMagnitude);

        if(Distance.sqrMagnitude < Range * Range)
        {
            Debug.Log("あった田");
        }
    }

    public virtual void TakeDamage(float damage)//攻撃を食らったときの関数
    {
        Debug.Log("攻撃を受けた");
        SetIsHitFlag(true);

        m_hp -= damage;
        AudioManager.Instance.PlaySeAudio("Hit");
        SetHp();
    }
    public virtual void Die() 
    {
        Debug.Log("死んだ"); 
        BattleManager.Instance.m_CleaStage++;
        //内部処理を止める、ボスのHPバーを非表示
        TatuGameManager.Instance.SetMoveFlag(false);
        TatuGameManager.Instance.ActiveHpbar(CharaState.Boss, false);

        TryCharaState();
    }//死んだとき

    private void TryCharaState()
    {
        switch (e_CharaState)
        {
            case CharaState.Player:
                SaveManager.Instance.c_CurrentData.c_PlayerData.b_DieFlag = true; break;
            case CharaState.Boss:
                SaveManager.Instance.c_CurrentData.c_BossData.b_DieFlag = true; break;
        }
    }

    public virtual void SetDieFlag(bool IsDie) { b_IsDie = IsDie; }

    public virtual bool GetDieFlag() { return b_IsDie; }//死んだときのフラグ

    public virtual void SetIsAttackFlag(bool active) { m_IsAttack = active; }//攻撃初めのフラグ
    public virtual void SetIsHitFlag(bool active) { m_IsHit = active; }//攻撃が当たったときのフラグ
    public virtual bool GetIsAttackFlag() { return m_IsAttack; }//攻撃フラグを取る
    public virtual bool GetIsHitFlag() { return m_IsHit; }//ヒットフラグを取る
    public virtual int GetAnimeHashCode() { return c_SaveState.m_AnimeHashName; }
    public virtual void SetAnimaType(int type) { m_AnimeHashType = type; }//アニメーションタイプを設定

    public virtual void SetHp() //HpBarに反映させる処理
    {
        float value = m_hp / m_MaxHp;
        var clamp = Mathf.Clamp01(value);//０～１に収める
        OnHpBar?.Invoke(e_CharaState,clamp);
    }
    public virtual void ChangePlayer() { }

    protected SpriteRenderer s_Sprite;
    protected Animator a_Animator;
    protected float m_hp;//マックスhpを参照する
    protected float m_MaxHp;//自分の設定する
    protected int m_AnimeHashType;//
    protected SaveState c_SaveState = new SaveState();//自分にあったSaveManagerにあるものに書き込む
    protected CharaState e_CharaState;//自分が何のキャラクターかしまう変数

    private bool m_IsAttack = false;//攻撃をしているか
    private bool m_IsHit = false;//攻撃が当たっているか
    private bool b_IsDie = false;//死んだときのフラグ

    public int CurrentDirection;//現在の向き

    public event Action<CharaState,float> OnHpBar;

    //時間の管理
    private float HitTime = 0;
    private float Duraction = 1f;
    protected Camera c_Camera;

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

    public float m_Inihp;//現在のhp
    public Vector3 v_IniPosition;//現在の自分の場所
    public Quaternion q_IniRotate;//現在の回転値
    public bool b_IsAttack;//攻撃フラグ
    public bool b_IsNextFrame;//次のフレームにアクションイベントを呼ぶ
    public float m_HitTimer;//無敵時間
    public bool b_DieFlag;//死亡フラグ
    //プレイヤー専用
    public Player.PlayerState e_PlayerState;

    public float m_JumpHeightValue;
    public float m_JumpVelocity;
    public bool b_IsJumpFlag;
    public float m_GroundY;
    public bool b_IsGround;

    //ボス戦用
    public float m_ActionTime;//現在のアニメーションの時間
    public BossBehaviorManager.BossAwake e_BossAwake;//段階
    public bool b_IsMove;//コルーチンのフラグ
    public bool b_IsTransparent;//現在透明化かどうか

    public List<BossBulletManager> l_ObjList = new List<BossBulletManager>();

    public void Init()//初期化
    {
        m_AnimeHash = 0;
        m_AnimeStateValue = 0;
        m_AnimeTime = 0;
        m_AnimeHashName = 0;
        m_ActionTime = 0;

        m_Inihp = 0;
        m_HitTimer = 0;
        v_IniPosition = Vector3.zero;
        q_IniRotate = Quaternion.identity;
        b_IsAttack = false;
        b_IsMove = false;
        b_IsTransparent = false;
        b_IsNextFrame = false;
    }
}