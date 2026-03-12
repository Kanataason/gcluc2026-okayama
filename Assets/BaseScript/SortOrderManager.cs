using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
public class SortOrderManager : MonoBehaviour
{
    public static SortOrderManager Instance { get; private set; }
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
    public List<SpriteRenderer> l_SceneObj = new();

    int m_Frame = 0;
    private void Start()
    {
        
    }
    void LateUpdate()
    {
        m_Frame++;
        if (m_Frame % 10 != 0) return;
        foreach (var sp in l_SceneObj)
        {
            sp.sortingOrder = (int)Mathf.Abs(sp.transform.position.y * 100);
        }
    }

    public void SetList(SpriteRenderer obj)
    {
        l_SceneObj.Add(obj);
    }
    public void RemoveList(SpriteRenderer obj)
    {
        l_SceneObj.Remove(obj);
    }
    public void SetSortOrder(Renderer renderer)
    {
        renderer.sortingOrder = (int)Mathf.Abs(renderer.transform.parent.position.y * 101);
    }
    public void SetSpriteOrder(SpriteRenderer renderer)
    {
        renderer.sortingOrder = (int)Mathf.Abs(renderer.transform.position.y * 100);
    }
}
