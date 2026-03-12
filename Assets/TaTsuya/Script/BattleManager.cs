using System;
using TMPro;
using UnityEngine;
using State = StateMachine<BattleManager>.State;
public class BattleManager : MonoBehaviour
{
    public float m_StageMax;
    public float m_StageMin;
    public enum BattleState
    {
        GameStating,
        GamePlaying,
        GamePose,
        GameEnd,
        Menu
    }
    public static BattleManager Instance { get; private set; }

    private StateMachine<BattleManager> c_StateMachine;
    private SaveManager c_SaveData;
    private BossBehaviorManager c_BossBehaviorManager;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int m_TotalRound;
    public int m_CurrentRound = 1;
    public float m_TimeLimit;
    public float m_CurrentTime;
    public bool b_IsLoading = false;

    public event Action<StageSaveData> OnGetStageInfo;
    public event Action OnSetStageInfo;

    public TextMeshProUGUI debagte;
    void Start()
    {
        Application.targetFrameRate = 60;
        InitTransition();
        InitRoundInfo();

        c_BossBehaviorManager = GameObject.Find("Boss").GetComponent<BossBehaviorManager>();
        c_SaveData = SaveManager.Instance;
    }
    private void InitRoundInfo()
    {
        m_CurrentRound = 1;
        m_TotalRound = m_CurrentRound;
        m_CurrentTime = 0;
    }
    private void InitTransition()//ここで状態を追加
    {
        c_StateMachine = new StateMachine<BattleManager>(this);

        c_StateMachine.AddTransition<Other, GameEnter>((int)BattleState.GameStating);
        c_StateMachine.AddTransition<GameEnter, GameStay>((int)BattleState.GamePlaying);
        c_StateMachine.AddTransition<GameStay, GameExit>((int)BattleState.GameEnd);
        c_StateMachine.AddTransition<GameExit, GameEnter>((int)BattleState.GameStating);
        c_StateMachine.AnyAddTrasition<GameStop>((int)BattleState.GamePose);
        c_StateMachine.Start<Other>();

    }
    private void Update()
    {
        c_StateMachine.Updata();
    }
    private class Other: State
    {
        protected override void OnEnter(State prevstate)
        {
            NextFrame.Run(owner, 0.5f, () =>
            {
                stateMachine.Dispatch((int)BattleState.GameStating);
            });
        }
        protected override void OnUpdata()
        {

        }
        protected override void OnExit(State nextstate)
        {
            base.OnExit(nextstate);
        }
    }
    private class GameEnter:State//切り替わった瞬間
    {
        BattleManager manager;
        protected override void OnEnter(State prevstate)//ここで順番を確認
        {
            manager = stateMachine.owner;
            var data = manager.c_SaveData.c_CurrentData;
            manager.OnGetStageInfo?.Invoke(data);
        }
        protected override void OnUpdata()
        {
            stateMachine.Dispatch((int)BattleState.GamePlaying);
        }
        protected override void OnExit(State nextstate)
        {
             Debug.Log("終わり");  
        }
    }
    private class GameStay : State//ランダムで切り替え時間を設定
    {
        private float m_RandamNum;
        private float m_UpdataTimer;
        protected override void OnEnter(State prevstate)
        {
            m_UpdataTimer = 0;
            m_RandamNum = (int)UnityEngine.Random.Range(10,15);
        }
        protected override void OnUpdata()
        {
            m_RandamNum -= Time.deltaTime;
            m_UpdataTimer += Time.deltaTime;
            if (m_RandamNum <= 0.5f)
            {
                m_UpdataTimer = 1;
                stateMachine.Dispatch((int)BattleState.GameEnd);
            }
            if (m_UpdataTimer >= 0.1f)
            {
                m_UpdataTimer = 0;
                owner.debagte.SetText("time : {0:0}", m_RandamNum);
            }
        }   
        protected override void OnExit(State nextstate)
        {
            base.OnExit(nextstate);
        }
    }
    private class GameStop : State
    {
        protected override void OnEnter(State prevstate)
        {
            base.OnEnter(prevstate);
        }
        protected override void OnUpdata()
        {
            base.OnUpdata();
        }
        protected override void OnExit(State nextstate)
        {
            base.OnExit(nextstate);
        }
    }
    private class GameExit:State//切り替わる前の処理
    {
        BattleManager manager;
        protected override void OnEnter(State prevstate)
        {
            owner.c_SaveData.SetCurrenBossInfo(owner.c_BossBehaviorManager.e_AwakeHp, owner.c_BossBehaviorManager.m_CurrentActionTime);
            owner.OnSetStageInfo?.Invoke();
            Debug.Log("切り替え");
            manager = stateMachine.owner;
            manager.m_CurrentRound++;
            manager.m_TotalRound++;
            if (manager.m_CurrentRound > 2)
            {
                manager.m_CurrentRound = 1;
            }
            manager.c_SaveData.CheckRound();
        }
        protected override void OnUpdata()
        {
            stateMachine.Dispatch((int)BattleState.GameStating);
        }
        protected override void OnExit(State nextstate)
        {
            base.OnExit(nextstate);
        }
    }
}
