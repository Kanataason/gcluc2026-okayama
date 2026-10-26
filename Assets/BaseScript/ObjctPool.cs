using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public enum CharaState //キャラの種類
{
    Player = 1,
    Boss = 2,
    Other = 3
}
public class ObjctPool : MonoBehaviour
{
    public enum EfectType//エフェクトの種類
    {
        Die = 1,
        Slash = 2,
        Hit = 3,
        Shot = 4,
        Fire = 5
    }

    [System.Serializable]
    public class EfectData//エフェクトのデータ　インスペクターで出したいエフェクトを追加する
    {
        public CharaState e_CharaState;
        public EfectType e_EfectType;
        public GameObject g_Prefab;
        public int m_InstantiateNumber;
    }
    public List<EfectData> l_DataList = new();

    public Dictionary<(CharaState,EfectType), Queue<GameObject>> d_EfectList = new();//現在所有している数のlist
    public Dictionary<(CharaState,EfectType), GameObject> d_PrefabList = new();//設計図リスト
    void Start()
    {
        Init();
    }
    private void OnDisable()
    {
        d_EfectList.Clear();
        d_PrefabList.Clear();
        l_DataList.Clear();
        Destroy(this.gameObject);
    }
    void Init()
    {
        foreach (var list in l_DataList)//キャラ、オブジェクトごとに分けて作る
        {
            d_PrefabList[(list.e_CharaState,list.e_EfectType)] = list.g_Prefab;
            d_EfectList[(list.e_CharaState, list.e_EfectType)] = new Queue<GameObject>();

            for(int i =0; i < list.m_InstantiateNumber; i++)
            {
                var obj = CreateObject(list.g_Prefab);
                obj.SetActive(false);
                d_EfectList[(list.e_CharaState,list.e_EfectType)].Enqueue(obj);
            }
        }
    }

    public GameObject GetObject(CharaState CType,EfectType EType)//objゲットする関数
    {
        if(!d_EfectList.ContainsKey((CType, EType)))
        {
            Debug.LogWarning($"この{CType}{EType}は登録されてません");
            return null;
        }
        Queue<GameObject> list = d_EfectList[(CType, EType)];

        if(list.Count > 0)
        {
            var obj = list.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            var obj = CreateObject(d_PrefabList[(CType, EType)]);
            obj.SetActive(true);
            return obj;
        }
    }
    private GameObject CreateObject(GameObject prefab)//自分の子で生成して、2画面用に自分と同じレイヤーにする
    {
        var obj = Instantiate(prefab, transform);
        foreach (var child in obj.GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = gameObject.layer;
        return obj;
    }
    public void ReturnObject(CharaState Ctype, EfectType Etype, GameObject obj)//obj返すときの関数
    {
        if (obj == null) return;

        obj.SetActive(false);
        d_EfectList[(Ctype, Etype)].Enqueue(obj);
    }
}
