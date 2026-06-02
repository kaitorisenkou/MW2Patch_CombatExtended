using CombatExtended;
using HarmonyLib;
using LudeonTK;
using ModularWeapons2;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using static HarmonyLib.Code;

namespace MW2Patch_CombatExtended {
    [StaticConstructorOnStartup]
    public class MW2Patch_CombatExtended {
        static MW2Patch_CombatExtended() {
            Log.Message("[MW2Patch_CE] Author of MW2 hates CE, so why I'm here???");
            var harmony = new Harmony("kaitorisenkou.ModularWeapons2.Patch_CE");

            harmony.Patch(
                AccessTools.PropertyGetter(typeof(BipodComp), nameof(BipodComp.ShouldSetUp)),
                prefix: new HarmonyMethod(typeof(MW2Patch_CombatExtended), nameof(Prefix_BipodShouldSetUp), null));
            harmony.Patch(
                AccessTools.Method(typeof(BipodComp), nameof(BipodComp.CompGetGizmosExtra)),
                prefix: new HarmonyMethod(typeof(MW2Patch_CombatExtended), nameof(Prefix_BipodCompGetGizmosExtra), null));
            harmony.Patch(
                AccessTools.Method(typeof(StatWorker_BipodDisplay), nameof(StatWorker_BipodDisplay.ShouldShowFor)),
                postfix: new HarmonyMethod(typeof(MW2Patch_CombatExtended), nameof(Postfix_BipodStatShouldShowFor), null));
            harmony.Patch(
                AccessTools.PropertyGetter(typeof(CompUnderBarrel), nameof(CompUnderBarrel.DefVerbProps)),
                prefix: new HarmonyMethod(typeof(MW2Patch_CombatExtended), nameof(Prefix_CompUB_DefVerbProps), null));
            harmony.Patch(
                AccessTools.Method(typeof(ThingWithComps), nameof(ThingWithComps.GetCompByDefType)),
                transpiler: new HarmonyMethod(typeof(MW2Patch_CombatExtended), nameof(Transpiler_GetCompByDefType), null));
            harmony.Patch(
                AccessTools.Method(typeof(CompFireModes), nameof(CompFireModes.Initialize)),
                postfix: new HarmonyMethod(typeof(MW2Patch_CombatExtended), nameof(Postfix_CompFireModeInit), null));
            harmony.Patch(
                AccessTools.Method(typeof(PawnCapacityUtility), nameof(PawnCapacityUtility.CalculateNaturalPartsAverageEfficiency)),
                prefix: new HarmonyMethod(typeof(MW2Patch_CombatExtended), nameof(Prefix_CalculateNaturalPartsAverageEfficiency), null));
            harmony.Patch(
                AccessTools.Method(typeof(StatWorker_Magazine), nameof(StatWorker_Magazine.GetValueUnfinalized)),
                prefix: new HarmonyMethod(typeof(MW2Patch_CombatExtended), nameof(Prefix_MagazineStatValueUnfinalized), null));
            harmony.Patch(
                AccessTools.Method(typeof(StatWorker_Magazine), nameof(StatWorker_Magazine.GetStatDrawEntryLabel)),
                transpiler: new HarmonyMethod(typeof(MW2Patch_CombatExtended), nameof(Transpiler_MagazineStatLabel), null));
            harmony.Patch(
                AccessTools.Method(typeof(StatWorker_Magazine), nameof(StatWorker_Magazine.GetExplanationUnfinalized)),
                transpiler: new HarmonyMethod(typeof(MW2Patch_CombatExtended), nameof(Transpiler_MagazinetExplanation), null));

            /*
#if DEBUG
            harmony.Patch(
                AccessTools.Method(typeof(Verb), "IsStillUsableBy"),
                postfix: new HarmonyMethod(typeof(MW2Patch_CombatExtended), nameof(DebugPostfix_IsStillUsableBy), null));
#endif
            */

            Log.Message("[MW2Patch_CE] Harmony patch complete.");

            MW2Mod.CEBreakPoint_PostOpenGunsmith += MW2CE_BreakpointActions.PostOpenGunsmith;
            MW2Mod.CEBreakPoint_RefleshParts += MW2CE_BreakpointActions.RefleshParts;
            Log.Message("[MW2Patch_CE] Breakpoints initalized.");
            MW2Mod.statDefsForceNonImmutable.AddRange(new StatDef[]
            {
                CE_StatDefOf.MagazineCapacity,
                CE_StatDefOf.ReloadTime,
                CE_StatDefOf.ReloadSpeed
            });
            MW2Mod.lessIsBetter.AddRange(new string[] {
                CE_StatDefOf.Bulk.label.CapitalizeFirst(),
                CE_StatDefOf.Recoil.label.CapitalizeFirst(),
                CE_StatDefOf.ShotSpread.label.CapitalizeFirst(),
                CE_StatDefOf.SwayFactor.label.CapitalizeFirst(),
                CE_StatDefOf.CE_RangedWeapon_RecoilMultiplier.label.CapitalizeFirst()
            });
            MW2Mod.verbTypes_ActivateTacDevice.AddRange(new Type[] {
                typeof(Verb_LaunchProjectileCE)
            });
            CombatExtended.Compatibility.Patches.UsedAmmoCallbacks.Add(UsedAmmoInParts);
            Log.Message("[MW2Patch_CE] Misc initializations complete.");
        }
        public static IEnumerable<ThingDef> UsedAmmoInParts() {
            foreach (var i in DefDatabase<ModularPartsDef>.AllDefsListForReading) {
                var ammoset = i?.GetModExtension<MW2PartsExtension_CEAmmoUser>()?.ammoSetOverride;
                if (ammoset != null){
                    foreach(var j in ammoset.ammoTypes) {
                        yield return j.ammo;
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        public static void DebugLogMessage(object obj) {
            Log.Message(obj);
        }

        [Conditional("DEBUG")]
        public static void DebugLogWarning(object obj) {
            Log.Warning(obj.ToString());
        }

        [Conditional("DEBUG")]
        public static void DebugLogError(object obj) {
            Log.Error(obj.ToString());
        }
#if DEBUG
        [DebugAction("ModularWeapons2", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void EnsureBayonetTool() {
            var def = DefDatabase<ModularPartsDef>.GetNamed("Muzzle_ARBayonet");
            if (def?.effects.tools != null) {
                foreach (var i in def.effects.tools) {
                    DebugLogMessage("[MW2] tool: " + i.label + "(" + i.GetType().ToString() + ")");
                }
            } else {
                DebugLogMessage("[MW2] bayonet def" + def != null ? "found, no tools" : "not found");
            }
        }
#endif

        static bool Prefix_BipodShouldSetUp(ref bool __result, BipodComp __instance) {
            if(__instance!=null && __instance.Props.catDef == MW2CE_DefOf.MW2CE_noBipod) {
                __result = false;
                return false;
            }
            return true;
        }
        static bool Prefix_BipodCompGetGizmosExtra(ref IEnumerable<Gizmo> __result, BipodComp __instance) {
            if (__instance != null && __instance.Props.catDef == MW2CE_DefOf.MW2CE_noBipod) {
                __result = Enumerable.Empty<Gizmo>();
                return false;
            }
            return true;
        }

        static void Postfix_BipodStatShouldShowFor(StatRequest req, ref bool __result) {
            if (!__result)
                return;
            Thing thing = req.Thing;
            if (thing == null) {
                __result =
                    !(req.Def is ThingDef) ||
                    !((ThingDef)req.Def).comps.Any(
                        t => t is CompProperties_BipodComp cpbc &&
                        cpbc.catDef == MW2CE_DefOf.MW2CE_noBipod
                        );
            } else {
                var comp = thing.TryGetComp<BipodComp>();
                __result = comp == null || comp.Props.catDef != MW2CE_DefOf.MW2CE_noBipod;
            }
        }

        static bool Prefix_CompUB_DefVerbProps(ref VerbProperties __result, CompUnderBarrel __instance) {
            if (__instance._defVerbProps != null) {
                return true;
            }
            var compMW = __instance.parent.TryGetComp<CompModularWeapon>();
            if (compMW != null) {
                __instance._defVerbProps = compMW.VerbPropertiesForOverride.FirstOrFallback(null);
            }
            return true;
        }
        static void Postfix_CompFireModeInit(/*CompFireModes __instance, */ref Verb ___verbInt) {
            ___verbInt = null;
        }

        /// <summary>
        /// Rimworld本体のバグ修正
        /// 
        /// if (this.comps[i].props.compClass == def.compClass)
        ///         |
        ///         V
        /// if (this.comps[i].props!=null && this.comps[i].props.compClass == def.compClass)
        /// </summary>
        static IEnumerable<CodeInstruction> Transpiler_GetCompByDefType(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            int patchCount = 0;
            var instructionList = instructions.ToList();
            var targetInfo = AccessTools.Field(typeof(ThingComp), nameof(ThingComp.props));
            for (int i = 0; i < instructionList.Count; i++) {
                if (instructionList[i].opcode == OpCodes.Ldfld && (FieldInfo)instructionList[i].operand == targetInfo) {
                    patchCount++; 
                    DebugLogMessage("[MW2_CE] Transpiler_GetCompByDefType: 1");
                    for (int j = i; j < instructionList.Count; j++) {
                        if (instructionList[j].opcode == OpCodes.Brfalse_S) {
                            var brCode = new CodeInstruction(OpCodes.Brfalse, instructionList[j].operand);
                            patchCount++;
                            DebugLogMessage("[MW2_CE] Transpiler_GetCompByDefType: 2");
                            instructionList.InsertRange(i+1, new CodeInstruction[] {
                                brCode,
                                new CodeInstruction(instructionList[i-4]),
                                new CodeInstruction(instructionList[i-3]),
                                new CodeInstruction(instructionList[i-2]),
                                new CodeInstruction(instructionList[i-1]),
                                new CodeInstruction(instructionList[i])
                                });
                            break;
                        }
                    }
                    break;
                }
            }
            if (patchCount < 2) {
                Log.Error("[MW2_CE]patch failed : Transpiler_GetCompByDefType (patchCount:" + patchCount + ")");
            }
            DebugLogMessage("[MW2_CE] Transpiler_GetCompByDefType done");
            return instructionList;
        }

        static bool Prefix_CalculateNaturalPartsAverageEfficiency(ref float __result, HediffSet diffSet, BodyPartGroupDef bodyPartGroup) {
            CompModularWeapon compMW = diffSet?.pawn?.equipment?.Primary?.TryGetComp<CompModularWeapon>();
            bool flag = compMW?.Tools.Any(t => t.linkedBodyPartsGroup == bodyPartGroup) ?? false;
            bool result;
            if (flag) {
                __result = 1f;
                result = false;
            } else {
                result = true;
            }
            return result;
        }

        static bool Prefix_MagazineStatValueUnfinalized(StatRequest req, ref float __result) {
            var compMW = req.Thing?.TryGetComp<CompModularWeapon>();
            if (compMW == null) {
                return true;
            }
            var ammoUser = req.Thing.TryGetComp<CompAmmoUser>();
            if (ammoUser == null) {
                return true;
            }
            __result = ammoUser.Props.magazineSize;
            return false;
        }
        /// <summary>
        /// result = GetMagSize(optionalReq).ToString() + " / " + compProperties_AmmoUser2.reloadTime...
        ///         |
        ///         V
        /// result = GetMagSize(optionalReq).ToString() + " / " + GetReloadTimeForStatLabel(...
        /// </summary>
        static IEnumerable<CodeInstruction> Transpiler_MagazineStatLabel(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            int patchCount = 0;
            var instructionList = instructions.ToList();
            var targetInfo = AccessTools.Field(typeof(CompProperties_AmmoUser), nameof(CompProperties_AmmoUser.reloadTime));
            for (int i = 0; i < instructionList.Count; i++) {
                if (instructionList[i].opcode == OpCodes.Ldfld && (FieldInfo)instructionList[i].operand == targetInfo) {
                    instructionList.InsertRange(i + 1, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldarg_S,4),
                        new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(MW2Patch_CombatExtended), nameof(GetReloadTimeForStatLabel)))
                    });
                    patchCount++;
                }
                if (patchCount > 100) {
                    Log.Error("[MW2_CE]patch failed : Transpiler_MagazineStatLabel (infinity loop detected)");
                    return instructions;
                }
            }
            if (patchCount < 2) {
                Log.Error("[MW2_CE]patch failed : Transpiler_MagazineStatLabel (patchCount:" + patchCount + ")");
            }
            DebugLogMessage("[MW2_CE] Transpiler_MagazineStatLabel done");
            return instructionList;
        }
        static IEnumerable<CodeInstruction> Transpiler_MagazinetExplanation(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            int patchCount = 0;
            var instructionList = instructions.ToList();
            var targetInfo1 = AccessTools.Field(typeof(CompProperties_AmmoUser), nameof(CompProperties_AmmoUser.magazineSize));
            var targetInfo2 = AccessTools.Field(typeof(CompProperties_AmmoUser), nameof(CompProperties_AmmoUser.reloadTime));
            for (int i = 0; i < instructionList.Count; i++) {
                if (instructionList[i].opcode == OpCodes.Ldfld && (FieldInfo)instructionList[i].operand == targetInfo1) {
                    instructionList.InsertRange(i + 1, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(MW2Patch_CombatExtended), nameof(GetMagSizeForStatLabel)))
                    });
                    patchCount++;
                }
                if (instructionList[i].opcode == OpCodes.Ldfld && (FieldInfo)instructionList[i].operand == targetInfo2) {
                    instructionList.InsertRange(i + 1, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(MW2Patch_CombatExtended), nameof(GetReloadTimeForStatLabel)))
                    });
                    patchCount++;
                }
                if (patchCount > 100) {
                    Log.Error("[MW2_CE]patch failed : Transpiler_MagazineStatLabel (infinity loop detected)");
                    return instructions;
                }
            }
            if (patchCount < 2) {
                Log.Error("[MW2_CE]patch failed : Transpiler_MagazineStatLabel (patchCount:" + patchCount + ")");
            }
            DebugLogMessage("[MW2_CE] Transpiler_MagazineStatLabel done");
            return instructionList;
        }
        public static float GetMagSizeForStatLabel(int defaultValue, StatRequest req) {
            if (req.HasThing) {
                var ammouser = req.Thing.TryGetComp<CompAmmoUser>();
                if (ammouser != null) {
                    return ammouser.Props.magazineSize;
                }
            }
            return defaultValue;
        }
        public static float GetReloadTimeForStatLabel(float defaultValue, StatRequest req) {
            if (req.HasThing) {
                var ammouser = req.Thing.TryGetComp<CompAmmoUser>();
                if (ammouser != null) {
                    return ammouser.Props.reloadTime;
                }
            }
            return defaultValue;
        }

        static void DebugPostfix_IsStillUsableBy(Pawn pawn, Verb __instance, bool __result) {
            if (!__instance.Available()) {
                DebugLogMessage("[MW2CE]not Available()");
            }
            if (!__instance.DirectOwner.VerbsStillUsableBy(pawn)) {
                DebugLogMessage("[MW2CE]not VerbsStillUsableBy(pawn)");
            }
            var dmg = __instance.verbProps.GetDamageFactorFor(__instance, pawn);
            if (dmg == 0f) {
                
            }
            DebugLogMessage("[MW2CE]GetDamageFactorFor(__instance, pawn): " + dmg);
            DebugLogMessage("[MW2CE]HediffCompSource: " + __instance.HediffCompSource?.ToString());
            DebugLogMessage("[MW2CE]AdjustedLinkedBodyPartsGroup: " + __instance.verbProps?.AdjustedLinkedBodyPartsGroup(__instance.tool)?.ToString());
            if (__instance.verbProps.category == VerbCategory.Ignite) {
                DebugLogMessage("[MW2CE]VerbCategory.Ignite");
            }
            if ((__instance.tool as ToolCE)?.restrictedGender != Gender.None) {
                DebugLogMessage("[MW2CE]!= Gender.None");
            }
            if (!__result) {
                DebugLogMessage("[MW2CE]" + __instance.ToString() + " is NOT usable by " + pawn.Label);
            } else {
                DebugLogMessage("[MW2CE]" + __instance.ToString() + " is usable by " + pawn.Label);
            }
        }
    }
}
