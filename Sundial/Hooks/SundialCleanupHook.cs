using AetharNet.Moonbow.Experimental.Templates;
using Staxel.Core;
using Staxel.Modding;

namespace AetharNet.Sundial.Hooks
{
    internal class SundialCleanupHook : ModHookV4Template, IModHookV4
    {
        public override void CleanupOldSession()
        {
            Constants.SecondsPerIngameDay = 1320;
        }
    }
}