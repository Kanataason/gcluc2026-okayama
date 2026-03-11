using UnityEngine;

public class Player : MonoBehaviour
{
    //プレイヤー状態
    public enum PlayerState
    {
        Idle,     //待機
        Move,     //移動
        Jump,     //ジャンプ
        Attack,   //攻撃
        Damage,   //ダメージ
        Die       //死亡
    }
}
