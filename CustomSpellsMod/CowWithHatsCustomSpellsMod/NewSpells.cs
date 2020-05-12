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
using Kingmaker.RuleSystem.Rules;

namespace CowWithHatsCustomSpellsMod
{
    public class NewSpells
    {
        static LibraryScriptableObject library => Main.library;

        static public BlueprintAbility suggestion;
        static public BlueprintAbility suggestion_mass;
        static public BlueprintAbility glue_seal;
        static public BlueprintAbility heightened_awareness;
        static public BlueprintAbility acute_senses;
        static public BlueprintAbility mages_disjunction;

        public static void load()
        {
            createSuggestion();
            createGlueSeal();
            createHeightenedAwareness();
            createAcuteSenses();
            createMagesDisjunction();
        }

        public static void createMagesDisjunction()
        {
            // guid for greater dispel magic target 6d490c80598f1d34bb277735b52d52c1
            // guid for greater dispel magic area b9be852b03568064b8d2275a6cf9e2de these two are abilities
            // guid for the spell itself f0f761b808dc4b149b08eaf44b99f633
            //BlueprintAbility greaterTargetDispel = library.Get<BlueprintAbility>("6d490c80598f1d34bb277735b52d52c1");
            //foreach(BlueprintComponent bc in greaterTargetDispel.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log("Component of greater target dispel: " + bc.name + " and type " + bc.GetType().ToString());
            //}
            //above gets a spell component, an ability effect run action, which seems like the stuff, and an ability spawn fx which we probably want to steal
            //AbilityEffectRunAction aera = greaterTargetDispel.GetComponent<AbilityEffectRunAction>();
            //foreach (GameAction ga in aera.Actions.Actions)
            //{
            //    Main.logger.Log("Game action in target dispel's ability effect run action name: " + ga.name + " and type: " + ga.GetType().ToString());
            //    ContextActionDispelMagic cadm = (ContextActionDispelMagic)ga;
            //    if (cadm != null)
            //    {
            //        Main.logger.Log(" context action dispel magic exists");
            //        var stopAfterOne = Helpers.GetField(cadm, "m_StopAfterFirstRemoved");
            //        Main.logger.Log("stops after 1? " + stopAfterOne.ToString());
            //        foreach (GameAction successAction in cadm.OnSuccess.Actions)
            //            Main.logger.Log("game action for on success of dispelling name: " + successAction.name + " and type " + successAction.GetType().ToString());
            //        foreach (GameAction failureAction in cadm.OnFail.Actions)
            //            Main.logger.Log("game action for on failure of dispelling name: " + failureAction.name + " and type " + failureAction.GetType().ToString());
            //    }
            //}
            //so it look like greaterTargetDispel has a spell component, which is presumably its school, an effect run action which is the ContextActionDispelMagic carring no success or failure actions, 
            //and a spawn fx which is the effect that plays on cast


            BlueprintAbility greaterAreaDispel = library.Get<BlueprintAbility>("b9be852b03568064b8d2275a6cf9e2de");
            AbilitySpawnFx greaterDispelFx = greaterAreaDispel.GetComponent<AbilitySpawnFx>(); //well there is that
            //this one has a AbilityTargetsAround and all the stuff that the targeted one has. I definitely want this one's ability spawn fx not the the target one
            //foreach (BlueprintComponent bc in greaterAreaDispel.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log("Component of greater area dispel: " + bc.name + " and type " + bc.GetType().ToString());
            //}
            //AbilityEffectRunAction areaAera = greaterAreaDispel.GetComponent<AbilityEffectRunAction>();
            //foreach (GameAction ga in areaAera.Actions.Actions)
            //{
            //    Main.logger.Log("Game action in area dispel's ability effect run action name: " + ga.name + " and type: " + ga.GetType().ToString());
            //    ContextActionDispelMagic cadm = (ContextActionDispelMagic)ga;
            //    var stopAfterOne = Helpers.GetField(cadm, "m_StopAfterFirstRemoved");
            //    Main.logger.Log("stops after 1? " + stopAfterOne.ToString());
            //}
            //mages_disjunction.SetNameDescriptionIcon("Mages Disjunction", "All magical effects in the area are unraveled as if by a dispel magic effect", shadowEvocIcon);
            //magesDisjunctionDispel.ReplaceComponent<bc>
            //ContextActionDispelMagic dispel = Common.createContextActionDispelMagic(SpellDescriptor.None, new SpellSchool[0], RuleDispelMagic.CheckType.None);
            //Helpers.SetField(dispel, "m_BuffType", 1); //The enum value 1 should equal ContextActionDispelMagic.BuffType.FromSpells
            //Helpers.SetField(dispel, "m_StopAfterFirstRemoved", false); //this should allow the spell to effect everything on each target but does not appear to at the moment

            var shadowEvocIcon = library.Get<BlueprintAbility>("237427308e48c3341b3d532b9d3a001f").Icon; //Shadow Evoc

            AbilityExecuteActionOnCast castExecution = new AbilityExecuteActionOnCast
            {
                Actions = Helpers.CreateActionList(Helpers.Create<ContextActionDispellMagicAreasAndSummons>())
            };
            //Maybe I want to implement it "As each creature within range is hit with a burst of disjunctive magic. All magical effects on theses creatures are dispelled including buffs provided by items
            //Any area effect spells which a creature is being effected by are also dispelled
            //All summoned creatures are dismissed"
            mages_disjunction = Helpers.CreateAbility("MagesDisjunctionAbility",
                                                      "Mage's Disjunction",
                                                      "You cause disjunctive magic to surge through each creature within range. " +
                                                      "All spell effects on these creatures are dispelled including spell effects from items." +
                                                      "Any summoned creatures in the area are dismissed. " +
                                                      "Any magical area effect that is touching an effected creture is disrupted by the spells magic.",
                                                      //"This spell targets a point within range and strips away all nearby magical effects and spells. All area spell effects which touch " +
                                                      //"the target point are ended. All creatures within 20 feet of the" +
                                                      //"target point have all magical effects dispelled. This includes spell like buffs provided by items." +
                                                      //"Additionally, all summoned creatures near the target point are dismissed.",
                                                      "05d81b81d7c54375b38f2a3bebd3fd6e",
                                                      shadowEvocIcon,
                                                      AbilityType.Spell,
                                                      UnitCommand.CommandType.Standard,
                                                      AbilityRange.Medium,
                                                      "",
                                                      "",
                                                      Helpers.CreateAbilityTargetsAround(20.Feet(), TargetType.Any),
                                                      Helpers.CreateSpellComponent(SpellSchool.Abjuration),
                                                      //castExecution,
                                                      greaterDispelFx,
                                                      Helpers.CreateRunActions(Helpers.Create<ContextActionDispellMagicAreasAndSummons>())
                                                      //Helpers.CreateRunActions(dispel)
                                                      );
            mages_disjunction.setMiscAbilityParametersRangedDirectional();

            mages_disjunction.AvailableMetamagic = Metamagic.Quicken | Metamagic.Reach;

            mages_disjunction.AddToSpellList(Helpers.wizardSpellList, 9);
            mages_disjunction.AddSpellAndScroll("4b2d0e65fb9775341b6c4f7c178f0fe5");//guid for scroll of dispel magic 4b2d0e65fb9775341b6c4f7c178f0fe5
        }

        public static void createAcuteSenses()
        {
            var foresightIcon = library.Get<BlueprintBuff>("8c385a7610aa409468f3a6c0f904ac92").Icon;
            //This corresponds to base value until level 7 is 10, then until level 15 is value 20, then until level 20 is value 30
            (int, int)[] statProgression = new (int, int)[] { (7, 10), (15, 20), (20, 30) };

            var acute_senses_buff = Helpers.CreateBuff("AcuteSensesBuff",
                                                        "Acute Senses",
                                                        "The target gains a +10 enhancement bonus on Perception checks. The bonus increases to +20 at caster level 8th, and +30 (the maximum) at caster level 16th.",
                                                        "99da40d7f3f54a9f97c696fa19d6bddc ",//fresh guid
                                                        //"",
                                                        foresightIcon,
                                                        null,
                                                        Helpers.CreateAddContextStatBonus(StatType.SkillPerception, ModifierDescriptor.Enhancement, ContextValueType.Rank, AbilityRankType.Default),
                                                        //perceptionBuff,
                                                        Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel,
                                                                    progression:ContextRankProgression.Custom, 
                                                                    customProgression: statProgression
                                                                    )//, type:AbilityRankType.StatBonus)
                                                        );
            var apply_buff = Common.createContextActionApplyBuff(acute_senses_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus), DurationRate.Minutes), true);

            acute_senses = Helpers.CreateAbility("AcuteSensesAbility",
                                                    "Acute Senses",
                                                    acute_senses_buff.Description,
                                                    "b431e4098cd44849b8c3245933fdd588",//fresh guid
                                                    //"",
                                                    foresightIcon,
                                                    AbilityType.Spell,
                                                    UnitCommand.CommandType.Standard,
                                                    AbilityRange.Touch,
                                                    Helpers.minutesPerLevelDuration,
                                                    "",
                                                    Helpers.CreateRunActions(apply_buff),
                                                    Helpers.CreateSpellComponent(SpellSchool.Divination)
                                                    );
            acute_senses.setMiscAbilityParametersTouchFriendly();
            //perceptionBuff.Value
            //99da40d7f3f54a9f97c696fa19d6bddc guid for the buff

            acute_senses.AvailableMetamagic = Metamagic.Heighten | Metamagic.Extend | Metamagic.Quicken;

            acute_senses.AddToSpellList(Helpers.alchemistSpellList, 2);
            acute_senses.AddToSpellList(Helpers.bardSpellList, 2);
            acute_senses.AddSpellAndScroll("d4dc796367b4fb342ad7e8221fb8d813");//28968352ed826da4e9c2af856aad7096 guid for scroll of foresight
        }

        public static void createHeightenedAwareness()
        {
            BuffSkillBonus perceptionBuff = CreationFunctions.CreateBuffSkillBonus(2, StatType.SkillPerception, ModifierDescriptor.Competence);
            BuffSkillBonus knowledgeArcanaBuff = CreationFunctions.CreateBuffSkillBonus(2, StatType.SkillKnowledgeArcana, ModifierDescriptor.Competence);
            BuffSkillBonus knowledgeWorldBuff = CreationFunctions.CreateBuffSkillBonus(2, StatType.SkillKnowledgeWorld, ModifierDescriptor.Competence);
            BuffSkillBonus loreNatureBuff = CreationFunctions.CreateBuffSkillBonus(2, StatType.SkillLoreNature, ModifierDescriptor.Competence);
            BuffSkillBonus loreReligionBuff = CreationFunctions.CreateBuffSkillBonus(2, StatType.SkillLoreReligion, ModifierDescriptor.Competence);

            BuffSkillBonus initiativeBuff = CreationFunctions.CreateBuffSkillBonus(4, StatType.Initiative, ModifierDescriptor.None);

            var foresightIcon = library.Get<BlueprintBuff>("8c385a7610aa409468f3a6c0f904ac92").Icon;
            var senseVitalsIcon = library.Get<BlueprintBuff>("dea0dba1f7bff064987e03f1307bfa84").Icon;

            var skill_buff = Helpers.CreateBuff("HeightenedAwarenessSkillBuff",
                                                "Heightened Awareness",
                                                "You are in a heigtened state of awareness. You receive a +2 competence bonus on all Perception, Knowledge and Lore checks. You may dismiss this bonus to receive a +4 to initiative bonus checks for 3 rounds",
                                                "77219411ea054e03a838c96aaa4ef207", //fresh guid
                                                //"",
                                                senseVitalsIcon,
                                                null,
                                                perceptionBuff,
                                                knowledgeArcanaBuff,
                                                knowledgeWorldBuff,
                                                loreNatureBuff,
                                                loreReligionBuff
                                                );

            var apply_buff = Common.createContextActionApplyBuff(skill_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus), DurationRate.TenMinutes), true);

            var init_buff = Helpers.CreateBuff("HeightenAwarenessInitiativeBuff",
                                                "Heightened Initiative",
                                                "After dismissing Heigtened Awareness, you have received a +4 to initiative bonus checks for 3 rounds",
                                                "2816472d6c0b4156b04ffdb96bd7e133", //fresh guid
                                                //"",
                                                foresightIcon,
                                                null,
                                                initiativeBuff
                                                );

            var remove_buff = Common.createContextActionRemoveBuffFromCaster(skill_buff);
            var apply_init_buff = Common.createContextActionApplyBuff(init_buff, Helpers.CreateContextDuration(3, DurationRate.Rounds));

            var dismissAbility = Helpers.CreateAbility("HeightenedAwarenessDismissAbility",
                                                     "Initiative Boost",
                                                     "Use this ability to dismiss the skill bonuses of Heightened Awareness and gain a +4 bonus to initiative for 3 rounds",
                                                     "01f899d450474f66998c7ec4bea2760b", //fresh guid
                                                     //"",
                                                     senseVitalsIcon,
                                                     AbilityType.SpellLike,
                                                     UnitCommand.CommandType.Free,
                                                     AbilityRange.Personal,
                                                     "",
                                                     "",
                                                     Helpers.CreateRunActions(apply_init_buff, remove_buff)
                                                     //Helpers.CreateRunActions(remove_buff)
                                                     );

            heightened_awareness = Helpers.CreateAbility("HeightenedAwarenessAbility",
                                                             skill_buff.Name,
                                                             skill_buff.Description,
                                                             "9e142ed6623d4b8c8b1d4fb5804dbb5c",
                                                             //"",
                                                             senseVitalsIcon,
                                                             AbilityType.Spell,
                                                             UnitCommand.CommandType.Standard,
                                                             AbilityRange.Personal,
                                                             Helpers.tenMinPerLevelDuration,
                                                             "",
                                                             Helpers.CreateRunActions(apply_buff),
                                                             Helpers.CreateSpellComponent(SpellSchool.Divination)
                                                           );

            skill_buff.AddComponent(Helpers.CreateAddFact(dismissAbility));

            heightened_awareness.AvailableMetamagic = Metamagic.Heighten | Metamagic.Extend | Metamagic.Quicken;

            heightened_awareness.AddToSpellList(Helpers.alchemistSpellList, 1);
            heightened_awareness.AddToSpellList(Helpers.wizardSpellList, 1);
            heightened_awareness.AddToSpellList(Helpers.bardSpellList, 1);
            heightened_awareness.AddToSpellList(Helpers.druidSpellList, 1);
            heightened_awareness.AddToSpellList(Helpers.inquisitorSpellList, 1);

            heightened_awareness.AddSpellAndScroll("d4dc796367b4fb342ad7e8221fb8d813"); // scroll of sense vitals
        }

        public static void createGlueSeal()
        {
            var entangledBuff = library.CopyAndAdd<BlueprintBuff>("a719abac0ea0ce346b401060754cc1c0", "GlueSealEntangledBuff", "bc79b5792c6642ef9e33fdef7e4b63f1"); //Guid for web grappled buff
            var tarpoolBuff = library.Get<BlueprintBuff>("631d255f6b89afe45b32cf66c35a4205"); //TarPoolEntangledBuff guid 

            entangledBuff.SetNameDescriptionIcon("Stuck", "This creature is stuck in glue. It is entangled and cannot move until it breaks out.", tarpoolBuff.Icon); //guid for tarpool entangled buff, looks like tar glue
            entangledBuff.FxOnStart = tarpoolBuff.FxOnStart;
            entangledBuff.FxOnRemove = tarpoolBuff.FxOnRemove;

            var difficult_terrain = library.CopyAndAdd<BlueprintBuff>("525e4ff20086404419b3aab63917d6a0", "GlueSealDifficultTerrainBuff", "1a5e8d1825874231a740f83791905b84"); // guid for tarpool difficult terrain buff 525e4ff20086404419b3aab63917d6a0
            var glueArea = library.CopyAndAdd<BlueprintAbilityAreaEffect>("fd323c05f76390749a8555b13156813d", "GlueSealArea", "eb78bfdb47154e9fbc14216079328688"); //web area
            glueArea.Size = 5.Feet();
            //glueArea.ReplaceComponent<AbilityAreaEffectBuff>(a => a.Buff = difficult_terrain);
            glueArea.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            glueArea.Fx.AssetId = "28b114249fcea5241ab216930ddea100"; // guid for puddle tar 5ft 28b114249fcea5241ab216930ddea100

            var apply_entangled = Common.createContextActionApplyBuff(entangledBuff, Helpers.CreateContextDuration(), is_child: true, is_permanent: true);
            var break_free = Helpers.Create<CallOfTheWild.CombatManeuverMechanics.ContextActionBreakFreeFromSpellGrapple>(c =>
            {
                c.Failure = Helpers.CreateActionList(apply_entangled);
                c.Success = Helpers.CreateActionList();
            });

            var area_effect = Helpers.CreateAreaEffectRunAction(unitEnter: new GameAction[] { Common.createContextActionApplyBuff(difficult_terrain, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false) },
                                                    unitExit: new GameAction[] { Helpers.Create<ContextActionRemoveBuffSingleStack>(r => r.TargetBuff = difficult_terrain), Helpers.Create<ContextActionRemoveBuffSingleStack>(r => r.TargetBuff = entangledBuff) },
                                                    unitMove: new GameAction[] {break_free});
                                                    //unitExit: Helpers.Create<ContextActionRemoveBuffSingleStack>(r => r.TargetBuff = difficult_terrain),
                                                    //unitMove: break_free);
            //var area_effect = Helpers.CreateAreaEffectRunAction(unitEnter: Common.createContextActionApplyBuff(difficult_terrain, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false),
            //                                        unitExit: Helpers.Create<ContextActionRemoveBuffSingleStack>(r => r.TargetBuff = difficult_terrain),
            //                                        unitMove: break_free);
            glueArea.ReplaceComponent<AbilityAreaEffectRunAction>(area_effect);
            var spawn_area = Common.createContextActionSpawnAreaEffect(glueArea, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes));

            glue_seal = Helpers.CreateAbility("GlueSealAbility",
                                              "Glue Seal",
                                              "You conjure a layer of sticky glue. Anyone in the area when the spell is cast must attempt a Reflex Save. Those who fail become trapped in the glue. They are entangled and unable to move. They can break free with a combat maneuver or mobility check as a standard action against the DC of this spell. A creature moving through the area must make a mobility check in order to avoid being entangled.",
                                              //Its unclear why but failing to initialize this ability with a guid causes many crashes. Something to do with area loading (its unclear)
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
            glue_seal.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Ground));

            var new_round_actions = entangledBuff.GetComponent<AddFactContextActions>().NewRound;
            var new_actions = Common.replaceActions<ContextActionBreakFree>(new_round_actions.Actions,
                                                                                a => Helpers.Create<CallOfTheWild.CombatManeuverMechanics.ContextActionBreakFreeFromSpellGrapple>(c =>
                                                                                {
                                                                                    c.Failure = a.Failure;
                                                                                    c.Success = a.Success;
                                                                                }
                                                                                                                                                                    )
                                                                             );
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
                                      //"",
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
                //"",
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
