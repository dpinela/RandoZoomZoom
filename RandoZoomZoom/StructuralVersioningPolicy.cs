using RandoSettingsManager.SettingsManagement.Versioning;

namespace RandoZoomZoom
{
    internal class StructuralVersioningPolicy : VersioningPolicy<Signature>
    {
        internal Func<RandoSettings> settingsGetter;

        public override Signature Version => new() { FeatureSet = FeatureSetForSettings(settingsGetter()) };

        private static List<string> FeatureSetForSettings(RandoSettings rs) =>
            SupportedFeatures.Where(f => f.feature(rs)).Select(f => f.name).ToList();

        public override bool Allow(Signature s) => s.FeatureSet.All(name => SupportedFeatures.Any(sf => sf.name == name));

        private static List<(Predicate<RandoSettings> feature, string name)> SupportedFeatures = new()
        {
            (rs => rs.GoFast, "GoFast"),
            (rs => rs.GetRich, "GetRich")
        };
    }

    internal struct Signature
    {
        public List<string> FeatureSet;
    }
}