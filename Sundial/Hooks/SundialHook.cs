using System;
using AetharNet.Moonbow.Experimental.Interfaces;
using AetharNet.Moonbow.Experimental.Templates;
using AetharNet.Moonbow.Experimental.Utilities;
using AetharNet.Sundial.Effects;
using Plukit.Base;
using Staxel.Core;
using Staxel.Effects;
using Staxel.Logic;

namespace AetharNet.Sundial.Hooks
{
    internal class SundialHook : ServerConnectionHookTemplate, IServerConnectionHook
    {
        public const string ModName = "Sundial";
        public static readonly WorldModDirectoryManager ModDirectoryManager = new(ModName);
        public static bool IsEnabled;

        static SundialHook()
        {
            if (!ModDirectoryManager.FileExists("config.json")) return;
            
            try
            {
                var config = ModDirectoryManager.ReadFileAsBlob("config.json");
                var secondsPerDay = config.GetDouble("secondsPerDay");
                Constants.SecondsPerIngameDay = secondsPerDay;
                IsEnabled = true;
            }
            catch(Exception ex)
            {
                var exceptionMessage = ex.Message.Replace("json", "JSON").Replace("None", "(Root Object)");
                Logger.WriteLine($"[Sundial] Encountered error while retrieving configuration file: {exceptionMessage}");
            }
        }
        
        public override void OnPlayerConnect(Entity playerEntity)
        {
            if (!IsEnabled) return;
            
            ModHelper.RequestModCheck(playerEntity, ModName, isInstalled =>
            {
                if (isInstalled)
                    SynchronizePlayer(playerEntity);
                else
                    KickPlayer(playerEntity);
            });
        }

        public static void KickPlayer(Entity playerEntity)
        {
            foreach (var connection in GameUtilities.ServerMainLoop.FetchConnectionsByUid(playerEntity.PlayerEntityLogic.Uid()))
            {
                connection.ErrorAndCloseTranslation("mods.Sundial.hooks.SundialHook.disconnect");
            }
        }

        public static void SynchronizePlayer(Entity playerEntity)
        {
            var data = BlobAllocator.Blob(true); 
            data.SetDouble("secondsPerDay", Constants.SecondsPerIngameDay);
            data.SetLong("entityId", playerEntity.Id.Id);
            playerEntity.Effects.Trigger(new EffectTrigger(SynchronizeDayLengthEffectBuilder.KindCode(), data));
        }
    }
}