using UnityEngine;

public class CameraMove : MonoBehaviour
{

    private InputInfo s_inputlist;

    public GameObject g_Player;
    public float m_Offset;
    public int StageNum;

    private void Start()//イベントを登録
    {
        BattleManager.Instance.OnCreateCharacters += Init;
    }
    private void OnDisable()
    {
        BattleManager.Instance.OnCreateCharacters -= Init;
    }
    public void Init(GameObject boss, InputInfo inputInfo)//キャラクターが生成されたら呼ばれる
    {
        NextFrame.Run(this, 0.5f, () =>
        {
            if (inputInfo.PlayerNum != StageNum) return;
            g_Player = inputInfo.TargetObj.GetComponentInChildren<PlayerMove>().gameObject;
            s_inputlist = inputInfo;
            Debug.Log($"Pos{g_Player.transform.position}/ower{this.gameObject.name}");
            SetCemeraPos(g_Player);
        });
    }
    private void SetCemeraPos(GameObject target)
    {
        Vector3 targetPos = target.transform.position;
        transform.position = new Vector3(targetPos.x, targetPos.y, -11);
    }
    private void LateUpdate()
    {
        if (g_Player == null) return;
        if (transform.position.x > 130) return;
        if (TatuGameManager.Instance == null) return;
        if (TatuGameManager.Instance.GetCameraMoveflag()) return;
        if (s_inputlist.PlayerNum != StageNum) return;
        Vector3 cam = this.gameObject.transform.position;

        if (g_Player.transform.position.x > cam.x + m_Offset)
        {
            cam.x = g_Player.transform.position.x - m_Offset;
            this.gameObject.transform.position = cam;
        }

    }
}
