using UnityEngine;

public class PlayerSteato : MonoBehaviour
{
    //プレイヤー状態
    public enum PreyerState
    {
        Idle,     //待機
        Move,     //移動
        Attack,   //攻撃
        Damage,   //ダメージ
        Die       //死亡
    }
}
