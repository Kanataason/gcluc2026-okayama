using UnityEngine;


//プレイヤー管理クラス
// プレイヤーの各Managerを管理する
public class PlayerManager : CharaBase
{
    //移動管理クラス
    PlayerMoveManager c_PlayerMove;

    // 初期化
    public override void Start()
    {
        base.Start();

        //移動マネージャ取得
        c_PlayerMove = GetComponent<PlayerMoveManager>();
    }

    /// 毎フレーム更新
    public override void Update()
    {
        //移動処理呼び出し
        if (c_PlayerMove != null)
        {
            c_PlayerMove.MoveUpdate();
        }
    }
}