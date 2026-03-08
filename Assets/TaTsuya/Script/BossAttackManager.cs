using UnityEngine;
using System.Collections.Generic;
using System;
public class BossAttackManager : MonoBehaviour
{

    Animator a_Animetor;
    public List<BossBulletManager> l_BulletList = new();
    public Transform SpownPos;
    void Start()
    {
        a_Animetor = GetComponent<Animator>();
        Init();
    }
    void Init()
    {
    }


    public void AttackEnter()
    {
        a_Animetor.SetTrigger("Attack");
        Debug.Log("Ç†Ç¡ÇΩÇ¡Ç≠");
        GameObject obj = ObjctPool.Instance.GetObject(ObjctPool.CharaState.Boss, ObjctPool.EfectType.Die);
        obj.transform.localPosition = SpownPos.localPosition;
        Debug.Log($"{obj.transform.localPosition}s{SpownPos.localPosition}");
        SortOrderManager.Instance.SetSortOrder(obj.GetComponent<Renderer>());
        SetBulletInfo(obj);
    }

    //çUåÇ&ÉZÉbÉgèàóù
    private void SetBulletInfo(GameObject obj)
    {
        var script = obj.GetComponent<BossBulletManager>();
        l_BulletList.Add(script);
        script.DestroyObjEvent += DestroyInfoList;
    }
    private void DestroyInfoList(GameObject obj)
    {
       var script = obj.GetComponent<BossBulletManager>();
        script.DestroyObjEvent -= DestroyInfoList;

        if (l_BulletList.Remove(script))
        {
            ObjctPool.Instance.ReturnObject(ObjctPool.EfectType.Die, ObjctPool.CharaState.Boss, obj);
        }
    }
}


