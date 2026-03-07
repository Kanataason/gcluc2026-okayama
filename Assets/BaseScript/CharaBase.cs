using UnityEngine;

public class CharaBase : MonoBehaviour
{
    public virtual void SetPos() { }//画面切り替え時点でいる場所を保存
    public virtual void GetPos() { }//前回の場面を取ってセット
    public virtual void SetState() { }//画面切り替え時点何をしているのかhp,flagや（アニメーション）を保存
                              //アニメーションが現在何秒まで進んでいるのかを
    public virtual void GetState() {}//前回のステータスをセット
    public virtual void TakeDamage(int damage) { m_hp -= damage; }//攻撃を食らったときの関数
    public virtual void Die() { }//死んだとき

    protected Animator a_Animator;
    protected int m_hp;
    protected SaveState c_SaveState = new SaveState();
}
[System.Serializable]
public class SaveState
{
    //アニメの処理
    public float m_AnimeTime;
    public int m_AnimeHash;

    public int m_Inihp;
    public Vector3 v_IniPosition;
    public Quaternion q_IniRotate;
}