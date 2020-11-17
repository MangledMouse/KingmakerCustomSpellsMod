using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CowWithHatsCustomSpellsMod
{
    public static class BalanceUpdates
    {
        static LibraryScriptableObject library => Main.library;

        internal static void load()
        {
            RemoveExcessiveLichImmunities();
        }

        private static void RemoveExcessiveLichImmunities()
        {
            //        "Blueprint:efe0344bca1290244a277ed5c45d9ff2:ImmunityToEnergyDrain",
            //"Blueprint:bd9df2d4a4cef274285b8827b6769bde:ImmunityToStun",
            //"Blueprint:c263f44f72df009489409af122b5eefc:ImmunityToSleep",
            //"Blueprint:7e3f3228be49cce49bda37f7901bf246:ImmunityToPoison",
            //            "Blueprint:52f8ef060a751a247964adae7fcb7e64:ImmunityToBleed",
            //            "Blueprint:3eb606c0564d0814ea01a824dbe42fb0:ImmunityToMindAffecting",
            //"Blueprint:ced0f4e5d02d5914a9f9ff74acacf26d:ImmunityToCritical",
            BlueprintUnitFact energyDrainImmunity = library.Get<BlueprintUnitFact>("efe0344bca1290244a277ed5c45d9ff2");
            BlueprintUnitFact stunImmunity = library.Get<BlueprintUnitFact>("bd9df2d4a4cef274285b8827b6769bde");
            BlueprintUnitFact sleepImmunity = library.Get<BlueprintUnitFact>("c263f44f72df009489409af122b5eefc");
            BlueprintUnitFact poisonImmunity = library.Get<BlueprintUnitFact>("7e3f3228be49cce49bda37f7901bf246");
            BlueprintUnitFact bleedImmunity = library.Get<BlueprintUnitFact>("52f8ef060a751a247964adae7fcb7e64");
            BlueprintUnitFact mindAffectingImmunity = library.Get<BlueprintUnitFact>("3eb606c0564d0814ea01a824dbe42fb0");
            //BlueprintUnitFact undeadImmunities = library.Get<BlueprintUnitFact>("8a75eb16bfff86949a4ddcb3dd2f83ae");
            BlueprintUnit lich = library.Get<BlueprintUnit>("d58b4a0df3282b84c97b751590053bcf");
            foreach (BlueprintUnitFact buf in lich.AddFacts)
            {
                Main.logger.Log($"lich facts {buf.name}");
            }
            List<BlueprintUnitFact> lichFacts = new List<BlueprintUnitFact>();
            foreach (BlueprintUnitFact buf in lich.AddFacts)
            {
                if (buf != energyDrainImmunity &&
                    buf != stunImmunity &&
                    buf != sleepImmunity &&
                    buf != poisonImmunity &&
                    buf != bleedImmunity &&
                    buf != mindAffectingImmunity)
                    //&& buf != undeadImmunities)
                    lichFacts.Add(buf);
            }
            lich.AddFacts = lichFacts.ToArray();
            //lich.AddFacts.RemoveFromArray(energyDrainImmunity);
            //lich.AddFacts.RemoveFromArray(stunImmunity);
            //lich.AddFacts.RemoveFromArray(sleepImmunity);
            //lich.AddFacts.RemoveFromArray(poisonImmunity);
            //lich.AddFacts.RemoveFromArray(bleedImmunity);
            //lich.AddFacts.RemoveFromArray(mindAffectingImmunity);
            Main.logger.Log($"     lich facts after change");
            foreach (BlueprintUnitFact buf in lich.AddFacts)
            {
                Main.logger.Log($"lich facts {buf.name}");
            }
        }
    }
}
