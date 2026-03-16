using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;
using static UnityEngine.AdaptivePerformance.Provider.AdaptivePerformanceSubsystemDescriptor;

public class BossBulletManager : MonoBehaviour
{
    private static readonly int m_AttackHash = Animator.StringToHash("Attack");

    public float m_MyScaleX;
    public float m_MyScaleY;

    public float m_DestroyTime = 0.5f;
    public float m_StartAnima = 1.5f;
    private float m_Time = 0;
    private float m_AnimaTime = 0;
    private Vector3 v_CurrentDirection;

    private bool b_IsMove = false;
    private bool b_IsStop = false;
    private bool b_IsFirst = false;
    private bool b_IsCollider = false;

    private CharaBase g_Player = null;
    private CharaBase g_Boss = null;

    public AttackInfo c_AttackInfo = new AttackInfo();

    public BossBehaviorManager.BossAttackType e_Attacktype;

    public Animator a_Anima;
    public SpriteRenderer s_Sprite;
    public Renderer r_Renderer;
    public VfxInfo c_VfxInfo;

    public event Action<GameObject,int,int> DestroyObjEvent;

    public int m_CharaType;
    public int m_EfectType;

    void Start()
    {
        m_AnimaTime = 0;
       if(a_Anima != null) b_IsStop = true;
    }
    public void Init(float timer,bool IsStop,bool IsFirst,CharaBase Chara = null,bool IsAttack =true)
    {
        g_Player = Chara;
        m_StartAnima = timer;
        b_IsStop = IsStop;
        b_IsFirst = IsFirst;
        m_Time = 0;
        b_IsMove = false;
        ActiveCollider(IsAttack);
       if(s_Sprite != null) SortOrderManager.Instance.SetSpriteOrder(s_Sprite);
    }

    // Update is called once per frame
    void Update()
    {
        if (BattleManager.Instance.b_IsLoading) return;

        if (!b_IsFirst) m_AnimaTime += Time.deltaTime;
        else m_Time += Time.deltaTime;

        if (m_AnimaTime >= m_StartAnima)
        {
            b_IsFirst = true;
            m_AnimaTime = 0;
            b_IsStop = false;
            if (a_Anima != null)
                a_Anima.SetTrigger(m_AttackHash);
            ActiveCollider(true);
        }
        if (b_IsStop) return;
        if (m_Time > m_DestroyTime)
        {
            DestroyInfo();
        }

        if (b_IsMove)
        {
            if (g_Player == null) return;

           if(b_IsCollider) g_Player.CheckCollision(m_MyScaleX,m_MyScaleY, transform.position, g_Player.transform.position);
            transform.Translate(v_CurrentDirection * 10f * Time.deltaTime);
        }
    }
    private void DestroyInfo()
    {
        Init(0,true, false,null,false);
        DestroyObjEvent?.Invoke(this.gameObject,m_CharaType,m_EfectType);
    }
    public void Move(CharaBase Chara)
    {
        if(g_Player == null) g_Player = Chara;
        if(g_Boss == null) g_Boss = SaveManager.Instance.c_CurrentData.GetCharacter(CharaState.Boss).GetComponent<CharaBase>();

        float height = Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;

        int Direction = g_Boss.CurrentDirection == 1 ? 1 : -1; // 1:left -1:right

        Vector3 left = new Vector3(Camera.main.transform.position.x - width, TatuGameManager.Instance.m_StageScaleMinY / 2, 0);
        Vector3 right = new Vector3(Camera.main.transform.position.x + width, TatuGameManager.Instance.m_StageScaleMinY / 2, 0);
        v_CurrentDirection = Direction == 1
            ? (left - right).normalized
            : (right - left).normalized;

        b_IsMove = true;
    }
    public void StopClock()//時間が来た時
    {
        b_IsStop = true;

        if (a_Anima != null)
        {
            var info = a_Anima.GetCurrentAnimatorStateInfo(0);
            c_AttackInfo.m_CurrentAnimaTime = info.normalizedTime;
            c_AttackInfo.m_Animahash = info.fullPathHash;
            c_AttackInfo.m_AnimaTime = m_AnimaTime;
            a_Anima.speed = 0;
        }
        c_AttackInfo.m_CurrentTime = m_Time;

        bool IsPause = true;
        bool IsEnable = false;
        bool IsAttack = false;
        float Delay = 0.5f;
        float VfxValue = 0;
        ObjActive(IsPause, IsEnable,Delay, VfxValue);
        ActiveCollider(IsAttack);
    }
    public void RestartClock()//状況をセットするとき
    {
        bool IsPause = false;
        bool IsEnable = true;
        bool IsAttack = true;
        float Delay = 0f;
        float VfxValue = c_VfxInfo.m_VfxValue;
        ObjActive(IsPause, IsEnable,Delay, VfxValue);
        ActiveCollider(IsAttack);
        if (a_Anima != null)
        {
            a_Anima.Play(c_AttackInfo.m_Animahash, 0, c_AttackInfo.m_CurrentAnimaTime);
            m_AnimaTime = c_AttackInfo.m_AnimaTime;
            a_Anima.speed = 1;
        }
        b_IsStop = false;
        m_Time = c_AttackInfo.m_CurrentTime;
        c_AttackInfo.Init();
  
    }
    public void ObjActive(bool IsPause,bool IsEnable,float Delay,float VfxValue)
    {
        bool IsNoVfx = c_VfxInfo.c_Vfx == null;
        bool IsDelay = Delay == 0;

        if (IsNoVfx)
        {
            Debug.Log("s");
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
    public void ActiveCollider(bool IsTrue) { b_IsCollider = IsTrue; }//sinceアニメーションイベント
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