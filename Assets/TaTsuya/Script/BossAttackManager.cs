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

    public List<BossBulletManager> l_BulletList = new();
    public Transform t_SpownPos;
   [SerializeField] private SpriteRenderer r_SpriteRen;
   [SerializeField] private SpriteRenderer r_Shadow;

    public int m_CurrentAnime;
    public AnimaType e_AnimaType;
    public bool m_IsBossCoroutine1 = false;

    private BossBaseManager c_BossMoveManager;
    private ObjctPool c_ObjectPool;

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
        CharaBase pl = SaveManager.Instance.c_CurrentData.GetCharacter(CharaState.Player).GetComponent<CharaBase>();
        SetBulletInfo(obj,pl);
    }

    //攻撃&セット処理
    private void SetBulletInfo(GameObject obj,CharaBase Chara = null)
    {
        var script = obj.GetComponent<BossBulletManager>();
        l_BulletList.Add(script);
        script.DestroyObjEvent += DestroyInfoList;

        bool IsStop = false;
        bool IsFirst = true;
        float Duration = 0f;

        switch (script.e_Attacktype)
        {
            case BossBehaviorManager.BossAttackType.Attack1:
                {
                    Duration = 1.6f;
                    IsStop = true;
                    IsFirst = false;
                    script.Init(Duration, IsStop, IsFirst, Chara);
                    return;
                }
            case BossBehaviorManager.BossAttackType.Attack2:
                {
                    script.Init(Duration, IsStop, IsFirst, Chara);
                    script.Move(Chara);
                    return;
                } 
            case BossBehaviorManager.BossAttackType.Attack3:break;
            default:break;
        }
        script.Init(Duration, IsStop, IsFirst, Chara);
    }
    private void DestroyInfoList(GameObject obj, int CharaType, int EfectType)
    {
        var script = obj.GetComponent<BossBulletManager>();
        script.DestroyObjEvent -= DestroyInfoList;
        obj.transform.parent = c_ObjectPool.transform;
        CharaState e_CharaType = (CharaState)CharaType;
        ObjctPool.EfectType e_EfectType = (ObjctPool.EfectType)EfectType;

        l_BulletList.Remove(script);
       if(l_BulletList.Count == 0) ResetAttackFlag();
        Debug.Log("消すことに成功");
        c_ObjectPool.ReturnObject(e_CharaType, e_EfectType, obj);

    }
    //--------------------ここからアニメーションの値参照
    public void ResetAttackFlag()
    {
        a_Animator.SetInteger(BossAttackType, 0);
        c_BossMoveManager.SetIsAttackFlag(false);
        ReserAnima();
        
    }//リセットフラグ
    public void ReserAnima()
    {
        m_IsBossCoroutine1 = false;
    }
    public void SetAnima() => a_Animator.SetFloat(BossMove, 0);
    public void SetIsMove()
    {
        m_IsBossCoroutine1 = true;
    }

    //----------------ここから攻撃処理の中身
    float nextTime = 0;
 
    public void Attack1(int InstantiateValue)//召喚魔法
    {
        c_BossMoveManager.SetIsAttackFlag(true);
        CharaBase pl = SaveManager.Instance.c_CurrentData.GetCharacter(CharaState.Player).GetComponent<CharaBase>();
        for (int i = 0; i < InstantiateValue; i++)
        {
            var RandomPosY = UnityEngine.Random.Range(BattleManager.Instance.m_StageMin, BattleManager.Instance.m_StageMax);
            var RandomPosX = UnityEngine.Random.Range(-22, 22);//ここも変える
            var obj = c_ObjectPool.GetObject(CharaState.Boss, ObjctPool.EfectType.Shot);
            obj.transform.parent = null;
            obj.transform.position = new Vector3(RandomPosX, RandomPosY, 0);
            SetBulletInfo(obj,pl);
        }
    }
    public void Attack2(int InstantiateValue)//ジャンプが必要な攻撃
    {
        c_BossMoveManager.SetIsAttackFlag(true);
        a_Animator.SetInteger(BossAttackType, 4);
        SetIsMove();

        StartCoroutine(Attack2Instantiate(InstantiateValue));

    }
    public void Attack3(int attacktype)
    {
        c_BossMoveManager.SetIsAttackFlag(true);
        SetIsMove();
        StartCoroutine(Attack3Move(attacktype));
    }
    IEnumerator Attack2Instantiate(int Value)
    {
        float Duration = 2f;
        float CurrentTime = 2f;
        float height = Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;
        float CurrentDirection = c_BossMoveManager.CurrentDirection;
        int CurrentRound = BattleManager.Instance.m_CurrentRound;

        CharaBase pl = SaveManager.Instance.c_CurrentData.GetCharacter(CharaState.Player).GetComponent<CharaBase>();
        while (true)
        {
            if (!m_IsBossCoroutine1||CurrentRound != BattleManager.Instance.m_CurrentRound)
            {
                yield return null;
                continue;
            }
            CurrentTime += Time.deltaTime;
            if (CurrentTime >= Duration)
            {
                Debug.Log($"value{Value}");
                if (Value <= 0) break;
                CurrentTime = 0f;
                var obj = c_ObjectPool.GetObject(CharaState.Boss, ObjctPool.EfectType.Fire);
                obj.transform.position = new Vector3(width * CurrentDirection,
                                                      BattleManager.Instance.m_StageMin / 2, 0);
                obj.transform.parent = null;
                SetBulletInfo(obj,pl);

                Value--;
            }
            yield return null;
        }
        a_Animator.SetInteger(BossAttackType, 0);
        c_BossMoveManager.SetIsAttackFlag(false);
        ReserAnima();
    }
    IEnumerator Attack3Move(int attacktype)
    {
        float offset = 6f;
        a_Animator.SetInteger(BossAttackType, (int)BossBehaviorManager.BossAttackType.Attack3Hide);
        Vector3 pl = SaveManager.Instance.c_CurrentData.GetCharacter(CharaState.Player).transform.position;
        Vector3 oppPos = new Vector3(pl.x + (-c_BossMoveManager.CurrentDirection*offset),pl.y,pl.z);

        Color col = r_SpriteRen.color;
        Color col1 = r_Shadow.color;
        int CurrentRound = BattleManager.Instance.m_CurrentRound;

        float t = 1;
        while (true)
        {
            if (!m_IsBossCoroutine1||CurrentRound != BattleManager.Instance.m_CurrentRound)
            {

                col.a = 1;//値から才最大値まで往復
                col1.a = 1;

                r_SpriteRen.color = col;
                r_Shadow.color = col1;

                yield return new WaitUntil(()=>m_IsBossCoroutine1);
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
