using UnityEngine;
using System.Collections.Generic;
using System;
public class BossAttackManager : MonoBehaviour
{

    Animator a_Animetor;
    private List<GameObject> l_BulletList = new();
    public GameObject pre;
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
        GameObject obj = Instantiate(pre, Vector3.zero, Quaternion.identity);
        SetBulletInfo(obj);
    }

    //çUåÇ&ÉZÉbÉgèàóù
    private void SetBulletInfo(GameObject obj)
    {
        l_BulletList.Add(obj);

    }
    private void DestroyInfoList()
    {

    }
}


