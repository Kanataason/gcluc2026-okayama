using System;
using UnityEngine;

public class BossBulletManager : MonoBehaviour
{
    public float m_DestroyTime = 0.5f;
    private float m_Time = 0;
    private bool m_IsStop = false;
    public AttackInfo c_AttackInfo = new AttackInfo();
    public Animator a_Anima;

    public event Action<GameObject,int,int> DestroyObjEvent;

    public int m_CharaType;
    public int m_EfectType;

    void Start()
    {
        if (a_Anima != null)
        {
            m_IsStop = true;
            NextFrame.Run(this, 3, () =>
            {
                m_IsStop = false;
                a_Anima.SetTrigger("Attack");
            });

        }
    }
    // Update is called once per frame
    void Update()
    {
        if (m_IsStop) return;
        m_Time += Time.deltaTime;
        if(m_Time > m_DestroyTime)
        {
            DestroyInfo();
        }
    }
    private void DestroyInfo()
    {
        m_Time = 0;
        DestroyObjEvent?.Invoke(this.gameObject,m_CharaType,m_EfectType);
    }
    public void StopClock()
    {
        m_IsStop = true;
        c_AttackInfo.m_CurrentTime = m_Time;
        gameObject.SetActive(false);
    }
    public void RestartClock()
    {
        m_IsStop = false;
        m_Time = c_AttackInfo.m_CurrentTime;
        gameObject.SetActive(true);
    }
}
[Serializable]
public class AttackInfo//セーブ用
{
    public GameObject g_Obj;//実体
    public float m_CurrentTime;//現在の進行時間
    public Vector3 v_IniPos;//現在の位置
}
