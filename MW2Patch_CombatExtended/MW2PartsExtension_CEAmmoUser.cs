using CombatExtended;
using ModularWeapons2;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace MW2Patch_CombatExtended {
    public class MW2PartsExtension_CEAmmoUser : DefModExtension, IPartsModExtensionStatChangeText {
        public AmmoSetDef ammoSetOverride = null;
        public int magazineSizeOffset = 0;
        public float magazineSizeMultiplier = 1.0f;
        public float reloadTimeOffset = 0;
        public float reloadTimeFactor = 1.0f;


        public IEnumerable<(TaggedString text, int priority)> Texts {
            get {
                var builder = new StringBuilder();
                if (ammoSetOverride != null) {
                    builder.Append("MW2_ChangeCaliber".Translate());
                    builder.Append(ammoSetOverride.label);
                    yield return (builder.ToString(), 5500);
                    builder.Clear();
                }
                if (magazineSizeOffset != 0) {
                    builder.Append(CE_StatDefOf.MagazineCapacity.label);
                    builder.Append(" ");
                    if (magazineSizeOffset > 0) {
                        builder.Append("<color=\"green\">+");
                    } else {
                        builder.Append("<color=\"red\">");
                    }
                    builder.Append(magazineSizeOffset.ToString());
                    builder.Append("</color>");
                    yield return (builder.ToString(), CE_StatDefOf.MagazineCapacity.displayPriorityInCategory);
                    builder.Clear();
                }
                if (Math.Abs(magazineSizeMultiplier) > 0.0001f) {
                    builder.Append(CE_StatDefOf.MagazineCapacity.label);
                    builder.Append(" ");
                    if (magazineSizeOffset > 0) {
                        builder.Append("<color=\"green\">+");
                    } else {
                        builder.Append("<color=\"red\">");
                    }
                    builder.Append((magazineSizeOffset * 100f).ToString());
                    builder.Append("%</color>");
                    yield return (builder.ToString(), CE_StatDefOf.MagazineCapacity.displayPriorityInCategory);
                    builder.Clear();
                }
                if (reloadTimeOffset != 0) {
                    builder.Append(CE_StatDefOf.MagazineCapacity.label);
                    builder.Append(" ");
                    if (reloadTimeOffset > 0) {
                        builder.Append("<color=\"red\">+");
                    } else {
                        builder.Append("<color=\"green\">");
                    }
                    builder.Append(reloadTimeOffset.ToString());
                    builder.Append("</color>");
                    yield return (builder.ToString(), CE_StatDefOf.MagazineCapacity.displayPriorityInCategory);
                    builder.Clear();
                }
                if (Math.Abs(reloadTimeFactor) > 0.0001f) {
                    builder.Append(CE_StatDefOf.MagazineCapacity.label);
                    builder.Append(" ");
                    if (reloadTimeFactor > 0) {
                        builder.Append("<color=\"red\">+");
                    } else {
                        builder.Append("<color=\"green\">");
                    }
                    builder.Append((reloadTimeFactor * 100f).ToString());
                    builder.Append("%</color>");
                    yield return (builder.ToString(), CE_StatDefOf.MagazineCapacity.displayPriorityInCategory);
                    builder.Clear();
                }
            }
        }
    }
}
