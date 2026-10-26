using UnityEngine;

public class CameraMove : MonoBehaviour
{
    private const float CameraZ = -11f;
    private const float MaxCameraX = 130f;//これより右には動かない

    public float m_Offset;
    public int StageNum;

    private StageContext c_Stage;
    private Transform t_Player;
    private float m_CameraY;

    public void Init(StageContext stage, float cameraY)//キャラクターが生成されたらBattleManagerから呼ばれる
    {
        c_Stage = stage;
        StageNum = stage.m_StageNum;
        t_Player = stage.c_Player.transform;
        m_CameraY = cameraY;

        transform.position = new Vector3(t_Player.position.x, m_CameraY, CameraZ);
    }
    private void LateUpdate()
    {
        if (c_Stage == null || t_Player == null) return;
        if (transform.position.x > MaxCameraX) return;
        if (c_Stage.b_InBattle) return;//ボス戦中はカメラを止める

        Vector3 cam = transform.position;
        if (t_Player.position.x > cam.x + m_Offset)
        {
            cam.x = t_Player.position.x - m_Offset;
            transform.position = cam;
        }
    }
}
