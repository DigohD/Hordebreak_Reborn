using System.Numerics;
using FNZ.Client.Model.Entity.Components.Inventory;
using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Client.View.Audio;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Model.Entity.Components.Crafting;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Utils;
using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Client.Model.Entity.Components.Crafting
{
	public delegate void OnCraftUpdate();

	public class CraftingComponentClient : CraftingComponentShared, IInteractableComponent
	{
		public OnCraftUpdate d_OnCraftUpdate;

		private CraftingRecipeData m_ActiveRecipe = null;
		private int m_CraftAmount = 1;

		public void SetActiveRecipe(CraftingRecipeData newRecipe)
		{
			m_ActiveRecipe = newRecipe;
			m_CraftAmount = 1;
		}

		public void OnInteract()
		{
			m_ActiveRecipe = DataBank.Instance.GetData<CraftingRecipeData>(Data.recipes[0]);
			m_CraftAmount = 1;
			GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>().SetOpenedInteractable(this);
		}

		public void OnPlayerExitRange()
		{
			var playerComp = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();

			if (playerComp.OpenedInteractable == this)
				playerComp.SetOpenedInteractable(null);
		}

		public void OnPlayerInRange()
		{ }

		public void NE_Send_Craft()
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)CraftingNetEvent.CRAFT,
				new CraftData()
				{
					recipeRef = m_ActiveRecipe.Id,
					amount = m_CraftAmount
				}
			);
		}

        public override void OnNetEvent(NetIncomingMessage incMsg)
        {
            base.OnNetEvent(incMsg);

            switch ((CraftingNetEvent)incMsg.ReadByte())
            {
                case CraftingNetEvent.CRAFT:
                    NE_Receive_Craft(incMsg);
                    break;
            }
        }

        private void NE_Receive_Craft(NetIncomingMessage incMsg)
        {
            var data = new CraftData();
            data.Deserialize(incMsg);

            var recipe = DataBank.Instance.GetData<CraftingRecipeData>(data.recipeRef);

            if(!string.IsNullOrEmpty(recipe.craftSFXRef))
                AudioManager.Instance.PlaySfx3dClip(recipe.craftSFXRef, ParentEntity.Position);

            if(!string.IsNullOrEmpty(recipe.craftVFXRef))
	            GameClient.ViewAPI.SpawnVFX(
		            recipe.craftVFXRef, 
		            ParentEntity.Position + new float2(0.5f, 0.5f),
		            0,
		            1
	            );
            
            var product = DataBank.Instance.GetData<ItemData>(recipe.productRef);
            GameClient.ViewAPI.SpawnWorldPrompt(
                "<color=#FFFF00>" + data.amount + "</color> " + Localization.GetString(product.nameRef),
                ParentEntity.Position    
            );
        }

        public CraftingRecipeData GetActiveRecipe()
		{
			return m_ActiveRecipe;
		}

		public int GetCraftAmount()
		{
			return m_CraftAmount;
		}

		public void AmountAddOne()
		{
			m_CraftAmount++;
		}

		public void AmountSetDirect(int newAmount)
		{
			int max = CalculateMaximumCraft();
			if (newAmount > max)
				newAmount = max;

			m_CraftAmount = newAmount;
		}

		public void AmountSubtractOne()
		{
			m_CraftAmount = m_CraftAmount > 1 ? m_CraftAmount - 1 : 1;
		}

		public void AmountSetMaximum()
		{
			m_CraftAmount = CalculateMaximumCraft();

			d_OnCraftUpdate?.Invoke();
		}

		private int CalculateMaximumCraft()
        {
			var inventory = GameClient.LocalPlayerEntity.GetComponent<InventoryComponentClient>();

			int max = int.MaxValue;
			foreach (var materialDef in m_ActiveRecipe.requiredMaterials)
			{
				int materialLimit = inventory.GetItemCount(materialDef.itemRef) / materialDef.amount;
				max = materialLimit < max ? materialLimit : max;
			}

			if (max == 0)
				max = 1;

			return max;
		}

		public string InteractionPromptMessageRef()
		{
			return "crafting_component_interact";
		}

		public bool IsInteractable()
		{
			return true;
		}
	}
}
