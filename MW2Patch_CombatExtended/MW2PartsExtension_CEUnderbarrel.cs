using CombatExtended;
using ModularWeapons2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace MW2Patch_CombatExtended {
    public class MW2PartsExtension_CEUnderbarrel : DefModExtension, IPartsModExtensionStatChangeText {
        /*
        public CompProperties_AmmoUser propsUnderBarrel;
        public VerbPropertiesCE verbPropsUnderBarrel;
        public CompProperties_FireModes propsFireModesUnderBarrel;
        */
        public CompProperties_UnderBarrel underBarrelProps;
        // No effect when < 0
        public float ShotSpreadOverride = -1f;


        static Lazy<StatDef> statDef_ubgl = new Lazy<StatDef>(() => DefDatabase<StatDef>.GetNamed("UBGLInfo"));
        public IEnumerable<(TaggedString text, int priority)> Texts {
            get {
                var builder = new StringBuilder();
                builder.Append(statDef_ubgl.Value.label);
                builder.Append(": ");
                builder.Append(underBarrelProps.propsUnderBarrel.ammoSet.label);
                yield return (builder.ToString(), CE_StatDefOf.BurstShotCount.displayPriorityInCategory);
            }
        }
    }
}
