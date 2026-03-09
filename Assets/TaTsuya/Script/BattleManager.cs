using System;
using UnityEngine;
using State = StateMachine<BattleManager>.State;
public class BattleManager : MonoBehaviour
{
    public enum BattleState
    {
        GameStating = 1, 
        GamePose = 2,
        GameEnd = 3,
        GamePlaying = 4,
        Menu = 5
    }
    private StateMachine<BattleManager> c_BattleManager;
    private SaveManager c_SaveData;
    public static BattleManager Instance { get; private set; }
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

    public event Action<StageSaveData> OnSetStageInfo; 
    void Start()
    {
        InitTransition();
        InitRoundInfo();

        c_SaveData = SaveManager.Instance;
        c_SaveData.c_Stage1SaveData = new StageSaveData();
        c_SaveData.c_Stage2SaveData = new StageSaveData();
    }
    private void InitRoundInfo()
    {
        m_CurrentRound = 1;
        m_TotalRound = m_CurrentRound;
        m_CurrentTime = 0;
    }
    private void InitTransition()//ここで状態を追加
    {
        c_BattleManager = new StateMachine<BattleManager>(this);

        c_BattleManager.AddTransition<Other, GameEnter>((int)BattleState.GameStating);
        c_BattleManager.AddTransition<GameEnter, GameStay>((int)BattleState.GamePlaying);
        c_BattleManager.AddTransition<GameStay, GameStop>((int)BattleState.GamePose);
        c_BattleManager.AnyAddTrasition<GameExit>((int)BattleState.GameEnd);
        c_BattleManager.AnyAddTrasition<GameEnter>((int)BattleState.GamePlaying);
        c_BattleManager.Start<Other>();
    }
    private void Update()
    {
        c_BattleManager.Updata();
    }

    private class GameEnter:State
    {
        BattleManager manager;
        protected override void OnEnter(State prevstate)//ここで順番を確認
        {
            Debug.Log("開始");
            manager = stateMachine.owner;
            var data = manager.m_CurrentRound == 1 ? manager.c_SaveData.c_Stage1SaveData : manager.c_SaveData.c_Stage2SaveData;
            manager.OnSetStageInfo?.Invoke(data);
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
    private class GameStay : State//ランダムで切り替え時間を設定
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
    private class GameExit:State
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

    private class Other : State
    {
        protected override void OnEnter(State prevstate)
        {
            SaveManager save = SaveManager.Instance;
            save.c_Stage1SaveData.InitState();
            save.c_Stage2SaveData.InitState();
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
}
