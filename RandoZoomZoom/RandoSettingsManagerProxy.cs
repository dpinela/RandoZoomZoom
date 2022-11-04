using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace RandoZoomZoom
{
    internal class RandoSettingsManagerProxy : RandoSettingsProxy<RandoSettings, Signature>
    {
        internal Func<RandoSettings> getter;
        internal Action<RandoSettings> setter;

        public override string ModKey => nameof(RandoZoomZoom);

        public override VersioningPolicy<Signature> VersioningPolicy => new StructuralVersioningPolicy() { settingsGetter = this.getter };

        public override bool TryProvideSettings(out RandoSettings? sent)
        {
            sent = getter();
            return true;
        }

        public override void ReceiveSettings(RandoSettings? received)
        {
            setter(received ?? new());
        }
    }
}