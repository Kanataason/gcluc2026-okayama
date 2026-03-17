using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
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
    [Serializable]
    public class Texts
    {
        public UiTextState e_TextState;
        public TextMeshProUGUI t_Text;
    }
    [Serializable]
    public class Hpbars
    {
        public UiSliderState e_SliderState;
        public Image s_Image;
    }
    [Serializable]
    public class Panel
    {
        public UiPanelState e_PanelState;
        public GameObject g_Panel;
    }
    public List<Texts> l_TextList;
    public List<Hpbars> l_SliderList;
    public List<Panel> l_PanelList;
    private Dictionary<UiTextState, TextMeshProUGUI> d_TextDictionary = new();
    private Dictionary<UiSliderState, Image> d_SliderDictionary = new();
    private Dictionary<UiPanelState, GameObject> d_PanelDictionary = new();

    public List<GameObject> l_TutorialList;

    public static TatuGameManager Instance { get; private set; }

    public float m_StageScaleMaxY;
    public float m_StageScaleMinY;
    public bool m_IsTutorial;

    private bool m_StopMoveCamera;
    public bool m_BossTeleport;

    public List<StageInfo> l_Infolist = new ();
    public BossBehaviorManager.BossAwake e_Awake = BossBehaviorManager.BossAwake.FirstForm;
    private GameObject g_Player;
    private GameObject g_Boss;

    public event Action OnBattle;

    private CharaBase c_Boss;
    private CharaBase c_Player;


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

    private void OnDisable()
    {
        if(c_Boss == null||c_Player == null) { Debug.Log("解除できなかった"); }
        if (c_Boss != null) c_Boss.OnHpBar -= OnUpdateHpbar;
        if (c_Player != null) c_Player.OnHpBar -= OnUpdateHpbar;
        OnBattle = null;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_IsTutorial = true;
        m_StopMoveCamera = false;
        g_Player = GameObject.FindWithTag("Player");
        g_Boss = GameObject.FindWithTag("Boss");
        InitList();
        EventEnter();
    }
    private void InitList()
    {
        foreach (var text in l_TextList)
        {
            d_TextDictionary[text.e_TextState] = text.t_Text;
        }
        foreach (var slider in l_SliderList)
        {
            d_SliderDictionary[slider.e_SliderState] = slider.s_Image;
        }
        foreach(var panel in l_PanelList)
        {
            d_PanelDictionary[panel.e_PanelState] = panel.g_Panel;
        }
    }
    private void EventEnter()
    {
        c_Boss = g_Boss.GetComponent<CharaBase>();
        c_Player = g_Player.GetComponent<TestPlayerMovess>();

        c_Boss.OnHpBar += OnUpdateHpbar;
        c_Player.OnHpBar += OnUpdateHpbar;
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
            AudioManager.Instance.PlayBGMAudio("ボス",0);
            Debug.Log("ssssss");
            if(e_Awake != BossBehaviorManager.BossAwake.SecondForm)
                e_Awake = BossBehaviorManager.BossAwake.SecondForm;
            else
                e_Awake = BossBehaviorManager.BossAwake.FinalForm;
            OnBattle?.Invoke();
        }
    }
    public void SetMoveFlag(bool IsTrue)
    {
        m_StopMoveCamera = IsTrue;
        m_BossTeleport = IsTrue;
    }
    public bool GetCameraMoveflag() => m_StopMoveCamera;
    public void ChangeAwake(BossBehaviorManager.BossAwake awake)
    {
        e_Awake = awake;
    }
    private void OnUpdateHpbar(CharaState state,float hp)
    {
        var slider = state == CharaState.Player ? GetSlider(UiSliderState.PlayerHpbar) :
                                               GetSlider(UiSliderState.BossHpBar);
        slider.fillAmount = hp;
    }
    public void ActiveHpbar(CharaState state,bool IsActive)
    {
        var slider = state == CharaState.Player ? GetSlider(UiSliderState.PlayerHpbar) :
                                       GetSlider(UiSliderState.BossHpBar);

        slider.transform.parent.gameObject.SetActive(IsActive);
    }
    public void ChangePanel(UiPanelState state,bool IsActive)
    {
        var panel = GetPanel(state);

        if (panel == null) return;
        panel.SetActive(IsActive);
        var text = GetText(UiTextState.StageInfo);

        var script = g_Boss.GetComponent<BossBaseManager>();
        if (script.GetDieFlag() == true)
            text.text = $"タイム\n\n {SaveManager.Instance.c_CurrentData.m_TimeScore}";
        else text.text = "死んでしまった。";
    }
    public void ResaltPanel(string Score)
    {
        var panel = GetPanel(UiPanelState.Score);
        var text = GetText(UiTextState.StageInfo);

        if (panel == null) return;
        panel.SetActive(true);
        text.text = Score;
    }
    int Count = -1;
    public void Tutorial(int IsYes)//1 true
    {
        GetPanel(UiPanelState.tutorial).SetActive(false);
        int prev = Count;
        if (IsYes == 0)
        {
            if (Count == -1) { m_IsTutorial = false; return; }
            Count--;
            l_TutorialList[prev].SetActive(false);
            l_TutorialList[Count].SetActive(true);
            return;
        }

        Count++;

        if (Count >= l_TutorialList.Count)
        {
            m_IsTutorial = false;
            l_TutorialList[prev].SetActive(false);
            GetPanel(UiPanelState.tutorial).SetActive(false);
            return;
        }
        l_TutorialList[Count].SetActive(true);

        if (prev == -1) return;
        l_TutorialList[prev].SetActive(false);

    }
    public Image GetSlider(UiSliderState state)
    {
        if(d_SliderDictionary.TryGetValue(state, out var slider))
        {
            return slider;
        }
        return null;
    }
    public GameObject GetPanel(UiPanelState state)
    {
        if(d_PanelDictionary.TryGetValue(state, out var panel))
        {
            return panel;
        }
        return null;
    }
    public TextMeshProUGUI GetText(UiTextState state)
    {
        if (d_TextDictionary.TryGetValue(state, out var text))
        {
            return text;
        }
        return null;
    }
}
[Serializable]
public class StageInfo
{
    public float m_EncounterBorder;
    public Vector3 v_TeleportPos;
}
