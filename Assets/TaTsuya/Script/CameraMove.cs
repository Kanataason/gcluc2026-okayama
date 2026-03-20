using UnityEngine;

public class CameraMove : MonoBehaviour
{

    private GameObject g_Player;

    public float m_Offset;
    private void Start()
    {
        NextFrame.Run(this, 0.5f, () =>
        {
            g_Player = BattleManager.Instance.g_Player;
        });
    }
    private void LateUpdate()
    {
        if (g_Player == null) return;
        if (transform.position.x > 130) return;
        if (TatuGameManager.Instance == null) return;
        if (TatuGameManager.Instance.GetCameraMoveflag()) return;


        Vector3 cam = Camera.main.transform.position;

        if (g_Player.transform.position.x > cam.x + m_Offset)
        {
            cam.x = g_Player.transform.position.x - m_Offset;
            Camera.main.transform.position = cam;
        }

    }
}
