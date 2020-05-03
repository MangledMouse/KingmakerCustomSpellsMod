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
        static public BlueprintAbility glue_seal;

        public static void load()
        {
            createSuggestion();
            createGlueSeal();
        }

        public static void createGlueSeal()
        {
            //var tanglefootEntangleAddCondition = library.Get<BlueprintBuff>("3642547f895815f4786b7f2ac716e03e").GetComponent<AddCondition>();//guid for the entangle buff 3642547f895815f4786b7f2ac716e03e
            //var tanglefootStuckAddCondition = library.Get<BlueprintBuff>("3642547f895815f4786b7f2ac716e03e").GetComponent<AddCondition>();//guid for the stuck buff 3642547f895815f4786b7f2ac716e03e
            var entangledBuff = library.CopyAndAdd<BlueprintBuff>("a719abac0ea0ce346b401060754cc1c0", "GlueSealEntangledBuff", "bc79b5792c6642ef9e33fdef7e4b63f1"); //Guid for web grappled buff
            var tarpoolBuff = library.Get<BlueprintBuff>("631d255f6b89afe45b32cf66c35a4205"); //TarPoolEntangledBuff guid 

            entangledBuff.SetNameDescriptionIcon("Stuck", "This creature is stuck in glue. It is entangled and cannot move until it breaks out.", tarpoolBuff.Icon); //guid for tarpool entangled buff, looks like tar glue
            entangledBuff.FxOnStart = tarpoolBuff.FxOnStart;
            entangledBuff.FxOnRemove= tarpoolBuff.FxOnRemove;

            var difficult_terrain = library.CopyAndAdd<BlueprintBuff>("525e4ff20086404419b3aab63917d6a0", "GlueSealDifficultTerrainBuff", "1a5e8d1825874231a740f83791905b84"); // guid for tarpool difficult terrain buff 525e4ff20086404419b3aab63917d6a0
            var glueArea = library.CopyAndAdd<BlueprintAbilityAreaEffect>("eca936a9e235875498d1e74ff7c09ecd", "GlueSealArea", "eb78bfdb47154e9fbc14216079328688"); //spike stones area
            glueArea.Size = 5.Feet();
            glueArea.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Ground));
            glueArea.ReplaceComponent<AbilityAreaEffectBuff>(a => a.Buff = difficult_terrain);
            glueArea.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            glueArea.Fx.AssetId = "28b114249fcea5241ab216930ddea100"; // guid for puddle tar 5ft 28b114249fcea5241ab216930ddea100

            var apply_entangled = Common.createContextActionApplyBuff(entangledBuff, Helpers.CreateContextDuration(), is_child: true, is_permanent: true);
            var break_free = Helpers.Create<CallOfTheWild.CombatManeuverMechanics.ContextActionBreakFreeFromSpellGrapple>(c =>
            {
                c.Failure = Helpers.CreateActionList(apply_entangled);
                c.Success = null;
            });
            var area_effect = Helpers.CreateAreaEffectRunAction(unitEnter: Common.createContextActionApplyBuff(difficult_terrain, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false),
                                                    unitExit: Helpers.Create<ContextActionRemoveBuffSingleStack>(r => r.TargetBuff = difficult_terrain),
                                                    unitMove: break_free);
            glueArea.ReplaceComponent<AbilityAreaEffectRunAction>(area_effect);
            var spawn_area = Common.createContextActionSpawnAreaEffect(glueArea, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));

            glue_seal = Helpers.CreateAbility("GlueSealAbility",
                                              "Glue Seal",
                                              "You conjure a layer of sticky glue. Anyone in the area when the spell is cast must attempt a Reflex Save. Those who fail become trapped in the glue. They are entangled and unable to move. They can break free with a combat maneuver or mobility check as a standard action against the DC of this spell. A creature moving through the area must make a mobility check in order to avoid being entangled.",
                                              "227fcc4e1c77404988b3f4ca5ee9ea46",
                                              library.Get<BlueprintAbility>("e48638596c955a74c8a32dbc90b518c1").Icon, //icon for obsidian flow better than anything else I could find
                                              AbilityType.Spell,
                                              Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                              AbilityRange.Close,
                                              Helpers.minutesPerLevelDuration,
                                              library.Get<BlueprintAbility>("f8cea58227f59c64399044a82c9735c4").LocalizedSavingThrow,//f8cea58227f59c64399044a82c9735c4 is the id for chains of light which is reflex negates
                                              Helpers.CreateRunActions(SavingThrowType.Reflex, Helpers.CreateConditionalSaved(null, apply_entangled)),
                                              Helpers.Create<AbilityEffectRunActionOnClickedTarget>(a => a.Action = Helpers.CreateActionList(spawn_area)),
                                              Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                                              Helpers.CreateAbilityTargetsAround(5.Feet(), TargetType.Any)
                                              );
            glue_seal.setMiscAbilityParametersRangedDirectional();

            var new_round_actions = new ActionList();
            var newBreakFreeAction = Helpers.Create<CallOfTheWild.CombatManeuverMechanics.ContextActionBreakFreeFromSpellGrapple>();
            new_round_actions.Actions.AddToArray<GameAction>(newBreakFreeAction);
            var new_actions = Common.replaceActions<ContextActionBreakFree>(new_round_actions.Actions, newBreakFreeAction);
            new_round_actions.Actions = new_actions;

            glue_seal.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Heighten | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;

            glue_seal.AddToSpellList(Helpers.wizardSpellList, 1);
            glue_seal.AddToSpellList(Helpers.bardSpellList, 1);
            glue_seal.AddToSpellList(Helpers.magusSpellList, 1);

            glue_seal.AddSpellAndScroll("de05d46f44c8439488a8bbcc0059c09f"); // scroll of icy guid de05d46f44c8439488a8bbcc0059c09f, looks pretty gluey
            //Main.logger.Log("create glue seal finished");

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
