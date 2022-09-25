using FNZ.Client.View.Player.PlayerStitching;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EquipmentSystem;
using FNZ.Shared.Model.Entity.Components.Player;
using FNZ.Shared.Model.Items;
using UnityEngine;

namespace FNZ.Client.View.UI.StartMenu
{
	public class StartMenuPlayerStitcher : MonoBehaviour
	{
		void Start()
		{
			var stitcher = GetComponent<PlayerMeshStitcher>();
			var playerData = DataBank.Instance.GetData<FNEEntityData>("player");
			stitcher.SetDefaultMeshes(playerData.GetComponentData<PlayerComponentData>().viewVariations, 0);
			stitcher.UpdateItemSlot(Item.GenerateItem("armor_hi_tech_chest"), Slot.Torso);
			stitcher.UpdateItemSlot(Item.GenerateItem("armor_hi_tech_pants"), Slot.Legs);
			stitcher.UpdateItemSlot(Item.GenerateItem("armor_hi_tech_boots"), Slot.Feet);
			stitcher.UpdateItemSlot(Item.GenerateItem("armor_hi_tech_gloves"), Slot.Hands);

			stitcher.UpdateItemSlot(Item.GenerateItem("weapon_energy_lancer"), Slot.Weapon1);

		}
	}
}