using System;
using UnityEngine;
using System.Collections.Generic;
public class TatuGameManager : MonoBehaviour
{
    public static TatuGameManager Instance { get; private set; }

    public float m_StageScaleMaxY;
    public float m_StageScaleMinY;

    private bool m_StopMoveCamera;
    public bool m_BossTeleport;

    public List<StageInfo> l_Infolist = new ();
    public BossBehaviorManager.BossAwake e_Awake = BossBehaviorManager.BossAwake.FirstForm;
    private GameObject g_Player;

    public event Action OnBattle;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_StopMoveCamera = false;
        g_Player = GameObject.FindWithTag("Player");
    }
    private void FixedUpdate()
    {
        if (m_StopMoveCamera == true) return;

        if (g_Player != null)
            CheckCollision(1, l_Infolist[(int)e_Awake / 2].m_EncounterBorder, g_Player.transform.position.x);
    }
    public void CheckCollision(float ScaleX,float MyX, float OpX)//当たり判定 奥行きはｚで判定
    { 

        float dx = Mathf.Abs(MyX - OpX);

        if (dx < ScaleX)//ここでリストのやつを更新
        {
            Debug.Log("ssssss");
            e_Awake = BossBehaviorManager.BossAwake.SecondForm;
            OnBattle?.Invoke();
        }
    }
    public void SetMoveFlag(bool IsTrue)
    {
        m_StopMoveCamera = false;
        m_BossTeleport = false;
    }
    public bool GetCameraMoveflag() => m_StopMoveCamera;
}
[Serializable]
public class StageInfo
{
    public float m_EncounterBorder;
}
