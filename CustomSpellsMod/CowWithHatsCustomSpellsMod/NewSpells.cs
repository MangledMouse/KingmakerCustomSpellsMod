﻿using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Buffs.Conditions;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using CallOfTheWild;

namespace CowWithHatsCustomSpellsMod
{
    public class NewSpells
    {
        static LibraryScriptableObject library => Main.library;

        static public BlueprintAbility suggestion;
        static public BlueprintAbility suggestion_mass;

        public static void load()
        {
            createSuggestion();
            //this needs to come after creation of all relevant spells
            ReplaceDomainSpells();
            ReplaceEvangelistSpells();
        }

        public static void createSuggestion()
        {
            var buff = library.Get<BlueprintBuff>("6477ae917b0ec7a4ca76bc9f36b023ac"); //rainbow pattern
            var echolocation = library.Get<BlueprintAbility>("20b548bf09bb3ea4bafea78dcb4f3db6"); //Echolocation
            var hold_monster = library.Get<BlueprintAbility>("41e8a952da7a5c247b3ec1c2dbb73018");
            var checker_fact = hold_monster.GetComponents<AbilityTargetHasNoFactUnless>().ToArray();
            var does_not_work = hold_monster.GetComponent<AbilityTargetHasFact>();
            buff.SetDescription("");
            var apply_buff = CallOfTheWild.Common.createContextActionApplyBuff(buff, CallOfTheWild.Helpers.CreateContextDuration(CallOfTheWild.Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));
            var action = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionHasFacts(false, checker_fact[0].CheckedFacts), Common.createContextConditionCasterHasFact(checker_fact[0].UnlessFact, has: false)),
                                                    null,
                                                    Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionHasFacts(false, checker_fact[1].CheckedFacts), Common.createContextConditionCasterHasFact(checker_fact[1].UnlessFact, has: false)),
                                                                              null,
                                                                                Helpers.CreateConditional(Common.createContextConditionHasFacts(false, does_not_work.CheckedFacts),
                                                                                                        null,
                                                                                                        apply_buff
                                                                                                        )
                                                                             )
                                                    );

            buff.SetIcon(echolocation.Icon);
            buff.SetNameDescription("Suggestion", "This creature is under the compulsive effects of a Suggestion spell. They are lost in thought and they will take no actions until they are harmed.");

            suggestion = CallOfTheWild.Helpers.CreateAbility("SuggestionAbility",
                                      "Suggestion",
                                      "You suggest to a single creature that they should avoid combat and turn their thoughts inward. The spell magically influences the creature to follow the suggestion. They are fascinated by the effect for the duration or until they are harmed.",
                                      "",
                                      echolocation.Icon,
                                      AbilityType.Spell,
                                      UnitCommand.CommandType.Standard,
                                      AbilityRange.Close,
                                      CallOfTheWild.Helpers.minutesPerLevelDuration,
                                      CallOfTheWild.Helpers.willNegates,
                                      CallOfTheWild.Helpers.CreateRunActions(SavingThrowType.Will, CallOfTheWild.Helpers.CreateConditionalSaved(null, apply_buff)),
                                      Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion),
                                      CallOfTheWild.Helpers.CreateSpellComponent(SpellSchool.Enchantment)
                                      );
            suggestion.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            suggestion.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action));
            suggestion.SpellResistance = true;
            suggestion.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Piercing;

            //Creating Suggestion, Mass          

            suggestion_mass = CallOfTheWild.Helpers.CreateAbility("SuggestionMassAbility",
                "Suggestion, Mass",
                "This spell functions like Suggestion, except several creatures may be affected. \n" +suggestion.Description,
                "",
                echolocation.Icon,
                AbilityType.Spell,
                UnitCommand.CommandType.Standard,
                AbilityRange.Medium,
                Helpers.minutesPerLevelDuration,
                Helpers.willNegates,
                Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Enemy),
                CallOfTheWild.Helpers.CreateRunActions(SavingThrowType.Will, CallOfTheWild.Helpers.CreateConditionalSaved(null, apply_buff)),
                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion),
                CallOfTheWild.Helpers.CreateSpellComponent(SpellSchool.Enchantment)
                );
            suggestion_mass.setMiscAbilityParametersRangedDirectional();
            suggestion_mass.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action));
            suggestion_mass.SpellResistance = true;
            suggestion_mass.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Piercing;

            suggestion.AddToSpellList(Helpers.bardSpellList, 2);
            suggestion.AddToSpellList(Helpers.wizardSpellList, 3);
            suggestion.AddToSpellList(library.Get<BlueprintSpellList>("422490cf62744e16a3e131efd94cf290"), 3); // witch spell list
            suggestion.AddToSpellList(library.Get<BlueprintSpellList>("b9aacf55018e41aea0ce204f235aa883"), 2); //psychic detective spell list
            suggestion.AddSpellAndScroll("4d80ff5fde0655a41bf3c8bfa653bfe9"); //scroll of euphoric tranquility

            suggestion_mass.AddToSpellList(Helpers.bardSpellList, 5);
            suggestion_mass.AddToSpellList(Helpers.wizardSpellList, 6);
            suggestion_mass.AddToSpellList(library.Get<BlueprintSpellList>("b9aacf55018e41aea0ce204f235aa883"), 5); //psychic detective spell list
            suggestion_mass.AddToSpellList(library.Get<BlueprintSpellList>("422490cf62744e16a3e131efd94cf290"), 6); // witch spell list
            suggestion_mass.AddSpellAndScroll("4d80ff5fde0655a41bf3c8bfa653bfe9");  //scroll of euphoric tranquility
        }

        public static void ReplaceDomainSpells()
        {
            Common.replaceDomainSpell(library.Get<BlueprintProgression>("b5c056787d1bf544588ec3a150ed0b3b"), suggestion, 3); //charm domain replacement
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
                suggestion,
                CallOfTheWild.NewSpells.command_greater,
                library.Get<BlueprintAbility>("d316d3d94d20c674db2c24d7de96f6a7"), //serenity
                suggestion_mass,
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
