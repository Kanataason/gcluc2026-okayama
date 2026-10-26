using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//1人分のステージを管理する(2画面のときは2つ作られる)
//プレイヤー・ボス・カメラ・HPバー・戦闘中かどうかをステージごとに持つ
public class StageContext : MonoBehaviour
{
    private const float EncounterRange = 1f;//接敵の判定距離
    private const float InfoShowTime = 2.5f;//お知らせを出しておく時間

    public int m_StageNum;//1P:1 2P:2
    public int m_Layer;//このステージのキャラクターを置くレイヤー
    public Camera c_Camera;
    public StageHud c_Hud;

    public PlayerMove c_Player;
    public BossBaseManager c_Boss;
    public BossBehaviorManager c_BossBehavior;

    public float m_TimeScore;//戦闘していた時間

    public bool b_InBattle { get; private set; }//ボスと戦闘中か
    public bool b_IsFinished { get; private set; }//決着がついたか

    public event Action OnBattle;//接敵したとき

    private int m_InfoId;//最後に出したお知らせの番号

    public void Init(int stageNum, int layer, Camera camera, StageHud hud)
    {
        m_StageNum = stageNum;
        m_Layer = layer;
        c_Camera = camera;
        c_Hud = hud;

        c_Hud.SetActive(CharaState.Boss, false);
    }
    public void SetCharacters(PlayerMove player, BossBaseManager boss)
    {
        c_Player = player;
        c_Boss = boss;
        c_BossBehavior = boss.GetComponent<BossBehaviorManager>();

        c_Player.OnHpBar += c_Hud.SetFill;
        c_Boss.OnHpBar += c_Hud.SetFill;
    }
    private void OnDestroy()
    {
        if (c_Player != null) c_Player.OnHpBar -= c_Hud.SetFill;
        if (c_Boss != null) c_Boss.OnHpBar -= c_Hud.SetFill;
        OnBattle = null;
    }
    private void Update()
    {
        if (b_InBattle && !b_IsFinished) m_TimeScore += Time.deltaTime;
    }
    private void FixedUpdate()
    {
        CheckEncounter();
    }
    private void CheckEncounter()//プレイヤーが接敵ラインに来たらボス戦開始
    {
        if (b_InBattle || b_IsFinished || c_Player == null || c_BossBehavior == null) return;

        float border = TatuGameManager.Instance.l_Infolist[c_BossBehavior.FormIndex].m_EncounterBorder;
        if (Mathf.Abs(border - c_Player.transform.position.x) < EncounterRange)
        {
            BattleManager.Instance.PlayBossBgm();
            OnBattle?.Invoke();
        }
    }
    public void SetInBattle(bool isInBattle) => b_InBattle = isInBattle;
    public GameObject GetCharacter(CharaState state)
    {
        switch (state)
        {
            case CharaState.Player: return c_Player != null ? c_Player.gameObject : null;
            case CharaState.Boss: return c_Boss != null ? c_Boss.gameObject : null;
            default: return null;
        }
    }
    public void OnCharacterDie(CharaState state)//プレイヤーかボスがやられたとき
    {
        SetInBattle(false);
        c_Hud.HideAll();//倒した・やられたステージのUIは全部消す
        BattleManager.Instance.OnStageFinished(this, state == CharaState.Boss);
    }
    public void Finish()//決着がついたら止めて、UI(HPバー・お知らせ)も全部消す
    {
        b_IsFinished = true;
        SetInBattle(false);
        c_Hud.HideAll();
    }
    public void OnBossAwakened()//このステージのボスが覚醒したとき
    {
        BattleManager.Instance.OnBossAwakened(this);
    }
    public void ShowInfo(string info)//このステージの画面にお知らせを出す(一定時間で消える)
    {
        int infoId = ++m_InfoId;
        c_Hud.SetInfo(info);
        NextFrame.Run(this, InfoShowTime, () =>
        {
            if (infoId == m_InfoId) c_Hud.SetInfo("");//後から別のお知らせが出ていたら消さない
        });
    }
    public static void SetLayerRecursively(GameObject obj, int layer)
    {
        foreach (var child in obj.GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = layer;
    }
}
public class StageHud//ステージごとのHPバーとお知らせ
{
    private const float InfoHeightRate = 0.7f;//お知らせを出す高さ(担当する画面の下から何割か)
    private const float InfoFontSize = 40f;

    private readonly Image c_PlayerHp;
    private readonly Image c_BossHp;
    private readonly TextMeshProUGUI c_InfoText;
    private bool b_IsHidden;//HideAllの後はもう出さない

    public StageHud(Image playerHp, Image bossHp, TextMeshProUGUI infoText)
    {
        c_PlayerHp = playerHp;
        c_BossHp = bossHp;
        c_InfoText = infoText;
        SetInfo("");
    }
    public void SetFill(CharaState state, float value) => GetBar(state).fillAmount = value;
    public void SetActive(CharaState state, bool isActive)
    {
        if (b_IsHidden && isActive) return;
        GetBar(state).transform.parent.gameObject.SetActive(isActive);
    }
    public void SetInfo(string info)
    {
        if (b_IsHidden || c_InfoText == null) return;
        c_InfoText.text = info;
    }
    public void HideAll()//HPバーとお知らせを全部消す
    {
        SetActive(CharaState.Player, false);
        SetActive(CharaState.Boss, false);
        SetInfo("");
        b_IsHidden = true;
    }
    public void FitToViewport(Rect viewport)//バーとお知らせの位置を担当する画面の範囲に収める
    {
        FitToViewport(c_PlayerHp, viewport);
        FitToViewport(c_BossHp, viewport);
        FitInfoText(viewport);
    }
    private void FitInfoText(Rect viewport)//お知らせは担当する画面の上のほうの真ん中に出す
    {
        if (c_InfoText == null) return;

        var rect = c_InfoText.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, viewport.y + viewport.height * InfoHeightRate);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(900, 160);

        c_InfoText.alignment = TextAlignmentOptions.Center;
        c_InfoText.enableAutoSizing = false;
        c_InfoText.fontSize = InfoFontSize;
    }
    private void FitToViewport(Image bar, Rect viewport)
    {
        var rect = (RectTransform)bar.transform.parent;
        rect.anchorMin = ToViewport(rect.anchorMin, viewport);
        rect.anchorMax = ToViewport(rect.anchorMax, viewport);
    }
    private Vector2 ToViewport(Vector2 anchor, Rect viewport)
        => new Vector2(viewport.x + anchor.x * viewport.width, viewport.y + anchor.y * viewport.height);
    private Image GetBar(CharaState state) => state == CharaState.Player ? c_PlayerHp : c_BossHp;
}
