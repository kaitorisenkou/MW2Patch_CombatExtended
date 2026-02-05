using CombatExtended;
using RimWorld;

namespace MW2Patch_CombatExtended {
    [DefOf]
    public class MW2CE_DefOf {
        static MW2CE_DefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(MW2CE_DefOf));
        }
        public static BipodCategoryDef MW2CE_noBipod;
    }
}
