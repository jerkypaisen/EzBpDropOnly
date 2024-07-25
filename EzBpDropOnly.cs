using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Oxide.Core;

namespace Oxide.Plugins
{
    [Info("EzBpDropOnly", "jerkypaisen", "1.0.0")]
    [Description("BP is dropped. Research and tree are disabled.")]
    public class EzBpDropOnly : RustPlugin
    {

        #region Oxide Hooks
        private object CanResearchItem(BasePlayer player, Item item)
        {
            if (item == null || player == null) return null;
            SendReply(player, lang.GetMessage("ResearchItem", this, player.UserIDString));
            return false;
        }

        private object CanUnlockTechTreeNode(BasePlayer player, TechTreeData.NodeInstance node, TechTreeData techTree)
        {
            if (techTree == null || node == null || player == null) return null;
            SendReply(player, lang.GetMessage("UnlockTechTree", this, player.UserIDString));
            return false;
        }

        private void OnItemAction(Item item, string action, BasePlayer player)
        {
            if (player != null && action == "study" && item.IsBlueprint())
            {
                string itemName = item.blueprintTargetDef.displayName.translated;
                string msg = player.displayName + " さんが " + itemName + " をおぼえました";
                Server.Broadcast(msg, null);
            }
        }

        private void SpawnLoot(LootContainer container)
        {
            if (container == null) return;

            var idx = UnityEngine.Random.Range(0, container.inventory.itemList.Count);
            var item = container.inventory.itemList[idx];
            if (item == null) return;
            if (item.info.Blueprint == null) return;
            if (item.info.Blueprint.isResearchable && _config.DropRate >= UnityEngine.Random.Range(0f, 100f)) 
            {
                var shortName = "blueprintbase";
                var bpitem = ItemManager.CreateByName(shortName, 1, 0);
                bpitem.name = "Recipe";
                bpitem.blueprintTarget = item.info.itemid;
                container.inventory.itemList[idx] = bpitem;
                container.inventory.MarkDirty();
            }
        }

        private void OnLootSpawn(LootContainer container)
        {
            NextTick(() => { SpawnLoot(container); });
        }
        #endregion


        #region Config
        private static Configuration _config;

        public class Configuration
        {
            [JsonProperty(PropertyName = "drop rate of the blueprint as a float.")]
            public float DropRate;

            public static Configuration DefaultConfig()
            {
                return new Configuration{DropRate = 0.01f};
            }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _config = Config.ReadObject<Configuration>();
                if (_config == null) LoadDefaultConfig();
                SaveConfig();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                PrintWarning("Creating new config file.");
                LoadDefaultConfig();
            }
        }

        protected override void LoadDefaultConfig() => _config = Configuration.DefaultConfig();
        protected override void SaveConfig() => Config.WriteObject(_config);

        #endregion
        #region Localization
        void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["ResearchItem"] = "You can not research!",
                ["UnlockTechTree"] = "You can not unlock!",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["ResearchItem"] = "このアイテムのリサーチはできません。",
                ["UnlockTechTree"] = "テックツリーをアンロックすることはできません。",
            }, this, "ja");

        }
        #endregion
    }
}