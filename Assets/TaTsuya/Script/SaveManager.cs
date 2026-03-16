using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    //それぞれ一個ずつでいいかも
    private StageSaveData c_Stage1SaveData = new();
    private StageSaveData c_Stage2SaveData  = new();

    public StageSaveData c_CurrentData = new StageSaveData();

    private void Start()
    {
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"s1{c_Stage1SaveData.c_BossData.l_ObjList.Count}  / s2{c_Stage2SaveData.c_BossData.l_ObjList.Count}");
        }
    }

    public void ResetSaveData()
    {
        c_Stage1SaveData = null;
        c_Stage2SaveData = null;
    }

    public void SetSaveData(CharaState state,SaveState data)//セーブデータを設定する
    {
        if(c_Stage1SaveData.c_PlayerData.g_Character == null || c_Stage2SaveData.c_BossData.g_Character == null)//初期値を与える
        {
            if (state == CharaState.Player)
            {
                 c_Stage1SaveData.SetPlayerState(data);
                 c_Stage2SaveData.SetPlayerState(data);
            }
            else if (state == CharaState.Boss)
            {
                c_Stage1SaveData.SetBossState(data);
                c_Stage2SaveData.SetBossState(data);
            }

            CheckRound();
            return;
        }
       // Debug.Log("セット");
        if (state == CharaState.Player)
        {
            c_CurrentData.SetPlayerState(data);
        }
        else if(state == CharaState.Boss)
        {
            c_CurrentData.SetBossState(data);
        }
       if(data.l_ObjList.Count >0) data.l_ObjList.Clear();
    }
    public void SetCurrenBossInfo(BossBehaviorManager.BossAwake awake, float CurrentTime)//ボスの状況を保存
    {
        c_CurrentData.c_BossData.e_BossAwake = awake;
        c_CurrentData.c_BossData.m_ActionTime = CurrentTime;
    }
    public void CheckRound()
    {
       // Debug.Log("チェック");
        BattleManager battle = BattleManager.Instance;
        c_CurrentData = battle.m_CurrentRound == 1 ? c_Stage1SaveData : c_Stage2SaveData;
    }
    public void RemoveList(CharaState State,int Round)
    {
        List<BossBulletManager> CurrentDatas = null;
        StageSaveData data = Round == 1 ? c_Stage1SaveData : c_Stage2SaveData;
        CurrentDatas = data.GetObjList(State);
        if (CurrentDatas.Count > 0)
        {
            CurrentDatas.Clear();
            Debug.Log("削除");
        }
    }
    public StageSaveData GetCurrentData(int CurrentRound) => CurrentRound == 1 ? c_Stage1SaveData : c_Stage2SaveData;
}
[System.Serializable]
public class StageSaveData//全体のsave
{
    public SaveState c_PlayerData = new SaveState();
    public SaveState c_BossData = new SaveState();

    //ラウンドごとの数値
    public int m_TotalRound =1;
    public float m_TimeScore =0;
    public bool b_IsTeleport;
    public Vector3 v_CameraPos = new Vector3(0, 0, -11);
    public BossBehaviorManager.BossAwake e_Awake = BossBehaviorManager.BossAwake.FirstForm;
    public void InitState()
    {
        c_PlayerData.Init();

        c_BossData.Init();
    }
    //合わせてもいいがあえてこのままでいく
    public void SetPlayerState(SaveState data)
    {
        c_PlayerData.m_AnimeTime = data.m_AnimeTime;
        c_PlayerData.m_AnimeHash = data.m_AnimeHash;
        c_PlayerData.m_AnimeHashName = data.m_AnimeHashName;
        c_PlayerData.m_AnimeStateValue = data.m_AnimeStateValue;

        c_PlayerData.m_Inihp = data.m_Inihp;
        c_PlayerData.v_IniPosition = data.v_IniPosition;
        c_PlayerData.q_IniRotate = data.q_IniRotate;
        c_PlayerData.b_IsAttack = data.b_IsAttack;

        c_PlayerData.g_Character = data.g_Character;

        c_PlayerData.l_ObjList = new System.Collections.Generic.List<BossBulletManager>(data.l_ObjList);
    }
    public void SetBossState(SaveState data)
    {
        c_BossData.m_AnimeTime = data.m_AnimeTime;
        c_BossData.m_AnimeHash = data.m_AnimeHash;
        c_BossData.m_AnimeHashName = data.m_AnimeHashName;
        c_BossData.m_AnimeStateValue = data.m_AnimeStateValue;

        c_BossData.m_Inihp = data.m_Inihp;
        c_BossData.v_IniPosition = data.v_IniPosition;
        c_BossData.q_IniRotate = data.q_IniRotate;
        c_BossData.b_IsAttack = data.b_IsAttack;

        c_BossData.g_Character = data.g_Character;
        c_BossData.b_IsMove = data.b_IsMove;
        c_BossData.b_IsTransparent = data.b_IsTransparent;

        c_BossData.l_ObjList = new System.Collections.Generic.List<BossBulletManager>(data.l_ObjList);
    }
    public SaveState GetPlayerState(StageSaveData data)
    {
        SaveState save = new SaveState();

        save.m_AnimeHash = data.c_BossData.m_AnimeHash;
        save.m_AnimeTime = data.c_BossData.m_AnimeTime;
        save.m_AnimeStateValue = data.c_BossData.m_AnimeStateValue;
        save.m_AnimeHashName = data.c_BossData.m_AnimeHashName;

        save.m_Inihp = data.c_BossData.m_Inihp;
        save.v_IniPosition = data.c_BossData.v_IniPosition;
        save.q_IniRotate = data.c_BossData.q_IniRotate;
        save.b_IsAttack = data.c_BossData.b_IsAttack;

        save.l_ObjList = data.c_BossData.l_ObjList;

        return save;
    }
    public SaveState GetBossState(StageSaveData data)
    {
        SaveState save = new SaveState();

        save.m_AnimeHash = data.c_BossData.m_AnimeHash;
        save.m_AnimeTime = data.c_BossData.m_AnimeTime;
        save.m_AnimeStateValue = data.c_BossData.m_AnimeStateValue;
        save.m_AnimeHashName = data.c_BossData.m_AnimeHashName;

        save.m_Inihp = data.c_BossData.m_Inihp;
        save.v_IniPosition = data.c_BossData.v_IniPosition;
        save.q_IniRotate = data.c_BossData.q_IniRotate;
        save.b_IsAttack = data.c_BossData.b_IsAttack;
        save.b_IsMove = data.c_BossData.b_IsMove;

        save.l_ObjList = data.c_BossData.l_ObjList;

        save.m_ActionTime = data.c_BossData.m_ActionTime;
        save.e_BossAwake = data.c_BossData.e_BossAwake;
        save.b_IsTransparent = data.c_BossData.b_IsTransparent;

        return save;
    }

    public List<BossBulletManager> GetObjList(CharaState state)
    {
        List<BossBulletManager> save = null;
        switch (state)
        {
            case CharaState.Player:save = c_PlayerData.l_ObjList; break;
            case CharaState.Boss:save = c_BossData.l_ObjList; break;
            default:break;
        }
        return save;
    }
    public GameObject GetCharacter(CharaState TargetState) //欲しい方のステータスを設定
    {
        GameObject obj = null;
        switch (TargetState)
        {
            case CharaState.Player: obj = c_PlayerData.g_Character; break;
            case CharaState.Boss: obj = c_BossData.g_Character; break;
            default: break;
        }
        return obj;
    }
}