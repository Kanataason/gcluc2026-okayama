using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using State = StateMachine<BossBehaviorManager>.State;
public class BossBehaviorManager : MonoBehaviour
{
    public enum BossState
    {
        Idle = 1,
        Attack = 2,
        Invincible = 4,
    }
    public enum BossAwake//値は行動間隔の短縮秒数にも使っている
    {
        FirstForm = 0,
        SecondForm = 2,
        FinalForm = 4
    }
    public enum BossAttackType//アニメーターのAttackTypeの値
    {
        Attack1 = 1,
        Attack2 = 2,
        Attack3 = 3,
        Hide = 4//Attack2のジャンプ中とAttack3の移動中
    }

    private const float BaseLotteryTime = 7f;//行動抽選の間隔

    //覚醒するHP 0:FirstForm→SecondForm 1:SecondForm→FinalForm
    private readonly int[] m_AwakeningHp = { 100, 50 };
    //形態ごとの攻撃の重み 並びはl_AttackEventと同じ(Attack3,Attack2,Attack1)
    private readonly int[][] m_AttackWeights =
    {
        new int[] { 40, 50, 60 },//FirstForm
        new int[] { 30, 45, 50 },//SecondForm
        new int[] { 40, 0, 70 },//FinalForm
    };

    public class AttackEvent//攻撃の情報
    {
        public BossAttackType e_BossAttackType;
        public int m_Weight;
    }

    private List<AttackEvent> l_AttackEvent;

    [FormerlySerializedAs("e_AwakeHp")]
    public BossAwake e_AwakeForm = BossAwake.FirstForm;

    private StateMachine<BossBehaviorManager> c_StateMachine;
    private BossAttackManager c_AttackManager;
    private BossBaseManager c_BaseManager;
    private StageContext c_Stage;

    public int FormIndex => (int)e_AwakeForm / 2;//形態の番号 0,1,2

    private void OnDisable()
    {
        if (c_Stage != null)
            c_Stage.OnBattle -= OnSetBattle;
    }
    public void Init(InputInfo inputInfo, StageContext stage)
    {
        GetComponents();
        c_Stage = stage;
        c_AttackManager.Init(inputInfo, stage);
        c_BaseManager.Init();
        c_Stage.OnBattle += OnSetBattle;

        InitAttackList();
        InitTransition();
        TeleportPos();
    }
    private void GetComponents()
    {
        c_BaseManager = GetComponent<BossBaseManager>();
        c_AttackManager = GetComponent<BossAttackManager>();
        c_StateMachine = new StateMachine<BossBehaviorManager>(this);
    }
    public void TeleportPos()//アニメーションイベント(名前変更不可)からも呼ばれる
    {
        //2画面のステージは同じ座標に重ねてあるので出現位置は共通
        transform.position = TatuGameManager.Instance.l_Infolist[FormIndex].v_TeleportPos;

        if (e_AwakeForm != BossAwake.FirstForm)
            c_AttackManager.SetTransparent(false, false);
    }
    public void ChangeClass(BossState state)
    {
        //ステートマシンの開始前(Init後0.5秒)に呼ばれても落ちないようにする
        if (c_StateMachine.CurrentState == null) return;
        c_StateMachine.Dispatch((int)state);
    }
    private void OnSetBattle()//接敵したとき
    {
        c_Stage.c_Hud.SetActive(CharaState.Boss, true);
        c_Stage.SetInBattle(true);
        c_BaseManager.SetHp();
        ChangeClass(BossState.Idle);

        c_AttackManager.PlayTeleportAnima();
    }
    private void InitAttackList()//攻撃を追加するときはこことm_AttackWeightsに追加
    {
        l_AttackEvent = new()
        {
            new AttackEvent(){ e_BossAttackType = BossAttackType.Attack3 },
            new AttackEvent(){ e_BossAttackType = BossAttackType.Attack2 },
            new AttackEvent(){ e_BossAttackType = BossAttackType.Attack1 },
        };
        SetAttackWeights();
    }
    private void SetAttackWeights()//今の形態の攻撃の重みにする
    {
        if (l_AttackEvent == null) return;

        int[] weights = m_AttackWeights[FormIndex];
        for (int i = 0; i < l_AttackEvent.Count; i++)
        {
            l_AttackEvent[i].m_Weight = weights[i];
        }
    }
    private void Update()
    {
        if (c_Stage == null || !c_Stage.b_InBattle) return;

        if (c_StateMachine.CurrentState != null) c_StateMachine.Updata();
    }
    private void InitTransition()//行動追加
    {
        c_StateMachine.AddTransition<Idle, Attack>((int)BossState.Attack);
        c_StateMachine.AddTransition<Attack, Idle>((int)BossState.Idle);
        c_StateMachine.AddTransition<Invincible, Idle>((int)BossState.Idle);
        c_StateMachine.AnyAddTrasition<Invincible>((int)BossState.Invincible);
        NextFrame.Run(this, 0.5f, () =>
        {
            c_StateMachine.Start<Idle>();
        });
    }
    private AttackEvent LotteryAction()//行動抽選
    {
        int total = 0;
        foreach (var attackEvent in l_AttackEvent)
            total += attackEvent.m_Weight;

        int randomValue = Random.Range(0, total);
        total = 0;
        foreach (var attackEvent in l_AttackEvent)
        {
            total += attackEvent.m_Weight;
            if (randomValue < total)
                return attackEvent;
        }
        return l_AttackEvent[l_AttackEvent.Count - 1];//重みが全部0のとき
    }
    public bool TryAwakening(float currentHp, out float awakeningHp)//覚醒するかの確認 覚醒したらHPをしきい値で止める
    {
        awakeningHp = currentHp;
        if (currentHp <= 0 || FormIndex >= m_AwakeningHp.Length) return false;
        if (currentHp >= m_AwakeningHp[FormIndex]) return false;

        awakeningHp = m_AwakeningHp[FormIndex];
        e_AwakeForm = e_AwakeForm == BossAwake.FirstForm ? BossAwake.SecondForm : BossAwake.FinalForm;
        SetAttackWeights();
        OnAwakening();
        return true;
    }
    private void OnAwakening()
    {
        c_Stage.SetInBattle(false);
        c_Stage.c_Hud.SetActive(CharaState.Boss, false);
        c_AttackManager.SetTransparent(false, true);
        ChangeClass(BossState.Invincible);

        c_Stage.OnBossAwakened();//相手のステージへの妨害
    }

    private class Idle : State
    {
        private float m_LotteryTime;
        private float m_CurrentTime;
        protected override void OnEnter(State prevstate)
        {
            Init();
        }
        protected override void OnUpdata()
        {
            if (owner.c_AttackManager.b_IsBossCoroutine) return;

            m_CurrentTime += Time.deltaTime;

            if (m_CurrentTime >= m_LotteryTime)
            {
                m_CurrentTime = 0;
                stateMachine.Dispatch((int)BossState.Attack);
            }
        }
        private void Init()
        {
            m_LotteryTime = BaseLotteryTime - (int)owner.e_AwakeForm;
            m_CurrentTime = 0;
        }
    }
    private class Attack : State
    {
        private bool b_WasAttacking = false;//前フレームで攻撃中だったか
        protected override void OnEnter(State prevstate)
        {
            b_WasAttacking = false;
            var action = owner.LotteryAction();
            owner.c_AttackManager.AttackEnter((int)action.e_BossAttackType);
        }
        protected override void OnUpdata()
        {
            bool isAttack = owner.c_BaseManager.GetIsAttackFlag();

            if (b_WasAttacking && !isAttack)
            {
                stateMachine.Dispatch((int)BossState.Idle);
            }

            b_WasAttacking = isAttack;
        }
    }
    private class Invincible : State { }//覚醒演出中 次の接敵まで何もしない
}
