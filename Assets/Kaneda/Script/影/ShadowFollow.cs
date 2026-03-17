using UnityEngine;

// 影追従クラス
// プレイヤーの地面位置だけを追従する
public class ShadowFollow : MonoBehaviour
{
    // プレイヤー
    Transform t_Player;

    // プレイヤー移動管理
    PlayerMoveManager c_PlayerMoveManager;

    // 影のXずれ
    const float OFFSET_X = 0f;

    // 影のYずれ
    const float OFFSET_Y = -0.5f;

    // 影のZ位置
    const float SHADOW_Z = 0f;

    void Start()
    {
        // 同じ親から Player を探す
        Transform parent = transform.parent;

        if (parent != null)
        {
            t_Player = parent.Find("Player");
        }

        // PlayerMoveManager を取得
        if (t_Player != null)
        {
            c_PlayerMoveManager = t_Player.GetComponent<PlayerMoveManager>();
        }
    }

    void LateUpdate()
    {
        if (t_Player == null) return;
        if (c_PlayerMoveManager == null) return;

        // プレイヤーのX位置
        float playerX = t_Player.position.x;

        // プレイヤーの地面Y位置
        float groundY = c_PlayerMoveManager.GetShadowGroundY();

        // 影の位置更新
        transform.position = new Vector3(
            playerX + OFFSET_X,
            groundY + OFFSET_Y,
            SHADOW_Z
        );
    }
}