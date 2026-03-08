using System;
using UnityEngine;

public class BossBulletManager : MonoBehaviour
{
    private float m_DestroyTime = 5;
    private float m_Time = 0;
    public float m_CurrentProgressTime;
    AttackInfo c_AttackInfo = new AttackInfo();

    public event Action<GameObject> DestroyObjEvent;
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        m_Time += Time.deltaTime;
        if(m_Time > m_DestroyTime)
        {
            DestroyInfo();
            Debug.Log("壊した");
        }
    }
    private void DestroyInfo()
    {
        m_Time = 0;
        DestroyObjEvent?.Invoke(this.gameObject);
    }
}
[Serializable]
public class AttackInfo//セーブ用
{
    public GameObject g_Obj;//実体
    public float m_CurrentTime;//現在の進行時間
    public Vector3 v_IniPos;//現在の位置
}
