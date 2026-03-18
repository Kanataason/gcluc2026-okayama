using JetBrains.Annotations;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using State = StateMachine<BattleManager>.State;
public class BattleManager : MonoBehaviour
{
    public enum BattleState
    {
        GameStating,
        GamePlaying,
        GamePose,
        GameEnd,
        Menu
    }
    private static readonly int StartAnima = Animator.StringToHash("Start");
    public static BattleManager Instance { get; private set; }

    private StateMachine<BattleManager> c_BattleManager;
    private SaveManager c_SaveData;
    private BossBehaviorManager c_BossBehaviorManager;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public int m_CurrentRound = 1;
    public float m_TimeLimit;
    public float m_CurrentTime;
    public float m_TimeScore;
    public bool b_IsLoading = false;
    public int m_CleaStage;

    public event Action<StageSaveData> OnGetStageInfo;
    public event Action OnSetStageInfo;

    public TextMeshProUGUI debagte;
    public TextMeshProUGUI Stage;
    public Animator a_CanvasAnima;

    private void OnDestroy()
    {
        OnGetStageInfo = null;
        OnSetStageInfo = null;
    }
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
        m_CurrentTime = 0;
    }
    private void InitTransition()//ここで状態を追加
    {
        c_BattleManager = new StateMachine<BattleManager>(this);

        c_BattleManager.AddTransition<Other, GameEnter>((int)BattleState.GameStating);
        c_BattleManager.AddTransition<GameEnter, GameStay>((int)BattleState.GamePlaying);
        c_BattleManager.AddTransition<GameStay, GameExit>((int)BattleState.GameEnd);
        c_BattleManager.AddTransition<GameExit, GameEnter>((int)BattleState.GameStating);
        c_BattleManager.AnyAddTrasition<GameStop>((int)BattleState.GamePose);
        c_BattleManager.Start<Other>();

    }
    private void Update()
    {
        c_BattleManager.Updata();
    }

    public void SetAnimaEvent(int Anima)//0true  1 false これは切り替え時の演出用
    {
        bool IsStart = Anima == 0 ? true : false;

        if (IsStart) a_CanvasAnima.SetTrigger(StartAnima);
        else c_BattleManager.Dispatch((int)BattleState.GameStating);
    }
    public void ChangeState(int state)
    {
        if (m_CleaStage > 2) SceneManager.LoadScene("title");
        c_BattleManager.Dispatch(state);
        TatuGameManager.Instance.ChangePanel(TatuGameManager.UiPanelState.Score, false); }
    private class Other: State
    {
        protected override void OnEnter(State prevstate)
        {
            NextFrame.Run(owner, 0.4f, () =>
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
            owner.b_IsLoading = false;
            Load();
        }
        protected override void OnUpdata()
        {
            stateMachine.Dispatch((int)BattleState.GamePlaying);
        }
        protected override void OnExit(State nextstate)
        {
             Debug.Log("終わり");  
        }
        private void Load()
        {
            manager = stateMachine.owner;
            var data = manager.c_SaveData.c_CurrentData;
            owner.m_TimeScore = data.m_TimeScore;
            Camera.main.transform.position = data.v_CameraPos;
            TatuGameManager.Instance.ActiveHpbar(CharaState.Boss, data.b_IsTeleport);
            TatuGameManager.Instance.SetMoveFlag(data.b_IsTeleport);
            TatuGameManager.Instance.ChangeAwake(data.e_Awake);
            manager.OnGetStageInfo?.Invoke(data);
            Debug.Log(data.m_CurrentAudioTime); 
            if (TatuGameManager.Instance.m_BossTeleport == true)
                AudioManager.Instance.PlayBGMAudio("ボス",data.m_CurrentAudioTime);
        }
    }
    private class GameStay : State//ランダムで切り替え時間を設定
    {
        private float m_RandamNum;
        private float m_UpdataTimer;
        protected override void OnEnter(State prevstate)
        {
            m_UpdataTimer = 0;
            m_RandamNum = 10;//(int)UnityEngine.Random.Range(30,100);
        }
        protected override void OnUpdata()
        {
            m_UpdataTimer += Time.deltaTime;
            owner.m_TimeScore += Time.deltaTime;

            if (owner.m_CleaStage > 0) return;
            if (!TatuGameManager.Instance.GetCameraMoveflag()) return;

            m_RandamNum -= Time.deltaTime;
            if (m_RandamNum <= 0.5f)
            {
                m_UpdataTimer = 1;
                stateMachine.Dispatch((int)BattleState.GameEnd);
            }
            //if (m_UpdataTimer >= 0.1f)
            //{
            //    m_UpdataTimer = 0;
            //    owner.debagte.SetText("切り替え時間 : {0:0}", m_RandamNum);
            //}
        }   
        protected override void OnExit(State nextstate)
        {
            owner.b_IsLoading = true;
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
            SetData();
            Debug.Log(owner.m_CleaStage);
            owner.OnSetStageInfo?.Invoke();//セーブ

            manager.c_SaveData.CheckRound();
            owner.Stage.text = $"次はPlayer{owner.m_CurrentRound}\n 現在{manager.c_SaveData.c_CurrentData.m_TotalRound}ラウンド目";
            if (owner.m_CleaStage > 1)
            {
                NextFrame.Run(owner, 1, () =>
                {
                    var stage1 = owner.c_SaveData.GetCurrentData(1);
                    var stage2 = owner.c_SaveData.GetCurrentData(2);

                    var St1Die = stage1.b_IsBossDie == true ? true : false;
                    var St2Die = stage2.b_IsBossDie == true ? true : false;

                    if (St1Die || St2Die)
                    {
                        if (stage1.m_TimeScore < stage2.m_TimeScore)
                        {
                            TatuGameManager.Instance.ResaltPanel($"プレイヤー１の勝ち");
                        }
                        else
                        {
                            TatuGameManager.Instance.ResaltPanel($"プレイヤー２の勝ち");
                        }
                    }
                    else
                    {
                        TatuGameManager.Instance.ResaltPanel($"勝者無し");
                    }
                });
                owner.m_CleaStage++;
            }
            else
            {
                owner.SetAnimaEvent(0);
            }

        }
        protected override void OnUpdata()
        {
        }
        protected override void OnExit(State nextstate)
        {
            base.OnExit(nextstate);
    
        }
        private void SetData()
        {
            manager = stateMachine.owner;
            StageSaveData CurrentData = manager.c_SaveData.c_CurrentData;
            CurrentData.m_CurrentAudioTime = AudioManager.Instance.GetTime();
            AudioManager.Instance.StopBGM();
            CurrentData.m_TotalRound++;
            CurrentData.m_TimeScore = owner.m_TimeScore;
            CurrentData.b_IsTeleport = TatuGameManager.Instance.m_BossTeleport;
            CurrentData.v_CameraPos = Camera.main.transform.position;
            CurrentData.e_Awake = TatuGameManager.Instance.e_Awake;
            TatuGameManager.Instance.SetMoveFlag(false);
            TatuGameManager.Instance.ActiveHpbar(CharaState.Boss, false);

            manager.m_CurrentRound++;
            if (manager.m_CurrentRound > 2) manager.m_CurrentRound = 1;

            owner.c_SaveData.SetCurrenBossInfo(owner.c_BossBehaviorManager.e_AwakeHp, owner.c_BossBehaviorManager.m_CurrentActionTime);
        }
    }
}
