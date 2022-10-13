namespace RandoZoomZoom
{
    public class RandoSettings
    {
        public bool GoFast;
        public bool GetRich;

        internal bool Enabled() => GoFast || GetRich;
    }
}