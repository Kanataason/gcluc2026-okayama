using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static BossBehaviorManager;
public class BossAttackManager : MonoBehaviour
{
    public readonly int BossMove = Animator.StringToHash("Move");
    public readonly int BossAttack = Animator.StringToHash("Attack");
    public readonly int BossAttackType = Animator.StringToHash("AttackType");
    public readonly int BossFirstAnima = Animator.StringToHash("Teleport");
    public readonly int BossTransparent = Animator.StringToHash("Transparent");
    public readonly int BossHide = Animator.StringToHash("Hide");
    public enum AnimaType
    {
        Move = 0,
        Attack
    }
    Animator a_Animator;

    public List<BossBulletManager> l_BulletList = new();
    public Transform t_SpownPos;
   [SerializeField] private SpriteRenderer r_SpriteRen;
   [SerializeField] private SpriteRenderer r_Shadow;
    //アニメの情報
    public int m_CurrentAnime;
    public AnimaType e_AnimaType;
    public bool b_IsBossCoroutine = false;

    private GameObject g_Player;
    //参照先
    private BossBaseManager c_BossMoveManager;
    private BossBehaviorManager c_BossBehaviorManager;
    private PlayerMove c_PlayerMove;
    private ObjctPool c_ObjectPool;
    private Camera c_Camera;

    private void OnDisable()
    {
        TatuGameManager.Instance.OnBattle -= OnSetBattle;
    }

    void Start()
    {
        m_CurrentAnime = BossMove;
        Init();
        ActionEvent();
    }
    private void Init()
    {
        c_Camera = Camera.main;
        g_Player = GameObject.FindWithTag("Player");
        c_PlayerMove = g_Player.GetComponent<PlayerMove>();
        c_BossBehaviorManager = GetComponent<BossBehaviorManager>();
        c_ObjectPool = GetComponentInChildren<ObjctPool>();
        c_BossMoveManager = GetComponent<BossBaseManager>();
        a_Animator = GetComponent<Animator>();
    }
    void ActionEvent()
    {
        TatuGameManager.Instance.OnBattle += OnSetBattle;
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
        PlayAnimation(AttackType);

        m_CurrentAnime = BossAttack;
        e_AnimaType = AnimaType.Attack;
    }
    private void PlayAnimation(int AttackType)
    {
        a_Animator.SetInteger(BossAttackType, AttackType);
        a_Animator.SetTrigger(BossAttack);

        c_BossMoveManager.SetAnimaType((int)e_AnimaType);
        c_BossMoveManager.SetIsAttackFlag(true);
    }
    public void SpawnEfect(int EventType)//p1 b2 o3    Die1 slash2 hit3 shot4
    {
        //文字列で分けてintに変換
        ObjctPool.EfectType e_EfectType = (ObjctPool.EfectType)EventType;
        GameObject obj = c_ObjectPool.GetObject(CharaState.Boss, e_EfectType);

        InitBulletInfo(obj);
        SetBulletInfo(obj);
    }
    private void InitBulletInfo(GameObject obj)
    {
        SortOrderManager.Instance.SetSortOrder(obj.GetComponent<Renderer>());
        obj.transform.parent = null;
        Vector3 pos = t_SpownPos.position;
        obj.transform.position = new Vector3(pos.x, pos.y, transform.position.y);
    }

    //攻撃&セット処理
    private void SetBulletInfo(GameObject obj)
    {
        var script = obj.GetComponent<BossBulletManager>();
        l_BulletList.Add(script);
        script.DestroyObjEvent += DestroyInfoList;

        ApplyAwake(script);
    }
    private void ApplyAwake(BossBulletManager script)
    {
        int IsAttack = 0;
        float Duration = 0f;

        switch (script.e_Attacktype)
        {
            case BossBehaviorManager.BossAttackType.Attack1:
                {
                    Duration = 1.2f;
                    script.Init(Duration, BossBulletManager.BulletState.Stop, c_PlayerMove, IsAttack);
                    return;
                }
            case BossBehaviorManager.BossAttackType.Attack2:
                {
                    script.Init(Duration, BossBulletManager.BulletState.Move, c_PlayerMove);
                    script.Move(c_PlayerMove);
                    return;
                }
            case BossBehaviorManager.BossAttackType.Attack3: break;
            default: break;
        }
        script.Init(Duration, BossBulletManager.BulletState.Destroy,c_PlayerMove);
    }
    private void DestroyInfoList(BossBulletManager obj, int CharaType, int EfectType)
    {
        obj.DestroyObjEvent -= DestroyInfoList;
        obj.transform.parent = c_ObjectPool.transform;

        l_BulletList.Remove(obj);
       if(l_BulletList.Count == 0)
            ResetAttackFlag();
        Debug.Log("消すことに成功");

        RemoveEfect(obj.gameObject,CharaType,EfectType);
    }
    private void RemoveEfect(GameObject obj,int Charatype,int EfectType)
    {
        CharaState e_CharaType = (CharaState)Charatype;
        ObjctPool.EfectType e_EfectType = (ObjctPool.EfectType)EfectType;

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
        b_IsBossCoroutine = false;
    }
    public void StopSe()
    {
        AudioManager.Instance.StopSe();
    }
    public void PlaySe(string name)
    {
        AudioManager.Instance.PlaySeAudio(name);
    }
    public void SetAnima() => a_Animator.SetFloat(BossMove, 0);
    public void SetIsMove()
    {
        b_IsBossCoroutine = true;
    }
    public void PlayorStopTransparent(bool IsTrue,bool IsHide)
    {
        a_Animator.SetBool(BossTransparent, IsTrue);
        a_Animator.SetBool(BossHide, IsHide);
    }
    private void OnSetBattle()//接敵したとき
    {
        ApplyAwake();
        ChangeBossInfo();

        //アニメーション再生
        PlayorStopTransparent(true, false);
        a_Animator.SetTrigger(BossFirstAnima);
    }
    private void ChangeBossInfo()
    {
        TatuGameManager.Instance.ActiveHpbar(CharaState.Boss, true);
        TatuGameManager.Instance.SetMoveFlag(true);
        c_BossBehaviorManager.ChangeClass(BossState.Idle);
    }
    private void ApplyAwake()
    {
        switch (c_BossBehaviorManager.e_AwakeHp)//追加するかも
        {
            case BossAwake.FirstForm: c_BossMoveManager.SetHp(); break;
            case BossAwake.SecondForm: break;
            case BossAwake.FinalForm: break;
        }
    }
    //----------------ここから攻撃処理の中身
    //ここに書くべきではなかった
    float nextTime = 0;
 
    public void Attack1(int InstantiateValue)//召喚魔法
    {
        c_BossMoveManager.SetIsAttackFlag(true);
        float height = c_Camera.orthographicSize;
        float width = height * c_Camera.aspect;
        for (int i = 0; i < InstantiateValue; i++)
        {
            var RandomPosY = UnityEngine.Random.Range(TatuGameManager.Instance.m_StageScaleMinY, TatuGameManager.Instance.m_StageScaleMaxY);
            var RandomPosX = UnityEngine.Random.Range(c_Camera.transform.position.x + width, c_Camera.transform.position.x + -width);//ここも変える

            var obj = c_ObjectPool.GetObject(CharaState.Boss, ObjctPool.EfectType.Shot);
            Vector3 Pos = new Vector3(RandomPosX, RandomPosY, RandomPosY);
            obj.transform.parent = null;
            SetBulletInfo(obj, Pos);
        }
    }
    public void Attack2(int InstantiateValue)//ジャンプが必要な攻撃
    {
        c_BossMoveManager.SetIsAttackFlag(true);
        a_Animator.SetInteger(BossAttackType, 4);
        SetIsMove();

        StartCoroutine(Attack2Instantiate(InstantiateValue));

    }
    public void Attack3(int attacktype)//移動して近接攻撃
    {
        c_BossMoveManager.SetIsAttackFlag(false);
        SetIsMove();
        StartCoroutine(Attack3Move(attacktype));
    }
    IEnumerator Attack2Instantiate(int Value)
    {
        float Duration = 1f;
        float CurrentTime = 1f;
        float height = c_Camera.orthographicSize;
        float width = height * c_Camera.aspect;
        float CurrentDirection = c_BossMoveManager.CurrentDirection;
        int CurrentRound = BattleManager.Instance.m_CurrentRound;

        while (true)
        {
            if (!b_IsBossCoroutine||CurrentRound != BattleManager.Instance.m_CurrentRound)
            {
                yield return null;
                continue;
            }
            c_BossMoveManager.SetIsHitFlag(true);
            CurrentTime += Time.deltaTime;
            if (CurrentTime >= Duration)
            {
                if (Value <= 0) break;
                CurrentTime = 0f;

                var obj = c_ObjectPool.GetObject(CharaState.Boss, ObjctPool.EfectType.Fire);
                Vector3 Pos = new Vector3(c_Camera.transform.position.x + (width * CurrentDirection),
                                              TatuGameManager.Instance.m_StageScaleMinY / 1.6f, TatuGameManager.Instance.m_StageScaleMinY / 1.6f);
                SetBulletInfo(obj,Pos);

                Value--;
            }
            yield return null;
        }
        PlayAnima();
        ReserAnima();
    }
    private void PlayAnima()
    {
        c_BossMoveManager.SetIsHitFlag(false);
        a_Animator.SetInteger(BossAttackType, 0);
        c_BossMoveManager.SetIsAttackFlag(false);
    }
    private void SetBulletInfo(GameObject obj,Vector3 Pos)//ここ変えるかどうか
    {
        obj.transform.position = Pos;
       if(obj.transform.parent != null)
            obj.transform.parent = null;

        SetBulletInfo(obj);
    }
    IEnumerator Attack3Move(int attacktype)
    {
        float offset = 3f;
        a_Animator.SetInteger(BossAttackType, (int)BossBehaviorManager.BossAttackType.Attack3Hide);

        Color col = r_SpriteRen.color;
        Color col1 = r_Shadow.color;
        int CurrentRound = BattleManager.Instance.m_CurrentRound;

        float t = 1;
        while (true)
        {
            if (!b_IsBossCoroutine||CurrentRound != BattleManager.Instance.m_CurrentRound)
            {

                col.a = 1;//値から才最大値まで往復
                col1.a = 1;

                r_SpriteRen.color = col;
                r_Shadow.color = col1;

                yield return new WaitUntil(()=>b_IsBossCoroutine);
            }
            t -= Time.deltaTime;

            col.a = Mathf.PingPong(t, 1);//値から才最大値まで往復
            col1.a = Mathf.PingPong(t, 1);
            r_SpriteRen.color = col;
            r_Shadow.color = col1;
 
            // αがほぼ0になった瞬間
            if (col.a < 0.01f)
            {
                Vector3 pl = SaveManager.Instance.c_CurrentData.GetCharacter(CharaState.Player).transform.position;

                Vector3 ReversePos = new Vector3(pl.x + (-c_BossMoveManager.CurrentDirection * offset), pl.y, pl.z);
                Vector3 oppPos = new Vector3(pl.x + (c_BossMoveManager.CurrentDirection * offset), pl.y, pl.z);
                //ここで画面外の場合は前に出す
                transform.position = oppPos; // 瞬間移動

                col.a = 1f;
                col1.a = 1f;// すぐ表示
                r_SpriteRen.color = col;
                r_Shadow.color = col1;

                break;
            }

            yield return null;
        }
        c_BossMoveManager.SetIsAttackFlag(true);
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
