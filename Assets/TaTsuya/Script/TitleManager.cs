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
    [Serializable]
    public class UiInfo
    {
        public UiState e_UiState;
        public GameObject g_Panel;
    }
    public List<UiInfo> l_UiList;

    public TextMeshProUGUI t_text;

    private Dictionary<UiState, GameObject> d_UiDictionary = new();

    private GameObject g_PrevObj;
    private int m_Count = 0;
    public List<GameObject> l_ButtonList = new();
    void Start()
    {
     foreach(var list in l_UiList)
        {
            d_UiDictionary[list.e_UiState] = list.g_Panel;
        }
        g_PrevObj = GetObject(UiState.Title);
    }

    public void ChangePanel(int StateValue)//アニメーションイベントから呼ばれる
    {
        if (m_Count > 0)
        {
            for(int i = 0;i<l_ButtonList.Count - 1; i++)
            {
                l_ButtonList[i].SetActive(true);
            }
        }
        if (g_PrevObj != null)
        {
            Debug.Log("sss");
            g_PrevObj.SetActive(false);
            g_PrevObj = null;
        }

        m_Count = 0;
        UiState state = (UiState)StateValue;
        var obj = GetObject(state);
        SetActive(true,obj);
    }
    public void SetActive(bool IsActive,GameObject obj)
    {
        if (g_PrevObj == null) { g_PrevObj = obj;}
        obj.SetActive(IsActive);
    }
    public void PlayerControl(int Count)
    {
        t_text.text = $"プレイヤー{Count}さん選んでください";
    }
    public void SetplayerOrder(int Order)
    {
        GameObject obj = Order == 0 ? l_ButtonList[0] : l_ButtonList[1];
        obj.SetActive(false);
        m_Count++;
        if(m_Count > 1)
        {
            m_Count = 0;
            StartButton();
        }
        PlayerControl(++m_Count);
    }
    public void ChangeScene()
    {
        SceneManager.LoadScene("TaTsuyaScene");
    }
    private void StartButton()
    {
        g_PrevObj.SetActive(false);
        var g = GetObject(UiState.Start);
        g.SetActive(true);
    }
    private GameObject GetObject(UiState state)
    {
        if(d_UiDictionary.TryGetValue(state,out var obj))
        {
            return obj;
        }
        return null;
    }
}
