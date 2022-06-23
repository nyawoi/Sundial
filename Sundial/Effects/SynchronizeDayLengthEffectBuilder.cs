using Plukit.Base;
using Staxel.Effects;
using Staxel.Logic;

namespace AetharNet.Sundial.Effects
{
    public class SynchronizeDayLengthEffectBuilder : IEffectBuilder
    {
        public static string KindCode() => "mods.Sundial.effects.SynchronizeDayLengthEffect";
        
        public void Dispose() {}

        public void Load() {}

        public string Kind() => KindCode();

        public IEffect Instance(
            Timestep step,
            Entity entity,
            EntityPainter painter,
            EntityUniverseFacade facade,
            Blob data,
            EffectDefinition definition,
            EffectMode mode)
        {
            return new SynchronizeDayLengthEffect(data);
        }
    }
}