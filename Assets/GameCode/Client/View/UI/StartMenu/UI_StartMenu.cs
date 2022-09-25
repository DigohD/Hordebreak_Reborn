using FNZ.Client.StaticData;
using FNZ.Client.View.Audio;
using FNZ.Client.View.UI.Utils;
using FNZ.Shared.Utils;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FNZ.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static FNZ.Shared.Utils.Localization;

namespace FNZ.Client.View.UI.StartMenu
{
    public class UI_StartMenu : MonoBehaviour
    {
        private string playerName = "";
        private string worldName = "";
        private string ip;
        private int port;

        public GameObject ipObj;
        public GameObject portObj;
        public GameObject OptionsMenu;

        public CanvasScaler CanvasScaler;

        public Image IMG_Fade;

        private AudioManager m_AudioManager;
        private bool startGame;

        public InputField INPUT_HostName, INPUT_JoinName, INPUT_WorldName;

        public void Awake()
        {
            ClientSettings.LoadSettings();
            QualitySettings.SetQualityLevel(3);
        }

        public void Start()
        {
            //var ipInput = ipObj.GetComponent<InputField>();
            //ip = ipInput.text;
            //var portInput = portObj.GetComponent<InputField>();
            //port = Int32.Parse(portInput.text);
            m_AudioManager = FindObjectOfType<AudioManager>();

            UI_Options UIOptionsMenu = OptionsMenu.GetComponent<UI_Options>();

            if (CanvasScaler != null && UIOptionsMenu != null)
            {
                UIOptionsMenu.SetUIScaleWidthRef(CanvasScaler.referenceResolution.x);
                UIOptionsMenu.SetUIScaleHeightRef(CanvasScaler.referenceResolution.y);

                CanvasScaler.referenceResolution = new Vector2(
                    (UIOptionsMenu.GetUIScaleWidthRef() / UI_Options.GetUIScale()),
                    (UIOptionsMenu.GetUIScaleHeightRef() / UI_Options.GetUIScale())
                );

                UIOptionsMenu.UpdateUIScaleSliderValue();
            }
        }


        void Update()
        {
            if (startGame)
            {
                IMG_Fade.color = new Color(
                    IMG_Fade.color.r,
                    IMG_Fade.color.g,
                    IMG_Fade.color.b,
                    Mathf.Lerp(IMG_Fade.color.a, 1.1f, 1f * Time.deltaTime)
                );
            }
        }

        public void OnContinueButtonClick()
        {
            if (!Directory.Exists("Saves")) return;
            //var worldDirs = Directory.GetDirectories("Saves");

            var dirInfo = new DirectoryInfo("Saves");
            var latestWorld = dirInfo
                .GetDirectories()
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefault();

            if (latestWorld == null) return;
            
            SharedConfigs.WorldName = latestWorld.Name;
            SharedConfigs.IsNewGame = false;
            worldName = dirInfo.Name;
            startGame = true;
            Invoke("LocalScene", 3);
        }

        public void OnHostButtonClick()
        {
            // string worldName;
            // // ett litet fulhaxx tills vi får vettigt UI till detta
            // for (int i = 1; ; i++)
            // {
            // 	if (!Directory.Exists("Saves\\World" + i))
            // 	{
            // 		worldName = "World" + i;
            // 		break;
            // 	}
            // }

            if (Directory.Exists("Saves"))
            {
            	var di = new DirectoryInfo("Saves");
            
            	foreach (var file in di.GetFiles())
            	{
            		file.Delete();
            	}
                
            	foreach (var dir in di.GetDirectories())
            	{
            		dir.Delete(true);
            	}
            }
            
            if (string.IsNullOrEmpty(worldName))
            {
                worldName = "NewWorld";
            }

            SharedConfigs.IsNewGame = true;
            SharedConfigs.WorldName = worldName;

            startGame = true;
            Invoke("LocalScene", 3);
        }

        private void LocalScene()
        {
            NetData.NET_MODE = NetMode.HOST;
            NetData.IP_ADRESS = "127.0.0.1";
            NetData.PORT = 7676;
            NetData.LOCAL_PLAYER_NAME = playerName.Length > 0 ? playerName : "Host";

            SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Single);
        }

        public void OnJoinButtonClick()
        {
            startGame = true;
            Invoke("ClientScene", 3);
        }

        public void OnQuitGameClick()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
        }

        private void ClientScene()
        {
            NetData.NET_MODE = NetMode.JOIN;
            NetData.IP_ADRESS = ip;
            NetData.PORT = port;
            NetData.LOCAL_PLAYER_NAME = playerName.Length > 0 ? playerName : "Client";

            SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Single);
        }

        public void OnIpChange(string newIp)
        {
            // UiSystem.PlayUISound(AudioBank.Clips[AudioBank.AudioClipID.UI_BUTTON_CLICK]);

            this.ip = newIp;
        }

        public void OnPortChange(string newPort)
        {
            // UiSystem.PlayUISound(AudioBank.Clips[AudioBank.AudioClipID.UI_BUTTON_CLICK]);

            this.port = int.Parse(newPort);
        }

        public void OnNameChange(string newName)
        {
            if (newName.Length < 16 && Regex.Match(newName, "^[a-zA-Z0-9_]*$").Success)
                playerName = newName;

            INPUT_JoinName.text = playerName;
            INPUT_HostName.text = playerName;
        }

        public void OnWorldNameChange(string newWorldName)
        {
            if (newWorldName.Length < 16 && Regex.Match(newWorldName, "^[a-zA-Z0-9_]*$").Success)
            {
                worldName = newWorldName;
                SharedConfigs.WorldName = worldName;
            }

            INPUT_WorldName.text = worldName;
        }

        public void OnLanguageClick(string lan)
        {
            switch (lan)
            {
                case "en":
                    Localization.SetActiveLanguage(Language.ENGLISH);
                    break;

                case "sv":
                    Localization.SetActiveLanguage(Language.SWEDISH);
                    break;
            }

            ClientSettings.SaveSettings();

            SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
        }
    }
}