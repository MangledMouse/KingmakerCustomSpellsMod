using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
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
            //UpdateInfectiousCharmsDiscovery();
            //UpdateLoreShmanCapstone();
        }

        public static void FixDomainsAndSpellLists()
        {
            Main.logger.Log("Reached fix domain spells");
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("b5c056787d1bf544588ec3a150ed0b3b"), NewSpells.suggestion, 3); //charm domain replacement
            NewSpells.suggestion_mass.AddToSpellList(library.Get<BlueprintSpellList>("b9aacf55018e41aea0ce204f235aa883"), 5); //psychic detective spell list
            NewSpells.suggestion_mass.AddToSpellList(library.Get<BlueprintSpellList>("422490cf62744e16a3e131efd94cf290"), 6); // witch spell list
            NewSpells.suggestion.AddToSpellList(library.Get<BlueprintSpellList>("422490cf62744e16a3e131efd94cf290"), 3); // witch spell list
            NewSpells.suggestion.AddToSpellList(library.Get<BlueprintSpellList>("b9aacf55018e41aea0ce204f235aa883"), 2); //psychic detective spell list
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

        private static void UpdateInfectiousCharmsDiscovery()
        {
            BlueprintFeature infectious_charms_feature = library.Get<BlueprintFeature>("1a4bd53d5b8848d9a717084d7f9dfa0c");//the feature
            BlueprintAbility infectious_charms_base = library.Get<BlueprintAbility>("f4e6208e0cbf4807bd023f168dd1f616");//the base buff

            var spells = new BlueprintAbility[]
           {
                NewSpells.suggestion
           };

            var swift_abilites = new List<BlueprintAbility>();
            foreach (var spell in spells)
            {
                var buff = Helpers.CreateBuff("InfectiousCharms" + spell.name + "Buff",
                                              infectious_charms_feature.Name,
                                              infectious_charms_feature.Description,
                                              "",
                                              null,
                                              null);

                var apply_buff = Common.createContextActionApplyBuffToCaster(buff, Helpers.CreateContextDuration(1), dispellable: false);

                var swift_ability = library.CopyAndAdd<BlueprintAbility>(spell, "InfectiousCharms" + spell.name, "");
                swift_ability.ActionType = UnitCommand.CommandType.Swift;
                swift_ability.AddComponent(Common.createAbilityCasterHasFacts(buff));
                swift_ability.RemoveComponents<AbilityDeliverTouch>();

                swift_ability.Range = AbilityRange.Close;


                var remove_buff = Common.createAbilityExecuteActionOnCast(Helpers.CreateActionList(Common.createContextActionOnContextCaster(Helpers.Create<ContextActionRemoveBuff>(r => r.Buff = buff))));
                swift_ability.AddComponent(remove_buff);

                bool found = false;

                var new_actions = spell.GetComponent<AbilityEffectRunAction>().Actions.Actions;

                new_actions = Common.changeAction<ContextActionConditionalSaved>(new_actions,
                                                                                 c =>
                                                                                 {
                                                                                     c.Failed = Helpers.CreateActionList(c.Failed.Actions.AddToArray(apply_buff));
                                                                                     found = true;
                                                                                 });
                if (!found)
                {
                    new_actions = new_actions.AddToArray(apply_buff);
                }

                spell.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(new_actions));

                buff.AddComponent(Helpers.Create<ReplaceAbilityParamsWithContext>(r => r.Ability = swift_ability));
                swift_ability.Parent = infectious_charms_base;
                swift_abilites.Add(swift_ability);
            }

            List<BlueprintComponent> components = new List<BlueprintComponent>();
            components.Add(Helpers.CreateAbilityVariants(infectious_charms_base, swift_abilites.ToArray()));
            infectious_charms_base.ReplaceComponent<AbilityVariants>(a => a.Variants = a.Variants.AddToArray(swift_abilites.ToArray()));
        }

        private static void UpdateLoreShmanCapstone()
        {

        }
    }
}
