using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public enum UiState
    {
        Title = 1,
        Tutorial = 2,//チュートリアルを見るかの確認パネル
        Credit = 3,
        Start = 4
    }
    //オブジェクトリスト
    [SerializeField] GenericDictionary<UiState, GameObject> d_PanelDictionary;
    public List<GameObject> l_TutorialList = new();//チュートリアルのページ(表示する順番で入れる)

    private GameObject g_CurrentPanel;
    private int m_TutorialPage = -1;//今のページ -1:確認パネル

    void Start()
    {
        d_PanelDictionary.Init();
        g_CurrentPanel = d_PanelDictionary.Get(UiState.Title);
    }
    private void OnDestroy()
    {
        d_PanelDictionary.Clear();
    }

    public void ChangePanel(int StateValue)//ボタンやアニメーションイベントから呼ばれる
    {
        ShowPanel((UiState)StateValue);
    }
    private void ShowPanel(UiState state)
    {
        if (g_CurrentPanel != null) g_CurrentPanel.SetActive(false);
        HideTutorialPage();

        g_CurrentPanel = d_PanelDictionary.Get(state);
        if (g_CurrentPanel != null) g_CurrentPanel.SetActive(true);
    }
    //----------チュートリアル処理
    public void Tutorial(int IsYes)//ボタンから呼ばれる 1:見る・次へ 0:見ない・戻る
    {
        if (IsYes == 0)
        {
            if (m_TutorialPage == -1) { ShowPanel(UiState.Start); return; }//見ない

            SetTutorialPage(m_TutorialPage - 1);//最初のページで戻ると確認パネルに戻る
            return;
        }

        if (m_TutorialPage + 1 >= l_TutorialList.Count)//最後のページまで見たら始める画面へ
        {
            ShowPanel(UiState.Start);
            return;
        }
        SetTutorialPage(m_TutorialPage + 1);
    }
    private void SetTutorialPage(int page)//page:-1で確認パネルを表示
    {
        HideTutorialPage();
        m_TutorialPage = page;

        d_PanelDictionary.Get(UiState.Tutorial).SetActive(page == -1);
        if (page >= 0) l_TutorialList[page].SetActive(true);
    }
    private void HideTutorialPage()
    {
        if (m_TutorialPage >= 0) l_TutorialList[m_TutorialPage].SetActive(false);
        m_TutorialPage = -1;
    }

    public void ChangeScene()
    {
       SceneManager.LoadScene("TaTsuyaScene");
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
