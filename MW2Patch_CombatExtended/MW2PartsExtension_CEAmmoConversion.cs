using CombatExtended;
using Verse;

namespace MW2Patch_CombatExtended {
    public class MW2PartsExtension_CEAmmoUser : DefModExtension {
        public AmmoSetDef ammoSetOverride = null;
        public int magazineSizeOffset = 0;
        public float reloadTimeOffset = 0;
        public float reloadTimeFactor = 1.0f;
    }
}
