using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace CowWithHatsCustomSpellsMod
{
    class ClassUpdates
    {
        static LibraryScriptableObject library => Main.library;

        public static void load()
        {
            FixDomainsAndSpellLists();
            ReplaceEvangelistSpells();
            AddAppropriateSpellsToMindKnifePsychicAccess();
        }

        public static void AddAppropriateSpellsToMindKnifePsychicAccess()
        {
            //            PsychicAccess1ParametrizedFeature bf5b8dc7938d4e6c881ea9b585d22141    Kingmaker.Blueprints.Classes.Selection.BlueprintParametrizedFeature
            //PsychicAccess2ParametrizedFeature   650cfc1211e84ccea06e4471f6e55cf4 Kingmaker.Blueprints.Classes.Selection.BlueprintParametrizedFeature
            //PsychicAccess3ParametrizedFeature   03824a2bfc35414da5dc60721549ee17 Kingmaker.Blueprints.Classes.Selection.BlueprintParametrizedFeature
            //PsychicAccess4ParametrizedFeature   9494f83a9e6c48cc8886bf3352ac60ec Kingmaker.Blueprints.Classes.Selection.BlueprintParametrizedFeature
            //PsychicAccess5ParametrizedFeature   215b8ce7010143f5bceb27ebd8ad5117 Kingmaker.Blueprints.Classes.Selection.BlueprintParametrizedFeature
            //PsychicAccess6ParametrizedFeature   52a471ba48964f5ca52a375c9efd1040 Kingmaker.Blueprints.Classes.Selection.BlueprintParametrizedFeature
            var mindbladeList = CallOfTheWild.Archetypes.MindBlade.extra_psychic_spell_list;
            NewSpells.glue_seal.AddToSpellList(mindbladeList, 1);
            NewSpells.heightened_awareness.AddToSpellList(mindbladeList, 1);

            NewSpells.suggestion.AddToSpellList(mindbladeList, 2);
            NewSpells.euphoric_cloud.AddToSpellList(mindbladeList, 2);
            NewSpells.acute_senses.AddToSpellList(mindbladeList, 2);

            NewSpells.suggestion_mass.AddToSpellList(mindbladeList, 5);

            //var mindbladePsychicSpellAccessFeature1 = library.Get<BlueprintParametrizedFeature>("bf5b8dc7938d4e6c881ea9b585d22141");
            //NewSpells.glue_seal.AddToSpellList(mindbladePsychicSpellAccessFeature1.SpellList, 1);
            //NewSpells.heightened_awareness.AddToSpellList(mindbladePsychicSpellAccessFeature1.SpellList, 1);

            //var mindbladePsychicSpellAccessFeature2 = library.Get<BlueprintParametrizedFeature>("650cfc1211e84ccea06e4471f6e55cf4");
            //NewSpells.suggestion.AddToSpellList(mindbladePsychicSpellAccessFeature2.SpellList, 2);
            //NewSpells.acute_senses.AddToSpellList(mindbladePsychicSpellAccessFeature2.SpellList, 2);
            //NewSpells.euphoric_cloud.AddToSpellList(mindbladePsychicSpellAccessFeature2.SpellList, 2);

            //var mindbladePsychicSpellAccessFeature5 = library.Get<BlueprintParametrizedFeature>("215b8ce7010143f5bceb27ebd8ad5117");
            //NewSpells.suggestion_mass.AddToSpellList(mindbladePsychicSpellAccessFeature5.SpellList, 5);
        }

        public static void FixDomainsAndSpellLists()
        {
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("b5c056787d1bf544588ec3a150ed0b3b"), NewSpells.suggestion, 3); //charm domain replacement
            
            NewSpells.suggestion_mass.AddToSpellList(library.Get<BlueprintSpellList>("b9aacf55018e41aea0ce204f235aa883"), 5); //psychic detective spell list
            NewSpells.suggestion_mass.AddToSpellList(library.Get<BlueprintSpellList>("422490cf62744e16a3e131efd94cf290"), 6); // witch spell list
            NewSpells.suggestion.AddToSpellList(library.Get<BlueprintSpellList>("422490cf62744e16a3e131efd94cf290"), 3); // witch spell list
            NewSpells.suggestion.AddToSpellList(library.Get<BlueprintSpellList>("b9aacf55018e41aea0ce204f235aa883"), 2); //psychic detective spell list

            NewSpells.glue_seal.AddToSpellList(library.Get<BlueprintSpellList>("972048af37924e59b174653974b255a5"), 1); //Summoner spell list
            NewSpells.glue_seal.AddToSpellList(library.Get<BlueprintSpellList>("b9aacf55018e41aea0ce204f235aa883"), 1); //psychic detective spell list

            NewSpells.heightened_awareness.AddToSpellList(library.Get<BlueprintSpellList>("b9aacf55018e41aea0ce204f235aa883"), 1); // psychic destective spell list

            NewSpells.acute_senses.AddToSpellList(library.Get<BlueprintSpellList>("b9aacf55018e41aea0ce204f235aa883"), 2); //psychic detective spell list

            NewSpells.euphoric_cloud.AddToSpellList(library.Get<BlueprintSpellList>("b9aacf55018e41aea0ce204f235aa883"), 2); //psychic detective spell list
            NewSpells.euphoric_cloud.AddToSpellList(library.Get<BlueprintSpellList>("422490cf62744e16a3e131efd94cf290"), 2); //witch spell list
            NewSpells.euphoric_cloud.AddToSpellList(library.Get<BlueprintSpellList>("b161506e0b8f4116806a243f6838ae01"), 2); //hunter spell list
            
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

            var description = "An evangelist does not gain the ability to spontaneously cast cure or inflict spells by sacrificing prepared spells. However, an evangelist can spontaneously cast the following splls, by sacrificing a prepared spell of the noted level: ";
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
