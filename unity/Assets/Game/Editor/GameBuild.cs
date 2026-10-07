using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Game.EditorTools
{
    public static class GameBuild
    {
        [MenuItem("Game/Build Windows player")]
        public static void BuildWindows()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");

            var repository = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            var executable = Path.Combine(repository, "builds", "game-arena", "Arena.exe");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { GameArenaBuilder.ScenePath },
                locationPathName = executable,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });

            var summary = report.summary;
            Debug.Log("GAME_BUILD result=" + summary.result + " errors=" + summary.totalErrors + " warnings=" + summary.totalWarnings +
                " bytes=" + summary.totalSize + " seconds=" + summary.totalTime.TotalSeconds.ToString("F0") + " path=" + executable);
            if (summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Windows build failed: " + summary.result);
        }
    }
}
