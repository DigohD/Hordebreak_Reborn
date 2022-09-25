using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Effect.RealEffect;
using FNZ.Shared.Model.Entity.Components.Health;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Utils;
using System;
using UnityEngine;

namespace FNZ.Client.View.UI.HoverBox
{
	public partial class HoverBoxGen
	{
		public void CreateItemHoverBox(Item item)
		{
			CreateItemHoverBox(item.Data, item.amount);
		}

		public void CreateItemHoverBox(string itemRef)
		{
			CreateItemHoverBox(DataBank.Instance.GetData<ItemData>(itemRef), 0);
		}

		private void CreateItemHoverBox(ItemData data, int amount)
		{
			HB_Main hb = HB_Factory.CreateNewHoverBox(m_Canvas, data.Id);
			if (hb == null)
				return;

			if (data.maxStackSize == 1 || amount <= 1)
			{
				hb.AddTextItem(
					HB_Text.TextStyle.HEADER,
					new Color32(170, 170, 170, 255),
					Localization.GetString(data.nameRef)
				);
			}
			else
			{
				hb.AddTextItem(
					HB_Text.TextStyle.HEADER,
					new Color32(170, 170, 170, 255),
					"<color=yellow>" + amount + "x</color> " + Localization.GetString(data.nameRef)
				);
			}

			if (data.components.Count > 0)
			{
				hb.AddDividerLine(Color.magenta);

				foreach (var ic in data.components)
				{
					AddItemComponentToHoverBox(ic, hb);
				}
			}

			hb.AddDividerLine(
				Color.gray
			);

			hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, new Color32(170, 170, 170, 255), Localization.GetString(data.infoRef));

			hb.FinishConstruction();
		}

		private void AddItemComponentToHoverBox(ItemComponentData ic, HB_Main hb)
		{

			switch (ic)
			{
				case ItemWeaponComponentData weaponComp:
					WeaponComponent(weaponComp, hb);
					break;

				case ItemClothingComponentData clothingComp:
					ClothingComponent(clothingComp, hb);
					break;

				case ItemFuelComponentData fuelComp:
					FuelComponent(fuelComp, hb);
					break;

                case ItemBurnableComponentData burnableComp:
                    BurnableComponent(burnableComp, hb);
                    break;

				case ItemConsumableComponentData consumableComp:
					ConsumableComponent(consumableComp, hb);
					break;

				default:
					break;
			}
		}

		private void ClothingComponent(ItemClothingComponentData icc, HB_Main hb)
		{
			foreach (var entry in icc.statMods)
			{
				switch (entry.modType.ToString())
				{
					case StatModData.MAX_HEALTH:
						hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("max_health_label") + ": " + entry.amount);
						break;
					case StatModData.ARMOR:
						hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("armor_label") + ": " + entry.amount);
						break;
					case StatModData.MAX_SHIELDS:
						hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("max_shield_label") + ": " + entry.amount);
						break;
					default:
						break;
				}
			}
		}

		private void WeaponComponent(ItemWeaponComponentData iwc, HB_Main hb)
		{
			hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("ammo_in_clip_label") + ": " + iwc.ammoClipSize);

			if (iwc.isAutomatic)
				hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("is_automatic_label") + ": " + Localization.GetString("yes_label"));
			else
				hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("is_automatic_label") + ": " + Localization.GetString("no_label"));

			hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("reload_time_label") + ": " + iwc.reloadTimeInSeconds);
			hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("sound_range_label") + ": " + iwc.soundRange);
			hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("triggers_per_minute_label") + ": " + iwc.TriggersPerMinute);

			//This is here mainly for the 'else' part which should only happen on user error.
			if (iwc.effectRef != string.Empty)
			{
				var effectData = DataBank.Instance.GetData<EffectData>(iwc.effectRef);

				if (effectData.HasRealEffect())
				{
					//does it have a projectile? Add it's info to tooltip
					if (effectData.GetRealEffectDataType() == typeof(ProjectileEffectData))
					{
						var projectileData = (ProjectileEffectData)effectData.RealEffectData;

						if (projectileData.damage != 0)
						{
							if (projectileData.pellets > 1)
								hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("projectile_dmg_per_pellet_label") + ": " + projectileData.damage);
							else
								hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("projectile_dmg_label") + ": " + projectileData.damage);
						}

						if (projectileData.pellets > 1)
							hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("projectile_pellets_label") + ": " + projectileData.pellets);

						if (projectileData.inaccuracy != 0)
							hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("projectile_inaccuracy_label") + ": " + projectileData.inaccuracy);

						if (projectileData.lifetime != 0)
							hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("range_label") + ": " + Math.Round(projectileData.lifetime * projectileData.speed, 1));

						if (projectileData.damageTypeRef != string.Empty)
							hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("projectile_dmgtype_label") + ": " + projectileData.damageTypeRef);

						if (projectileData.healing != 0)
							hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("projectile_healing_label") + ": " + projectileData.healing);

					}

					//does it have an explosion? Add it's info to tooltip
					if (effectData.GetRealEffectDataType() == typeof(ExplosionEffectData))
					{
						var explosionData = (ExplosionEffectData)effectData.RealEffectData;

						if (explosionData.damage != 0)
							hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("projectile_explosion_maxdmg_label") + ": " + explosionData.damage);
						//hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, $"Projectile Max Damage: {explosionData.maxDamage}");

						if (explosionData.damageTypeRef != string.Empty)
							hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("projectile_explosion_dmgtype_label") + ": " + explosionData.damageTypeRef);
						//hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, $"Projectile Damage Type: {explosionData.damageTypeRef}");

						if (explosionData.healing != 0)
							hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, Localization.GetString("projectile_explosion_maxhealing_label") + ": " + explosionData.healing);
						//hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, $"Projectile Max Healing: {explosionData.maxHealing}");

					}
				}
			}
			else//This should not happen unless because of user error.
				Debug.LogError($"Error: WeaponItemComponentData.effectRef was null or empty.");
		}

		private void FuelComponent(ItemFuelComponentData ifc, HB_Main hb)
		{
			hb.AddTextItem(HB_Text.TextStyle.SUB_HEADER, Color.white, "<color=#ff00ff>" + Localization.GetString("excav4_fuel_label") + ":</color> " + ifc.fuelValue);
			//hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, $"ExcaV4 Fuel: {ifc.fuelValue}");
        }

        private void BurnableComponent(ItemBurnableComponentData ibc, HB_Main hb)
        {
            var burnableData = DataBank.Instance.GetData<BurnableData>(ibc.gradeRef);

            var IconTextItem = new HB_Factory.IconTextItem[1] {
                new HB_Factory.IconTextItem(
                    Localization.GetString("hoverbox_burnable") + " ( " + (ibc.burnTime/10.0f).ToString() + "s )", 
                    SpriteBank.GetSprite(burnableData.iconRef) 
                )
            };

            hb.AddIconTextRow(IconTextItem, Color.grey);
        }

		private void ConsumableComponent(ItemConsumableComponentData icc, HB_Main hb)
		{
			hb.AddTextItem(HB_Text.TextStyle.SUB_HEADER, Color.white, Localization.GetString("string_item_consumable"));

			if (icc.buff != ConsumableBuff.NONE)
			{
				var buffText = "";
				switch (icc.buff)
				{
					case ConsumableBuff.HEALTH_GAIN:
						buffText = "<color=#00ff00>" + Localization.GetString("health_gain_title") + ":</color> ";
						break;

					case ConsumableBuff.HEALTH_LOSS:
						buffText = "<color=#ff0000>" + Localization.GetString("health_loss_title") + ":</color> ";
						break;
				}

				hb.AddTextItem(HB_Text.TextStyle.BREAD_TEXT, Color.white, buffText + icc.amount);
			}
		}
	}
}