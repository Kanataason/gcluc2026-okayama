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
    [SerializeField] List<AudioClip> l_clips;
    Dictionary<string, AudioClip> clipMap;
    public AudioSource BGMaudio;
    public AudioSource Seaudio;
    void Start()
    {
        clipMap = new Dictionary<string, AudioClip>();

        foreach (var clip in l_clips)
        {
            clipMap.Add(clip.name, clip);
        }
    }
    public void StopBGM()
    {
        BGMaudio.Stop();
        Seaudio.Stop();
    }
    public void StopSe()
    {
        Seaudio.Stop();
    }
    public float GetTime()
    {
        return BGMaudio.time;
    }
    public void PlayBGMAudio(string clipname,float time)
    {
      var clip = GetClip(clipname);
        BGMaudio.Stop();
        if (clip == null) return;
        BGMaudio.clip = clip;
        BGMaudio.time = time;
        BGMaudio.Play();
    }
    public void PlaySeAudio(string name)
    {
        var clip = GetClip(name);
        if (clip == null) return;
        Seaudio.PlayOneShot(clip);
    }
    private AudioClip GetClip(string name)
    {
        if(clipMap.TryGetValue(name,out var clip))
        {
            return clip;
        }
        return null;
    }
}
