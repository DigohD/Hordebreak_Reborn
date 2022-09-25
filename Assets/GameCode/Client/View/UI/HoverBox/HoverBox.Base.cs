using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Environment;
using FNZ.Shared.Utils;
using System.Collections.Generic;
using UnityEngine;
using static FNZ.Client.View.UI.HoverBox.HB_Factory;

namespace FNZ.Client.View.UI.HoverBox
{

	public partial class HoverBoxGen
	{
		public void CreateRoomHoverBox(long Id)
		{
			var Room = GameClient.RoomManager.GetRoom(Id);

			HB_Main hb = HB_Factory.CreateNewHoverBox(m_Canvas, Id);
			if (hb == null)
				return;

			hb.AddTextItem(
				HB_Text.TextStyle.HEADER,
				new Color32(170, 170, 170, 255),
				Room.Name
			);

			hb.AddDividerLine(Color.magenta);

			foreach (var roomValue in Room.roomEnvironmentValues)
			{
				var data = DataBank.Instance.GetData<EnvironmentData>(roomValue.Key);
				if (roomValue.Value == 0)
					continue;

				AddEnvironmentValueToHoverBox(data, roomValue.Value, hb);
			}

			hb.AddDividerLine(
				Color.gray
			);

			AddRoomPropertiesToHoverBox(Room.RoomProperties, hb);

			hb.AddDividerLine(
				Color.gray
			);


			hb.FinishConstruction();
		}

		private void AddEnvironmentValueToHoverBox(EnvironmentData data, int value, HB_Main hb)
		{
			hb.AddTextItem(
				HB_Text.TextStyle.BREAD_TEXT, 
				new Color32(170, 170, 170, 255), 
				Localization.GetString(data.nameRef) + ": " + value
			);
		}

		private void AddRoomPropertiesToHoverBox(Dictionary<string, byte> roomProperties, HB_Main hb)
		{
			IconTextItem[] entries = new IconTextItem[roomProperties.Count];
			int entryCount = 0;
			foreach (var property in roomProperties)
			{
				var data = DataBank.Instance.GetData<RoomPropertyData>(property.Key);

				// property.value can also be 0. This means it is an "absence" type property.
				string prefix = "";
				switch (property.Value)
				{
					case 1:
						prefix = Localization.GetString("room_property_half");
						break;

					case 2:
						prefix = Localization.GetString("room_property_full");
						break;
				}

				entries[entryCount++] = new IconTextItem(
					prefix + " " + Localization.GetString(data.displayNameRef),
					SpriteBank.GetSprite(data.iconRef)
				);
			}

			hb.AddIconTextRow(
				entries,
				Color.cyan
			);
		}
	}
}