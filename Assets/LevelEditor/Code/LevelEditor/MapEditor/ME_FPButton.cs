using FNZ.Client.View.Prefab;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using UnityEngine;
using UnityEngine.UI;
namespace FNZ.LevelEditor
{
	public class ME_FPButton : MonoBehaviour
	{

		public ME_Control.SnapMode SnapMode;
		public string fpType;

		public void Start()
		{
			var data = DataBank.Instance.GetData<FNEEntityData>(fpType);
			if (!PrefabBank.DoesPrefabExist(data.entityViewVariations[0]))
			{
				GetComponentInChildren<Text>().color = new Color(0.8f, 0.1f, 0.1f);
				GetComponent<Button>().enabled = false;
				Destroy(this);
			}
		}

		public void Update()
		{
			if (ME_Control.activePaintMode == ME_Control.PaintMode.FLOATING_ENTITY && ME_Control.SelectedFPObject == fpType)
				GetComponent<Image>().color = new Color(0.1f, 0.6f, 0.1f);
			else
				GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f);
		}

		public void OnClick()
		{
			ME_Control.activeSnapMode = SnapMode;
			ME_Control.activePaintMode = ME_Control.PaintMode.FLOATING_ENTITY;
			ME_Control.SelectedFPObject = fpType;
		}
	}
}
