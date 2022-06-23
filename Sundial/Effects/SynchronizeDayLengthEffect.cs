using AetharNet.Moonbow.Experimental.Utilities;
using Plukit.Base;
using Staxel.Core;
using Staxel.Draw;
using Staxel.Effects;
using Staxel.Logic;
using Staxel.Rendering;

namespace AetharNet.Sundial.Effects
{
    public class SynchronizeDayLengthEffect : IEffect
    {
        public SynchronizeDayLengthEffect(Blob data)
        {
            var entityId = data.GetLong("entityId");
            if (GameUtilities.ClientMainLoop?.Avatar()?.Id.Id != entityId) return;

            var secondsPerDay = data.GetDouble("secondsPerDay");
            Constants.SecondsPerIngameDay = secondsPerDay;
            
            var minutes = (secondsPerDay / 60).ToString("F2");
            ClientMessaging.WriteTranslation("mods.Sundial.effects.SynchronizeDayLengthEffect.synchronized", new object[]{minutes});
        }
        
        public void Dispose() {}

        public bool Completed() => true;

        public void Render(
            Entity entity,
            EntityPainter painter,
            Timestep renderTimestep,
            DeviceContext graphics,
            ref Matrix4F matrix,
            Vector3D renderOrigin,
            Vector3D position,
            RenderMode renderMode) {}

        public void Stop() {}

        public void Pause() {}

        public void Resume() {}
    }
}