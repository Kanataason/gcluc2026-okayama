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
    public StageSaveData c_Stage1SaveData;
    public StageSaveData c_Stage2SaveData;

    private StageSaveData CurrentData;
    public void SetSaveData(CharaState state,SaveState data)
    {
        BattleManager battle = BattleManager.Instance;
        CurrentData = battle.m_CurrentRound == 1 ? c_Stage1SaveData : c_Stage2SaveData;

        if (state == CharaState.Player)
        {
            CurrentData.SetPlayerState(data);
        }
        else if(state == CharaState.Boss)
        {
            CurrentData.SetBossState(data);
        }
        Debug.Log($"{CurrentData.b_BIsAttack}");
    }
}
[System.Serializable]
public class StageSaveData//全体のsave
{
    //ボスの処理
    //アニメの処理
    public float m_BAnimeTime;//現在のアニメ進行時間
    public int m_BAnimeHash;//どこでアニメーションをするか
    public int m_BAnimeHashName;//アニメーション単体の名前
    public float m_BAnimeStateValue;//現在のアニメのステートの値

    public int m_BInihp;//現在のhp
    public Vector3 v_BIniPosition;//現在の自分の場所
    public Quaternion q_BIniRotate;//現在の回転値
    public bool b_BIsAttack;//攻撃フラグ

    //プレイヤーの処理
    //アニメの処理
    public float m_PAnimeTime;//現在のアニメ進行時間
    public int m_PAnimeHash;//どこでアニメーションをするか
    public int m_PAnimeHashName;//アニメーション単体の名前
    public float m_PAnimeStateValue;//現在のアニメのステートの値

    public int m_PInihp;//現在のhp
    public Vector3 v_PIniPosition;//現在の自分の場所
    public Quaternion q_PIniRotate;//現在の回転値
    public bool b_PIsAttack;//攻撃フラグ

    public void SetPlayerState(SaveState data)
    {
        m_PAnimeTime = data.m_AnimeTime;
        m_PAnimeHash = data.m_AnimeHash;
        m_PAnimeHashName = data.m_AnimeHashName;
        m_PAnimeStateValue = data.m_AnimeStateValue;

        m_PInihp = data.m_Inihp;
        v_PIniPosition = data.v_IniPosition;
        q_PIniRotate = data.q_IniRotate;
        b_PIsAttack = data.b_IsAttack;
    }
    public void SetBossState(SaveState data)
    {
        m_BAnimeTime = data.m_AnimeTime;
        m_BAnimeHash = data.m_AnimeHash;
        m_BAnimeHashName = data.m_AnimeHashName;
        m_BAnimeStateValue = data.m_AnimeStateValue;

        m_BInihp = data.m_Inihp;
        v_BIniPosition = data.v_IniPosition;
        q_BIniRotate = data.q_IniRotate;
        b_BIsAttack = data.b_IsAttack;
    }
}