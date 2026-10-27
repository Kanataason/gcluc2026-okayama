using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//コントローラー・キーボードでボタンを操作できるようにする
//ボタンが何も選ばれていないときは、表示中のボタンから一番上(同じ高さなら一番左)を自動で選ぶ
//シーンに置かなくても、ゲーム開始時に自動で作られる
public class UiSelectionKeeper : MonoBehaviour
{
    private const float SelectDelay = 0.2f;//選択が外れてから選び直すまでの時間(パネルの切り替わりを待つ)

    private static float s_SuppressUntil;//この時刻までは自動で選ばない

    private GameObject g_LastSelected;
    private float m_NoSelectionTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        var obj = new GameObject(nameof(UiSelectionKeeper));
        DontDestroyOnLoad(obj);
        obj.AddComponent<UiSelectionKeeper>();
    }
    public static void SuppressFor(float seconds)//しばらく自動で選ばない(結果画面で連打による押し間違いを防ぐ)
    {
        s_SuppressUntil = Time.unscaledTime + seconds;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }
    private void Update()
    {
        var eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        var selected = eventSystem.currentSelectedGameObject;
        if (IsSelectable(selected))
        {
            g_LastSelected = selected;
            m_NoSelectionTime = 0;
            return;
        }
        if (Time.unscaledTime < s_SuppressUntil) return;

        m_NoSelectionTime += Time.unscaledDeltaTime;
        if (m_NoSelectionTime < SelectDelay) return;

        //さっきまで選んでいたボタンがまだ表示されていればそれを、なければ一番上のボタンを選ぶ
        var target = IsSelectable(g_LastSelected) ? g_LastSelected : FindFirstSelectable();
        if (target != null) eventSystem.SetSelectedGameObject(target);
    }
    private static bool IsSelectable(GameObject obj)
    {
        if (obj == null || !obj.activeInHierarchy) return false;

        var selectable = obj.GetComponent<Selectable>();
        return selectable != null && selectable.IsInteractable();
    }
    private static GameObject FindFirstSelectable()//表示中で押せるボタンのうち、画面の一番上(同じ高さなら一番左)
    {
        Selectable first = null;
        foreach (var selectable in Selectable.allSelectablesArray)
        {
            if (!selectable.IsInteractable()) continue;
            if (first == null || IsUpperLeft(selectable.transform.position, first.transform.position))
                first = selectable;
        }
        return first != null ? first.gameObject : null;
    }
    private static bool IsUpperLeft(Vector3 a, Vector3 b)
    {
        if (!Mathf.Approximately(a.y, b.y)) return a.y > b.y;
        return a.x < b.x;
    }
}
