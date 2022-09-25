using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Model.Entity.Components.Refinement;
using FNZ.Shared.Model.Items;
using Lidgren.Network;

namespace FNZ.Client.Model.Entity.Components.Refinement
{
	public delegate void OnRefinementUpdate();
	public delegate void OnRefinementProgressUpdate();
	public delegate void OnRefinementWorkingUpdate(bool isWorking);

	public class RefinementComponentClient : RefinementComponentShared, IInteractableComponent
	{
		public OnRefinementUpdate d_OnRefinementUpdate;
		public OnRefinementProgressUpdate d_OnRefinementProgressUpdate;
		public OnRefinementWorkingUpdate d_OnRefinementWorkingUpdate;

		public override void Init()
		{
			base.Init();

			if (Data.recipes.Count > 0)
				m_ActiveRecipe = DataBank.Instance.GetData<RefinementRecipeData>(Data.recipes[0]);
		}

		public void OnInteract()
		{
			GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>().SetOpenedInteractable(this);
		}

		public void OnPlayerExitRange()
		{
			var playerComp = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();

			if (playerComp.OpenedInteractable == this)
				playerComp.SetOpenedInteractable(null);
		}

		public void OnPlayerInRange()
		{

		}

		public void NE_Send_SetActiveRecipe(ushort recipeIndex)
		{
			if (m_ActiveRecipe.Id == DataBank.Instance.GetData<RefinementRecipeData>(Data.recipes[recipeIndex]).Id)
				return;

			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)RefinementNetEvent.SET_ACTIVE_RECIPE,
				new SetRefinementRecipeData() { RecipeIndex = recipeIndex }
			);
		}

		public void NE_Send_SlotClick(RefinementSlotType slotType, byte slotIndex)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)RefinementNetEvent.SLOT_CLICK,
				new RefinmentSlotClickData()
				{
					SlotType = slotType,
					SlotIndex = slotIndex
				}
			);
		}

		public void NE_Send_SlotRightClick(RefinementSlotType slotType, byte slotIndex)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)RefinementNetEvent.SLOT_RIGHT_CLICK,
				new RefinmentSlotClickData()
				{
					SlotType = slotType,
					SlotIndex = slotIndex
				}
			);
		}

		public void NE_Send_SlotShiftClick(RefinementSlotType slotType, byte slotIndex)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)RefinementNetEvent.SLOT_SHIFT_CLICK,
				new RefinmentSlotClickData()
				{
					SlotType = slotType,
					SlotIndex = slotIndex
				}
			);
		}

		public void NE_Send_SlotShiftRightClick(RefinementSlotType slotType, byte slotIndex)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)RefinementNetEvent.SLOT_SHIFT_RIGHT_CLICK,
				new RefinmentSlotClickData()
				{
					SlotType = slotType,
					SlotIndex = slotIndex
				}
			);
		}

		public override void OnNetEvent(NetIncomingMessage incMsg)
		{
			base.OnNetEvent(incMsg);

			switch ((RefinementNetEvent)incMsg.ReadByte())
			{
				case RefinementNetEvent.SET_ACTIVE_RECIPE:
					NE_Receive_SetActiveRecipe(incMsg);
					break;

				case RefinementNetEvent.SLOT_UPDATE:
					NE_Receive_SlotUpdate(incMsg);
					break;

				case RefinementNetEvent.PROGRESS_UPDATE:
					NE_Receive_ProgressUpdate(incMsg);
					break;

				case RefinementNetEvent.IS_WORKING_UPDATE:
					NE_Receive_IsWorkingUpdate(incMsg);
					break;
			}
		}

		private void NE_Receive_SetActiveRecipe(NetIncomingMessage incMsg)
		{
			var data = new SetRefinementRecipeData();
			data.Deserialize(incMsg);

			m_ActiveRecipe = DataBank.Instance.GetData<RefinementRecipeData>(Data.recipes[data.RecipeIndex]);

			d_OnRefinementUpdate?.Invoke();
		}

		private void NE_Receive_SlotUpdate(NetIncomingMessage incMsg)
		{
			var data = new RefinmentSlotUpdateData();
			data.Deserialize(incMsg);

			if (data.itemRef == "")
			{
				slots[(byte)data.SlotType, data.SlotIndex] = null;
			}
			else
			{
				slots[(byte)data.SlotType, data.SlotIndex] = Item.GenerateItem(data.itemRef, data.itemAmount, true);
			}


			d_OnRefinementUpdate?.Invoke();
		}

		private void NE_Receive_ProgressUpdate(NetIncomingMessage incMsg)
		{
			var data = new RefinementProgressUpdateData();
			data.Deserialize(incMsg);

			m_ProcessTicks = data.ProcessTicks;

			d_OnRefinementProgressUpdate?.Invoke();
		}

		private void NE_Receive_IsWorkingUpdate(NetIncomingMessage incMsg)
		{
			var data = new IsWorkingUpdateData();
			data.Deserialize(incMsg);

			m_IsWorking = data.IsWorking;

			d_OnRefinementWorkingUpdate?.Invoke(m_IsWorking);
		}

		public RefinementRecipeData GetActiveRecipe()
		{
			return m_ActiveRecipe;
		}

		public Item GetItemInSlot(RefinementSlotType slotType, byte slotIndex)
		{
			return slots[(byte)slotType, slotIndex];
		}

		public float GetProgress()
		{
			return (float)m_ProcessTicks / (float)m_ActiveRecipe.processTime;
		}

		public string InteractionPromptMessageRef()
		{
			return "refinement_component_interact";
		}

		public bool IsInteractable()
		{
			return true;
		}
	}
}
