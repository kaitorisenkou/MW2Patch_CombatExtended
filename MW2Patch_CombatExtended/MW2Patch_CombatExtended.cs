using CombatExtended;
using HarmonyLib;
using ModularWeapons2;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
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

            Log.Message("[MW2Patch_CE] Harmony patch complete.");

            MW2Mod.CEBreakPoint_PostOpenGunsmith += MW2CE_BreakpointActions.PostOpenGunsmith;
            MW2Mod.CEBreakPoint_RefleshParts += MW2CE_BreakpointActions.RefleshParts;
            Log.Message("[MW2Patch_CE] Breakpoints initalized.");
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
    }
}
