using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class TatuGameManager : MonoBehaviour
{
    public enum UiTextState
    {
        StageInfo = 0,
        Timer =1,
    }
    public enum UiSliderState
    {
        BossHpBar,
        PlayerHpbar
    }
    public enum UiPanelState
    {
        Pose,
        Score,
        tutorial,
    }
    //短くできると思うクラスを使えば ジェネリックがたにしたら行ける

    [SerializeField]GenericDictionary<UiTextState,TextMeshProUGUI> d_TextDictionary = new();
    [SerializeField]GenericDictionary<UiPanelState,GameObject> d_PanelDictionary = new();
    [SerializeField]GenericDictionary<UiSliderState,Image> d_SliderDictionary = new();

    public List<GameObject> l_TutorialList;
    public List<StageInfo> l_Infolist = new();
    public static TatuGameManager Instance { get; private set; }
    //ステージのY軸の制限
    public float m_StageScaleMaxY;
    public float m_StageScaleMinY;

    public bool b_IsTutorial;
    public bool b_BossTeleport;
    private bool b_StopMoveCamera;

    public BossBehaviorManager.BossAwake e_Awake = BossBehaviorManager.BossAwake.FirstForm;
    private GameObject g_Player;
    private GameObject g_Boss;

    public event Action OnBattle;

    private BossBaseManager c_Boss;
    private PlayerMove c_Player;


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
    private void Start()
    {
        BattleManager.Instance.OnCreateCharacters += OnInit;
    }
    private void OnDisable()
    {
        BattleManager.Instance.OnCreateCharacters -= OnInit;

        if (c_Boss == null||c_Player == null) { Debug.Log("解除できなかった"); }
        if (c_Boss != null) c_Boss.OnHpBar -= OnUpdateHpbar;
        if (c_Player != null) c_Player.OnHpBar -= OnUpdateHpbar;
        OnBattle = null;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnInit(GameObject Bo, InputInfo inputInfo)
    {
        b_IsTutorial = true;
        b_StopMoveCamera = false;
        SetCharacter(Bo, inputInfo.TargetObj);
        InitList();
    }
    private void InitList()
    {
        d_PanelDictionary.Init();
        d_SliderDictionary.Init();
        d_TextDictionary.Init();
    }
    private void SetCharacter(GameObject Bo,GameObject Pl)
    {
        Debug.Log($"player{Pl}");
        g_Player = Pl.GetComponentInChildren<PlayerMove>().gameObject;
        g_Boss = Bo;
        EventEnter();
    }
    private void EventEnter()
    {
        c_Boss = g_Boss.GetComponent<BossBaseManager>();
        c_Player = g_Player.GetComponentInChildren<PlayerMove>();
        c_Boss.OnHpBar += OnUpdateHpbar;
        c_Player.OnHpBar += OnUpdateHpbar;
    }
    private void FixedUpdate()
    {
        if (b_StopMoveCamera == true) return;

        if (g_Player != null)
            CheckCollision(1, l_Infolist[(int)e_Awake / 2].m_EncounterBorder, g_Player.transform.position.x);
    }
    public void CheckCollision(float ScaleX,float MyX, float OpX)//当たり判定 奥行きはｚで判定
    { 

        float dx = Mathf.Abs(MyX - OpX);

        if (dx < ScaleX)//ここでリストのやつを更新
        {
            ApplyAwake();
            OnBattle?.Invoke();
        }
    }
    private void ApplyAwake()
    {
        switch (e_Awake)
        {
            case BossBehaviorManager.BossAwake.FirstForm:
                AudioManager.Instance.PlayBGMAudio("ボス", 0);
                e_Awake = BossBehaviorManager.BossAwake.SecondForm; break;
            case BossBehaviorManager.BossAwake.SecondForm:
                e_Awake = BossBehaviorManager.BossAwake.FinalForm; break;
            case BossBehaviorManager.BossAwake.FinalForm: break;
        }
    }
    public void SetMoveFlag(bool IsTrue)
    {
        b_StopMoveCamera = IsTrue;
        b_BossTeleport = IsTrue;
    }
    public bool GetCameraMoveflag() => b_StopMoveCamera;
    public void ChangeAwake(BossBehaviorManager.BossAwake awake)
    {
        e_Awake = awake;
    }
    private void OnUpdateHpbar(CharaState state,float hp)
    {
        var slider = state == CharaState.Player ? d_SliderDictionary.Get(UiSliderState.PlayerHpbar) :
                                               d_SliderDictionary.Get(UiSliderState.BossHpBar);
        slider.fillAmount = hp;
    }
    public void ActiveHpbar(CharaState state,bool IsActive)
    {
        var slider = state == CharaState.Player ? d_SliderDictionary.Get(UiSliderState.PlayerHpbar) :
                                       d_SliderDictionary.Get(UiSliderState.BossHpBar);

        slider.transform.parent.gameObject.SetActive(IsActive);
    }
    public void ChangePanel(UiPanelState state,bool IsActive)
    {
        c_Player.SetDieFlag(true);

        var panel = d_PanelDictionary.Get(state);

        if (panel == null) return;
        panel.SetActive(IsActive);


        var data = SaveManager.Instance.GetCurrentData(BattleManager.Instance.m_CurrentRound);

        SaveData(data);//表示前にデータを保存
        ActiveText();//Uiに表示
    }
    private void SaveData(StageSaveData data)
    {
        SaveManager.Instance.c_CurrentData.m_TimeScore = BattleManager.Instance.m_TimeScore;
        data.m_TimeScore = BattleManager.Instance.m_TimeScore;
    }
    private void ActiveText()
    {
        var text = d_TextDictionary.Get(UiTextState.StageInfo);

        if (c_Boss.GetDieFlag() == true)
            text.text = $"タイム\n\n {(int)SaveManager.Instance.c_CurrentData.m_TimeScore}";
        else text.text = "死んでしまった。";
    }

    public void ResaltPanel(string Score)
    {
        var panel = d_PanelDictionary.Get(UiPanelState.Score);
        var text = d_TextDictionary.Get(UiTextState.StageInfo);

        if (panel == null) return;
        panel.SetActive(true);
        text.text = Score;
    }
    //----------チュートリアル処理
    int Count = -1;
    public void Tutorial(int IsYes)//1 true
    {
        d_PanelDictionary.Get(UiPanelState.tutorial).SetActive(false);
        int prev = Count;
        if (IsYes == 0)//戻るボタンを押したら
        {
            if (Count == -1) { b_IsTutorial = false; return; }
            Count--;
            l_TutorialList[prev].SetActive(false);
            l_TutorialList[Count].SetActive(true);
            return;
        }

        Count++;

        if (Count >= l_TutorialList.Count)//最後のページに到達したら
        {
            b_IsTutorial = false;
            l_TutorialList[prev].SetActive(false);
            d_PanelDictionary.Get(UiPanelState.tutorial).SetActive(false);
            return;
        }
        l_TutorialList[Count].SetActive(true);

        if (prev == -1) return;
        l_TutorialList[prev].SetActive(false);

    }

}
[Serializable]
public class StageInfo
{
    public float m_EncounterBorder;
    public Vector3 v_TeleportPos;
}
