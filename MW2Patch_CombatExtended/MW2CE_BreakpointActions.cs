using ModularWeapons2;
using Verse;

namespace MW2Patch_CombatExtended {
    public static class MW2CE_BreakpointActions {
        public static void PostOpenGunsmith(Thing thing) {
            /* What to code here is:
             *   Unload magazine 
             *     then drop ammo on ground or put ammo to inventory
             *   
             *   If using underbarrel, switch to main gun
             *   
             *   DO NOT FORGET make both(B and UB) guns unloaded
             *   
             */
        }
        public static void RefleshParts(CompModularWeapon comp) {
            /* What to code here is:
             *   Make following comp affected by CompModularWeapons;
             *     CompAmmoUser
             *     BipodComp
             *     CompFireModes
             *     CompUnderbarrel
             */
        }
    }
}
