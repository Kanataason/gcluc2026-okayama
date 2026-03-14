using UnityEngine;

public class CameraMove : MonoBehaviour
{

    private GameObject g_Player;

    private void Start()
    {
        g_Player = GameObject.FindWithTag("Player");
    }
    private void LateUpdate()
    {
        if (TatuGameManager.Instance == null) return;
        if (TatuGameManager.Instance.GetCameraMoveflag()) return;
        if (g_Player == null) return;

        float offset = -2f;

        Vector3 cam = Camera.main.transform.position;

        if (g_Player.transform.position.x > cam.x + offset)
        {
            cam.x = g_Player.transform.position.x - offset;
            Camera.main.transform.position = cam;
        }

    }
}
