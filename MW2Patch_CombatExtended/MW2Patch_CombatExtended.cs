using CombatExtended;
using HarmonyLib;
using ModularWeapons2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

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

            Log.Message("[MW2Patch_CE] Harmony patch complete.");

            MW2Mod.CEBreakPoint_PostOpenGunsmith = MW2CE_BreakpointActions.PostOpenGunsmith;
            MW2Mod.CEBreakPoint_RefleshParts = MW2CE_BreakpointActions.RefleshParts;
            Log.Message("[MW2Patch_CE] Breakpoints initalized.");
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

        //TODO: CompUnderbarrelの切替処理がdefだけを参照してるのでそれを是正
    }
}
