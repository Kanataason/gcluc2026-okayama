using UnityEngine;
using System.Collections.Generic;
public class SetChracterInfoManager : MonoBehaviour
{
   public List<StageInfo> l_InfoList;
    public int StageNum;
    private void Start()
    {
        BattleManager.Instance.OnCreateCharacters += OnInit;
    }
    private void OnDisable()
    {
        BattleManager.Instance.OnCreateCharacters -= OnInit;
    }
    private  void OnInit(GameObject boss,InputInfo list)
    {
        if (StageNum != list.PlayerNum) return;
        int index = StageNum -1;
        NextFrame.Run(this,1, () => SetPosition(l_InfoList[index], boss, list));
    }
    private void SetPosition(StageInfo list,GameObject Boss,InputInfo infolist)
    {
        Boss.transform.position = list.BossPos;
        infolist.TargetObj.transform.position = list.PlayerPos;
        Debug.Log(infolist.TargetObj.transform.position);
    }
    [System.Serializable]
    public struct StageInfo
    {
        public Vector3 BossPos;
        public Vector3 PlayerPos;
    }
}
