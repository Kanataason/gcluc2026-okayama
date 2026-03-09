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

   [HideInInspector] public StageSaveData CurrentData = new StageSaveData();
    private void Start()
    {
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
        if(c_Stage1SaveData == null || c_Stage2SaveData == null)
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
        }
        Debug.Log("セット");
        if (state == CharaState.Player)
        {
            CurrentData.SetPlayerState(data);
        }
        else if(state == CharaState.Boss)
        {
            CurrentData.SetBossState(data);
        }
    }
    public void CheckRound()
    {
        Debug.Log("チェック");
        BattleManager battle = BattleManager.Instance;
        CurrentData = battle.m_CurrentRound == 1 ? c_Stage1SaveData : c_Stage2SaveData;
    }
}
[System.Serializable]
public class StageSaveData//全体のsave
{
    public SaveState c_PlayerData = new SaveState();
    public SaveState c_BossDate = new SaveState();
    public void InitState()
    {
        c_PlayerData.Init();

        c_BossDate.Init();
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
    }
    public void SetBossState(SaveState data)
    {
        c_BossDate.m_AnimeTime = data.m_AnimeTime;
        c_BossDate.m_AnimeHash = data.m_AnimeHash;
        c_BossDate.m_AnimeHashName = data.m_AnimeHashName;
        c_BossDate.m_AnimeStateValue = data.m_AnimeStateValue;

        c_BossDate.m_Inihp = data.m_Inihp;
        c_BossDate.v_IniPosition = data.v_IniPosition;
        c_BossDate.q_IniRotate = data.q_IniRotate;
        c_BossDate.b_IsAttack = data.b_IsAttack;
    }
    public SaveState GetPlayerState(StageSaveData data)
    {
        SaveState save = new SaveState();

        save.m_AnimeHash = data.c_BossDate.m_AnimeHash;
        save.m_AnimeTime = data.c_BossDate.m_AnimeTime;
        save.m_AnimeStateValue = data.c_BossDate.m_AnimeStateValue;
        save.m_AnimeHashName = data.c_BossDate.m_AnimeHashName;

        save.m_Inihp = data.c_BossDate.m_Inihp;
        save.v_IniPosition = data.c_BossDate.v_IniPosition;
        save.q_IniRotate = data.c_BossDate.q_IniRotate;
        save.b_IsAttack = data.c_BossDate.b_IsAttack;

        return save;
    }
    public SaveState GetBossState(StageSaveData data)
    {
        SaveState save = new SaveState();

        save.m_AnimeHash = data.c_BossDate.m_AnimeHash;
        save.m_AnimeTime = data.c_BossDate.m_AnimeTime;
        save.m_AnimeStateValue = data.c_BossDate.m_AnimeStateValue;
        save.m_AnimeHashName = data.c_BossDate.m_AnimeHashName;

        save.m_Inihp = data.c_BossDate.m_Inihp;
        save.v_IniPosition = data.c_BossDate.v_IniPosition;
        save.q_IniRotate = data.c_BossDate.q_IniRotate;
        save.b_IsAttack = data.c_BossDate.b_IsAttack;

        return save;
    }
}