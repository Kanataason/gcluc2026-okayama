using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    [SerializeField] List<AudioClip> l_Cliplist;
    Dictionary<string, AudioClip> d_ClipMap;//名前を入力することですぐに検索できるようにするため
    //SeとBgmは分けて再生するためにソースを別にする
    public AudioSource BgmAudio;
    public AudioSource SeAudio;
    void Start()
    {
        //初期化
        d_ClipMap = new Dictionary<string, AudioClip>();

        foreach (var clip in l_Cliplist)
        {
            d_ClipMap.Add(clip.name, clip);
        }
    }
    public void AllStopAudio()//すべてを止める
    {
        BgmAudio.Stop();
        SeAudio.Stop();
    }
    public void StopSe()//Seだけを止める
    {
        SeAudio.Stop();
    }
    public float GetTime()//遷移時にBｇｍがどこまで進んでいるかを取るための関数
    {
        return BgmAudio.time;
    }
    public void PlayBGMAudio(string clipname,float time)//Clipの名前　どれくらいＢＧｍが進んでいるかをセット
    {
      var clip = GetClip(clipname);//Clip高速検索
        BgmAudio.Stop();

        if (clip == null) return;

        BgmAudio.clip = clip;
        BgmAudio.time = time;
        BgmAudio.Play();
    }
    public void PlaySeAudio(string name)//Clipの名前
    {
        var clip = GetClip(name);//Clip高速検索

        if (clip == null) return;

        SeAudio.PlayOneShot(clip);
    }
    private AudioClip GetClip(string name)
    {
        if(d_ClipMap.TryGetValue(name,out var clip))
        {
            return clip;
        }
        return null;
    }
}
