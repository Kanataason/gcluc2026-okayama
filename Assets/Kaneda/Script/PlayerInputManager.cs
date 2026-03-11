using UnityEngine;

// プレイヤー入力管理クラス
// WASD入力を取得する
public class PlayerInputManager : MonoBehaviour
{
    //横入力
    float h_Input;

    //縦入力
    float v_Input;
    
    //ジャンプ入力
    bool j_Input;
    void Update()
    {
        //入力リセット
        h_Input = 0;
        v_Input = 0;
        //Aキー（左移動）
        if (Input.GetKey(KeyCode.A))
        {
            h_Input = -1;
        }
        //Dキー（右移動）
        if (Input.GetKey(KeyCode.D))
        {
            h_Input = 1;
        }
        //Wキー（上移動）
        if (Input.GetKey(KeyCode.W))
        {
            v_Input = 1;
        }
        //Sキー（下移動）
        if (Input.GetKey(KeyCode.S))
        {
            v_Input = -1;
        }

        //ジャンプ
        j_Input = Input.GetKeyDown(KeyCode.Space);
    }
    // 横入力取得
    public float GetHorizontal()
    {
        return h_Input;
    }
    // 縦入力取得
    public float GetVertical()
    {
        return v_Input;
    }

    public bool GetJump()
    {
        return j_Input;
    }
}