/*
TODO:
- Check if all permissions work
- Fix doors not working
*/

using System;
namespace Oxide.Plugins
{
    [Info("MasterKey", "Wulf/lukespragg", "0.3.0", ResourceId = 1151)]
    [Description("Gain access to any locked object with permissions.")]

    class MasterKey : RustPlugin
    {
        // Do NOT edit this file, instead edit MasterKey.json in server/<identity>/oxide/config

        #region Configuration

        // Messages
        string MasterKeyUsed => GetConfig("MasterKeyUsed", "{player} ({steamid}) used master key at {position}");
        string UnlockedWith => GetConfig("UnlockedWith", "<size=20>Unlocked {object} with master key!</size>");

        // Settings
        bool LogUsage => GetConfig("LogUsage", true);
        bool ShowMessages => GetConfig("ShowMessages", true);

        protected override void LoadDefaultConfig()
        {
            // Messages
            Config["MasterKeyUsed"] = MasterKeyUsed;
            Config["UnlockedWith"] = UnlockedWith;

            // Settings
            Config["LogUsage"] = LogUsage;
            Config["ShowMessages"] = ShowMessages;

            SaveConfig();        }

        #endregion

        #region General Setup

        void Loaded()
        {
            LoadDefaultConfig();

            permission.RegisterPermission("masterkey.all", this);
            permission.RegisterPermission("masterkey.cupboards", this);
            permission.RegisterPermission("masterkey.boxes", this);
            permission.RegisterPermission("masterkey.doors", this);
            permission.RegisterPermission("masterkey.furances", this);
            permission.RegisterPermission("masterkey.gates", this);
            permission.RegisterPermission("masterkey.stashes", this);
        }

        #endregion

        #region Lock Access

        object CanUseDoor(BasePlayer player, BaseLock door)
        {
            var parent = door.parentEntity.Get(true);
            var prefab = parent.LookupPrefabName();
            if (!door.IsLocked()) return true;

            if (prefab.Contains("box.wooden") || prefab.Contains("woodbox_deployed"))
            {
                if (!HasPermission(player, "masterkey.all") && !HasPermission(player, "masterkey.boxes")) return null;
                if (ShowMessages) PrintToChat(player, UnlockedWith.Replace("{object}", "box"));
                if (LogUsage) LogToFile(player, MasterKeyUsed);
                return true;
            }

            if (prefab.Contains("door.hinged"))
            {
                if (!HasPermission(player, "masterkey.all") && !HasPermission(player, "masterkey.doors")) return null;
                if (!parent.IsOpen()) return null;
                if (ShowMessages) PrintToChat(player, UnlockedWith.Replace("{object}", "door"));
                if (LogUsage) LogToFile(player, MasterKeyUsed);
                return true;
            }

            if (prefab.Contains("furnace"))
            {
                if (!HasPermission(player, "masterkey.all") && !HasPermission(player, "masterkey.furnaces")) return null;
                if (ShowMessages) PrintToChat(player, UnlockedWith.Replace("{object}", "furnace"));
                if (LogUsage) LogToFile(player, MasterKeyUsed);
                return true;
            }

            if (prefab.Contains("gates.external"))
            {
                if (!HasPermission(player, "masterkey.all") && !HasPermission(player, "masterkey.gates")) return null;
                if (!parent.IsOpen()) return null;
                if (ShowMessages) PrintToChat(player, UnlockedWith.Replace("{object}", "gate"));
                if (LogUsage) LogToFile(player, MasterKeyUsed);
                return true;
            }

            if (prefab.Contains("stash_deployed"))
            {
                if (!HasPermission(player, "masterkey.all") && !HasPermission(player, "masterkey.stashes")) return null;
                if (ShowMessages) PrintToChat(player, UnlockedWith.Replace("{object}", "stash"));
                if (LogUsage) LogToFile(player, MasterKeyUsed);
                return true;
            }

            return null;
        }

        #endregion

        #region Cupboard Access

        void OnEntityEnter(TriggerBase trigger, BaseEntity entity)
        {
            var player = entity as BasePlayer;

            if (player == null || !(trigger is BuildPrivilegeTrigger)) return;
            if (!HasPermission(player, "masterkey.all") && !HasPermission(player, "masterkey.cupboards")) return;

            timer.Once(0.1f, () => player.SetPlayerFlag(BasePlayer.PlayerFlags.HasBuildingPrivilege, true));
            if (ShowMessages) PrintToChat(player, UnlockedWith.Replace("{object}", "cupboard"));
            if (LogUsage) LogToFile(player, UnlockedWith);
        }

        #endregion

        #region Helper Methods

        T GetConfig<T>(string name, T defaultValue)
        {
            if (Config[name] == null) return defaultValue;
            return (T)Convert.ChangeType(Config[name], typeof(T));
        }

        static void LogToFile(BasePlayer player, string message)
        {
            var dateTime = DateTime.Now.ToString("yyyy-M-d");
            var position = $"{player.transform.position.x}, {player.transform.position.y}, {player.transform.position.z}";
            message = message.Replace("{player}", player.displayName).Replace("{steamid}", player.UserIDString).Replace("{position}", position);
            ConVar.Server.Log($"oxide/logs/masterkeys_{dateTime}.txt", message);
        }

        bool HasPermission(BasePlayer player, string perm) => permission.UserHasPermission(player.UserIDString, perm);

        #endregion
    }
}
