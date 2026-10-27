using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//全ステージ共通のUI(パネル・チュートリアル)とステージの設定を持つ
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
    }

    [SerializeField]GenericDictionary<UiTextState,TextMeshProUGUI> d_TextDictionary = new();
    [SerializeField]GenericDictionary<UiPanelState,GameObject> d_PanelDictionary = new();
    [SerializeField]GenericDictionary<UiSliderState,Image> d_SliderDictionary = new();

    public List<StageInfo> l_Infolist = new();//形態ごとの接敵ラインとボスの出現位置(全ステージ共通)
    public static TatuGameManager Instance { get; private set; }
    //ステージのY軸の制限
    public float m_StageScaleMaxY;
    public float m_StageScaleMinY;

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
    public void Init()//BattleManagerから最初に1回呼ばれる
    {
        d_PanelDictionary.Init();
        d_SliderDictionary.Init();
        d_TextDictionary.Init();
    }
    public StageHud[] CreateHuds(Rect[] viewports)//ステージごとのHPバーとお知らせを用意する 2P以降は1Pのバーを複製する
    {
        Image playerHp = d_SliderDictionary.Get(UiSliderState.PlayerHpbar);
        Image bossHp = d_SliderDictionary.Get(UiSliderState.BossHpBar);

        var huds = new StageHud[viewports.Length];
        for (int i = 0; i < viewports.Length; i++)
        {
            //位置を変える前に複製するので、先に全部作ってから位置を合わせる
            var infoText = CreateInfoText(playerHp.transform.parent.parent, i + 1);
            huds[i] = i == 0
                ? new StageHud(playerHp, bossHp, infoText)
                : new StageHud(CloneBar(playerHp), CloneBar(bossHp), infoText);
        }
        for (int i = 0; i < viewports.Length; i++)
            huds[i].FitToViewport(viewports[i]);

        return huds;
    }
    private Image CloneBar(Image bar)//HPバーを親ごと複製して、中の同じ名前のImageを返す
    {
        Transform parent = bar.transform.parent;
        Transform clone = Instantiate(parent, parent.parent);
        return clone.Find(bar.name).GetComponent<Image>();
    }
    private TextMeshProUGUI CreateInfoText(Transform canvas, int stageNum)//お知らせ用のテキストを結果表示のテキストから作る(日本語フォントをそのまま使うため)
    {
        var template = d_TextDictionary.Get(UiTextState.StageInfo);
        if (template == null) return null;

        var text = Instantiate(template, canvas, false);
        text.name = $"Stage{stageNum} InfoText";
        text.raycastTarget = false;//ボタンの邪魔をしない
        text.rectTransform.localScale = Vector3.one;//複製元はScaleが3倍なので戻す(大きさはフォントサイズで決める)
        text.gameObject.SetActive(true);
        return text;
    }
    //----------ポーズメニュー
    public GameObject CreatePauseMenu(Action onResume, Action onTitle, Action onQuit)//ポーズパネルにボタンを3つ作る 作れなければnull
    {
        var scorePanel = d_PanelDictionary.Get(UiPanelState.Score);
        var pausePanel = GetPausePanel(scorePanel);
        if (scorePanel == null || pausePanel == null) return null;

        //結果画面のボタンを複製して使う(見た目と日本語フォントをそろえるため)
        var template = scorePanel.GetComponentInChildren<Button>(true);
        if (template == null) return null;

        CreatePauseButton(template, pausePanel.transform, "続ける", 0, onResume);
        CreatePauseButton(template, pausePanel.transform, "タイトル", 1, onTitle);
        CreatePauseButton(template, pausePanel.transform, "ゲームを終了", 2, onQuit);

        pausePanel.SetActive(false);
        return pausePanel;
    }
    private GameObject GetPausePanel(GameObject scorePanel)//パネル一覧に登録されていなければ、結果画面と同じ親の「Pose」を使う
    {
        var panel = d_PanelDictionary.Get(UiPanelState.Pose);
        if (panel != null || scorePanel == null) return panel;

        var found = scorePanel.transform.parent.Find("Pose");
        return found != null ? found.gameObject : null;
    }
    private void CreatePauseButton(Button template, Transform parent, string label, int index, Action onClick)
    {
        const float ButtonSpaceRate = 1.5f;//ボタンの高さに対する間隔

        var button = Instantiate(template, parent, false);
        button.name = $"PauseButton({label})";
        button.onClick = new Button.ButtonClickedEvent();//複製元(結果画面)のイベントは使わない
        button.onClick.AddListener(() => onClick());

        var rect = (RectTransform)button.transform;
        float space = rect.sizeDelta.y * rect.localScale.y * ButtonSpaceRate;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, space * (1 - index));//上から順に並べる

        var text = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null) text.text = label;

        button.gameObject.SetActive(true);
    }
    public void ChangePanel(UiPanelState state,bool IsActive)
    {
        var panel = d_PanelDictionary.Get(state);
        if (panel == null) return;

        panel.SetActive(IsActive);
    }
    public void ResaltPanel(string Score)
    {
        var panel = d_PanelDictionary.Get(UiPanelState.Score);
        var text = d_TextDictionary.Get(UiTextState.StageInfo);

        if (panel == null) return;
        panel.SetActive(true);
        text.text = Score;
    }

}
[Serializable]
public class StageInfo
{
    public float m_EncounterBorder;
    public Vector3 v_TeleportPos;
}
