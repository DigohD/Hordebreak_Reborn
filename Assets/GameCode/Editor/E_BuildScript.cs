using UnityEditor;

namespace FNZ.Editor
{
	public static class E_BuildScript
	{
		private static string GameName = "Hordebreak";
		
		[MenuItem("Build/Release/BuildGame")]
		public static void BuildLocal_Release()
		{
			var locationPathName = $"Builds/Release/Local/{GameName}.exe";
			
			var scenes = new[]
			{
				"Assets/Scenes/GameScenes/StartMenu.unity", 
				"Assets/Scenes/GameScenes/LoadingScreen.unity",
				"Assets/Scenes/GameScenes/Local_HDRP_NightLights.unity"
			};

			BuildPipeline.BuildPlayer(GetReleaseBuildPlayerOptions(
				BuildTarget.StandaloneWindows64,
				false,
				locationPathName,
				scenes)
			);
		}

		[MenuItem("Build/Dev/BuildGame")]
		public static void BuildLocal_Dev()
		{
			var locationPathName = $"Builds/Debug/Local/{GameName}.exe";
			var scenes = new[]
			{
				"Assets/Scenes/GameScenes/StartMenu.unity", 
				"Assets/Scenes/GameScenes/LoadingScreen.unity",
				"Assets/Scenes/GameScenes/Local_HDRP_NightLights.unity",
			};

			BuildPipeline.BuildPlayer(GetDevBuildPlayerOptions(
				BuildTarget.StandaloneWindows64,
				false,
				locationPathName,
				scenes)
			);
		}

		[MenuItem("Build/Release/BuildLevelEditor")]
		public static void BuildLevelEditor_Release()
		{
			var locationPathName = "Builds/Release/LevelEditor/Kryst_LevelEditor.exe";
			var scenes = new[] { "Assets/Scenes/LevelEditorScenes/LevelEditorScene.unity" };

			BuildPipeline.BuildPlayer(GetReleaseBuildPlayerOptions(
				BuildTarget.StandaloneWindows64,
				false,
				locationPathName,
				scenes)
			);
		}

		[MenuItem("Build/Dev/BuildLevelEditor")]
		public static void BuildLevelEditor_Dev()
		{
			var locationPathName = "Builds/Debug/LevelEditor/Kryst_LevelEditor.exe";
			var scenes = new[] { "Assets/Scenes/LevelEditorScenes/LevelEditorScene.unity" };

			BuildPipeline.BuildPlayer(GetDevBuildPlayerOptions(
				BuildTarget.StandaloneWindows64,
				false,
				locationPathName,
				scenes)
			);
		}

		[MenuItem("Build/Release/BuildClientOnly")]
		public static void BuildClient_Release()
		{
			var locationPathName = "Builds/Client/KrystClient.exe";
			var scenes = new[] { "Assets/Scenes/GameScenes/StartMenu.unity", "Assets/Scenes/GameScenes/Client.unity" };

			BuildPipeline.BuildPlayer(GetReleaseBuildPlayerOptions(BuildTarget.StandaloneWindows64, false, locationPathName, scenes));
		}

		[MenuItem("Build/Dev/BuildClientOnly")]
		public static void BuildClient_Dev()
		{
			var locationPathName = "Builds/Client/KrystClient.exe";
			var scenes = new[] { "Assets/Scenes/GameScenes/StartMenu.unity", "Assets/Scenes/GameScenes/Client.unity" };

			BuildPipeline.BuildPlayer(GetDevBuildPlayerOptions(BuildTarget.StandaloneWindows64, false, locationPathName, scenes));
		}

		[MenuItem("Build/Release/BuildServerOnly")]
		public static void BuildServer_Release()
		{
			var locationPathName = "Builds/Server/KrystServer.exe";
			var scenes = new[] { "Assets/Scenes/GameScenes/Server.unity" };

			BuildPipeline.BuildPlayer(GetReleaseBuildPlayerOptions(BuildTarget.StandaloneWindows64, false, locationPathName, scenes));
		}

		[MenuItem("Build/Dev/BuildServerOnly")]
		public static void BuildServer_Dev()
		{
			var locationPathName = "Builds/Server/KrystServer.exe";
			var scenes = new[] { "Assets/Scenes/GameScenes/Server.unity" };

			BuildPipeline.BuildPlayer(GetDevBuildPlayerOptions(BuildTarget.StandaloneWindows64, false, locationPathName, scenes));
		}

		[MenuItem("Build/Release/BuildHeadlessWin64Server")]
		public static void BuildHeadlessWin64Server_Release()
		{
			var locationPathName = "Builds/Release/Server/KrystHeadlessWin64Server.exe";
			var scenes = new[] { "Assets/Scenes/GameScenes/Server.unity" };

			BuildPipeline.BuildPlayer(GetReleaseBuildPlayerOptions(
				BuildTarget.StandaloneWindows64,
				true,
				locationPathName,
				scenes)
			);
		}

		[MenuItem("Build/Dev/BuildHeadlessWin64Server")]
		public static void BuildHeadlessWin64Server_Dev()
		{
			var locationPathName = "Builds/Debug/Server/KrystHeadlessWin64Server.exe";
			var scenes = new[] { "Assets/Scenes/GameScenes/Server.unity" };

			BuildPipeline.BuildPlayer(GetDevBuildPlayerOptions(
				BuildTarget.StandaloneWindows64,
				true,
				locationPathName,
				scenes)
			);
		}

		[MenuItem("Build/Release/BuildHeadlessLinux64Server")]
		public static void BuildHeadlessLinux64Server_Release()
		{
			var locationPathName = "Builds/Server/KrystHeadlessLinux64Server.exe";
			var scenes = new[] { "Assets/Scenes/GameScenes/Server.unity" };

			BuildPipeline.BuildPlayer(GetReleaseBuildPlayerOptions(
				BuildTarget.StandaloneLinux64,
				true,
				locationPathName,
				scenes)
			);
		}

		[MenuItem("Build/Dev/BuildHeadlessLinux64Server")]
		public static void BuildHeadlessLinux64Server_Dev()
		{
			var locationPathName = "Builds/Server/KrystHeadlessLinux64Server.exe";
			var scenes = new[] { "Assets/Scenes/GameScenes/Server.unity" };

			BuildPipeline.BuildPlayer(GetDevBuildPlayerOptions(
				BuildTarget.StandaloneLinux64,
				true,
				locationPathName,
				scenes)
			);
		}

		[MenuItem("Build/Release/BuildAll")]
		public static void BuildAll_Release()
		{
			BuildLevelEditor_Release();

			BuildHeadlessWin64Server_Release();
			BuildHeadlessLinux64Server_Release();
			BuildServer_Release();

			BuildLocal_Release();
			BuildClient_Release();
		}

		[MenuItem("Build/Dev/BuildAll")]
		public static void BuildAll_Dev()
		{
			BuildLevelEditor_Dev();

			BuildHeadlessWin64Server_Dev();
			BuildHeadlessLinux64Server_Dev();
			BuildServer_Dev();

			BuildLocal_Dev();
			BuildClient_Dev();
		}

		private static BuildPlayerOptions GetReleaseBuildPlayerOptions(
			BuildTarget buildTarget,
			bool enableHeadlessMode,
			string buildLocationPath,
			params string[] sceneNames)
		{
			return new BuildPlayerOptions
			{
				scenes = sceneNames,
				locationPathName = buildLocationPath,
				target = buildTarget,
				options = (enableHeadlessMode ? BuildOptions.EnableHeadlessMode : BuildOptions.None)
			};
		}

		private static BuildPlayerOptions GetDevBuildPlayerOptions(
			BuildTarget buildTarget,
			bool enableHeadlessMode,
			string buildLocationPath,
			params string[] sceneNames)
		{
			return new BuildPlayerOptions
			{
				scenes = sceneNames,
				locationPathName = buildLocationPath,
				target = buildTarget,
				options = (enableHeadlessMode ? BuildOptions.EnableHeadlessMode : BuildOptions.None)
					| BuildOptions.Development
					| BuildOptions.ConnectWithProfiler
					| BuildOptions.AllowDebugging,
			};
		}
	}
}



