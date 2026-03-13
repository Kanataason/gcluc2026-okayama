using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static BossBehaviorManager;
public class BossAttackManager : MonoBehaviour
{
    public enum AnimaType
    {
        Move = 0,
        Attack
    }
    private readonly int BossMove = Animator.StringToHash("Move");
    private readonly int BossAttack = Animator.StringToHash("Attack");
    private readonly int BossAttackType = Animator.StringToHash("AttackType");
    Animator a_Animator;
    private int m_EventIndex;

    public List<BossBulletManager> l_BulletList = new();
    public Transform t_SpownPos;
   [SerializeField] private SpriteRenderer r_SpriteRen;
   [SerializeField] private SpriteRenderer r_Shadow;

    public int m_CurrentAnime;
    public AnimaType e_AnimaType;
    public bool m_IsBossCoroutine = false;

    private BossBaseManager c_BossMoveManager;
    private ObjctPool c_ObjectPool;
   // public List<AnimaInfo> c_AnimaList;

    void Start()
    {
        m_CurrentAnime = BossMove;
        c_ObjectPool = GetComponentInChildren<ObjctPool>();
        c_BossMoveManager = GetComponent<BossBaseManager>();
        a_Animator = GetComponent<Animator>();
        Init();
    }
    void Init()
    {
        m_EventIndex = 0;
    }
    //private void Update()
    //{
    //    if (!c_BossMoveManager.GetIsAttackFlag()) return;

    //    AnimatorStateInfo state = a_Animator.GetCurrentAnimatorStateInfo(0);

    //    var anima = c_AnimaList[a_Animator.GetInteger(BossAttackType)-1];

    //    if (m_EventIndex < anima.m_AnimeEventtime.Length &&
    //        state.normalizedTime > anima.m_AnimeEventtime[m_EventIndex])
    //    {
    //        anima.u_Evnet.Invoke();
    //        m_EventIndex++;
    //    }
    //    if (state.normalizedTime > 0.99f)
    //    {
    //        ResetAttackFlag();
    //    }
    //}
    public void AttackEnter(int AttackType)
    {
        c_BossMoveManager.SetIsAttackFlag(true);
        m_CurrentAnime = BossAttack;
        a_Animator.SetInteger(BossAttackType, AttackType);
        a_Animator.SetTrigger(BossAttack);
        e_AnimaType = AnimaType.Attack;
        c_BossMoveManager.SetAnimaType((int)e_AnimaType);
        Debug.Log("あったっく");
    }
    public void SpawnEfect(int EventType)//p1 b2 o3    Die1 slash2 hit3 shot4
    {
        //文字列で分けてintに変換
        ObjctPool.EfectType e_EfectType = (ObjctPool.EfectType)EventType;
        GameObject obj = c_ObjectPool.GetObject(CharaState.Boss, e_EfectType);

        obj.transform.localPosition = t_SpownPos.localPosition;
        SortOrderManager.Instance.SetSortOrder(obj.GetComponent<Renderer>());
        SetBulletInfo(obj);
    }

    //攻撃&セット処理
    private void SetBulletInfo(GameObject obj)
    {
        var script = obj.GetComponent<BossBulletManager>();
        l_BulletList.Add(script);
        if (script.a_Anima != null)
        {
            float Duration = 2;
            script.Init(Duration, true);
        }
        script.DestroyObjEvent += DestroyInfoList;
    }
    private void DestroyInfoList(GameObject obj, int CharaType, int EfectType)
    {
        ResetAttackFlag();
        var script = obj.GetComponent<BossBulletManager>();
        script.DestroyObjEvent -= DestroyInfoList;
        obj.transform.parent = c_ObjectPool.transform;
        CharaState e_CharaType = (CharaState)CharaType;
        ObjctPool.EfectType e_EfectType = (ObjctPool.EfectType)EfectType;

        l_BulletList.Clear();
        Debug.Log("消すことに成功");
        c_ObjectPool.ReturnObject(e_CharaType, e_EfectType, obj);

    }
    //--------------------ここからアニメーションの値参照
    public void ResetAttackFlag()
    {
        m_EventIndex = 0;
        a_Animator.SetInteger(BossAttackType, 0);
        c_BossMoveManager.SetIsAttackFlag(false);
    }//リセットフラグ
    public void ReserAnima()
    {
        m_IsBossCoroutine = false;
    }
    public void SetAnima() => a_Animator.SetFloat(BossMove, 0);
    public void SetIsMove() => m_IsBossCoroutine = true;


    //----------------ここから攻撃処理の中身
    float nextTime = 0;
 
    public void Attack1(int InstantiateValue)//召喚魔法
    {
        for (int i = 0; i < InstantiateValue; i++)
        {
            var RandomPosY = UnityEngine.Random.Range(BattleManager.Instance.m_StageMin, BattleManager.Instance.m_StageMax);
            var RandomPosX = UnityEngine.Random.Range(-22, 22);//ここも変える
            var obj = c_ObjectPool.GetObject(CharaState.Boss, ObjctPool.EfectType.Shot);
            obj.transform.parent = null;
            obj.transform.position = new Vector3(RandomPosX, RandomPosY, 0);
            SetBulletInfo(obj);
        }
    }
    public void Attack2()
    {
        a_Animator.SetInteger(BossAttackType, 4);
        NextFrame.Run(this, 2, () =>
        {
            a_Animator.SetInteger(BossAttackType, 0);
        });
    }
    public void Attack3(int attacktype)
    {
        SetIsMove();
        StartCoroutine(Attack3Move(attacktype));
    }
    IEnumerator Attack3Move(int attacktype)
    {
        float offset = 6f;
        a_Animator.SetInteger(BossAttackType, (int)BossBehaviorManager.BossAttackType.Attack3Hide);
        Vector3 pl = SaveManager.Instance.c_CurrentData.GetCharacter(CharaState.Player).transform.position;
        Vector3 oppPos = new Vector3(pl.x + (-c_BossMoveManager.CurrentDirection*offset),pl.y,pl.z);

        Color col = r_SpriteRen.color;
        Color col1 = r_Shadow.color;

        float t = 1;
        while (true)
        {
            if (!m_IsBossCoroutine)
            {

                col.a = 1;//値から才最大値まで往復
                col1.a = 1;

                r_SpriteRen.color = col;
                r_Shadow.color = col1;

                yield return new WaitUntil(()=>m_IsBossCoroutine);
            }
            t -= Time.deltaTime;

            col.a = Mathf.PingPong(t, 1);//値から才最大値まで往復
            col1.a = Mathf.PingPong(t, 1);
            r_SpriteRen.color = col;
            r_Shadow.color = col1;
 
            // αがほぼ0になった瞬間
            if (col.a < 0.01f)
            {
                transform.position = oppPos; // 瞬間移動

                col.a = 1f;
                col1.a = 1f;// すぐ表示
                r_SpriteRen.color = col;
                r_Shadow.color = col1;

                break;
            }

            yield return null;
        }
        a_Animator.SetInteger(BossAttackType,attacktype);

        ReserAnima();
    }
}
[Serializable]
public class AnimaInfo
{
    public int m_HashInfo;
    public float[] m_AnimeEventtime;
    public UnityEvent u_Evnet;
}
