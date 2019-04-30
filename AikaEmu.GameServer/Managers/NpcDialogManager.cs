using System;
using AikaEmu.GameServer.Models.CharacterM;
using AikaEmu.GameServer.Models.NpcM;
using AikaEmu.GameServer.Models.NpcM.Dialog;
using AikaEmu.GameServer.Network.Packets.Game;
using AikaEmu.GameServer.Utils;
using AikaEmu.Shared.Utils;
using NLog;

namespace AikaEmu.GameServer.Managers
{
    public class NpcDialogManager : Singleton<NpcDialogManager>
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public void BuyFromShop(Character character, uint npcConId, int index, uint quantity)
        {
            if (character.OpenedShopNpcConId != npcConId || character.OpenedShopType != ShopType.Store)
            {
                character.Connection.Close();
                return;
            }

            if (quantity <= 0) return;

            var npc = WorldManager.Instance.GetNpc(npcConId);
            if (npc == null) return;

            if (npc.StoreItems[index] <= 0) return;

            var item = DataManager.Instance.ItemsData.GetItemData(npc.StoreItems[index]);
            if (item == null) return;

            if (character.Money < item.BuyPrice * quantity) return;

            if (!character.Inventory.AddItem(SlotType.Inventory, quantity, npc.StoreItems[index])) return;

            character.SendPacket(new Unk303D(character, 0));
            character.Money -= item.BuyPrice * quantity;
            character.SendPacket(new UpdateCharGold(character));
            character.SendPacket(new Unk303D(character, 1));

            character.Save(PartialSave.Inventory);
        }

        public void StartDialog(Character character, uint npcId, DialogType optionId, int unk)
        {
            if (character.OpenedShopType != ShopType.None) return;

            var npc = WorldManager.Instance.GetNpc(npcId);
            if (npc == null) return;

            if (!MathUtils.CheckInRange(character.Position, npc.Position, 5)) return;

            if (optionId <= 0)
            {
                if (npc.DialogList == null || npc.DialogList.Count <= 0) return;
                character.SendPacket(new OpenNpcChat(npc.Id));
                character.SendPacket(new PlaySound(npc.SoundId, npc.SoundType));
                character.SendPacket(new ResetChatOptions());

                foreach (var dialog in npc.DialogList)
                {
                    if (dialog.SubOptionId == 0)
                    {
                        var temp = new NpcDialog(dialog.OptionId, 0, dialog.Text);
                        character.SendPacket(new SendNpcOption(temp));
                    }
                    else
                    {
                        // TODO - IMPLEMENT SUB TYPE
                        character.SendPacket(new CloseNpcChat());
                    }
                }
            }
            else
            {
                var dialogInfo = npc.GetDialog(optionId);
                character.SendPacket(new CloseNpcChat());
                if (dialogInfo == null) return;

                // TODO - CHECK IF CAN OPEN
                switch (optionId)
                {
                    case DialogType.Talk:
                        break;
                    case DialogType.Quest:
                        break;
                    case DialogType.Teleport:
                        break;
                    case DialogType.Store:
                    {
                        if (npc.StoreItems != null && npc.StoreType == StoreType.Normal)
                        {
                            character.SendPacket(new NpcStoreOpen((ushort) npc.Id, npc.StoreType, npc.StoreItems));
                            character.OpenedShopType = ShopType.Store;
                            character.OpenedShopNpcConId = npc.Id;
                        }

                        _log.Debug("Store: NpcId: {0}, StoreType: {1}", npc.NpcId, npc.StoreType);
                    }
                        break;
                    case DialogType.StoreSkill:
                    {
                        // TODO - THIS STORE CHANGES DYNAMICALLY
                        // This is just placeholder
                        if (npc.StoreItems != null && npc.StoreType == StoreType.Skill)
                        {
                            character.SendPacket(new NpcStoreOpen((ushort) npc.Id, npc.StoreType, npc.StoreItems));
                            character.OpenedShopType = ShopType.Store;
                            character.OpenedShopNpcConId = npc.Id;
                        }

                        _log.Debug("Store: NpcId: {0}, StoreType: {1}", npc.NpcId, npc.StoreType);
                    }
                        break;
                    case DialogType.Bank:
                        character.Inventory.SendBankData();
                        OpenShop(character, ShopType.Bank, npc.Id);
                        break;
                    case DialogType.PranStation:
                        character.Inventory.SendBankData();
                        OpenShop(character, ShopType.PranStation, npc.Id);
                        break;
                    case DialogType.GuildCreate:
                        break;
                    case DialogType.GuildBank:
                        break;
                    case DialogType.DialogEnd:
                        break;
                    case DialogType.Fortification:
                        OpenShop(character, ShopType.Fortification, npc.Id);
                        break;
                    case DialogType.Enchant:
                        OpenShop(character, ShopType.Enchant, npc.Id);
                        break;
                    case DialogType.ChangeNation:
                        break;
                    case DialogType.QuestMenu:
                        break;
                    case DialogType.QuestAccept:
                        break;
                    case DialogType.QuestReward:
                        break;
                    case DialogType.SaveLocation:
                        break;
                    case DialogType.EnterInstance:
                        break;
                    case DialogType.Repair:
                        OpenShop(character, ShopType.Repair, npc.Id);
                        break;
                    case DialogType.RepairAll:
                        OpenShop(character, ShopType.RepairAll, npc.Id);
                        break;
                    case DialogType.BlessFree:
                        break;
                    case DialogType.Upgrade:
                        break;
                    case DialogType.QuestReward2:
                        break;
                    case DialogType.NationStatus:
                        break;
                    case DialogType.GuildSkills:
                        break;
                    case DialogType.UpgradeCostume:
                        break;
                    case DialogType.BlessPaid:
                        break;
                    case DialogType.Evolution:
                        OpenShop(character, ShopType.Evolution, npc.Id);
                        break;
                    case DialogType.BattleField:
                        break;
                    case DialogType.TeleportDisckeroa:
                    {
                        character.TeleportTo(1893.4f, 3787.4f);
                        _log.Debug("TeleportTo: Disckeroa");
                    }
                        break;
                    case DialogType.MoveToWar:
                        break;
                    case DialogType.RegisterWar:
                        break;
                    case DialogType.StoneRefinement:
                        OpenShop(character, ShopType.StoneRefinement, npc.Id);
                        break;
                    case DialogType.StoneEnchant:
                        OpenShop(character, ShopType.StoneEnchant, npc.Id);
                        break;
                    case DialogType.StoneCombination:
                        OpenShop(character, ShopType.StoneCombination, npc.Id);
                        break;
                    case DialogType.Craft:
                        OpenShop(character, ShopType.Craft, npc.Id);
                        break;
                    case DialogType.Dismantle:
                        OpenShop(character, ShopType.Dismantle, npc.Id);
                        break;
                    case DialogType.Transfer:
                        OpenShop(character, ShopType.Transfer, npc.Id);
                        break;
                    case DialogType.LevelDown:
                        OpenShop(character, ShopType.LevelDown, npc.Id);
                        break;
                    case DialogType.ChatClose:
                        break;
                    default:
                        return;
                }
            }
        }

        public void CloseShop(Character character, ShopType type)
        {
            if (character.OpenedShopType != type)
            {
                character.Connection.Close();
                _log.Warn("Character: \"{0}\". Opened store dont match with closed store!", character.Name);
            }
            else
            {
                character.OpenedShopType = ShopType.None;
                character.OpenedShopNpcConId = 0;
            }
        }

        private void OpenShop(Character character, ShopType type, uint npcConId)
        {
            character.SendPacket(new OpenNpcShop(type));
            character.OpenedShopType = type;
            character.OpenedShopNpcConId = npcConId;
            _log.Debug("Character: {0}, ShopType: {1}", character.Name, type);
        }
    }
}