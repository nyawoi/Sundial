using System;
using System.IO;
using AetharNet.Moonbow.Experimental.Interfaces;
using AetharNet.Moonbow.Experimental.Templates;
using AetharNet.Moonbow.Experimental.Utilities;
using AetharNet.Sundial.Effects;
using Plukit.Base;
using Staxel;
using Staxel.Core;
using Staxel.Effects;
using Staxel.Logic;

namespace AetharNet.Sundial.Hooks
{
    internal class SundialHook : ServerConnectionHookTemplate, IServerConnectionHook
    {
        public const string ModName = "Sundial";
        private static readonly WorldModDirectoryManager ModDirectoryManager = new(ModName);
        public static bool IsSundialEnabled;
        public static readonly long ModCheckTimeout = 3000;
        public static readonly bool IsModCheckEnabled = true;
        private static readonly Blob ConfigTemplate;

        static SundialHook()
        {
            using var configTemplateStream = File.OpenRead(Path.Combine(GameContext.ContentLoader.RootDirectory, "mods", "Sundial", "config_v2.template"));
            var configTemplateBlob = BlobAllocator.Blob(true);
            configTemplateBlob.LoadJsonStream(configTemplateStream);
            ConfigTemplate = configTemplateBlob;

            if (!ModDirectoryManager.FileExists("config.json")) return;
            
            try
            {
                var config = ModDirectoryManager.ReadFileAsBlob("config.json");

                if (config.Contains("__version__") && config.GetEntryKind("__version__") == BlobEntryKind.Int)
                {
                    switch (config.GetLong("__version__", 1))
                    {
                        case 2:
                            if (config.Contains("Sundial") && config.GetEntryKind("Sundial") == BlobEntryKind.Blob)
                            {
                                IsSundialEnabled = config.GetBlob("Sundial").GetBool("enabled", true);

                                if (IsSundialEnabled)
                                {
                                    var secondsPerDay = config.GetBlob("Sundial").GetDouble("secondsPerDay", Constants.SecondsPerIngameDay);

                                    if (secondsPerDay < 1) throw new Exception("Invalid day length");
                                    
                                    Constants.SecondsPerIngameDay = secondsPerDay;
                                }
                            }

                            if (IsSundialEnabled && config.Contains("ModCheck") && config.GetEntryKind("ModCheck") == BlobEntryKind.Blob)
                            {
                                IsModCheckEnabled = config.GetBlob("ModCheck").GetBool("enabled", true);

                                if (IsModCheckEnabled)
                                {
                                    var timeout = config.GetBlob("ModCheck").GetLong("timeout", 3000);

                                    if (timeout < 1) throw new Exception("Invalid timeout length");
                                
                                    ModCheckTimeout = timeout;
                                }
                            }
                            break;
                        default:
                            throw new Exception("Invalid Sundial config version");
                    }
                }
                else
                {
                    var secondsPerDay = config.GetDouble("secondsPerDay", Constants.SecondsPerIngameDay);

                    if (secondsPerDay < 1) throw new Exception("Invalid day length");
                    
                    IsSundialEnabled = (int) secondsPerDay != (int) Constants.SecondsPerIngameDay;
                    Constants.SecondsPerIngameDay = secondsPerDay;
                }
            }
            catch(Exception ex)
            {
                var exceptionMessage = ex.Message.Replace("json", "JSON").Replace("None", "(Root Object)");
                Logger.WriteLine($"[Sundial] Encountered error while retrieving configuration file: {exceptionMessage}");
            }
        }
        
        public override void OnPlayerConnect(Entity playerEntity)
        {
            if (!IsSundialEnabled) return;

            if (IsModCheckEnabled)
            {
                ModHelper.RequestModCheck(playerEntity, ModName, ModCheckTimeout, isInstalled =>
                {
                    if (isInstalled)
                        SynchronizePlayer(playerEntity);
                    else
                        KickPlayer(playerEntity);
                });
            }
            else
            {
                ServerMessaging.MessagePlayer(playerEntity, "mods.Sundial.hooks.SundialHook.modCheckDisabled", new object[0]);
                SynchronizePlayer(playerEntity);
            }
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

        public static void SaveConfigChanges()
        {
            ConfigTemplate.SetLong("__version__", 2);
            ConfigTemplate.GetBlob("Sundial").SetBool("enabled", IsSundialEnabled);
            ConfigTemplate.GetBlob("Sundial").SetDouble("secondsPerDay", Constants.SecondsPerIngameDay);
            ConfigTemplate.GetBlob("ModCheck").SetBool("enabled", IsModCheckEnabled);
            ConfigTemplate.GetBlob("ModCheck").SetLong("timeout", ModCheckTimeout);
            
            ModDirectoryManager.WriteFileFromBlob("config.json", ConfigTemplate);
        }
    }
}