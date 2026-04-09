using JetBrains.Annotations;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;
using State = StateMachine<BattleManager>.State;
public class BattleManager : MonoBehaviour
{
    private static readonly int StartAnima = Animator.StringToHash("Start");
    public enum BattleState
    {
        GameStating,
        GamePlaying,
        GamePose,
        GameEnd,
        Menu
    }
    public static BattleManager Instance { get; private set; }
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
    //-----------ターゲットのオブジェクト
    public GameObject g_PrefabBoss;
    public GameObject g_PrefabPlayer;
    public GameObject g_PrefabCamera;
    //-----------参照先
    private StateMachine<BattleManager> c_BattleManager;
    private SaveManager c_SaveData;
    private BossBehaviorManager c_BossBehaviorManager;
    private BossBaseManager c_BossBaseManager;
    private PlayerMove c_TestPlayerMove;
    //-----------バトルの情報
    public int m_CurrentRound = 1;
    public float m_TimeLimit;
    public float m_CurrentTime;
    public float m_TimeScore;
    public bool b_IsLoading = false;
    public int m_CleaStage;
    //-------データをセットするアクション
    public event Action<StageSaveData> OnGetStageInfo;
    public event Action OnSetStageInfo;
    public event Action<GameObject, InputInfo> OnCreateCharacters;
    //---------Ui関連
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

        c_SaveData = SaveManager.Instance;
        SetInputDevice();
    }
    private void SetInputDevice()
    {
        var deviceList = CheckConnectingDeviceManager.Instance.GetDeviceList();
        int Count = 1;
        foreach (var device in deviceList)
        {
            var ControlScheme = GetControlScheme(device);
            Debug.Log(ControlScheme);
            var list = new InputInfo()
            {
                ControllerDevice = device,
                ControllerScheme = ControlScheme,
                PlayerNum = Count
            };
            Count++;
            CreateCharacter(list);
        }
    }
    private string GetControlScheme(InputDevice device)
    {
        if (device is Gamepad)
            return "Gamepad";

        if (device is Keyboard || device is Mouse)
            return "Keyboard&Mouse";

        return "Unknown";
    }
    private void CreateCharacter(InputInfo devicelist)
    {
        var player = PlayerInput.Instantiate(g_PrefabPlayer,
            controlScheme:devicelist.ControllerScheme,
            pairWithDevice: devicelist.ControllerDevice).gameObject;
        var boss = Instantiate(g_PrefabBoss);
        devicelist.TargetObj = player;
        GetComponents(boss,devicelist);
        OnCreateCharacters?.Invoke(boss,devicelist);
    }
    private void GetComponents(GameObject boss,InputInfo inputInfo)
    {
            c_BossBehaviorManager = boss.GetComponent<BossBehaviorManager>();
            c_BossBaseManager = boss.GetComponent<BossBaseManager>();

        c_TestPlayerMove = inputInfo.TargetObj.GetComponentInChildren<PlayerMove>();
        var playermanager = inputInfo.TargetObj.GetComponentInChildren<PlayerManager>();
       // var cameraMove = camera.GetComponent<CameraMove>();
        if (c_BossBehaviorManager == null || c_TestPlayerMove == null)
        {
            Debug.LogError("コンポーネントがない");
            return;
        }
        //ここでplayerの初期化も呼んであげる
        //cameraMove.Init(camera);
        c_BossBehaviorManager.Init(inputInfo);
        playermanager.Init();
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
        if (m_CleaStage > 2) { SceneManager.LoadScene("title"); return; }
        c_BattleManager.Dispatch(state);
        TatuGameManager.Instance.ChangePanel(TatuGameManager.UiPanelState.Score, false); 
    }
    private void InitTextInfo(string info,int size)
    {
        debagte.text = info;
        debagte.fontSize = size;
    }
    private class Other: State
    {
        protected override void OnEnter(State prevstate)
        {
            NextFrame.Run(owner, 0.3f, () =>
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
            owner.InitTextInfo("", 32);

           owner.c_TestPlayerMove.SetDieFlag(false);
            owner.c_BossBaseManager.SetDieFlag(false);

            manager = stateMachine.owner;
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
            var data = manager.c_SaveData.c_CurrentData;

            LoadBossInfo(data);

            LoadBattleInfo(data);
        }
        private void LoadBossInfo(StageSaveData data)
        {
            TatuGameManager.Instance.ActiveHpbar(CharaState.Boss, data.b_IsTeleport);
            TatuGameManager.Instance.SetMoveFlag(data.b_IsTeleport);
            TatuGameManager.Instance.ChangeAwake(data.e_Awake);

            if (TatuGameManager.Instance.b_BossTeleport == true)
                AudioManager.Instance.PlayBGMAudio("ボス", data.m_CurrentAudioTime);
        }
        private void LoadBattleInfo(StageSaveData data)
        {
            owner.m_TimeScore = data.m_TimeScore;
            Camera.main.transform.position = data.v_CameraPos;
            manager.OnGetStageInfo?.Invoke(data);
        }
    }
    private class GameStay : State//ランダムで切り替え時間を設定
    {
        private float m_RandamNum;
        private float m_UpdataTimer;
        private int m_Prevtime;

        private float m_Strength = 10;
        private float m_Dump = 0.15f;
        private Coroutine c_BlinkingCoroutine;
        protected override void OnEnter(State prevstate)
        {
            m_RandamNum = (int)UnityEngine.Random.Range(50,100);
            Init();
        }
        private void Init()
        {
            m_Prevtime = (int)m_RandamNum;
            m_UpdataTimer = 0;
            c_BlinkingCoroutine = null;
        }
        protected override void OnUpdata()
        {
            m_UpdataTimer += Time.deltaTime;
            if (owner.m_CleaStage > 0) return;
            if (!TatuGameManager.Instance.GetCameraMoveflag()) return;

            m_RandamNum -= Time.deltaTime;
            owner.m_TimeScore += Time.deltaTime;//タイムスコア計算
            if (m_RandamNum <= 0.5f)
            {
                m_UpdataTimer = 1;
                stateMachine.Dispatch((int)BattleState.GameEnd);
            }
            if (m_RandamNum <= 30&&Mathf.RoundToInt(m_RandamNum) != m_Prevtime)
            {
                if(m_RandamNum <= 10)
                    UpdateTextBlinking();

                m_UpdataTimer = 0;
                m_Prevtime = (int)m_RandamNum;
                owner.debagte.SetText("切り替え時間 : {0:0}", (int)m_RandamNum);
            }
        }   

        protected override void OnExit(State nextstate)
        {
            owner.b_IsLoading = true;
            base.OnExit(nextstate);
        }
        private void UpdateTextBlinking()
        {
            if (c_BlinkingCoroutine != null)
            {
                c_BlinkingCoroutine = null;
            }
            c_BlinkingCoroutine = owner.StartCoroutine(TextBlinking());
        }
        private IEnumerator TextBlinking()
        {
            float duraction = 1;
            float eplased = 0;
            float offset = 2;

            while (eplased < duraction)
            {
                eplased += Time.deltaTime;
                var t = eplased / duraction;

                var pulse = 1 + Mathf.Sin(eplased * m_Strength) * (1 - t) * (1 - t)* (1 - t)* m_Dump;
                owner.debagte.rectTransform.localScale = (Vector3.one*offset) * pulse;
                yield return null;
            }
            c_BlinkingCoroutine = null;
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
            owner.OnSetStageInfo?.Invoke();//セーブ
            InitStageInfo();

            manager.c_SaveData.CheckRound();
            owner.Stage.text = $"次はPlayer{owner.m_CurrentRound}\n 現在{manager.c_SaveData.c_CurrentData.m_TotalRound}ラウンド目";

            if (owner.m_CleaStage <= 1) { owner.SetAnimaEvent(0); return; }

            CheckResalt();
          
            owner.m_CleaStage++;

        }
        protected override void OnExit(State nextstate)
        {
            base.OnExit(nextstate);
    
        }
        private void CheckResalt()
        {
            NextFrame.Run(owner, 1, () =>
            {
                var stage1 = owner.c_SaveData.GetCurrentData(1);
                var stage2 = owner.c_SaveData.GetCurrentData(2);

                var St1Die = stage1.c_BossData.b_DieFlag;
                var St2Die = stage2.c_BossData.b_DieFlag;

                var P1Die = stage1.c_PlayerData.b_DieFlag;
                var P2Die = stage2.c_PlayerData.b_DieFlag;

                if (P1Die && !P2Die)
                {
                    TatuGameManager.Instance.ResaltPanel("プレイヤー２の勝ち");
                    return;
                }
                if (!P1Die && P2Die)
                {
                    TatuGameManager.Instance.ResaltPanel("プレイヤー１の勝ち");
                    return;
                }

                if (St1Die || St2Die)
                {
                    if (stage1.m_TimeScore < stage2.m_TimeScore)
                    {
                        TatuGameManager.Instance.ResaltPanel("プレイヤー１の勝ち");
                    }
                    else
                    {
                        TatuGameManager.Instance.ResaltPanel("プレイヤー２の勝ち");
                    }
                }
                else
                {
                    TatuGameManager.Instance.ResaltPanel("引き分け");
                }
            });
        }
        private void SetData()
        {
            manager = stateMachine.owner;
            StageSaveData CurrentData = manager.c_SaveData.c_CurrentData;

            CurrentData.m_CurrentAudioTime = AudioManager.Instance.GetTime();
            CurrentData.m_TotalRound++;
            CurrentData.m_TimeScore = owner.m_TimeScore;
            CurrentData.b_IsTeleport = TatuGameManager.Instance.b_BossTeleport;
            CurrentData.v_CameraPos = Camera.main.transform.position;
            CurrentData.e_Awake = TatuGameManager.Instance.e_Awake;

        }
        private void SetInfo()//
        {
            AudioManager.Instance.AllStopAudio();
            TatuGameManager.Instance.SetMoveFlag(false);
            TatuGameManager.Instance.ActiveHpbar(CharaState.Boss, false);
        }
        private void InitStageInfo()//ステージの情報を更新
        {
            owner.m_TimeScore = 0;

            manager.m_CurrentRound++;
            Debug.Log("ラウンド追加");
            if (manager.m_CurrentRound > 2) manager.m_CurrentRound = 1;

            SetInfo();

            owner.c_SaveData.SetCurrenBossInfo(owner.c_BossBehaviorManager.e_AwakeHp, owner.c_BossBehaviorManager.m_CurrentActionTime);
        }
    }
}
