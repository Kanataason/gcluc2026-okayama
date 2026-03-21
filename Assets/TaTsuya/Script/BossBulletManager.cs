using System;
using UnityEngine;
using UnityEngine.VFX;

public class BossBulletManager : MonoBehaviour
{
    public enum BulletState
    {
        Move,
        Stop,
        Destroy,
        Attack,
        None
    }
    private static readonly int m_AttackHash = Animator.StringToHash("Attack");

    //攻撃をする際の当たり判定とダメージ
    public float m_MyScaleX;
    public float m_MyScaleY;
    public int m_Damage;
    public bool b_IsFlyAttack;

    //オブジェクトの情報
    public float m_DestroyTime = 0.5f;
    public float m_StartAnima = 1.5f;
    private float m_Time = 0;
    private float m_AnimaTime = 0;
    private Vector3 v_CurrentDirection;
    //エフェクトのタイプ
    public int m_CharaType;
    public int m_EfectType;
    public BossBehaviorManager.BossAttackType e_Attacktype;
    //フラグ
    private bool b_IsCollider = false;
    private BulletState e_BulletState;

    private CharaBase g_Player = null;
    private CharaBase g_Boss = null;

    private AttackInfo c_AttackInfo = new AttackInfo();
    private Camera c_Camera;

    //オブジェクトによって変える
    public Animator a_Anima;
    public SpriteRenderer s_Sprite;
    public Renderer r_Renderer;
    public VfxInfo c_VfxInfo;

    public event Action<BossBulletManager,int,int> DestroyObjEvent;

    private void OnDestroy()
    {
        DestroyObjEvent = null;
    }
    void Start()
    {
        c_Camera = Camera.main;
        m_AnimaTime = 0;
        if (a_Anima != null) e_BulletState = BulletState.Stop;
    }
    public void Init(float timer,BulletState state,TestPlayerMovess Chara = null,int IsAttack =1)
    {
        e_BulletState = state;
        g_Player = Chara;
        m_StartAnima = timer;
        m_Time = 0;
        ActiveCollider(IsAttack);
       if(s_Sprite != null) SortOrderManager.Instance.SetSpriteOrder(s_Sprite);
    }

    // Update is called once per frame
    void Update()
    {
        if (BattleManager.Instance.b_IsLoading) return;

        //if (!b_IsFirst) m_AnimaTime += Time.deltaTime;
        //else m_Time += Time.deltaTime;

        //if (m_AnimaTime >= m_StartAnima)
        //{
        //    b_IsStop = false;
        //    b_IsFirst = true;
        //    m_AnimaTime = 0;
        //    if (a_Anima != null)
        //        a_Anima.SetTrigger(m_AttackHash);
        //}
        //if (m_Time > m_DestroyTime&&!b_IsStop)
        //{
        //    DestroyInfo();
        //}
        //if (b_IsMove) transform.Translate(v_CurrentDirection * 15f * Time.deltaTime);

        switch (e_BulletState)
        {
            case BulletState.Destroy:
                UpDateDestroyTimer();break;
            case BulletState.Move:
                UpDateMoveTimer();break;
            case BulletState.Attack:
                PlayAttackAnima();break;
            case BulletState.Stop:
                UpDateAnimaTime();break;

        }

    }
    void PlayAttackAnima()
    {
        if (a_Anima != null)
            a_Anima.SetTrigger(m_AttackHash);
        e_BulletState = BulletState.Destroy;
    }
    void UpDateAnimaTime()
    {
        m_AnimaTime += Time.deltaTime;
        if (m_AnimaTime >= m_StartAnima)
        {
            m_AnimaTime = 0;
            e_BulletState = BulletState.Attack;
        }
    }
    void UpDateMoveTimer()
    {
        m_Time += Time.deltaTime;
        if (m_Time > m_DestroyTime)
        {
            DestroyInfo();
            e_BulletState = BulletState.None;
        }
        transform.Translate(v_CurrentDirection * 15f * Time.deltaTime);
    }
    void UpDateDestroyTimer()
    {
        m_Time += Time.deltaTime;
        if (m_Time > m_DestroyTime)
        {
            DestroyInfo();
            e_BulletState = BulletState.None;
        }
    }
    private void FixedUpdate()
    {
        if (g_Player == null) return;

        if (b_IsCollider) g_Player.CheckCollisionBox(m_MyScaleX,m_MyScaleY, transform.position, g_Player.transform.position,m_Damage,b_IsFlyAttack);
    }
    private void DestroyInfo()//破壊されたとき
    {
        Init(0,BulletState.None,null,0);
        DestroyObjEvent?.Invoke(this,m_CharaType,m_EfectType);
    }
    public void Move(TestPlayerMovess chara)//動く攻撃用
    {
        CheckCharacterNull(chara);

        float height = Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;

        int Direction = g_Boss.CurrentDirection == 1 ? 1 : -1; // 1:right -1:reft

        Vector3 left = new Vector3(Camera.main.transform.position.x - width, TatuGameManager.Instance.m_StageScaleMinY / 2, 0);
        Vector3 right = new Vector3(Camera.main.transform.position.x + width, TatuGameManager.Instance.m_StageScaleMinY / 2, 0);
        v_CurrentDirection = Direction == 1
            ? (left - right).normalized
            : (right - left).normalized;

    }
    private void CheckCharacterNull(TestPlayerMovess chara)
    {
        if (g_Player == null) g_Player = chara;
        if (g_Boss == null) g_Boss = SaveManager.Instance.c_CurrentData.GetCharacter(CharaState.Boss).GetComponent<CharaBase>();
    }
    public void PlaySe(string name)
    {
        AudioManager.Instance.StopSe(); 
        AudioManager.Instance.PlaySeAudio(name);
    }
    public void StopSe()
    {
        AudioManager.Instance.StopSe();
    }
    public void StopClock()//時間が来た時
    {
        c_AttackInfo.e_State = e_BulletState;
        e_BulletState = BulletState.None;
        c_AttackInfo.m_CurrentTime = m_Time;

        SetAnimaInfo(); 

        bool IsPause = true;
        bool IsEnable = false;
        int IsAttack = 0;
        float Delay = 0.5f;
        float VfxValue = 0;

        SetObjectInfo(IsPause, IsEnable, Delay, VfxValue, IsAttack);
    }
    private void SetAnimaInfo()
    {
        if (a_Anima != null)
        {
            var info = a_Anima.GetCurrentAnimatorStateInfo(0);
            c_AttackInfo.m_CurrentAnimaTime = info.normalizedTime;
            c_AttackInfo.m_Animahash = info.fullPathHash;
            c_AttackInfo.m_AnimaTime = m_AnimaTime;
            a_Anima.speed = 0;
        }
    }
    public void RestartClock()//状況をセットするとき
    {
        bool IsPause = false;
        bool IsEnable = true;
        int IsAttack = 1;
        float Delay = 0f;
        float VfxValue = c_VfxInfo.m_VfxValue;

        SetObjectInfo(IsPause, IsEnable, Delay, VfxValue, IsAttack);

        PlayAnimation();

        e_BulletState = c_AttackInfo.e_State;
        m_Time = c_AttackInfo.m_CurrentTime;

        c_AttackInfo.Init();
    }
    private void PlayAnimation()
    {
        if (a_Anima != null)
        {
            a_Anima.Play(c_AttackInfo.m_Animahash, 0, c_AttackInfo.m_CurrentAnimaTime);
            m_AnimaTime = c_AttackInfo.m_AnimaTime;
            a_Anima.speed = 1;
        }
    }
    private void SetObjectInfo(bool IsPause,bool IsEnable,float Delay,float VfxValue,int IsAttack)
    {
        ObjActive(IsPause, IsEnable, Delay, VfxValue);
        ActiveCollider(IsAttack);
    }
    public void ObjActive(bool IsPause,bool IsEnable,float Delay,float VfxValue)
    {
        bool IsNoVfx = c_VfxInfo.c_Vfx == null;
        bool IsDelay = Delay == 0;

        if (IsNoVfx)
        {
            if (IsDelay)
                gameObject.SetActive(IsEnable);
            else
                NextFrame.Run(this, Delay, () => { gameObject.SetActive(IsEnable); });
        }
        else
        {
            c_VfxInfo.c_Vfx.pause = IsPause;

            if (!string.IsNullOrEmpty(c_VfxInfo.m_PropertiesName))
                c_VfxInfo.c_Vfx.SetFloat(c_VfxInfo.m_PropertiesName, VfxValue);

            if (IsDelay)
                r_Renderer.enabled = IsEnable;
            else
                NextFrame.Run(this, Delay, () => { r_Renderer.enabled = IsEnable; });
        }
    }
    public void ActiveCollider(int IsTrue) { b_IsCollider = IsTrue == 1 ? true:false; }//sinceアニメーションイベント
}
[Serializable]
public class AttackInfo//セーブ用
{
    public float m_AnimaTime;
    public float m_CurrentAnimaTime;
    public int m_Animahash;

    public float m_VfxTime;

    public GameObject g_Obj;//実体
    public float m_CurrentTime;//現在の進行時間
    public Vector3 v_IniPos;//現在の位置
    public BossBulletManager.BulletState e_State;

    public void Init()
    {
        v_IniPos = Vector3.zero;
        m_CurrentTime = 0;
        m_AnimaTime = 0;
        m_CurrentAnimaTime = 0;
        m_Animahash = 0;
    }
}
[Serializable]
public class VfxInfo
{
    public VisualEffect c_Vfx;
    public string m_PropertiesName;
    public float m_VfxValue;
}