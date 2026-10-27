using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using State = StateMachine<BattleManager>.State;
public class BattleManager : MonoBehaviour
{
    private static readonly int StartAnima = Animator.StringToHash("Start");
    private const int StageLayerBase = 7;//1P:レイヤー8 2P:レイヤー9
    private const float BossDefeatResultDelay = 3f;//ボスを倒してから結果を出すまで
    private const float PlayerDieResultDelay = 1f;//やられてから結果を出すまで
    private const float ResultButtonDelay = 1f;//結果画面のボタンを選べるようになるまで(攻撃・ジャンプの連打で押さないため)
    private const float CountdownTime = 3f;//開始前のカウントダウンの秒数
    private const float GoTextTime = 1f;//「GO!」を出しておく時間

    public enum TextType
    {
        Stage,
        TIme,
        StageInfo
    }
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
    public ObjectLists<GameObject, CharaState> l_CharaList;
    //-----------参照先
    private StateMachine<BattleManager> c_BattleManager;
    private readonly List<StageContext> l_Stages = new();
    //-----------バトルの情報
    public bool b_IsLoading = false;//trueの間はキャラと弾が止まる(開始前のカウントダウン中・決着後)
    private bool b_IsFinished;
    private bool b_IsBossBgmPlaying;
    //-----------ポーズ
    private GameObject g_PausePanel;
    private bool b_IsPaused;
    private bool b_LoadingBeforePause;//ポーズ前のb_IsLoading(カウントダウン中にポーズしたときのため)
    [SerializeField] private float m_InterferencePowerUp = 0.5f;//相手がボスを覚醒させたときに上がる、自分のボスの攻撃力の倍率
    //-----------カメラ
    [SerializeField] private float m_SplitCameraSizeRate = 0.5f;//2画面のときのカメラの表示サイズの倍率(0.5で横幅が1画面のときと同じ)
    private float m_CameraY;
    public event Action<GameObject, InputInfo> OnCreateCharacters;
    //---------Ui関連
    public Animator a_CanvasAnima;
    public ObjectLists<TextMeshProUGUI, TextType> l_TextList;
    [SerializeField] private float m_CountdownFontSize = 120f;

    private void OnDestroy()
    {
        OnCreateCharacters = null;
    }
    void Start()
    {
        Application.targetFrameRate = 60;
        Time.timeScale = 1;//ポーズ中にシーンを移動してきた場合のため
        b_IsLoading = true;//カウントダウンが終わるまで動かさない
        InitList();
        TatuGameManager.Instance.Init();
        g_PausePanel = TatuGameManager.Instance.CreatePauseMenu(Resume, ReturnToTitle, QuitGame);

        CreateStages(CheckConnectingDeviceManager.Instance.GetDeviceList());
        InitTransition();
    }
    private void InitList()
    {
        l_TextList.Init();
        l_CharaList.Init();
    }
    //-----------ステージの生成
    private void CreateStages(List<InputDevice> devices)//つながっているデバイスの数だけステージを作る(最大2)
    {
        bool isSplit = devices.Count >= 2;
        var viewports = new Rect[devices.Count];
        for (int i = 0; i < devices.Count; i++)
            viewports[i] = GetViewport(i + 1, isSplit);

        var huds = TatuGameManager.Instance.CreateHuds(viewports);
        var cameras = CreateCameras(devices.Count, viewports, isSplit);

        for (int i = 0; i < devices.Count; i++)
        {
            int stageNum = i + 1;
            var stage = new GameObject($"Stage{stageNum}").AddComponent<StageContext>();
            stage.Init(stageNum, StageLayerBase + stageNum, cameras[i], huds[i]);
            l_Stages.Add(stage);

            var info = new InputInfo()
            {
                ControllerDevice = devices[i],
                ControllerScheme = GetControlScheme(devices[i]),
                PlayerNum = stageNum
            };
            CreateCharacter(info, stage);
        }
    }
    private Rect GetViewport(int stageNum, bool isSplit)//上が1P、下が2P
    {
        if (!isSplit) return new Rect(0, 0, 1, 1);
        return stageNum == 1 ? new Rect(0, 0.5f, 1, 0.5f) : new Rect(0, 0, 1, 0.5f);
    }
    private Camera[] CreateCameras(int count, Rect[] viewports, bool isSplit)//シーンのカメラを1P用にして、2P用は複製する
    {
        Camera mainCamera = Camera.main;
        float baseSize = mainCamera.orthographicSize;
        float size = isSplit ? baseSize * m_SplitCameraSizeRate : baseSize;
        m_CameraY = size - baseSize;//画面の下端を1画面のときと揃える(元のカメラはy=0)

        int allStageMask = 0;
        for (int i = 0; i < count; i++)
            allStageMask |= 1 << (StageLayerBase + i + 1);

        var cameras = new Camera[count];
        for (int i = 0; i < count; i++)
        {
            Camera camera = i == 0 ? mainCamera : CloneCamera(mainCamera, i + 1);
            camera.rect = viewports[i];
            camera.orthographicSize = size;
            //他のステージのレイヤーは映さない
            camera.cullingMask = (camera.cullingMask & ~allStageMask) | (1 << (StageLayerBase + i + 1));
            cameras[i] = camera;
        }
        return cameras;
    }
    private Camera CloneCamera(Camera original, int stageNum)
    {
        var clone = Instantiate(original.gameObject);
        clone.name = $"Stage{stageNum} Camera";
        clone.tag = "Untagged";//Camera.mainは1P側のまま

        var listener = clone.GetComponent<AudioListener>();//AudioListenerは1つだけにする
        if (listener != null) Destroy(listener);

        return clone.GetComponent<Camera>();
    }
    private string GetControlScheme(InputDevice device)
    {
        if (device is Gamepad)
            return "Gamepad";

        if (device is Keyboard || device is Mouse)
            return "Keyboard&Mouse";

        return "Unknown";
    }
    private void CreateCharacter(InputInfo info, StageContext stage)
    {
        var player = PlayerInput.Instantiate(l_CharaList.Get(CharaState.Player),
            controlScheme: info.ControllerScheme,
            pairWithDevice: info.ControllerDevice).gameObject;
        var boss = Instantiate(l_CharaList.Get(CharaState.Boss));
        info.TargetObj = player;

        //PlayerInputは子のPlayerに付いているので、影(Shadow)も含めて一番上の親から設定する
        SetStage(player.transform.root.gameObject, stage);
        SetStage(boss, stage);

        var playerMove = player.GetComponentInChildren<PlayerMove>();
        var playerManager = player.GetComponentInChildren<PlayerManager>();
        var bossBehavior = boss.GetComponent<BossBehaviorManager>();
        if (playerMove == null || playerManager == null || bossBehavior == null)
        {
            Debug.LogError("コンポーネントがない");
            return;
        }
        stage.SetCharacters(playerMove, boss.GetComponent<BossBaseManager>());

        bossBehavior.Init(info, stage);
        playerManager.Init();

        var cameraMove = stage.c_Camera.GetComponent<CameraMove>();
        if (cameraMove != null) cameraMove.Init(stage, m_CameraY);

        OnCreateCharacters?.Invoke(boss, info);
    }
    private void SetStage(GameObject character, StageContext stage)//レイヤーとステージをキャラクターに設定
    {
        StageContext.SetLayerRecursively(character, stage.m_Layer);
        foreach (var chara in character.GetComponentsInChildren<CharaBase>())
            chara.SetStage(stage);
    }
    //-----------勝敗
    public void OnStageFinished(StageContext stage, bool isBossDefeated)//どちらかのステージで決着がついたとき
    {
        if (b_IsFinished) return;
        b_IsFinished = true;

        foreach (var s in l_Stages)
            s.Finish();

        string result = GetResultText(stage, isBossDefeated);
        float delay = isBossDefeated ? BossDefeatResultDelay : PlayerDieResultDelay;
        NextFrame.Run(this, delay, () =>
        {
            b_IsLoading = true;
            UiSelectionKeeper.SuppressFor(ResultButtonDelay);
            TatuGameManager.Instance.ResaltPanel(result);
        });
    }
    private string GetResultText(StageContext stage, bool isBossDefeated)
    {
        if (l_Stages.Count == 1)
            return isBossDefeated ? $"クリア！\nタイム {(int)stage.m_TimeScore}" : "死んでしまった。";

        //先にボスを倒した方の勝ち やられたら相手の勝ち
        StageContext winner = isBossDefeated ? stage : l_Stages.Find(s => s != stage);
        return $"プレイヤー{winner.m_StageNum}の勝ち";
    }
    public void OnBossAwakened(StageContext stage)//ボスを覚醒させたら、相手のボスを強くする(妨害)
    {
        if (b_IsFinished) return;

        foreach (var other in l_Stages)
        {
            if (other == stage) continue;

            other.c_Boss.PowerUp(m_InterferencePowerUp);
            other.ShowInfo($"相手がボスを覚醒させた！\nボスの攻撃力 ×{other.c_Boss.m_AttackParame:0.0}");
            stage.ShowInfo("相手のボスを強くした！");
        }
    }
    public void PlayBossBgm()//最初に接敵したときだけボスBGMを流す
    {
        if (b_IsBossBgmPlaying) return;
        b_IsBossBgmPlaying = true;
        AudioManager.Instance.PlayBGMAudio("ボス", 0);
    }
    //-----------状態
    private void InitTransition()//ここで状態を追加
    {
        c_BattleManager = new StateMachine<BattleManager>(this);

        c_BattleManager.AddTransition<Other, Countdown>((int)BattleState.GameStating);
        c_BattleManager.AddTransition<Countdown, GameStay>((int)BattleState.GamePlaying);
        c_BattleManager.Start<Other>();
    }
    private void Update()
    {
        if (IsPausePressed()) TogglePause();
        if (b_IsPaused) return;

        c_BattleManager.Updata();
    }
    //-----------ポーズ(Esc・コントローラーのStartボタン)
    private bool IsPausePressed()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) return true;
        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad.startButton.wasPressedThisFrame) return true;
        }
        return false;
    }
    private void TogglePause()
    {
        if (b_IsPaused) Resume();
        else Pause();
    }
    private void Pause()
    {
        if (b_IsFinished || g_PausePanel == null) return;//決着後は結果画面を使う

        b_IsPaused = true;
        b_LoadingBeforePause = b_IsLoading;
        b_IsLoading = true;//ポーズ中の入力でキャラが動かないようにする
        Time.timeScale = 0;
        g_PausePanel.SetActive(true);
    }
    private void Resume()//ポーズメニューの「続ける」
    {
        if (!b_IsPaused) return;

        b_IsPaused = false;
        b_IsLoading = b_LoadingBeforePause;
        Time.timeScale = 1;
        g_PausePanel.SetActive(false);
    }
    private void ReturnToTitle()//ポーズメニューの「タイトルへ戻る」
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("title");
    }
    private void QuitGame()//ポーズメニューの「ゲームを終了」
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetAnimaEvent(int Anima)//アニメーションイベント(名前変更不可) 0:演出開始 1:ゲーム開始
    {
        if (Anima == 0) a_CanvasAnima.SetTrigger(StartAnima);
        else c_BattleManager.Dispatch((int)BattleState.GameStating);
    }
    public void ChangeState(int state)//スコア画面のボタンから呼ばれる
    {
        if (b_IsFinished) { SceneManager.LoadScene("title"); return; }
        c_BattleManager.Dispatch(state);
        TatuGameManager.Instance.ChangePanel(TatuGameManager.UiPanelState.Score, false);
    }
    private void InitCountdownText()//カウントダウンは2画面の境目(画面の真ん中)に大きく出す
    {
        var text = l_TextList.Get(TextType.TIme);
        var rect = text.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(600, 300);
        rect.localScale = Vector3.one;//元のテキストはScaleが2倍なので戻す(大きさはm_CountdownFontSizeで決める)

        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = m_CountdownFontSize;
        text.text = "";
    }
    private void ShowCountdown(string info)
    {
        l_TextList.Get(TextType.TIme).text = info;
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
    }
    private class Countdown : State//開始前のカウントダウン 3,2,1
    {
        private float m_RemainingTime;
        private int m_PrevCount;
        protected override void OnEnter(State prevstate)
        {
            owner.b_IsLoading = true;
            m_RemainingTime = CountdownTime;
            m_PrevCount = 0;
            owner.InitCountdownText();
        }
        protected override void OnUpdata()
        {
            m_RemainingTime -= Time.deltaTime;
            if (m_RemainingTime <= 0)
            {
                stateMachine.Dispatch((int)BattleState.GamePlaying);
                return;
            }

            int count = Mathf.CeilToInt(m_RemainingTime);
            if (count != m_PrevCount)
            {
                m_PrevCount = count;
                owner.ShowCountdown(count.ToString());
            }
        }
    }
    private class GameStay : State//プレイ中 タイムは各StageContextが数える
    {
        protected override void OnEnter(State prevstate)
        {
            owner.b_IsLoading = false;
            owner.ShowCountdown("GO!");
            NextFrame.Run(owner, GoTextTime, () => owner.ShowCountdown(""));
        }
    }
}

[Serializable]
public class ObjectLists<T,T1> where T:UnityEngine.Object where T1: Enum
{
    [Serializable]
    public class Parame
    {
        public T1 Key;
        public T Value;
    }
    [SerializeField] private System.Collections.Generic.List<Parame> l_ObjList;

    private System.Collections.Generic.Dictionary<T1, T> d_ObjList = new();

    public void Init()
    {
        foreach(var obj in l_ObjList)
        {
            d_ObjList[obj.Key] = obj.Value;
        }
    }
    public T Get(T1 state)
    {
        if(d_ObjList.TryGetValue(state,out var T))
        {
            return T;
        }
        return null;
    }
}
