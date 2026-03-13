using System;
using UnityEngine;
using UnityEngine.VFX;
using static UnityEngine.AdaptivePerformance.Provider.AdaptivePerformanceSubsystemDescriptor;

public class BossBulletManager : MonoBehaviour
{
    private static readonly int m_AttackHash = Animator.StringToHash("Attack");

    public float m_DestroyTime = 0.5f;
    public float m_StartAnima = 1.5f;
    private float m_Time = 0;
    private float m_AnimaTime = 0;
    private bool m_IsStop = false;
    private bool m_IsFirst = false;
    public AttackInfo c_AttackInfo = new AttackInfo();

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
       if(a_Anima != null) m_IsStop = true;
    }
    public void Init(float timer,bool IsStop)
    {
        m_StartAnima = timer;
        m_IsStop = IsStop;
        m_IsFirst = false;
        SortOrderManager.Instance.SetSpriteOrder(s_Sprite);
    }
    // Update is called once per frame
    void Update()
    {
        if (BattleManager.Instance.b_IsLoading) return;

        if (!m_IsFirst) m_AnimaTime += Time.deltaTime;
        else m_Time += Time.deltaTime;

        if (m_AnimaTime >= m_StartAnima)
        {
            m_IsFirst = true;
            m_AnimaTime = 0;
            m_IsStop = false;
            if (a_Anima != null)
                a_Anima.SetTrigger(m_AttackHash);
        }
        if (m_IsStop) return;
        if (m_Time > m_DestroyTime)
        {
            DestroyInfo();
        }
    }
    private void DestroyInfo()
    {
        m_Time = 0;
        DestroyObjEvent?.Invoke(this.gameObject,m_CharaType,m_EfectType);
    }
    public void StopClock()//時間が来た時
    {
        m_IsStop = true;

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
        float Delay = 0.5f;
        float VfxValue = 0;
        ObjActive(IsPause, IsEnable,Delay, VfxValue);
    }
    public void RestartClock()//状況をセットするとき
    {
        bool IsPause = false;
        bool IsEnable = true;
        float Delay = 0f;
        float VfxValue = c_VfxInfo.m_VfxValue;
        ObjActive(IsPause, IsEnable,Delay, VfxValue);

        if (a_Anima != null)
        {
            a_Anima.Play(c_AttackInfo.m_Animahash, 0, c_AttackInfo.m_CurrentAnimaTime);
            m_AnimaTime = c_AttackInfo.m_AnimaTime;
            a_Anima.speed = 1;
        }
        m_IsStop = false;
        m_Time = c_AttackInfo.m_CurrentTime;
        c_AttackInfo.Init();
  
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