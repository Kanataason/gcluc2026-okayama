using System;
using System.Collections.Generic;
using UnityEngine;
using State = StateMachine<BossBehaviorManager>.State;
public class BossBehaviorManager : MonoBehaviour
{
   public enum BossState
    {
        Idle = 1,
        Attack = 2,
        Die = 3,
        Invincible = 4,
    }
    public enum BossAwake
    {
        FirstForm = 0,
        SecondForm = 2,
        FinalForm = 4
    }
    private int[] m_AwakeningHp = { 100, 50, 30 };
    public enum BossAttackType
    {
        Attack1 = 1,
        Attack2 = 2,
        Attack3 = 3,
        Attack3Hide = 4
    }

    [Serializable]
    public class AttackEvent
    {
        public BossAttackType e_BossAttackType;
        public int m_Weight;
        public Action<int> a_AttackAction;
    }

    private List<AttackEvent> l_AttackEvent;

    //private BossState e_State = BossState.Idle;
    public BossAwake e_AwakeHp = BossAwake.FirstForm;
    public float m_CurrentActionTime = 0;

    private StateMachine<BossBehaviorManager> c_BossBehaviorManager;
    private BossAttackManager c_AttackManager;
    private BossBaseManager c_BaseManager;

    public bool m_IsAwakening => c_BaseManager.GetHp() % 50 == 0;

    void Start()
    {
        TeleportPos();
        c_BaseManager = GetComponent<BossBaseManager>();
        c_AttackManager = GetComponent<BossAttackManager>();
        c_BossBehaviorManager = new StateMachine<BossBehaviorManager>(this);
        InitDictionary();
        InitTransition();

    }
    public void TeleportPos()//これはアニメーションイベントから呼ばれたり最初に呼ばれる
    {
        Vector3 CurrentPos = Vector3.zero;
        switch (e_AwakeHp)
        {
            case BossAwake.FirstForm:
                CurrentPos = TatuGameManager.Instance.l_Infolist[0].v_TeleportPos;break;
            case BossAwake.SecondForm:
                CurrentPos = TatuGameManager.Instance.l_Infolist[1].v_TeleportPos;
                c_AttackManager.PlayorStopTransparent(false,false); break;
            case BossAwake.FinalForm:
                CurrentPos = TatuGameManager.Instance.l_Infolist[2].v_TeleportPos;
                c_AttackManager.PlayorStopTransparent(false,false); break;
        }
        transform.position = CurrentPos;
    }
    public void ChangeClass(BossState state) { c_BossBehaviorManager.Dispatch((int)state); }
    private void InitDictionary()//リスト初期化 攻撃を追加するときはここに追加
    {
        l_AttackEvent = new()
        {
            new AttackEvent(){m_Weight = 30,a_AttackAction = c_AttackManager.AttackEnter,e_BossAttackType = BossAttackType.Attack3},
             new AttackEvent(){m_Weight = 40,a_AttackAction = c_AttackManager.AttackEnter,e_BossAttackType = BossAttackType.Attack2},
              new AttackEvent(){m_Weight = 50,a_AttackAction = c_AttackManager.AttackEnter, e_BossAttackType = BossAttackType.Attack1},
        };
    }
    private void ChangeValue(int[] values)//確率を変えるための変数
    {
        for (int i = 0; i < l_AttackEvent.Count; i++)
        {
            l_AttackEvent[i].m_Weight = values[i];
        }
    }
    private void Update()
    {
        if (TatuGameManager.Instance == null||!TatuGameManager.Instance.m_BossTeleport) return;

        if (c_BossBehaviorManager.CurrentState != null) c_BossBehaviorManager.Updata();
    }
    private void InitTransition()//行動追加
    {
        c_BossBehaviorManager.AddTransition<Idle,Attack>((int)BossState.Attack);
        c_BossBehaviorManager.AddTransition<Attack,Idle>((int)BossState.Idle);
        c_BossBehaviorManager.AddTransition<Invincible, Idle>((int)BossState.Idle);
        c_BossBehaviorManager.AnyAddTrasition<Die>((int)BossState.Die);
        c_BossBehaviorManager.AnyAddTrasition<Invincible>((int)BossState.Invincible);
        NextFrame.Run(this, 0.5f, () =>
        {
            c_BossBehaviorManager.Start<Idle>();
        });
    }
    private AttackEvent LotteryAction()//行動抽選
    {
        int total = 0;
        foreach (var key in l_AttackEvent)
            total += key.m_Weight;

        int Random = UnityEngine.Random.Range(0, total);
        Debug.Log(Random);
        total = 0;
        foreach (var key in l_AttackEvent)
        {
            total += key.m_Weight;
            if (Random < total)
                return key;
        }
        return null;
    }
    public float CheckBossAwakening(float CurrentHp)//覚醒するかの確認
    {
        if (CurrentHp <= 0) return 0;
        switch (e_AwakeHp)
        {
            case BossAwake.FirstForm:
                if (CurrentHp < m_AwakeningHp[0])
                {
                    Debug.Log("覚醒");
                    e_AwakeHp = BossAwake.SecondForm;
                    int[] numsss = { 50, 45, 50 };
                    ChangeValue(numsss);
                    TatuGameManager.Instance.SetMoveFlag(false);
                    c_AttackManager.PlayorStopTransparent(false,true);
                    TatuGameManager.Instance.ActiveHpbar(CharaState.Boss, false);
                    ChangeClass(BossState.Invincible);
                    return m_AwakeningHp[0];
                }
                break;

            case BossAwake.SecondForm:
                if (CurrentHp < m_AwakeningHp[1])
                {
                    e_AwakeHp = BossAwake.FinalForm;
                    int[] numss = { 40, 0, 70 };
                    TatuGameManager.Instance.SetMoveFlag(false);
                    c_AttackManager.PlayorStopTransparent(false, true);
                    TatuGameManager.Instance.ActiveHpbar(CharaState.Boss, false);
                    ChangeClass(BossState.Invincible);
                    ChangeValue(numss);
                    return m_AwakeningHp[1];
                }
                break;
            default:
                int[] nums = { 50, 45, 50 };
                ChangeValue(nums); break;
        }
        return CurrentHp;

    }
    private class Idle : State
    {
        private float m_LotteryTime;
        private float m_CurrentTime;
        private float m_Duration;

        private Vector3 v_CurrentDirection;
        protected override void OnEnter(State prevstate)
        {
            owner.c_AttackManager.SetAnima();
            v_CurrentDirection = Vector3.zero;
            m_Duration = 2;
            m_LotteryTime = 7;
            m_CurrentTime = 0;
            m_LotteryTime -=(int)owner.e_AwakeHp;
            owner.c_AttackManager.e_AnimaType = BossAttackManager.AnimaType.Move;
        }
        protected override void OnUpdata()
        {
            if (owner.c_AttackManager.m_IsBossCoroutine1) return;

            if (BattleManager.Instance.b_IsLoading) SetTime();
            m_CurrentTime += Time.deltaTime;
        //  v_CurrentDirection = owner.c_AttackManager.Move(m_CurrentTime,m_Duration,v_CurrentDirection);
            owner.m_CurrentActionTime = m_CurrentTime;
            if(m_CurrentTime >= m_LotteryTime)
            {
                m_CurrentTime = 0;
                stateMachine.Dispatch((int)BossState.Attack);     
            }
        }
        protected override void OnExit(State nextstate)
        {
            Debug.Log("行動開始");
        }
        private void SetTime()
        {
            m_CurrentTime = owner.m_CurrentActionTime;
        }
    }
    private class Attack : State
    {
        bool IsFirst = false;
        protected override void OnEnter(State prevstate)
        {
            IsFirst = false;
         var action = owner.LotteryAction();
            action.a_AttackAction.Invoke((int)action.e_BossAttackType);
        }
        protected override void OnUpdata()
        {
            bool IsAttack = owner.c_BaseManager.GetIsAttackFlag();

            if (IsFirst && !IsAttack)
            {
                stateMachine.Dispatch((int)BossState.Idle);
            }

            IsFirst = IsAttack;
        }
        protected override void OnExit(State nextstate)
        {
            Debug.Log("攻撃終わりｂ");
        }

    }
    private class Die : State
    {
        protected override void OnEnter(State prevstate)
        {
            base.OnEnter(prevstate);
        }
        protected override void OnUpdata()
        {
            base.OnUpdata();
        }
        protected override void OnExit(State nextstate)
        {
            base.OnExit(nextstate);
        }
    }
    private class Invincible : State
    {
        protected override void OnEnter(State prevstate)
        {
            base.OnEnter(prevstate);
        }
        protected override void OnUpdata()
        {
            base.OnUpdata();
        }
        protected override void OnExit(State nextstate)
        {
            base.OnExit(nextstate);
        }
    }
   
}
public static class NextFrame
{
    private static readonly WaitForSeconds c_waitForSeconds = new WaitForSeconds(0.1f);
    public static void OneFrame(MonoBehaviour owner,System.Action action)
    {
        owner.StartCoroutine(RunOneFrame(action));
    }
    public static void Run(MonoBehaviour owner, float Timer, System.Action action)
    {
        owner.StartCoroutine(RunCoroutine(action,Timer));
    }
    private static System.Collections.IEnumerator RunOneFrame(System.Action action)
    {
        yield return c_waitForSeconds;
        action?.Invoke();
    }
    private static System.Collections.IEnumerator RunCoroutine(System.Action action,float Timer)
    {
        yield return new WaitForSeconds(Timer);
        action?.Invoke();
    }
}