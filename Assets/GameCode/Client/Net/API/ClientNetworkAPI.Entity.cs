using System.Collections.Generic;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Net;

namespace FNZ.Client.Net.API
{

	public partial class ClientNetworkAPI
	{
		//This method is DEBUG only
		public void CMD_Entity_UpdateHealth(float amount, string damageTypeRef, bool isDamage)
		{
			var message = m_EntityMessageFactory.CreateUpdateHealthMessage(amount, damageTypeRef, isDamage);
			Command(message);
		}

		public void CMD_Entity_UpdateComponent(FNEComponent component)
		{
			var message = m_EntityMessageFactory.CreateUpdateComponentMessage(component);
			Command(message);
		}

		public void CMD_Entity_UpdateComponents(FNEEntity parent)
		{
			var message = m_EntityMessageFactory.CreateUpdateComponentsMessage(parent);
			Command(message);
		}

		public void CMD_Entity_ComponentNetEvent(FNEComponent component, byte eventId, IComponentNetEventData data = null)
		{
			var message = m_EntityMessageFactory.CreateComponentNetEventMessage(component, eventId, data);
			Command(message);
		}

		public void CMD_Entity_ClientDisconnectFromServer()
		{
			var message = m_EntityMessageFactory.CreateClientDisconnectsMessage();
			Command(message);
		}

		public void CMD_Entity_ConfirmHordeEntityBatchSpawned(List<int> netIds)
		{
			var message = m_EntityMessageFactory.CreateConfirmHordeEntityBatchSpawnedMessage(netIds);
			Command(message);
		}
		
		public void CMD_Entity_ConfirmHordeEntityBatchDestroyed(List<int> netIds)
		{
			var message = m_EntityMessageFactory.CreateConfirmHordeEntityBatchDestroyedMessage(netIds);
			Command(message);
		}
	}
}