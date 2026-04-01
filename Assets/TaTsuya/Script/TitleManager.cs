using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public enum UiState
    {
        Title = 1,
        PlayerControl = 2,
        Credit = 3,
        Start =4
    }

    public TextMeshProUGUI t_text;
    //オブジェクトリスト
    [SerializeField] GenericDictionary<UiState, GameObject> d_PanelDictionary;
    public List<GameObject> l_ButtonList = new();

    private GameObject g_PrevObj;
    private int m_Count = 0;

    void Start()
    {
        d_PanelDictionary.Init();
        g_PrevObj = d_PanelDictionary.Get(UiState.Title);
    }
    private void OnDestroy()
    {
        d_PanelDictionary.Clear();
        l_ButtonList.Clear();
    }

    public void ChangePanel(int StateValue)//アニメーションイベントから呼ばれる
    {
        InitPlayerOrder();

        g_PrevObj?.SetActive(false);
        g_PrevObj = null;

        UiState state = (UiState)StateValue;
        var obj = d_PanelDictionary.Get(state);
        SetActive(true,obj);
    }
    private void InitPlayerOrder()
    {
        if (m_Count > 0)//SetplayerOrderこれの処理をした後だったとき残っていたら流れる
        {
            for (int i = 0; i < l_ButtonList.Count - 1; i++)
                l_ButtonList[i].SetActive(true);
        }
        m_Count = 0;
    }
    public void SetActive(bool IsActive,GameObject obj)//パネルを表示処理
    {
        if (g_PrevObj == null)
               g_PrevObj = obj;

        obj.SetActive(IsActive);
    }
    public void SetplayerOrder(int Order)//先攻後攻を決める溜めの処理
    {
        GameObject obj = Order == 0 ? l_ButtonList[0] : l_ButtonList[1];
        obj.SetActive(false);

        m_Count++;
        if(m_Count > 1)
        {
            m_Count = 0;
            StartButton();
        }
        PlayerControl(m_Count);
    }
    private void StartButton()//スタートボタンを表示させる
    {
        g_PrevObj.SetActive(false);

        var g = d_PanelDictionary.Get(UiState.Start);
        g.SetActive(true);
    }

    public void PlayerControl(int Count)
    => t_text.text = $"プレイヤー{++Count}さん選んでください";
    public void ChangeScene()
    {
       SceneManager.LoadScene("TaTsuyaScene");
       // SceneManager.LoadScene("TaTsuyaScene 2");
       // SceneManager.LoadScene("TaTsuyaScene 3", LoadSceneMode.Additive);
    }
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
