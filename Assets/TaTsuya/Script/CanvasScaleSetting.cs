using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//どのPCでもUIの見た目(大きさ・位置)を同じにする
//シーンを読み込むたびに、全部のCanvasを「1920x1080を基準に画面サイズへ合わせる」設定にする
public static class CanvasScaleSetting
{
    private static readonly Vector2 ReferenceResolution = new Vector2(1920, 1080);//UIを作ったときの画面サイズ
    private const float MatchWidthOrHeight = 0.5f;//縦横比が違う画面のとき 0:横幅に合わせる 1:高さに合わせる

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded += (scene, mode) => Apply();
    }
    public static void Apply()
    {
        foreach (var scaler in Object.FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var canvas = scaler.GetComponent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace) continue;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = MatchWidthOrHeight;
        }
    }
}
