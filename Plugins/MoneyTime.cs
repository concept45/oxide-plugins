﻿/*
TODO:
- Add option to disable payout for AFK players (store last moved time)
- Add option for daily/weekly login bonuses
- Rework the math so the earned money amounts aren't insane
- Utilize player.net.connection.GetSecondsConnected()
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using ProtoBuf;
using Oxide.Core;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("MoneyTime", "Wulf/lukespragg", "1.0.3", ResourceId = 836)]
    [Description("Pays players money via Economics for playing on your server.")]

    class MoneyTime : RustPlugin
    {
        // Do NOT edit this file, instead edit MoneyTime.json in server/<identity>/oxide/config

        #region Configuration

        // Messages
        string ReceivedForPlaying => GetConfig("ReceivedForPlaying", "You've received ${amount} for actively playing!");
        string ReceivedForTimeAlive => GetConfig("ReceivedForTimeAlive", "You've received ${amount} for staying alive for {time}!");
        string ReceivedWelcomeBonus => GetConfig("ReceivedWelcomeBonus", "You've received ${amount} as a welcome bonus!");

        // Settings
        float BasePayout => GetConfig("BasePayout", 5f);
        int PayoutInterval => GetConfig("PayoutInterval", 600);
        bool TimeAliveBonus => GetConfig("TimeAliveBonus", true);
        float TimeAliveMultiplier => GetConfig("TimeAliveMultiplier", 1.0f);
        float WelcomeBonus => GetConfig("WelcomeBonus", 500f);

        protected override void LoadDefaultConfig()
        {
            // Messages
            Config["ReceivedForPlaying"] = ReceivedForPlaying;
            Config["ReceivedForTimeAlive"] = ReceivedForTimeAlive;
            Config["ReceivedWelcomeBonus"] = ReceivedWelcomeBonus;

            // Settings
            Config["BasePayout"] = BasePayout;
            Config["PayoutInterval"] = PayoutInterval;
            Config["TimeAliveBonus"] = TimeAliveBonus;
            Config["TimeAliveMultiplier"] = TimeAliveMultiplier;
            Config["WelcomeBonus"] = WelcomeBonus;

            SaveConfig();
        }

        #endregion

        #region Data Storage

        StoredData storedData;

        class StoredData
        {
            public Dictionary<ulong, PlayerInfo> Players = new Dictionary<ulong, PlayerInfo>();

            public StoredData()
            {
            }
        }

        class PlayerInfo
        {
            public string Name;
            public bool WelcomeBonus;

            public PlayerInfo()
            {
            }

            public PlayerInfo(BasePlayer player)
            {
                Name = player.displayName;
                WelcomeBonus = true;
            }
        }

        #endregion

        #region General Setup/Cleanup

        [PluginReference] Plugin Economics;
        [PluginReference] Plugin Reconomy;
        readonly Dictionary<ulong, Timer> payTimer = new Dictionary<ulong, Timer>();

        void Loaded()
        {
            LoadDefaultConfig();
            storedData = Interface.Oxide.DataFileSystem.ReadObject<StoredData>(Name);

            if (!Economics && !Reconomy) PrintWarning("No economy plugins found, plugin disabled");

            foreach (var player in BasePlayer.activePlayerList)
                payTimer[player.userID] = timer.Repeat(PayoutInterval, 0, () => Payout(player, BasePayout, ReceivedForPlaying));
        }

        void Unload()
        {
            foreach (var player in BasePlayer.activePlayerList) payTimer[player.userID].Destroy();
        }

        #endregion

        void Payout(BasePlayer player, double amount, string message)
        {
            if (!Economics && !Reconomy) return;

            Economics?.Call("Deposit", player.userID, amount);
            Reconomy?.Call("Deposit", player.userID, amount);

            PrintToChat(player, message.Replace("{amount}", amount.ToString()));
        }

        #region Welcome Bonus/Repeat

        void OnPlayerInit(BasePlayer player)
        {
            if (WelcomeBonus > 0f && !storedData.Players.ContainsKey(player.userID))
            {
                var info = new PlayerInfo(player);
                storedData.Players.Add(player.userID, info);
                Interface.Oxide.DataFileSystem.WriteObject(Name, storedData);

                Payout(player, WelcomeBonus, ReceivedWelcomeBonus);
            }

            payTimer[player.userID] = timer.Repeat(PayoutInterval, 0, () => Payout(player, BasePayout, ReceivedForPlaying));
        }

        #endregion

        #region Time Alive Bonus

        readonly FieldInfo previousLifeStory = typeof(BasePlayer).GetField("previousLifeStory", BindingFlags.NonPublic | BindingFlags.Instance);

        void OnPlayerRespawned(BasePlayer player)
        {
            if (!TimeAliveBonus) return;

            var lifeStory = (PlayerLifeStory)previousLifeStory?.GetValue(player);
            if (lifeStory == null) return;

            var secondsAlive = lifeStory.timeDied - lifeStory.timeBorn;
            const int math = 31 * 24 * 60 * 60;
            var amount = secondsAlive * TimeAliveMultiplier * ((1 - secondsAlive / math) + 2 * secondsAlive / math);
            var timeSpan = TimeSpan.FromSeconds(secondsAlive);
            var time = $"{timeSpan.TotalHours:00}h {timeSpan.Minutes:00}m {timeSpan.Seconds:00}s".TrimStart(' ', 'd', 'h', 'm', 's', '0');

            Payout(player, amount, ReceivedForTimeAlive.Replace("{time}", time));
        }

        void OnPlayerDisconnected(BasePlayer player) => payTimer[player.userID].Destroy();

        #endregion

        #region Helper Methods

        T GetConfig<T>(string name, T defaultValue)
        {
            if (Config[name] == null) return defaultValue;
            return (T) Convert.ChangeType(Config[name], typeof(T));
        }

        #endregion
    }
}
