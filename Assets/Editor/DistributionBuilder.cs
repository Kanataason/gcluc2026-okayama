using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

//配布用のビルドを作る(メニュー「配布 > 配布用ビルドを作成」)
//Windows向けにビルドして、遊び方とライセンスを同梱し、ZIPにまとめる
public static class DistributionBuilder
{
    private const string ProductName = "ボスは二度戦う";//ウィンドウのタイトル
    private const string ExeName = "BossWaNidoTatakau";//exeとフォルダの名前(日本語だと環境によって文字化けするため英字)
    private const string OutputRoot = "Build";//出力先(プロジェクトフォルダ直下、Gitの管理外)
    private const string DocumentFolder = "Distribution";//一緒に配布する文書(README.txt・LICENSE.txt)
    private const string FontLicensePath = "Assets/TaTsuya/Noto_Sans_JP (1)/OFL.txt";//ビルドに含まれるフォントのライセンス

    [MenuItem("配布/配布用ビルドを作成")]
    private static void Build()
    {
        string gameFolder = Path.Combine(OutputRoot, ExeName);
        string zipPath = Path.Combine(OutputRoot, ExeName + ".zip");
        if (Directory.Exists(gameFolder)) Directory.Delete(gameFolder, true);
        if (File.Exists(zipPath)) File.Delete(zipPath);

        PlayerSettings.productName = ProductName;

        var options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
            locationPathName = Path.Combine(gameFolder, ExeName + ".exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        };
        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            EditorUtility.DisplayDialog("配布用ビルド", $"ビルドに失敗しました({report.summary.result})\nConsoleを確認してください", "OK");
            return;
        }

        RemoveDoNotShipFolders(gameFolder);
        CopyDocuments(gameFolder);
        ZipFile.CreateFromDirectory(gameFolder, zipPath, CompressionLevel.Optimal, true);

        EditorUtility.RevealInFinder(zipPath);
        EditorUtility.DisplayDialog("配布用ビルド", $"作成しました\n{Path.GetFullPath(zipPath)}", "OK");
    }
    private static void RemoveDoNotShipFolders(string gameFolder)//配布してはいけないフォルダ(Burstのデバッグ情報など)を消す
    {
        foreach (var folder in Directory.GetDirectories(gameFolder))
        {
            if (folder.Contains("DoNotShip") || folder.Contains("DontShip")) Directory.Delete(folder, true);
        }
    }
    private static void CopyDocuments(string gameFolder)
    {
        if (Directory.Exists(DocumentFolder))
        {
            foreach (var file in Directory.GetFiles(DocumentFolder))
                File.Copy(file, Path.Combine(gameFolder, Path.GetFileName(file)), true);
        }
        if (File.Exists(FontLicensePath))
            File.Copy(FontLicensePath, Path.Combine(gameFolder, "NotoSansJP_OFL.txt"), true);
    }
}
