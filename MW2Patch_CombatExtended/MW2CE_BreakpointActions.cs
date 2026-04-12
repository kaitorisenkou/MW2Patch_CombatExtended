using CombatExtended;
using ModularWeapons2;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
            var compAmmo = thing.TryGetComp<CompAmmoUser>();
            var compUB = thing.TryGetComp<CompUnderBarrel>();
            if (compAmmo != null) {
                if (compUB != null && compUB.Props?.propsUnderBarrel != null) {
                    if (!compUB.usingUnderBarrel) {
                        compUB.SwitchToUB();
                    }
                    compAmmo.TryUnload(true);
                    compUB.SwithToB();
                }
                compAmmo.TryUnload(true);
            }
        }
        public static void RefleshParts(CompModularWeapon comp) {
            /* What to code here is:
             *   Make following comp affected by CompModularWeapons;
             *     BipodComp
             *     CompFireModes
             *     CompAmmoUser
             *     CompUnderbarrel
             *     
             *   ** Done, need debug **
             */

            var thing = comp.parent;

            //CompAmmoUser
            var newProp_AmmoUser = comp.GetCompProps_AmmoUser();
            if (newProp_AmmoUser != null) {
                /*compAmmo.props = newProp_AmmoUser;
                if (!compAmmo.CurrentAmmo.AmmoSetDefs.Contains(newProp_AmmoUser.ammoSet)) {
                    compAmmo.CurrentAmmo = newProp_AmmoUser.ammoSet.ammoTypes[0].ammo;
                }*/
                var compAmmo = thing.TryGetComp<CompAmmoUser>();
                compAmmo.CurrentAmmo = null;
                compAmmo.SelectedAmmo = null;
                compAmmo.Initialize(newProp_AmmoUser);
            }

            //CompFireModes
            var newProp_FireModes = comp.GetCompProps_FireModes();
            if(newProp_FireModes != null) {
                var compFMode = thing.TryGetComp<CompFireModes>();
                compFMode.Initialize(newProp_FireModes);
            }

            //BipodComp
            var compBipod = thing.TryGetComp<BipodComp>();
            if (compBipod != null) {
                var bipodProp = compBipod.Props;
                bipodProp.catDef = GetBipodCatDef(comp);
                bipodProp.swayMult = bipodProp.catDef.swayMult;
                bipodProp.swayPenalty = bipodProp.catDef.swayPenalty;
                bipodProp.additionalrange = bipodProp.catDef.ad_Range;
                bipodProp.recoilMulton = bipodProp.catDef.recoil_mult_setup;
                bipodProp.recoilMultoff = bipodProp.catDef.recoil_mult_NOT_setup;
                bipodProp.ticksToSetUp = bipodProp.catDef.setuptime;
                bipodProp.warmupMult = bipodProp.catDef.warmup_mult_setup;
                bipodProp.warmupPenalty = bipodProp.catDef.warmup_mult_NOT_setup;
            }

            //CompUnderBarrel
            var compUB = thing.TryGetComp<CompUnderBarrel>();
            if (compUB != null) {
                compUB._defVerbProps = null;
                compUB._compPropsAmmo = newProp_AmmoUser;
                compUB._compPropsFireModes = newProp_FireModes;
                compUB.props = comp.GetCompProps_UnderBarrel();
            }
        }



        public static CompProperties_AmmoUser GetCompProps_AmmoUser(this CompModularWeapon compMW) {
            var thing = compMW.parent;
            var baseProp = thing?.def.comps?.First(t => t.GetType() == typeof(CompProperties_AmmoUser)) as CompProperties_AmmoUser;
            if (baseProp == null) return null;
            var modExs = compMW.GetAllModExtensionsFromParts<MW2PartsExtension_CEAmmoUser>();
            if (modExs.EnumerableNullOrEmpty()) {
                return baseProp;
            }
            var result = new CompProperties_AmmoUser();
            //変更する奴
            result.magazineSize = baseProp.magazineSize + modExs.Select(t => t.magazineSizeOffset).Sum();
            result.reloadTime =
                (baseProp.reloadTime + modExs.Select(t => t.reloadTimeOffset).Sum())
                * modExs.Select(t => t.reloadTimeFactor).Sum();
            result.ammoSet =
                modExs.FirstOrFallback(t => t.ammoSetOverride != null)?.ammoSetOverride
                ?? baseProp.ammoSet;
            //元のままのやつ
            result.AmmoGenPerMagOverride = baseProp.AmmoGenPerMagOverride;
            result.reloadOneAtATime = baseProp.reloadOneAtATime;
            result.throwMote = baseProp.throwMote;
            result.loadedAmmoBulkFactor = baseProp.loadedAmmoBulkFactor;

            return result;
        }
        public static CompProperties_FireModes GetCompProps_FireModes(this CompModularWeapon compMW) {
            var thing = compMW.parent;
            var baseProp = thing?.def.comps?.First(t => t.GetType() == typeof(CompProperties_FireModes)) as CompProperties_FireModes;
            if (baseProp == null) return null;
            var modExs = compMW.GetAllModExtensionsFromParts<MW2PartsExtension_CEFireModes>();
            if (modExs.EnumerableNullOrEmpty()) {
                return baseProp;
            }
            var result = new CompProperties_FireModes();

            result.aimedBurstShotCount = baseProp.aimedBurstShotCount + modExs.Sum(t => t.aimedBurstShotCountOffset);

            var ext_aiUsesBurstModeOffset = modExs.Sum(t => t.aiUsesBurstModeOffset);
            result.aiUseBurstMode =
                ext_aiUsesBurstModeOffset > 0 ? true :
                ext_aiUsesBurstModeOffset < 0 ? false :
                baseProp.aiUseBurstMode;

            var ext_useSingleShotOffset = modExs.Sum(t => t.useSingleShotOffset);
            result.noSingleShot =
                ext_useSingleShotOffset > 0 ? false :
                ext_useSingleShotOffset < 0 ? true :
                baseProp.noSingleShot;

            var ext_useSnapshotOffset = modExs.Sum(t => t.useSnapshotOffset);
            result.noSnapshot =
                ext_useSnapshotOffset > 0 ? false :
                ext_useSnapshotOffset < 0 ? true :
                baseProp.noSnapshot;

            result.aiAimMode = baseProp.aiAimMode;

            return result;
        }
        public static CompProperties_UnderBarrel GetCompProps_UnderBarrel(this CompModularWeapon compMW) {
            var result =
                compMW.GetFirstModExtensionsFromParts<MW2PartsExtension_CEUnderbarrel>()?.underBarrelProps ??
                compMW.parent.def.comps.First(t => t.GetType() == typeof(CompProperties_UnderBarrel));
            return result as CompProperties_UnderBarrel;
        }
        public static BipodCategoryDef GetBipodCatDef(this CompModularWeapon compMW) {
            var thing = compMW.parent;
            var baseProp = thing?.def.comps?.First(t => t.GetType() == typeof(CompProperties_BipodComp)) as CompProperties_BipodComp;
            if (baseProp == null) return MW2CE_DefOf.MW2CE_noBipod;
            var modExt = compMW.GetFirstModExtensionsFromParts<MW2PartsExtension_CEBipod>();
            return modExt?.categoryDef ?? baseProp.catDef;
        }
    }
}
