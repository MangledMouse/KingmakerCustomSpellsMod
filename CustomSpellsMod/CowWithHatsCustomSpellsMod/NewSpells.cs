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

        public static void createSuggestion()
        {
            var buff = library.Get<BlueprintBuff>("6477ae917b0ec7a4ca76bc9f36b023ac"); //rainbow pattern
            var echolocation = library.Get<BlueprintAbility>("20b548bf09bb3ea4bafea78dcb4f3db6"); //Echolocation
            buff.SetDescription("");
            var apply_buff = CallOfTheWild.Common.createContextActionApplyBuff(buff, CallOfTheWild.Helpers.CreateContextDuration(CallOfTheWild.Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));

            var bloodline_undead_arcana = library.Get<BlueprintFeature>("1a5e7191279e7cd479b17a6ca438498c");
            var check_undead = Helpers.Create<AbilityTargetHasNoFactUnless>(a => { a.CheckedFacts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[] { CallOfTheWild.Common.undead }; a.UnlessFact = bloodline_undead_arcana; });
            var check_intelligent = CallOfTheWild.Common.createAbilityTargetHasFact(true, CallOfTheWild.Common.construct, CallOfTheWild.Common.plant, CallOfTheWild.Common.vermin);

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
                                      Helpers.CreateSpellDescriptor(SpellDescriptor.Compulsion),
                                      CallOfTheWild.Helpers.CreateSpellComponent(SpellSchool.Enchantment),
                                      check_intelligent,
                                      check_undead
                                      );
            suggestion.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting));
            suggestion.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            suggestion.SpellResistance = true;
            suggestion.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Piercing;

            //Creating  Suggestion, Mass
            suggestion_mass = CallOfTheWild.Helpers.CreateAbility("SuggestionMassAbility",
                                     "Suggestion, Mass",
                                     "As Suggestion but can effect more creatures. \r\nSuggestion:You suggest to a single creature that they should avoid combat and turn their thoughts inward. The spell magically influences the creature to follow the suggestion. They are fascinated by the effect for the duration or until they are harmed.",
                                     "",
                                     echolocation.Icon,
                                     AbilityType.Spell,
                                     UnitCommand.CommandType.Standard,
                                     AbilityRange.Medium,
                                     CallOfTheWild.Helpers.minutesPerLevelDuration,
                                     CallOfTheWild.Helpers.willNegates,
                                     Helpers.CreateAbilityTargetsAround(15.Feet(), TargetType.Enemy),
                                     CallOfTheWild.Helpers.CreateRunActions(SavingThrowType.Will, CallOfTheWild.Helpers.CreateConditionalSaved(null, apply_buff)),
                                     Helpers.CreateSpellDescriptor(SpellDescriptor.Compulsion),
                                     CallOfTheWild.Helpers.CreateSpellComponent(SpellSchool.Enchantment),
                                     check_intelligent,
                                     check_undead
                                     );
            suggestion_mass.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.MindAffecting));
            suggestion_mass.SpellResistance = true;
            suggestion_mass.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Piercing;

            suggestion.AddToSpellList(Helpers.bardSpellList, 2);
            suggestion.AddToSpellList(Helpers.wizardSpellList, 3);
            suggestion.AddSpellAndScroll("4d80ff5fde0655a41bf3c8bfa653bfe9"); //scroll of euphoric tranquility

            suggestion_mass.AddToSpellList(Helpers.bardSpellList, 5);
            suggestion_mass.AddToSpellList(Helpers.wizardSpellList, 6);
            suggestion_mass.AddSpellAndScroll("4d80ff5fde0655a41bf3c8bfa653bfe9");  //scroll of euphoric tranquility

        }

    }
}
