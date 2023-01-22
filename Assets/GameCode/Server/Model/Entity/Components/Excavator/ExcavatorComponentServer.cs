using FNZ.Server.Controller;
using FNZ.Server.Model.Entity.Components.Excavatable;
using FNZ.Server.Model.Entity.Components.Inventory;
using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Server.Services.QuestManager;
using FNZ.Shared.Model.Entity.Components.Excavator;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Utils;
using FNZ.Shared.Utils.CollisionUtils;
using Lidgren.Network;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Model.Entity.Components.Excavator
{
	public class ExcavatorComponentServer : ExcavatorComponentShared, ILateTickable
	{
        private PlayerComponentServer m_PlayerComp;
        private InventoryComponentServer m_Inventory;

        private byte m_ExcavateTicks;
        private NetConnection m_playerConnection;

        public override void InitComponentLinks()
        {
            base.InitComponentLinks();

            m_PlayerComp = ParentEntity.GetComponent<PlayerComponentServer>();
            m_Inventory = ParentEntity.GetComponent<InventoryComponentServer>();
        }

        public void NE_Send_FuelUpdate()
        {
            GameServer.NetAPI.BA_Entity_ComponentNetEvent(
                this,
                (byte)ExcavatorNetEvent.FUEL_UPDATE,
                new FuelUpdateData { fuel = m_Fuel }
            );
        }

        public void NE_Send_RefuelSlotUpdate()
        {
            GameServer.NetAPI.BA_Entity_ComponentNetEvent(
                this,
                (byte) ExcavatorNetEvent.REFUEL_SLOT_UPDATE,
                new RefuelSlotUpdateData { slotItem = RefuelSlot }
            );
        }

        public void Tick(float dt)
        {
            m_ExcavateTicks++;
        }

        public override void OnNetEvent(NetIncomingMessage incMsg)
        {
            base.OnNetEvent(incMsg);

            switch ((ExcavatorNetEvent) incMsg.ReadByte())
            {
                case ExcavatorNetEvent.LOCK_ENTITY:
                    NE_Receive_LockEntity(incMsg);
                    break;

                case ExcavatorNetEvent.UNLOCK_ENTITY:
                    NE_Receive_UnlockEntity(incMsg);
                    break;

                case ExcavatorNetEvent.REFUEL_SLOT_CLICK:
                    NE_Receive_RefuelSlotClick(incMsg);
                    break;

                case ExcavatorNetEvent.REFUEL_MAX_CLICK:
                    NE_Receive_RefuelMaxClick(incMsg);
                    break;

                case ExcavatorNetEvent.REFUEL_ONE_CLICK:
                    NE_Receive_RefuelOneClick(incMsg);
                    break;

                case ExcavatorNetEvent.TRIGGER_EXCAVATE:
                    NE_Receive_TriggerExcavate(incMsg);
                    break;
            }
        }

        private void Excavate(byte bonusIndex)
        {
            var excavatableComp = m_LockedEntity.GetComponent<ExcavatableComponentServer>();

            if(excavatableComp != null)
            {
                List<Item> loot = null;
                excavatableComp.Hit();

                if (excavatableComp.GetHitsRemaining() > 0)
                {
                    if(excavatableComp.Data.hitLootTable != null)
                        loot = Utils.LootGenerator.GenerateLoot(excavatableComp.Data.hitLootTable);
                }
                else
                {
                    if (excavatableComp.Data.destroyLootTable != null)
                        loot = Utils.LootGenerator.GenerateLoot(excavatableComp.Data.destroyLootTable);

                    foreach (var player in GameServer.NetConnector.GetConnectedClientEntities())
                    {
                        var excaComp = player.GetComponent<ExcavatorComponentServer>();
                        if (excaComp.m_LockedEntity == m_LockedEntity)
                        {
                            excaComp.NE_Send_UnlockEntity(m_LockedEntity.NetId);
                        }
                    }

                    m_LockedEntity = null;
                    m_LockedEntityExcavatable = null;
                    
                    NE_Send_FuelUpdate();
                   
                }

                if (loot != null)
                {
                    foreach (var item in loot)
                    {
                        var wasPlaced = m_Inventory.AutoPlaceIfPossible(item);
                        if (wasPlaced)
                        {
                            GameServer.NetAPI.Notification_SendNotification_STC(
                                "excavator_flat_icon",
                                "dark_grey",
                                "false",
                                item.amount + "x " + Localization.GetString(item.Data.nameRef),
                                m_playerConnection
                            );

                            GameServer.NetAPI.Entity_UpdateComponent_BA(m_Inventory);
                        }
                        else
                        {
                            GameServer.NetAPI.Notification_SendWarningNotification_STC(
                                Localization.GetString("inventory_full_message"),
                                 m_playerConnection
                            );
                            
                            var pos = new float2(
                                excavatableComp.ParentEntity.Position.x + 0.5f, 
                                excavatableComp.ParentEntity.Position.y + 0.5f
                            );
				
                            var v = new Vector2(0.5f, 0);
                            var finalOffset = Quaternion.Euler(0, 0, FNERandom.GetRandomFloatInRange(0, 360)) * v;
                            var itemOnGroundPos = new float2(pos.x + finalOffset.x, pos.y + finalOffset.y);
                            
                            GameServer.ItemsOnGroundManager.SpawnItemOnGround(itemOnGroundPos, item);
                        }
                        QuestManager.OnExcavateResource(item);
                    }
                }

                if(bonusIndex != 255)
                {
                    var bonus = excavatableComp.Data.excavatableBonuses[bonusIndex];
                    loot = Utils.LootGenerator.GenerateLoot(bonus.lootTable);

                    foreach (var item in loot)
                    {
                        var wasPlaced = m_Inventory.AutoPlaceIfPossible(item);
                        if (wasPlaced)
                        {
                            GameServer.NetAPI.Notification_SendNotification_STC(
                                "excavator_flat_icon",
                                bonus.colorRef,
                                "false",
                                item.amount + "x " + Localization.GetString(item.Data.nameRef),
                                m_playerConnection
                            );

                            GameServer.NetAPI.Entity_UpdateComponent_BA(m_Inventory);
                        }
                        else
                        {
                            GameServer.NetAPI.Notification_SendWarningNotification_STC(
                                Localization.GetString("inventory_full_message"),
                                 m_playerConnection
                            );
                            
                            var pos = new float2(
                                excavatableComp.ParentEntity.Position.x + 0.5f, 
                                excavatableComp.ParentEntity.Position.y + 0.5f
                            );
				
                            var v = new Vector2(0.5f, 0);
                            var finalOffset = Quaternion.Euler(0, 0, FNERandom.GetRandomFloatInRange(0, 360)) * v;
                            var itemOnGroundPos = new float2(pos.x + finalOffset.x, pos.y + finalOffset.y);
                            
                            GameServer.ItemsOnGroundManager.SpawnItemOnGround(itemOnGroundPos, item);
                        }
                        QuestManager.OnExcavateResource(item);
                    }
                }

            }
        }

        private void NE_Receive_LockEntity(NetIncomingMessage incMsg)
        {
            var data = new LockEntityData();
            data.Deserialize(incMsg);

            GameServer.NetAPI.Client_Immediate_Forward_To_Other(
                incMsg.Data,
                incMsg.SenderConnection
            );

            m_LockedEntity = GameServer.NetConnector.GetFneEntity(data.NetId);
            m_ExcavateTicks = 0;
        }

        private void NE_Receive_UnlockEntity(NetIncomingMessage incMsg)
        {
            m_LockedEntity = null;
            m_LockedEntityExcavatable = null;

            m_ExcavateTicks = 0;

            GameServer.NetAPI.Client_Immediate_Forward_To_Other(
               incMsg.Data,
               incMsg.SenderConnection
            );
        }

        private void NE_Receive_RefuelSlotClick(NetIncomingMessage incMsg)
        {
            Item onCursor = m_PlayerComp.GetItemOnCursor();
            
            if (onCursor != null && RefuelSlot == null && onCursor.GetComponent<ItemFuelComponent>() != null)
            {
                RefuelSlot = onCursor;
                m_PlayerComp.SetItemOnCursor(null);

                NE_Send_RefuelSlotUpdate();
            }
            else if (onCursor != null && RefuelSlot != null && onCursor.GetComponent<ItemFuelComponent>() != null)
            {
                RefuelSlot = onCursor;
                m_PlayerComp.SetItemOnCursor(RefuelSlot);

                NE_Send_RefuelSlotUpdate();
            }
            else if (onCursor == null && RefuelSlot != null)
            {
                m_PlayerComp.SetItemOnCursor(RefuelSlot);
                RefuelSlot = null;

                NE_Send_RefuelSlotUpdate();
            }
        }

        private void NE_Receive_RefuelMaxClick(NetIncomingMessage incMsg)
        {
            if (RefuelSlot == null || m_Fuel == Data.BaseFuel)
                return;

            int fuelValue = RefuelSlot.GetComponent<ItemFuelComponent>().Data.fuelValue;

            int diff = Data.BaseFuel - m_Fuel;
            int amountToBurn = diff / fuelValue;
            amountToBurn = diff % fuelValue == 0 ? amountToBurn : amountToBurn + 1;

            if(amountToBurn >= RefuelSlot.amount)
            {
                m_Fuel += fuelValue * RefuelSlot.amount;

                if (m_Fuel > Data.BaseFuel)
                    m_Fuel = Data.BaseFuel;

                RefuelSlot = null;
            }
            else
            {
                m_Fuel += fuelValue * amountToBurn;

                if (m_Fuel > Data.BaseFuel)
                    m_Fuel = Data.BaseFuel;

                RefuelSlot.amount -= amountToBurn;
            }

            NE_Send_FuelUpdate();
            NE_Send_RefuelSlotUpdate();
        }

        private void NE_Receive_RefuelOneClick(NetIncomingMessage incMsg)
        {
            if (RefuelSlot == null || m_Fuel == Data.BaseFuel)
                return;

            RefuelSlot.amount--;
            m_Fuel += RefuelSlot.GetComponent<ItemFuelComponent>().Data.fuelValue;

            if (m_Fuel > Data.BaseFuel)
                m_Fuel = Data.BaseFuel;

            NE_Send_FuelUpdate();

            if(RefuelSlot.amount == 0)
            {
                RefuelSlot = null;
            }

            NE_Send_RefuelSlotUpdate();
        }

        private void NE_Receive_TriggerExcavate(NetIncomingMessage incMsg)
        {
            if (m_LockedEntity != null)
            {
                var data = new TriggerExcavateData();
                data.Deserialize(incMsg);

                if (m_playerConnection == null)
                    m_playerConnection = GameServer.NetConnector.GetConnectionFromPlayer(ParentEntity);

                Excavate(data.BonusIndex);
            }
        }
        
        public void NE_Send_UnlockEntity(int netId)
        {
            GameServer.NetAPI.BA_Entity_ComponentNetEvent(
                this,
                (byte)ExcavatorNetEvent.UNLOCK_ENTITY,
                new LockEntityData(netId)
            );
        }

        public void LateTick(float dt)
        {
            /*if (m_LockedEntity != null && m_ExcavateTicks >= 5)
            {
                if (m_playerConnection == null)
                    m_playerConnection = GameServer.NetConnector.GetConnectionFromPlayer(ParentEntity);

                Excavate();
                m_ExcavateTicks = 0;
            }*/
        }
    }
}
