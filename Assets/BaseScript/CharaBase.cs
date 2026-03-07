using UnityEngine;

public class CharaBase : MonoBehaviour
{
    public virtual void Start() { a_Animator = GetComponent<Animator>(); }
    public virtual void Update() { }
    public virtual void SetPos(Vector3 Pos) { transform.position = Pos; }//画面切り替え時点でいる場所を保存
    public virtual Vector3 GetPos() { return transform.position; }//前回の場面を取ってセット
    public virtual void SetStatus(int AnimeName =0) //画面切り替え時点何をしているのかhp,flagや（アニメーション）を保存
                                   //アニメーションが現在何秒まで進んでいるのかを
    {
        c_SaveState.m_Inihp = m_hp;
        c_SaveState.v_IniPosition = GetPos();
        c_SaveState.q_IniRotate = transform.rotation;
        c_SaveState.b_IsAttack = GetIsAttackFlag();
        c_SaveState.m_AnimeHashName = AnimeName;
    }
    public virtual void GetStatus() //前回のステータスをセット
    {
        m_hp = c_SaveState.m_Inihp;
        SetPos(c_SaveState.v_IniPosition);
        transform.rotation = c_SaveState.q_IniRotate;
        m_IsAttack = c_SaveState.b_IsAttack;
    }
    public virtual void SetAnimetion(float nowAnime, float animeValue, int animeName)//現在のアニメーションの進行度、アニメのステートの値、名前
    {
        c_SaveState.m_AnimeTime = nowAnime;
        c_SaveState.m_AnimeHash = animeName;
        c_SaveState.m_AnimeStateValue = animeValue;
    }
    public virtual void TakeDamage(int damage) { m_hp -= damage; }//攻撃を食らったときの関数
    public virtual void Die() { Debug.Log("死んだ"); }//死んだとき

    public virtual void SetIsAttackFlag(bool active) { m_IsAttack = active; }//攻撃初めのフラグ
    public virtual bool GetIsAttackFlag() { return m_IsAttack; }
    public virtual int GetAnimeHashCode() { return m_AnimeHashcode; }

    protected Animator a_Animator;
    protected int m_hp;
    protected int m_AnimeHashcode;
    protected SaveState c_SaveState = new SaveState();

    private bool m_IsAttack = false;
}
[System.Serializable]
public class SaveState
{
    //アニメの処理
    public float m_AnimeTime;//現在のアニメ進行時間
    public int m_AnimeHash;//どこでアニメーションをするか
    public int m_AnimeHashName;//アニメーション単体の名前
    public float m_AnimeStateValue;//現在のアニメのステートの値

    public int m_Inihp;//現在のhp
    public Vector3 v_IniPosition;//現在の自分の場所
    public Quaternion q_IniRotate;//現在の回転値
    public bool b_IsAttack;//攻撃フラグ
}