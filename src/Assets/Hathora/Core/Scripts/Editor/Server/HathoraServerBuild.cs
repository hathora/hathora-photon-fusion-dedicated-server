// Created by dylan@hathora.dev

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hathora.Core.Scripts.Editor.Common;
using Hathora.Core.Scripts.Runtime.Common.Utils;
using Hathora.Core.Scripts.Runtime.Server;
using Hathora.Core.Scripts.Runtime.Server.Models;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Hathora.Core.Scripts.Editor.Server
{
    /// <summary>
    /// Contains build + deploy methods for Hathora Server.
    /// Trigger these from HathoraServerConfig ScriptableObject buttons.
    ///
    /// (!) bug: Scenes to build includes ALL scenes in the build settings, even if unchecked.
    ///          There's currently no [native] way to get only the checked scenes.
    /// </summary>
    public static class HathoraServerBuild
    {
        /// <summary>
        /// This needs to be a massive timeout range since some builds will be huge. 
        /// However, we still do need *a* timeout to prevent mem leaks from Unity weirdness.
        /// </summary>
        public const int DEPLOY_TIMEOUT_MINS = 60 * 3; // 3 hrs 
        
        /// <summary>
        /// Builds with HathoraServerConfig opts.
        /// </summary>
        /// <param name="_serverConfig">Find via menu `Hathora/Find Server Config(s)`</param>
        /// <param name="_cancelToken">This won't cancel the build itself, but things around it.</param>
        /// <returns>isSuccess</returns>
        public static async Task<BuildReport> BuildHathoraLinuxServer(
            HathoraServerConfig _serverConfig,
            CancellationToken _cancelToken = default)
        {
            string logPrefix = $"[{nameof(HathoraServerBuild)}.{nameof(BuildHathoraLinuxServer)}]";

            // Wipe the Deploy logs for the session to prevent confusion
            _serverConfig.HathoraDeployOpts.LastDeployLogsStrb.Clear();
            
            // Throughout this process, we'll lose focus on the config object.
            UnityEngine.Object previousSelection = Selection.activeObject; // Preserve focus - restore at end

            HathoraAutoBuildOpts buildOpts = _serverConfig.LinuxHathoraAutoBuildOpts; // We'll use this a lot
            
            // Prep logs cache
            buildOpts.LastBuildReport = null;
            StringBuilder strb = buildOpts.LastBuildLogsStrb;
            strb.Clear()
                .AppendLine(HathoraUtils.GetFriendlyDateTimeShortStr(DateTime.Now))
                .AppendLine("Preparing local server build...")
                .AppendLine($"overwriteExistingDockerfile? {buildOpts.OverwriteDockerfile}")
                .AppendLine();
            
            // Set your build options
            HathoraServerPaths configPaths = new(_serverConfig);

            // Create the build directory if it does not exist
            strb.AppendLine($"Cleaning/creating build dir @ path: `{configPaths.PathToBuildDir}` ...")
                .AppendLine();
            
            cleanCreateBuildDir(_serverConfig, configPaths.PathToBuildDir);
            _cancelToken.ThrowIfCancellationRequested();
            
            #region bug (when restored later): Recompiles - you lose all logs [including HathoraServerConfig logs]
            // // ----------------
            // // This will change your selected build setting: Cache here, revert later
            // BuildTarget originalBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            // BuildTargetGroup originalBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(originalBuildTarget);
            // int originalArchitecture = PlayerSettings.GetArchitecture(originalBuildTargetGroup);
            // ScriptingImplementation originalScriptingBackend = PlayerSettings.GetScriptingBackend(originalBuildTargetGroup);
            // ApiCompatibilityLevel originalApiCompatibility = PlayerSettings.GetApiCompatibilityLevel(originalBuildTargetGroup);
            //
            // // Sanity check: Does the 1st scene match the current scene we're on?
            // // Many devs intend to build on the scene they're working on 1st, but forget to swap the order
            // EditorBuildSettingsScene firstSceneInBuildSettings = EditorBuildSettings.scenes[0]; 
            // if (firstSceneInBuildSettings.path != SceneManager.GetActiveScene().path)
            // {
            //     Debug.Log($"{logPrefix} <color=orange>(!)</color> The 1st scene in build " +
            //         $"settings ({firstSceneInBuildSettings.path}) !matches the current Editor " +
            //         $"scene ({SceneManager.GetActiveScene().path}); intended?");
            // }
            #endregion // bug (when restored later): Recompiles - you lose all logs [including HathoraServerConfig logs]

            // ----------------
            // Generate build opts
            BuildPlayerOptions buildPlayerOptions = generateBuildPlayerOptions(
                _serverConfig,
                configPaths.PathToBuildExe);

            // ----------------
            // Build the server
            strb.AppendLine("BUILDING now (this may take a while), with opts:")
                .AppendLine("```")
                .AppendLine(getBuildOptsStr(buildPlayerOptions))
                .AppendLine("```")
                .AppendLine();
            
            // ----------------
            // Generate the Dockerfile to `.hathora/`: Paths will be different for each collaborator
            // Generate before the build in case we want to  *just* generate Dockerfile then cancel (generates fast).
            bool existingDockerfileExists = CheckIfDockerfileExists(configPaths);
            bool genDockerfile = buildOpts.OverwriteDockerfile || !existingDockerfileExists;
            if (genDockerfile)
            {
                strb.AppendLine($"Generating Dockerfile to `{configPaths.PathToDotHathoraDockerfile}` ...");
                Debug.Log($"{logPrefix} Generating new Dockerfile (if exists: overwriting)...");
                
                string dockerFileContent = HathoraDocker.GenerateDockerFileStr(configPaths);

                strb.AppendLine("```")
                    .AppendLine(dockerFileContent)
                    .AppendLine("```")
                    .AppendLine();

                await HathoraDocker.WriteDockerFileAsync(
                    configPaths.PathToDotHathoraDockerfile,
                    dockerFileContent,
                    _cancelToken);    
            }
            else if (!buildOpts.OverwriteDockerfile)
            {
                Debug.LogWarning($"{logPrefix} !buildOpts.OverwriteDockerfile: Leaving Dockerfile" +
                    "alone (to !overwrite customizations) at risk of desync, if any ServerConfig opts have changed.");
            }
            
            Debug.Log("BUILDING now (this may take a while): See HathoraServerConfig " +
                "'Generate Server Build Logs'"); // To regular console
            await Task.Delay(100, _cancelToken); // Give the logs a chance to update

            BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
            _cancelToken.ThrowIfCancellationRequested();
            
            // Did we fail? 
            string resultStr = Enum.GetName(typeof(BuildResult), buildReport.summary.result);
            if (buildReport.summary.result != BuildResult.Succeeded)
            {
                strb.AppendLine($"**BUILD FAILED: {resultStr}**");
                
                Selection.activeObject = previousSelection; // Restore focus
                return buildReport; // fail
            }
            
            #region bug: Recompiles - you lose all logs [including HathoraServerConfig logs]
            // // ----------------
            // // Delay since we don't want the rest of the block to cutoff (This causes a recompile)
            // EditorApplication.delayCall += () =>
            // {
            //     // Revert build settings since we changed them to headless Linux server
            //     EditorUserBuildSettings.SwitchActiveBuildTarget(originalBuildTargetGroup, originalBuildTarget);
            //     PlayerSettings.SetArchitecture(originalBuildTargetGroup, originalArchitecture);
            //     PlayerSettings.SetScriptingBackend(originalBuildTargetGroup, originalScriptingBackend);
            //     PlayerSettings.SetApiCompatibilityLevel(originalBuildTargetGroup, originalApiCompatibility);
            //
            //     Debug.Log(
            //         $"{logPrefix} Reverted build settings to original: " +
            //         $"[BuildTarget: {originalBuildTarget}, " +
            //         $"BuildTargetGroup: {originalBuildTargetGroup}, " +
            //         $"Architecture: {originalArchitecture}, " +
            //         $"ScriptingBackend: {originalScriptingBackend}, " +
            //         $"ApiCompatibility: {originalApiCompatibility}");
            // };
            #endregion // bug: Recompiles - you lose all logs [including HathoraServerConfig logs]
            
            strb.AppendLine($"**BUILD SUCCESS: {resultStr}**");

            // ----------------
            // Open the build directory - this will lose focus of the inspector
            // TODO: Play a small, subtle chime sfx?
            strb.AppendLine("Opening build dir ...").AppendLine();
            Debug.Log($"{logPrefix} Build succeeded @ path: `{configPaths.PathToBuildDir}`");
            
            EditorUtility.RevealInFinder(configPaths.PathToBuildExe);
            cacheFinishedBuildReportLogs(_serverConfig, buildReport);

            // ----------------
            // Restore focus and return the build report
            Selection.activeObject = previousSelection;
            
            return buildReport;
        }

        /// <summary>
        /// (!) bug: Scenes to build includes ALL scenes in the build settings, even if unchecked.
        ///          There's currently no [native] way to get only the checked scenes.
        /// </summary>
        /// <param name="_buildOpts"></param>
        /// <returns></returns>
        private static string getBuildOptsStr(BuildPlayerOptions _buildOpts)
        {
            string scenesToBuildStr = string.Join("`, `", _buildOpts.scenes); 
            return $"scenes: `{scenesToBuildStr}`\n\n" +
                $"locationPathName: `{_buildOpts.locationPathName}`\n" +
                $"target: `{_buildOpts.target}`\n" +
                $"options: `{_buildOpts.options}`\n" +
                $"standaloneBuildSubtarget `{_buildOpts.subtarget}`";
        }

        private static void cacheFinishedBuildReportLogs(
            HathoraServerConfig _serverConfig, 
            BuildReport _buildReport)
        {
            _serverConfig.LinuxHathoraAutoBuildOpts.LastBuildReport = _buildReport;
            
            TimeSpan totalTime = _buildReport.summary.totalTime;
            _serverConfig.LinuxHathoraAutoBuildOpts.LastBuildLogsStrb
                .AppendLine($"result: {Enum.GetName(typeof(BuildResult), _buildReport.summary.result)}")
                .AppendLine($"totalSize: {_buildReport.summary.totalSize / (1024 * 1024)}MB")
                .AppendLine($"totalWarnings: {_buildReport.summary.totalWarnings.ToString()}")
                .AppendLine($"totalErrors: {_buildReport.summary.totalErrors.ToString()}")
                .AppendLine()
                .Append($"{HathoraEditorUtils.StartGreenColor}Completed</color> ")
                .Append(HathoraUtils.GetFriendlyDateTimeShortStr(DateTime.Now)) // "{date} {time}"
                .Append(" (in ")
                .Append(HathoraUtils.GetFriendlyDateTimeDiff(totalTime, _exclude0: true)) // "{hh}h:{mm}m:{ss}s"; strips 0
                .AppendLine(")")
                .AppendLine("BUILD DONE");
            // #########################################################
            // {result list}
            //
            // {green}Completed{/green} {date} {time} (in {hh}h:{mm}m:{ss}s)
            // BUILD DONE
            // #########################################################
        }

        /// <summary></summary>
        /// <param name="_paths">Create this from a ServerConfig</param>
        public static bool CheckIfDockerfileExists(HathoraServerPaths _paths) => 
            File.Exists(_paths.PathToDotHathoraDockerfile);

        private static BuildPlayerOptions generateBuildPlayerOptions(
            HathoraServerConfig _serverConfig, 
            string _serverBuildExeFullPath)
        {
            EditorBuildSettingsScene[] scenesInBuildSettings = EditorBuildSettings.scenes; // From build settings
            string[] scenePaths = scenesInBuildSettings?.Select(scene => scene.path).ToArray();
            
            BuildPlayerOptions buildPlayerOpts = new()
            {
                scenes = scenePaths,
                locationPathName = _serverBuildExeFullPath,
                target = BuildTarget.StandaloneLinux64,
                options = _serverConfig.LinuxHathoraAutoBuildOpts.IsDevBuild 
                    ? BuildOptions.Development 
                    : BuildOptions.None,
            };
            
            // Ensure build is a headless Linux server (formerly set via `options = BuildOptions.EnableHeadlessMode`)
            EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;

            return buildPlayerOpts;
        }

        private static void cleanCreateBuildDir(
            HathoraServerConfig _serverConfig,
            string _serverBuildDirPath)
        {
            bool targetBuildDirExists = Directory.Exists(_serverBuildDirPath);

            if (_serverConfig.LinuxHathoraAutoBuildOpts.CleanBuildDir && targetBuildDirExists)
            {
                Debug.Log("[HathoraServerBuild] Found old build dir && " +
                    "_serverConfig.LinuxHathoraAutoBuildOpts.CleanBuildDir: Deleting...");
                Directory.Delete(_serverBuildDirPath, recursive: true);
            }
                
            if (!targetBuildDirExists)
                Directory.CreateDirectory(_serverBuildDirPath);
        }
    }
}
