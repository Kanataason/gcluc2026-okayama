using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public enum CharaState //キャラの種類
{
    Player,
    Boss,
    Other
}
public class ObjctPool : MonoBehaviour
{
    public enum EfectType//エフェクトの種類
    {
        Die,
        Slash,
        Hit,
        Shot
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
                var obj = Instantiate(list.g_Prefab, transform);//自分の子で設定
                obj.SetActive(false);
                d_EfectList[(list.e_CharaState,list.e_EfectType)].Enqueue(obj);
            }
        }
    }

    public GameObject GetObject(CharaState CType,EfectType EType)//objゲットする関数
    {
        if(!d_EfectList.ContainsKey((CType, EType)))
        {
            Debug.Log($"この{CType}{EType}は登録されてません");
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
            var obj = Instantiate(d_PrefabList[(CType, EType)], transform);
            obj.SetActive(true);
            return obj;
        }
    }
    public void ReturnObject(EfectType Etype, CharaState Ctype, GameObject obj)//obj返すときの関数
    {
        if (obj == null) return;

        obj.SetActive(false);
        d_EfectList[(Ctype, Etype)].Enqueue(obj);
    }
}
