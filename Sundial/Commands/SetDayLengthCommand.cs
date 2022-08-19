using AetharNet.Moonbow.Experimental.Utilities;
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
            
            Constants.SecondsPerIngameDay = minutes * 60;

            if (!SundialHook.IsSundialEnabled)
            {
                SundialHook.IsSundialEnabled = true;

                if (SundialHook.IsModCheckEnabled)
                {
                    foreach (var player in players)
                    {
                        ModHelper.RequestModCheck(player, SundialHook.ModName, SundialHook.ModCheckTimeout, isInstalled =>
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
            }
            else
            {
                foreach (var player in players)
                {
                    SundialHook.SynchronizePlayer(player);
                }
            }
            
            SundialHook.SaveConfigChanges();

            responseParams = new object[] {minutes.ToString("F2")};
            return "mods.Sundial.commands.SetDayLengthCommand.success";
        }
    }
}