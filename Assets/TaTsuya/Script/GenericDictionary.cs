using System;
using System.Collections.Generic;

[Serializable]
public class GenericDictionary <T1,T2>
{
    [Serializable]
    public class Entry
    {
        public T1 Key;
        public T2 Value;
    }
    //任意のリスト
    public List<Entry> l_Entry;

    //任意のKey Valueを入れるディクショナリー
    private Dictionary<T1, T2> d_Dictionary = new();

    public void Init()
    {
        d_Dictionary.Clear();
        foreach(var list in l_Entry)
        {
            d_Dictionary[list.Key] = list.Value;
        }
    }

    public T2 Get(T1 state)//対応したKeyを入れる
    {
        if(d_Dictionary.TryGetValue(state,out var obj))
        {
            return obj;
        }
        return default;
    }
    public void Clear()//デストロイするときにやる
    {
        d_Dictionary.Clear();
        l_Entry.Clear();
    }
}
