using Modding;
using RandomizerMod.RC;
using RandomizerMod.Menu;
using RandomizerMod.Logging;
using RandomizerMod.RandomizerData;
using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using MenuChanger.Extensions;

namespace RandoZoomZoom
{
    public class RandoZoomZoom : Mod, IGlobalSettings<RandoSettings>
    {
        public override string GetVersion() => "1.0";

        private RandoSettings Settings = new();

        public void OnLoadGlobal(RandoSettings rs)
        {
            Settings = rs;
        }

        public RandoSettings OnSaveGlobal() => Settings;

        public override void Initialize()
        {
            RequestBuilder.OnUpdate.Subscribe(6f, GiveZoomZoomCharms);
            RequestBuilder.OnUpdate.Subscribe(-9999f, MakeZoomZoomCharmsFree);
            RandomizerMenuAPI.AddMenuPage(BuildMenu, BuildButton);
            SettingsLog.AfterLogSettings += LogRandoSettings;
        }

        private void GiveZoomZoomCharms(RequestBuilder rb)
        {
            if (Settings.GoFast)
            {
                foreach (var charm in new string[] {"Dashmaster", "Sprintmaster"})
                {
                    // In case rando already gave these charms as starting items,
                    // we don't want to add another copy
                    rb.RemoveFromStart(charm);
                    rb.AddToStart(charm);
                    rb.RemoveItemByName(charm);
                    rb.EditItemRequest(charm, info =>
                    {
                        var orig = info.realItemCreator;
                        info.realItemCreator = (factory, placement) =>
                        {
                            var item = orig != null ? orig(factory, placement) : factory.MakeItem(charm);
                            item.GetOrAddTag<ItemChanger.Tags.EquipCharmOnGiveTag>();
                            return item;
                        };
                    });
                }
            }
        }

        private void MakeZoomZoomCharmsFree(RequestBuilder rb)
        {
            // index within the array is one less than the charm number
            const int Dashmaster = 31 - 1;
            const int Sprintmaster = 37 - 1;

            int PickAnotherCharm()
            {
                var i = -0xDEAD;
                do
                {
                    i = rb.rng.Next(40);
                }
                while (i == Dashmaster || i == Sprintmaster);
                return i;
            }

            foreach (var i in new int[] {Dashmaster, Sprintmaster})
            {
                // Transfer any notches given to these charms to other charms.
                while (rb.ctx.notchCosts[i] > 0)
                {
                    var j = PickAnotherCharm();
                    rb.ctx.notchCosts[i]--;
                    rb.ctx.notchCosts[j]++;
                }
            }
        }

        private MenuPage SettingsPage;

        private void BuildMenu(MenuPage landingPage)
        {
            SettingsPage = new MenuPage(GetName(), landingPage);
            var factory = new MenuElementFactory<RandoSettings>(SettingsPage, Settings);
            new VerticalItemPanel(SettingsPage, new(0, 300), 75f, true, factory.Elements);
        }

        private bool BuildButton(MenuPage landingPage, out SmallButton settingsButton)
        {
            settingsButton = new(landingPage, GetName());
            settingsButton.AddHideAndShowEvent(landingPage, SettingsPage);
            return true;
        }

        private void LogRandoSettings(LogArguments args, TextWriter w)
        {
            w.WriteLine("Logging RandoZoomZoom settings:");
            w.WriteLine(JsonUtil.Serialize(Settings));
        }
    }
}
