using System;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Spiritwalk", "Wulf/lukespragg", "0.2.0")]
    [Description("Leave your body behind, and enter the spirit realm.")]

    class Spiritwalk : RustLegacyPlugin
    {
        // Do NOT edit this file, instead edit the default Spiritwalk.en.json in oxide/lang,
        // or create a language file for another language using the 'en' file as a default.

        #region Localization

        readonly Dictionary<string, string> messages = new Dictionary<string, string>();

        void LoadDefaultMessages()
        {
            messages.Add("ChatCommand", "spirit");
            messages.Add("NoPermission", "Sorry, you can't use 'spirit' right now");
            messages.Add("SpiritFree", "Your spirit has been set free");
            messages.Add("SpiritReturned", "Your spirit has returned to your body");
            lang.RegisterMessages(messages, this);
        }

        #endregion

        #region Initialization

        void Loaded()
        {
            #if !RUSTLEGACY
            throw new NotSupportedException("This plugin does not support this game");
            #endif

            LoadDefaultMessages();
            permission.RegisterPermission("spiritwalk.allowed", this);
            cmd.AddChatCommand(GetMessage("ChatCommand"), this, "SpiritChatCmd");
        }

        #endregion

        #region Chat Command

        #if RUSTLEGACY
        void SpiritChatCmd(NetUser netuser)
        {
            if (!HasPermission(netuser.userID.ToString(), "spiritwalk.allowed"))
            {
                rust.SendChatMessage(netuser, GetMessage("NoPermission", netuser.userID.ToString()));
                return;
            }

            var character = rust.GetCharacter(netuser);
            const int hp = -100;
            if (character.takeDamage.maxHealth < 0 || character.takeDamage.health < 0)
            {
                character.takeDamage.maxHealth = 100;
                character.takeDamage.health = 100;

                rust.Notice(netuser, GetMessage("SpiritReturned", netuser.userID.ToString()));

                return;
            }
            character.takeDamage.maxHealth = 100;
            character.takeDamage.health = hp;

            rust.Notice(netuser, GetMessage("SpiritFree", netuser.userID.ToString()));
        }
        #endif

        #endregion

        #region Helper Methods

        T GetConfig<T>(string name, T defaultValue)
        {
            if (Config[name] == null) return defaultValue;
            return (T) Convert.ChangeType(Config[name], typeof(T));
        }

        string GetMessage(string key, string steamId = null) => lang.GetMessage(key, this, steamId);

        bool HasPermission(string steamId, string perm) => permission.UserHasPermission(steamId, perm);

        #endregion
    }
}
