using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CowWithHatsCustomSpellsMod
{
    class ClassUpdates
    {
        static LibraryScriptableObject library => Main.library;

        public static void load()
        {
            ReplaceDomainSpells();
            ReplaceEvangelistSpells();
        }


        public static void ReplaceDomainSpells()
        {
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("b5c056787d1bf544588ec3a150ed0b3b"), NewSpells.suggestion, 3); //charm domain replacement
        }

        private static void ReplaceEvangelistSpells()
        {
            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            BlueprintFeature spontaneous_casting = library.Get<BlueprintFeature>("48811518d8204f43b991e4dd967a87b8"); //evangelist spontaneous casting feature
            spontaneous_casting.RemoveComponents<SpontaneousSpellConversion>(); //clear all the spontaneous spell conversions that are there

            BlueprintAbility[] spells = new BlueprintAbility[]
            {
                CallOfTheWild.NewSpells.command,
                CallOfTheWild.NewSpells.hypnotic_pattern,
                library.Get<BlueprintAbility>("faabd2cc67efa4646ac58c7bb3e40fcc"), //prayer
                NewSpells.suggestion,
                CallOfTheWild.NewSpells.command_greater,
                library.Get<BlueprintAbility>("d316d3d94d20c674db2c24d7de96f6a7"), //serenity
                NewSpells.suggestion_mass,
                library.Get<BlueprintAbility>("cbf3bafa8375340498b86a3313a11e2f"), //euphoric tranquility
                library.Get<BlueprintAbility>("3c17035ec4717674cae2e841a190e757") //dominate monster
            };

            var description = "An evangelist does not gain the ability to spontaneously cast cure or inflict spells by sacrificing prepared spells.However, an evangelist can spontaneously cast the following splls, by sacrificing a prepared spell of the noted level: ";
            for (int i = 0; i < spells.Length; i++)
            {
                description += spells[i].Name + $" ({i + 1}{Common.getNumExtension(i + 1)})" + ((i + 1) == spells.Length ? "." : ", ");
            }

            var spells_array = Common.createSpelllistsForSpontaneousConversion(spells);

            spontaneous_casting.SetDescription(description);

            for (int i = 0; i < spells_array.Length; i++)
            {
                spontaneous_casting.AddComponent(Common.createSpontaneousSpellConversion(cleric, spells_array[i].ToArray()));
            }
        }
    }
}
