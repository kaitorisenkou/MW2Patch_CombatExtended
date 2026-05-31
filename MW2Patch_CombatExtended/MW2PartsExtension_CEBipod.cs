using CombatExtended;
using ModularWeapons2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MW2Patch_CombatExtended {
    public class MW2PartsExtension_CEBipod : DefModExtension, IPartsModExtensionStatChangeText {
        public BipodCategoryDef categoryDef;

        public IEnumerable<(TaggedString text, int priority)> Texts {
            get {
                var builder = new StringBuilder();
                builder.Append(CE_StatDefOf.BipodStats.label);
                builder.Append(": ");
                builder.Append(categoryDef.label);
                yield return (builder.ToString(), CE_StatDefOf.BipodStats.displayPriorityInCategory);
            }
        }
    }
}
