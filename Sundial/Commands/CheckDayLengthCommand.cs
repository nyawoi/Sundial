using Plukit.Base;
using Staxel.Commands;
using Staxel.Core;
using Staxel.Server;

namespace AetharNet.Sundial.Commands
{
    public class CheckDayLengthCommand : ICommandBuilder
    {
        public string Kind => "checkDayLength";
        
        public string Usage => "mods.Sundial.commands.CheckDayLengthCommand.description";
        
        public bool Public => true;
        
        public string Execute(
            string[] bits,
            Blob blob,
            ClientServerConnection connection,
            ICommandsApi api,
            out object[] responseParams)
        {
            var minutes = Constants.SecondsPerIngameDay / 60;
            responseParams = new object[] {minutes.ToString("F2")};
            return "mods.Sundial.commands.CheckDayLengthCommand.response";
        }
    }
}