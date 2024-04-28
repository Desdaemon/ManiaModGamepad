// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mods;
using HarmonyLib;

namespace osu.Game.Rulesets.Mania
{

    [HarmonyPatch(typeof(ManiaRuleset), "GetModsFor")]
    public static class ManiaRulesetGetModsForPatch
    {
        public static void Postfix(ref IEnumerable<Mod> __result, ModType type)
        {
            if (type is not ModType.Conversion) return;

            var ans = __result.ToArray();
            for (int i = 0; i < ans.Length; i++)
            {
                if (ans[i] is MultiMod) // key mods
                {
                    __result = ans[..i].Append(new Mods.ManiaModGamepad()).Concat(ans[i..]);
                    return;
                }
            }
            __result = [new Mods.ManiaModGamepad(), .. ans];
        }
    }
}
