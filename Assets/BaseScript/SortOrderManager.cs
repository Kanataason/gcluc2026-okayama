using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
public class SortOrderManager : MonoBehaviour
{
    public static SortOrderManager Instance { get; private set; }//シングルトン
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(Instance);
        }
    }
    public List<SpriteRenderer> l_SceneObj = new();//ソートを設定する

    int m_Frame = 0;//60フレームなら６回流れる
    void LateUpdate()
    {
        m_Frame++;
        if (m_Frame % 10 != 0) return;
        foreach (var sp in l_SceneObj)
        {
            sp.sortingOrder = (int)Mathf.Abs(sp.transform.position.y * 100);//百倍した値を代入
        }
    }

    public void SetList(SpriteRenderer obj)//リストに入れる
    {
        l_SceneObj.Add(obj);
    }
    public void RemoveList(SpriteRenderer obj)//リストから削除
    {
        l_SceneObj.Remove(obj);
    }
    public void SetSortOrder(Renderer renderer)//３ｄオブジェクトの順番を変える
    {
        renderer.sortingOrder = (int)Mathf.Abs(renderer.transform.parent.position.y * 101);
    }
    public void SetSpriteOrder(SpriteRenderer renderer)//スプライトの順番を変える
    {
        renderer.sortingOrder = (int)Mathf.Abs(renderer.transform.position.y * 100);
    }
}
