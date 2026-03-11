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
    private StageSaveData c_Stage1SaveData =null;
    private StageSaveData c_Stage2SaveData =null;

    public StageSaveData CurrentData = new StageSaveData();

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
    void InitSaveData()
    {
        c_Stage1SaveData = new StageSaveData();
        c_Stage2SaveData = new StageSaveData();
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
            InitSaveData();
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
            CurrentData.SetPlayerState(data);
        }
        else if(state == CharaState.Boss)
        {
            CurrentData.SetBossState(data);
        }
       if(data.l_ObjList.Count >0) data.l_ObjList.Clear();
    }
    public void SetCurrenBossInfo(BossBehaviorManager.BossAwake awake, float CurrentTime)//ボスの状況を保存
    {
        CurrentData.c_BossData.e_BossAwake = awake;
        CurrentData.c_BossData.m_ActionTime = CurrentTime;
    }
    public void CheckRound()
    {
       // Debug.Log("チェック");
        BattleManager battle = BattleManager.Instance;
        CurrentData = battle.m_CurrentRound == 1 ? c_Stage1SaveData : c_Stage2SaveData;
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
}
[System.Serializable]
public class StageSaveData//全体のsave
{
    public SaveState c_PlayerData = new SaveState();
    public SaveState c_BossData = new SaveState();
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

        save.l_ObjList = data.c_BossData.l_ObjList;

        save.m_ActionTime = data.c_BossData.m_ActionTime;
        save.e_BossAwake = data.c_BossData.e_BossAwake;
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
    public GameObject GetCharacter(CharaState state) //欲しい方のステータスを設定
    {
        GameObject obj = null;
        switch (state)
        {
            case CharaState.Player: obj = c_PlayerData.g_Character; break;
            case CharaState.Boss: obj = c_BossData.g_Character; break;
            default: break;
        }
        return obj;
    }
}