using AetharNet.Moonbow.Experimental.Utilities;
using AetharNet.Sundial.Effects;
using AetharNet.Sundial.Hooks;
using Plukit.Base;
using Staxel.Commands;
using Staxel.Core;
using Staxel.Logic;
using Staxel.Server;

namespace AetharNet.Sundial.Commands
{
    public class SetDayLengthCommand : ICommandBuilder
    {
        public string Kind => "setDayLength";
        
        public string Usage => "mods.Sundial.commands.SetDayLengthCommand.description";
        
        public bool Public => false;
        
        public string Execute(
            string[] bits,
            Blob blob,
            ClientServerConnection connection,
            ICommandsApi api,
            out object[] responseParams)
        {
            if (bits.Length != 2
                || !double.TryParse(bits[1], out var minutes)
                || minutes < 1)
            {
                responseParams = new object[0];
                return "mods.Sundial.commands.SetDayLengthCommand.invalidArgs";
            }

            var players = new Lyst<Entity>();
            api.Facade().GetPlayers(players);

            var data = BlobAllocator.Blob(true);
            data.SetDouble("secondsPerDay", minutes * 60);

            Constants.SecondsPerIngameDay = minutes * 60;
            SundialHook.ModDirectoryManager.WriteFileFromBlob("config.json", data);

            if (!SundialHook.IsEnabled)
            {
                SundialHook.IsEnabled = true;
                foreach (var player in players)
                {
                    ModHelper.RequestModCheck(player, SundialHook.ModName, isInstalled =>
                    {
                        if (isInstalled)
                            SundialHook.SynchronizePlayer(player);
                        else
                            SundialHook.KickPlayer(player);
                    });
                }
            }
            else
            {
                foreach (var player in players)
                {
                    SundialHook.SynchronizePlayer(player);
                }
            }

            responseParams = new object[] {minutes.ToString("F2")};
            return "mods.Sundial.commands.SetDayLengthCommand.success";
        }
    }
}