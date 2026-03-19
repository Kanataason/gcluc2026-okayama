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

    [SerializeField] GenericDictionary<UiState, GameObject> d_PanelDictionary;

    private GameObject g_PrevObj;
    private int m_Count = 0;
    public List<GameObject> l_ButtonList = new();
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
        if (m_Count > 0)
        {
            for(int i = 0;i<l_ButtonList.Count - 1; i++)
                l_ButtonList[i].SetActive(true);
        }

        g_PrevObj?.SetActive(false);
        g_PrevObj = null;
        m_Count = 0;

        UiState state = (UiState)StateValue;
        var obj = d_PanelDictionary.Get(state);
        SetActive(true,obj);
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
    private void StartButton()
    {
        g_PrevObj.SetActive(false);

        var g = d_PanelDictionary.Get(UiState.Start);
        g.SetActive(true);
    }

    public void PlayerControl(int Count)
    => t_text.text = $"プレイヤー{++Count}さん選んでください";
    public void ChangeScene()
        => SceneManager.LoadScene("TaTsuyaScene");
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
