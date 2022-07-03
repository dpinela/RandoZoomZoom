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
            RequestBuilder.OnUpdate.Subscribe(22f, RerollDreamers);
            RequestBuilder.OnUpdate.Subscribe(6f, GiveZoomZoomCharms);
            RequestBuilder.OnUpdate.Subscribe(-9999f, MakeZoomZoomCharmsFree);
            RandomizerMenuAPI.AddMenuPage(BuildMenu, BuildButton);
            SettingsLog.AfterLogSettings += LogRandoSettings;
        }

        private void GiveZoomZoomCharms(RequestBuilder rb)
        {
            var givenCharms = new List<string>();
            if (Settings.GoFast)
            {
                givenCharms.Add("Dashmaster");
                givenCharms.Add("Sprintmaster");
            }
            if (Settings.GetRich)
            {
                givenCharms.Add("Gathering_Swarm");
            }
            foreach (var charm in givenCharms)
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

        private void MakeZoomZoomCharmsFree(RequestBuilder rb)
        {
            // index within the array is one less than the charm number
            const int Dashmaster = 31 - 1;
            const int Sprintmaster = 37 - 1;
            const int GatheringSwarm = 1 - 1;

            var givenCharmSet = 0L;
            if (Settings.GoFast)
            {
                givenCharmSet |= 1L << Dashmaster;
                givenCharmSet |= 1L << Sprintmaster;
            }
            if (Settings.GetRich)
            {
                givenCharmSet |= 1L << GatheringSwarm;
            }

            int PickAnotherCharm()
            {
                var i = -0xDEAD;
                do
                {
                    i = rb.rng.Next(40);
                }
                while ((givenCharmSet & (1L << i)) != 0);
                return i;
            }

            for (var i = 0; i <= 40; i++)
            {
                if ((givenCharmSet & (1L << i)) != 0)
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
        }

        private void RerollDreamers(RequestBuilder rb)
        {
            if (Settings.SeeDouble)
            {
                var dreamers = new HashSet<string>() { "Lurien", "Monomon", "Herrah", PlaceholderItem.Prefix + "Dreamer" };
                var replacements = new string[] { "Lurien", "Monomon", "Herrah" };

                var numStarters = 0;
                foreach (var d in new string[] { "Lurien", "Monomon", "Herrah", "Dreamer" })
                {
                    numStarters += rb.StartItems.GetCount(d);
                    rb.RemoveFromStart(d);
                }
                for (var i = 0; i < numStarters; i++)
                {
                    rb.AddToStart(replacements[rb.rng.Next(replacements.Length)]);
                }

                IEnumerable<string> NRandomDreamers(string unused, int n)
                {
                    for (var i = 0; i < n; i++)
                    {
                        yield return replacements[rb.rng.Next(replacements.Length)];
                    }
                }

                rb.ReplaceItem(dreamers.Contains, NRandomDreamers);
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
