using UnityEngine;
using System.Collections.Generic;
using System;
public class BossAttackManager : MonoBehaviour
{
    public enum AnimaType
    {
        Move =0,
        Attack1
    }
    private readonly int BossMove = Animator.StringToHash("Move");
    private readonly int BossAttack = Animator.StringToHash("Attack");
    Animator a_Animator;
    public List<BossBulletManager> l_BulletList = new();
    public Transform SpownPos;

    public int CurrentAnime;
    public AnimaType e_AnimaType;

    private BossMoveManager c_BossMoveManager;
    private ObjctPool c_ObjectPool;
    void Start()
    {
        CurrentAnime = BossMove;
        c_ObjectPool = GetComponentInChildren<ObjctPool>();
        c_BossMoveManager = GetComponent<BossMoveManager>();
        a_Animator = GetComponent<Animator>();
        Init();
    }
    void Init()
    {
    }
    private void Update()
    {
        if (c_BossMoveManager.GetIsAttackFlag()) return;
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.A)) move += Vector3.left;
        if (Input.GetKey(KeyCode.D)) move += Vector3.right;
        if (Input.GetKey(KeyCode.W)) move += Vector3.up;
        if (Input.GetKey(KeyCode.S)) move += Vector3.down;

        if (move != Vector3.zero)
        {
            e_AnimaType = AnimaType.Move;
            CurrentAnime = BossMove;
            c_BossMoveManager.SetAnimaType((int)e_AnimaType);

            a_Animator.SetFloat("Move", 1f, 0.05f, Time.deltaTime);

            transform.Translate(move.normalized * 8f * Time.deltaTime);
        }
        else
        {
            a_Animator.SetFloat("Move", 0f, 0.05f, Time.deltaTime);
        }
    }

    public void AttackEnter()
    {
        CurrentAnime = BossAttack;
        a_Animator.SetInteger("AttackType", 1);
        a_Animator.SetTrigger("Attack");
        e_AnimaType = AnimaType.Attack1;
        c_BossMoveManager.SetAnimaType((int)e_AnimaType);
        Debug.Log("あったっく");
    }
    public void Collision()
    {
        GameObject obj = c_ObjectPool.GetObject(CharaState.Boss, ObjctPool.EfectType.Slash);
        obj.transform.localPosition = new Vector3(SpownPos.localPosition.x + UnityEngine.Random.Range(1,5),
                                                  SpownPos.localPosition.y + UnityEngine.Random.Range(1,5),
                                                  SpownPos.localPosition.z);
        Debug.Log($"{obj.transform.localPosition}s{SpownPos.localPosition}");
        SortOrderManager.Instance.SetSortOrder(obj.GetComponent<Renderer>());
        SetBulletInfo(obj);
    }

    //攻撃&セット処理
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
            c_ObjectPool.ReturnObject(ObjctPool.EfectType.Slash,CharaState.Boss, obj);
        }
    }
    //ここからアニメーションの値参照
    public void ResetAttackFlag() 
    {
        a_Animator.SetInteger("AttackType", 0);
        c_BossMoveManager.SetIsAttackFlag(false);
    }//リセットフラグ
}


