using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using HarmonyLib;
using System.Reflection;

namespace osu.Game.Rulesets.ManiaModGamepad
{
    public class ManiaModGamepad : Ruleset
    {

        static ManiaModGamepad()
        {
            var harmony = new Harmony("osu.Game.Rulesets.Mania.Mods.ManiaModGamepad");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override string Description => "ManiaGamepad";

        public override string ShortName => "GP";

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap)
        {
            throw new System.NotImplementedException();
        }

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap)
        {
            throw new System.NotImplementedException();
        }

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null)
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            throw new System.NotImplementedException();
        }
    }
}
    