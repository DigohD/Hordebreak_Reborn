using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Model.Entity.Components.Consumer;
using FNZ.Shared.Model.World;
using FNZ.Shared.Utils;
using UnityEngine;
using static FNZ.Client.View.UI.HoverBox.HB_Factory;

namespace FNZ.Client.Model.Entity.Components.Consumer
{
	public class ConsumerComponentClient : ConsumerComponentShared
	{
		public void AddToHoverBox(HB_Main hb)
		{
            var resources = Data.resources;
            IconTextItem[] entries = new IconTextItem[resources.Count];

            hb.AddDividerLine(
                Color.gray
            );

            for (int i = 0; i < entries.Length; i++)
            {
                var resourceData = DataBank.Instance.GetData<RoomResourceData>(resources[i].resourceRef);
                entries[i] = new IconTextItem(
                    "<color=#ff0000ff>-" + resources[i].amount + " </color>" + Localization.GetString(resourceData.nameRef),
                    SpriteBank.GetSprite(resourceData.iconRef)
                );
            }

            hb.AddIconTextRow(
                entries,
                new Color32(170, 170, 170, 255)
            );
        }
	}
}
