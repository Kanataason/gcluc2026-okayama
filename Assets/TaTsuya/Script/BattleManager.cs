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
    public int m_CurrentRound;
    public float m_TimeLimit;
    public float m_CurrentTime;
    void Start()
    {
        InitTransition();

        SaveManager save = SaveManager.Instance;
        save.c_Stage1SaveData = new StageSaveData();
        save.c_Stage2SaveData = new StageSaveData();
    }
    private void InitTransition()//Ç±Ç±Ç≈èÛë‘Çí«â¡
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
        protected override void OnEnter(State prevstate)//Ç±Ç±Ç≈èáî‘ÇämîF
        {
            Debug.Log("s");
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
    private class GameStay : State
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
}
