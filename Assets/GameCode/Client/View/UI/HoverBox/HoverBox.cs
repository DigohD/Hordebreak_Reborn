using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Utils;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using static FNZ.Client.View.UI.HoverBox.HB_Factory;

namespace FNZ.Client.View.UI.HoverBox
{
	public partial class HoverBoxGen
	{
		protected Canvas m_Canvas;

		public HoverBoxGen(Canvas canvas)
		{
			m_Canvas = canvas;
		}

        public void CreateSimpleTextHoverBox(string id)
        {
            HB_Main hb = HB_Factory.CreateNewHoverBox(m_Canvas, id);
            if (hb == null)
                return;

            hb.AddTextItem(
                HB_Text.TextStyle.HEADER,
                new Color32(170, 170, 170, 255),
                Localization.GetString(id)
            );

            hb.FinishConstruction();
        }

        public void CreateSimpleItemHoverBox(Item item, Vector3 position)
        {
            HB_Main hb = HB_Factory.CreateNewHoverBox(m_Canvas, item);
            if (hb == null)
                return;

            IconTextItem[] entries = new IconTextItem[1];
            entries[0] = new IconTextItem(
                "<color=#FFFF00>" + item.amount + "x</color> " + Localization.GetString(item.Data.nameRef),
                SpriteBank.GetSprite(item.Data.iconRef)
            );

            hb.AddIconTextRow(
                entries,
                Color.white
            );

            hb.FinishConstruction();

            hb.SetAnchorMode_WorldObject(position);
        }
    }
}