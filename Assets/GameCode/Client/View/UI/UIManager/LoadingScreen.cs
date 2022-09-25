using FNZ.Client.Utils;
using FNZ.Client.View.Audio;
using FNZ.Client.View.UI.Sprites;
using FNZ.Client.View.World;
using FNZ.Shared.Model;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Manager
{
	public class LoadingScreen : MonoBehaviour
	{
		private Image m_Img;

		[SerializeField]
		private Text m_Text = null;

		private void Start()
		{
			m_Img = GetComponent<Image>();
			m_Text.text = "Initializing...";
			//Start loading after 1 second
			Invoke("Init", 1);
		}

		private void Init()
		{
			StartCoroutine(ProgressLoader());
		}

		private IEnumerator ProgressLoader()
		{
			m_Text.text = "Loading XML...";
			var db = DataBank.Instance;
			yield return new WaitForEndOfFrame();

			m_Text.text = "Initializing XML Id translator...";
			m_Img.fillAmount = 0.2f;
			yield return new WaitForEndOfFrame();
			IdTranslator.Instance.Init();

			m_Text.text = "Validating XML-file data entries...";
			m_Img.fillAmount = 0.4f;
			yield return new WaitForEndOfFrame();
			DataBank.Instance.ValidateDataBank();

			m_Text.text = "Generating Equipment Sprites...";
			m_Img.fillAmount = 0.5f;
			GameObject.FindObjectOfType<ItemPhotoBooth>().RenderAllGear();
			yield return new WaitUntil(GameObject.FindObjectOfType<ItemPhotoBooth>().IsDone);

			m_Text.text = "Building Sprite Sheets...";
			m_Img.fillAmount = 0.6f;
			yield return new WaitForEndOfFrame();
			var sb = SpriteBank.GetSprite("default");

			m_Text.text = "Packing terrain tile sheets...";
			m_Img.fillAmount = 0.75f;
			yield return new WaitForEndOfFrame();
			var td = ClientTileSheetPacker.s_AllTilesData;

			m_Text.text = "Initializing client view utilities...";
			m_Img.fillAmount = 0.9f;
			yield return new WaitForEndOfFrame();
			var cm = ViewUtils.s_ChunkMaterials;

			m_Text.text = "Changing music to something a little more kickass...";
			m_Img.fillAmount = 0.95f;
			AudioManager.Instance.PlayMusic(SoundBank.STRING_RANDOM, 2);
			AudioManager.Instance.PlayAmbience("day", 6);
			yield return new WaitForSeconds(2);

			m_Text.text = "There we go!";
			yield return new WaitForSeconds(1);

			m_Img.fillAmount = 1;
			yield return new WaitForSeconds(0.25f);
			SceneManager.LoadSceneAsync("Local_HDRP_NightLights");
		}
	}
}