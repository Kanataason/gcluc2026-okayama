using UnityEngine;
public static class NextFrame
{
    private static readonly WaitForSeconds c_waitForSeconds = new WaitForSeconds(0.1f);
    public static void OneFrame(MonoBehaviour owner, System.Action action)//ñºëOÇÕOneFrameÇæÇ™é¿ç€ÇÕ0.1ïbë“Ç¬
    {
        owner.StartCoroutine(RunOneFrame(action));
    }
    public static void Run(MonoBehaviour owner, float Timer, System.Action action)
    {
        owner.StartCoroutine(RunCoroutine(action, Timer));
    }
    private static System.Collections.IEnumerator RunOneFrame(System.Action action)
    {
        yield return c_waitForSeconds;
        action?.Invoke();
    }
    private static System.Collections.IEnumerator RunCoroutine(System.Action action, float Timer)
    {
        yield return new WaitForSeconds(Timer);
        action?.Invoke();
    }
}
