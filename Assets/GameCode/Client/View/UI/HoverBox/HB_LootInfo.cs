using FNZ.Shared.Model.Items;
using UnityEngine;

public delegate bool OnHoverBoxLootClick(Item toLoot);

namespace FNZ.Client.View.UI.HoverBox
{

	public class HB_LootInfo : MonoBehaviour
	{

		/*public OnHoverBoxLootClick onHoverBoxLootClick;
	
	    private Item item;
	
	    public void Build(Item item, Color textColor)
	    {
	        this.item = item;
	
	        transform.GetChild(0).GetComponentInChildren<Image>().sprite = item.GetItemUISprite();
	        GetComponentInChildren<Text>().text = @"<color=#ffff00>" + item.amount + "x </color>" + item.data.displayName;
	        GetComponentInChildren<Text>().color = textColor;
	    }
	
	    public void OnClick()
	    {
	        bool wasLooted = onHoverBoxLootClick(item);
	        if (wasLooted)
	            Destroy(gameObject);
	    }*/
	}
}