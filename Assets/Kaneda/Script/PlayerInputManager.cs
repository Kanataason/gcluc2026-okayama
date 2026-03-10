using UnityEngine;


// プレイヤー入力管理クラス
// 入力の取得のみを担当する
public class PlayerInputManager : MonoBehaviour
{
    //横入力
    float h_Input;

    //縦入力
    float v_Input;
    // 毎フレーム入力を取得
    void Update()
    {
        h_Input = Input.GetAxisRaw("Horizontal");
        v_Input = Input.GetAxisRaw("Vertical");
    }


    // 横入力取得
    public float GetHorizontal()
    {
        return h_Input;
    }

    //縦入力取得
    public float GetVertical()
    {
        return v_Input;
    }
}