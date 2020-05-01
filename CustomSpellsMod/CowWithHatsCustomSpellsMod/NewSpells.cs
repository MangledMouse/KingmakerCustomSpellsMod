using Kingmaker.Blueprints;
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
        }

        public static void createSuggestion()
        {
            var buff = library.Get<BlueprintBuff>("6477ae917b0ec7a4ca76bc9f36b023ac"); //rainbow pattern
            var echolocation = library.Get<BlueprintAbility>("20b548bf09bb3ea4bafea78dcb4f3db6"); //Echolocation
            var hold_monster = library.Get<BlueprintAbility>("41e8a952da7a5c247b3ec1c2dbb73018");
            var checker_fact = hold_monster.GetComponents<AbilityTargetHasNoFactUnless>().ToArray();
            var does_not_work = hold_monster.GetComponent<AbilityTargetHasFact>();
            buff.SetDescription("");
            var apply_buff = CallOfTheWild.Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));
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

            suggestion = Helpers.CreateAbility("SuggestionAbility",
                                      "Suggestion",
                                      "You suggest to a single creature that they should avoid combat and turn their thoughts inward. The spell magically influences the creature to follow the suggestion. They are fascinated by the effect for the duration or until they are harmed.",
                                      "3682197d956543e2a694e163b5fdcc6c",
                                      echolocation.Icon,
                                      AbilityType.Spell,
                                      UnitCommand.CommandType.Standard,
                                      AbilityRange.Close,
                                      Helpers.minutesPerLevelDuration,
                                      Helpers.willNegates,
                                      Helpers.CreateRunActions(SavingThrowType.Will, Helpers.CreateConditionalSaved(null, apply_buff)),
                                      Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion),
                                      Helpers.CreateSpellComponent(SpellSchool.Enchantment)
                                      );
            suggestion.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            suggestion.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action));
            suggestion.SpellResistance = true;
            suggestion.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Piercing;

            //Creating Suggestion, Mass          

            suggestion_mass = Helpers.CreateAbility("SuggestionMassAbility",
                "Suggestion, Mass",
                "This spell functions like Suggestion, except several creatures may be affected. \n" +suggestion.Description,
                "033e70b6c72f46cab793eab5ff9f3a87",
                echolocation.Icon,
                AbilityType.Spell,
                UnitCommand.CommandType.Standard,
                AbilityRange.Medium,
                Helpers.minutesPerLevelDuration,
                Helpers.willNegates,
                Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Enemy),
                Helpers.CreateRunActions(SavingThrowType.Will, Helpers.CreateConditionalSaved(null, apply_buff)),
                Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion),
                Helpers.CreateSpellComponent(SpellSchool.Enchantment)
                );
            suggestion_mass.setMiscAbilityParametersRangedDirectional();
            suggestion_mass.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(action));
            suggestion_mass.SpellResistance = true;
            suggestion_mass.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Piercing;

            NewSpells.suggestion.AddToSpellList(Helpers.bardSpellList, 2);//bard spell list
            NewSpells.suggestion.AddToSpellList(Helpers.wizardSpellList, 3);//wizard spell list
            NewSpells.suggestion_mass.AddToSpellList(Helpers.bardSpellList, 5);
            NewSpells.suggestion_mass.AddToSpellList(Helpers.wizardSpellList, 6);
            NewSpells.suggestion.AddSpellAndScroll("4d80ff5fde0655a41bf3c8bfa653bfe9"); //scroll of euphoric tranquility
            NewSpells.suggestion_mass.AddSpellAndScroll("4d80ff5fde0655a41bf3c8bfa653bfe9");  //scroll of euphoric tranquility
        }

    }
}
