using System;
using UnityEngine;
using UnityEngine.VFX;
using static UnityEngine.AdaptivePerformance.Provider.AdaptivePerformanceSubsystemDescriptor;

public class BossBulletManager : MonoBehaviour
{
    public float m_DestroyTime = 0.5f;
    public float m_StartAnima = 1.5f;
    private float m_Time = 0;
    private float m_AnimaTime = 0;
    private bool m_IsStop = false;
    private bool m_IsFirst = false;
    public AttackInfo c_AttackInfo = new AttackInfo();

    public Animator a_Anima;
    public SpriteRenderer s_Sprite;
    public VisualEffect c_Vfx;
    public Renderer r_Renderer;

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
                a_Anima.SetTrigger("Attack");
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
        if (c_Vfx == null) gameObject.SetActive(false);
        else { c_Vfx.pause = false; r_Renderer.enabled = true; }
    }
    public void RestartClock()//状況をセットするとき
    {
        if (c_Vfx == null) gameObject.SetActive(true);
        else { c_Vfx.pause = false; r_Renderer.enabled = true; }
        
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
