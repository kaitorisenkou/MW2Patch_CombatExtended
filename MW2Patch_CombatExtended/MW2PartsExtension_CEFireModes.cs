using CombatExtended;
using ModularWeapons2;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace MW2Patch_CombatExtended {
    public class MW2PartsExtension_CEFireModes : DefModExtension, IPartsModExtensionStatChangeText {
        public int aimedBurstShotCountOffset = 0;

        // 0 -> no change
        //+1 -> enable
        //-1 -> disable
        public int aiUsesBurstModeOffset = 0;
        public int useSingleShotOffset = 0;
        public int useSnapshotOffset = 0;

        public IEnumerable<(TaggedString text, int priority)> Texts { get {
                if (aimedBurstShotCountOffset == 0) {
                    yield break;
                }
                var builder = new StringBuilder();
                builder.Append(CE_StatDefOf.BurstShotCount.label);
                builder.Append(" "); 
                if (aimedBurstShotCountOffset > 0) {
                    builder.Append("<color=\"green\">+");
                } else {
                    builder.Append("<color=\"green\">");
                }
                builder.Append(aimedBurstShotCountOffset.ToString());
                builder.Append("</color>");
                yield return (builder.ToString(), CE_StatDefOf.BurstShotCount.displayPriorityInCategory);
            }
        }
    }
}
