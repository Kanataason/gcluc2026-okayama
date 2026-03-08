using UnityEngine;
using System.Collections.Generic;
using System;
public class BossAttackManager : MonoBehaviour
{

    Animator a_Animetor;
    private List<GameObject> l_BulletList = new();
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
        GameObject obj = ObjctPool.Instance.GetObject(ObjctPool.CharaStatus.Boss, ObjctPool.EfectType.Die);
        SetBulletInfo(obj);
    }

    //çUåÇ&ÉZÉbÉgèàóù
    private void SetBulletInfo(GameObject obj)
    {
        l_BulletList.Add(obj);
        var script = obj.GetComponent<BossBulletManager>();
        script.DestroyObjEvent += DestroyInfoList;
    }
    private void DestroyInfoList(GameObject obj)
    {
       var script = obj.GetComponent<BossBulletManager>();
        script.DestroyObjEvent -= DestroyInfoList;

        if (l_BulletList.Remove(obj))
        {
            ObjctPool.Instance.ReturnObject(ObjctPool.EfectType.Die, ObjctPool.CharaStatus.Boss, obj);
        }
    }
}


