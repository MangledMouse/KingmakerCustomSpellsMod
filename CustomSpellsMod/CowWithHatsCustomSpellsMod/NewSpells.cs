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
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Blueprints.Classes.Prerequisites;

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
        static public BlueprintAbility euphoric_cloud;
        static public BlueprintAbility mydriatic_spontaneity;
        static public BlueprintAbility mydriatic_spontaneity_mass;

        static public BlueprintBuff mydriatic_spontaneity_buff;

        public static void load()
        {
            createSuggestion();
            createGlueSeal();
            createHeightenedAwareness();
            createAcuteSenses();
            createMagesDisjunction();
            createEuphoricCloud();
            createMydriaticSpontaneity();
            outputSpellInfoToLog();
        }

        public static void createMydriaticSpontaneity()
        {
            //41cf93453b027b94886901dbfc680cb9 is the blueprint ID for overwhelming presecnce
            //a1bc9bf104a3c614391eccdcdf37d4ad is the blueprint id for overwhelming presecnce scroll

            var overwhelming_presence = library.Get<BlueprintAbility>("41cf93453b027b94886901dbfc680cb9");//overwhelming presence

            var undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
            var construct = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            //Common.createAbilityTargetHasFact(true, undead),
            //Common.createAbilityTargetHasFact(true, construct),

            var blind = library.Get<BlueprintBuff>("0ec36e7596a4928489d2049e1e1c76a7");
            var dazzled = library.Get<BlueprintBuff>("df6d1025da07524429afbae248845ecc");

            var apply_blind = Common.createContextActionApplyBuff(blind, Helpers.CreateContextDuration(1));
            var apply_dazzled = Common.createContextActionApplyBuff(dazzled, Helpers.CreateContextDuration(1));
            
            var new_round_after = Common.createContextActionRandomize(apply_blind, apply_dazzled, null, null);//, apply_blind_quick, apply_dazzle_quick);
            BlueprintBuff stinkingCloudAfterBuff = library.Get<BlueprintBuff>("fa039d873ee3f3e42abaf19877abaae1"); //stinking cloud buff after
            mydriatic_spontaneity_buff = library.CopyAndAdd<BlueprintBuff>("fa039d873ee3f3e42abaf19877abaae1", "Mydriatic Spontaneity", "e9f794cc9da94a14861b200b83daa265");
            mydriatic_spontaneity_buff.ReplaceComponent<SpellDescriptorComponent>(Helpers.CreateSpellDescriptor(SpellDescriptor.Blindness));
            mydriatic_spontaneity_buff.AddComponent(Helpers.CreateAddFactContextActions(activated: new_round_after, newRound: new_round_after)); //activated: apply_nauseated is how we had it working before)
            mydriatic_spontaneity_buff.SetNameDescriptionIcon("Mydriatic Spontaneity",
                "Subject is being overstimulated by flashes of light and surges of shadow. They are racked with splitting headaches and are nauseated. Each round their eyes are randomly either rapidly dialated or contracted. This process has a 25% change to dazzle them and 25% change to blind them for that round",
                overwhelming_presence.Icon);


            var apply_buff = CallOfTheWild.Common.createContextActionApplyBuff(mydriatic_spontaneity_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds), is_from_spell: true);

            mydriatic_spontaneity = Helpers.CreateAbility("MydriaticSpontaneityAbility",
                "Mydriatic Spontaneity",
                "You overstimulate the target with alternating flashes of light and shadow within its eyes, causing its pupils to rapidly dilate and contract. While under the effects of this spell, the target is racked by splitting headaches and unable to see clearly, becoming nauseated for the spell’s duration. Each round, the target’s pupils randomly become dilated or contracted for 1 round. This has a 25% chance to blind the target and a 25% chance to dazzle them each turn.",
                "c0017bedbfab480781fa5a3aca39bb26",
                overwhelming_presence.Icon,
                AbilityType.Spell,
                UnitCommand.CommandType.Standard,
                AbilityRange.Close,
                Helpers.roundsPerLevelDuration,
                Helpers.willNegates,
                Common.createAbilitySpawnFx("52d413df527f9fa4a8cf5391fd593edd", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                Common.createAbilityTargetHasFact(true, undead),
                Common.createAbilityTargetHasFact(true, construct),
                Helpers.CreateRunActions(SavingThrowType.Will, Helpers.CreateConditionalSaved(null, apply_buff)),
                Helpers.CreateSpellDescriptor(SpellDescriptor.Blindness),
                Helpers.CreateSpellComponent(SpellSchool.Evocation)
                );

            mydriatic_spontaneity_mass = Helpers.CreateAbility("MydriaticSponaneityMassAbility",
                "Mydriatic Spontaneity, Mass",
                "This spell functions like Mydriatic Spontaneity, except several creatures may be affected. \n" + mydriatic_spontaneity.Description,
                "ec568d9c02574873a60d0b61434b8329",
                overwhelming_presence.Icon,
                AbilityType.Spell,
                UnitCommand.CommandType.Standard,
                AbilityRange.Close,
                Helpers.roundsPerLevelDuration,
                Helpers.willNegates,
                Common.createAbilitySpawnFx("52d413df527f9fa4a8cf5391fd593edd", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                Common.createAbilityTargetHasFact(true, undead),
                Common.createAbilityTargetHasFact(true, construct),
                Helpers.CreateAbilityTargetsAround(30.Feet(), TargetType.Enemy),
                Helpers.CreateRunActions(SavingThrowType.Will, Helpers.CreateConditionalSaved(null, apply_buff)),
                Helpers.CreateSpellDescriptor(SpellDescriptor.Blindness),
                Helpers.CreateSpellComponent(SpellSchool.Evocation)
                );

            mydriatic_spontaneity.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            mydriatic_spontaneity_mass.setMiscAbilityParametersSingleTargetRangedHarmful(true);
            mydriatic_spontaneity.SpellResistance = true;
            mydriatic_spontaneity_mass.SpellResistance = true;

            mydriatic_spontaneity.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Piercing;
            mydriatic_spontaneity_mass.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Piercing;

            NewSpells.mydriatic_spontaneity.AddToSpellList(Helpers.bardSpellList, 3);//bard spell list
            NewSpells.mydriatic_spontaneity.AddToSpellList(Helpers.wizardSpellList, 4);//wizard spell list
            NewSpells.mydriatic_spontaneity_mass.AddToSpellList(Helpers.bardSpellList, 5);
            NewSpells.mydriatic_spontaneity_mass.AddToSpellList(Helpers.wizardSpellList, 7);
            NewSpells.mydriatic_spontaneity.AddSpellAndScroll("a1bc9bf104a3c614391eccdcdf37d4ad"); //overwhelming presecnce scroll
            NewSpells.mydriatic_spontaneity_mass.AddSpellAndScroll("a1bc9bf104a3c614391eccdcdf37d4ad");  //overwhelming presecnce scroll
        }

        public static void createEuphoricCloud()
        {
            //Actual Euphoric Cloud Stuff
            //BlueprintBuff stinkingCloudBuff = library.Get<BlueprintBuff>("f85351ee696d98246ae5dc182b410447"); //StinkingCloudBuff
            BlueprintAbility mindFog = library.Get<BlueprintAbility>("eabf94e4edc6e714cabd96aa69f8b207");//mind fog base spell
            BlueprintAbilityAreaEffect euphoricCloudArea = library.CopyAndAdd<BlueprintAbilityAreaEffect>("aa2e0a0fe89693f4e9205fd52c5ba3e5", "EuphoricCloudBuff", "8f79fea823c946ca95529ee731a6500c");//stinking cloud area and then a new guid
            BlueprintBuff euphoricCloudBuff = library.CopyAndAdd<BlueprintBuff>("6477ae917b0ec7a4ca76bc9f36b023ac", "EuphoricCloudFascinateBuff", "78c37a35c0d74a4eb638fd59bcaaeb72"); //Rainbow pattern buff and then a new guid
            euphoricCloudBuff.ReplaceComponent<SpellDescriptorComponent>(Helpers.CreateSpellDescriptor(SpellDescriptor.Daze | SpellDescriptor.Poison));
            AddFactContextActions theAddFact = euphoricCloudBuff.GetComponent<AddFactContextActions>();
            euphoricCloudBuff.RemoveComponent(theAddFact); //this removes the sfx that plays when creatures get fascinated by rainbow pattern
            euphoricCloudArea.Fx = library.Get<BlueprintAbilityAreaEffect>("fe5102d734382b74586f56980086e5e8").Fx;
            euphoricCloudBuff.SetNameDescriptionIcon("Euphoric Cloud", "This unit is fascinated by the intoxicating vapors of a Euphoric Cloud", mindFog.Icon);

            ContextActionRemoveSelf removeSelf = Helpers.Create<ContextActionRemoveSelf>();
            ContextActionConditionalSaved whatHappensOnExtraFortSaves = Helpers.CreateConditionalSaved(new GameAction[] {removeSelf }, new GameAction[] { });
            ContextActionSavingThrow extraFortSaves = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(whatHappensOnExtraFortSaves));
            Conditional proximityCheck = Helpers.CreateConditional(Helpers.Create<ContextConditionHasFascinateBreaker>(), extraFortSaves);
            AddFactContextActions roundlyProximityCheck = Helpers.CreateAddFactContextActions(newRound: new GameAction[] { proximityCheck });
            euphoricCloudBuff.SetComponents(euphoricCloudBuff.ComponentsArray.AddToArray<BlueprintComponent>(roundlyProximityCheck));

            BlueprintBuff euphoricCloudLeavingBuff = library.CopyAndAdd<BlueprintBuff>("78c37a35c0d74a4eb638fd59bcaaeb72", "EuphoricCloudFascinateAfterBuff", "083e89bb9fc54c72a612a2d5b3424add"); //copy of new euphoricCloudBuff

            BlueprintBuff euphoricResistanceBuff = Helpers.CreateBuff("EuphoricResistanceBuff",
                "Euphoric Drug Resistance",
                "This creature has recently been effected by Euphoric Cloud's intoxicating vapors. While in the cloud and for a short period afterwards, " +
                "additional exposure to Euphoric Cloud will not cause fascination",
                "0108c3c478194ea68921a806c53b03ad", //fresh guid
                mindFog.Icon,
                null
                );
            BlueprintBuff euphoricTemporaryResistanceBuff = Helpers.CreateBuff("EuphoricResistanceAfterBuff",
               "Euphoric Drug Resistance",
               "This creature has recently been effected by Euphoric Cloud's intoxicating vapors. While in the cloud and for a short period afterwards, " +
               "additional exposure to Euphoric Cloud will not cause fascination",
               "83172efde22d487dbcb4215cfb0d4ed4", //fresh guid
               mindFog.Icon,
               null
               );

            //apply buff actions
            ContextActionApplyBuff applyPermanentResistance = Common.createContextActionApplyBuff(euphoricResistanceBuff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_permanent: true, dispellable: false, is_from_spell: true);
            ContextActionApplyBuff applyTempResistance = Common.createContextActionApplyBuff(euphoricTemporaryResistanceBuff, Helpers.CreateContextDuration(5, DurationRate.Rounds), dispellable: false, is_from_spell: true);
            ContextActionApplyBuff applyCloudBuffPermanent = Common.createContextActionApplyBuff(euphoricCloudBuff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), is_permanent: true, dispellable: false, is_from_spell: true);
            ContextActionApplyBuff applyCloudBuffTemp = Common.createContextActionApplyBuff(euphoricCloudLeavingBuff, Helpers.CreateContextDuration(1, diceType: DiceType.D4, diceCount: 1), dispellable: false, is_from_spell: true);

            ContextConditionHasFact doesntHaveEuphoricResistance = Common.createContextConditionHasFact(euphoricResistanceBuff, false);
            ContextConditionHasFact doesntHaveTempEuphoricResistance = Common.createContextConditionHasFact(euphoricTemporaryResistanceBuff, false);
            ContextConditionHasFact doesntHaveEuphoricCloudBuff = Common.createContextConditionHasFact(euphoricCloudBuff, false);
            ContextConditionHasFact hasEuphoricResistance = Common.createContextConditionHasFact(euphoricResistanceBuff);
            ContextConditionHasFact hasEuphoricCloudBuff = Common.createContextConditionHasFact(euphoricCloudBuff);

            BlueprintFeature undeadType = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");//undeadtype guid
            ContextConditionHasFact notUndead = Common.createContextConditionHasFact(undeadType, false);
            BlueprintFeature constructType = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");//contructType guid
            ContextConditionHasFact notConstruct = Common.createContextConditionHasFact(undeadType, false);

            Condition[] enterConditions = new Condition[] { notUndead, notConstruct, doesntHaveEuphoricCloudBuff, doesntHaveEuphoricResistance, doesntHaveTempEuphoricResistance };
            ContextActionRemoveBuffSingleStack removeTempCloudBuff = CreationFunctions.CreateContextRemoveSingleBuffStack(euphoricCloudLeavingBuff);
            GameAction[] enterActionsOnFailedSave = new GameAction[] { applyPermanentResistance,removeTempCloudBuff, applyCloudBuffPermanent };
            ContextActionConditionalSaved saveCondition = Helpers.CreateConditionalSaved(new GameAction[] { }, enterActionsOnFailedSave);
            //Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(apply_attack)));
            ContextActionSavingThrow inCloudSaveFailed = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(saveCondition));
            //Conditional onEnterConditional = Helpers.CreateConditional(enterConditions, null, inCloudSaveFailed);
            Conditional onEnterConditional = Helpers.CreateConditional(enterConditions, inCloudSaveFailed);

            Condition[] roundConditions = new Condition[] { notUndead, notConstruct, doesntHaveEuphoricCloudBuff, doesntHaveEuphoricResistance, doesntHaveTempEuphoricResistance };
            GameAction[] roundActionsOnFailedSave = new GameAction[] { applyCloudBuffPermanent, removeTempCloudBuff, applyPermanentResistance };
            //ContextActionConditionalSaved inCloudSaveFailed = Helpers.CreateConditionalSaved(enterActionsOnFailedSave, new GameAction[] { });
            //Conditional everyRoundConditional = Helpers.CreateConditional(enterConditions, null, inCloudSaveFailed);
            Conditional everyRoundConditional = Helpers.CreateConditional(enterConditions, inCloudSaveFailed);

            Condition[] exitingWithCloudBuff = new Condition[] { hasEuphoricCloudBuff };
            ContextActionRemoveBuffSingleStack removeEuphoriaBuff = CreationFunctions.CreateContextRemoveSingleBuffStack(euphoricCloudBuff);
            GameAction[] exitActionsWithCloudBuff = new GameAction[] { removeEuphoriaBuff, applyCloudBuffTemp};
            Conditional exitingConditional = Helpers.CreateConditional(exitingWithCloudBuff, exitActionsWithCloudBuff);

            Condition[] exitingWithResistanceBuff = new Condition[] { hasEuphoricResistance };
            ContextActionRemoveBuffSingleStack removeResistanceBuff = CreationFunctions.CreateContextRemoveSingleBuffStack(euphoricResistanceBuff);
            GameAction[] exitActionsWithResitance = new GameAction[] { removeResistanceBuff, applyTempResistance };
            Conditional exitingConditionForResistance = Helpers.CreateConditional(exitingWithResistanceBuff, exitActionsWithResitance);

            //euphoricCloudAreaActions = Helpers.CreateAreaEffectRunAction(unitEnter: onEnterConditional, round: everyRoundConditional, unitExit: exitingConditional);
            euphoricCloudArea.ReplaceComponent<AbilityAreaEffectRunAction>(Helpers.CreateAreaEffectRunAction(unitEnter: new GameAction[]{ onEnterConditional}, 
                round: new GameAction[] { everyRoundConditional }, 
                unitExit: new GameAction[] { exitingConditional, exitingConditionForResistance }));

            var spawn_area = Common.createContextActionSpawnAreaEffect(euphoricCloudArea, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Rounds));

            euphoric_cloud = Helpers.CreateAbility("EuphoricCloudAbility",
                "Euphoric Cloud",
                "You create a bank of fog similar to that created by fog cloud except its vapors are intoxicating. Living creatures are distracted by the fumes and behave as if dazed. " +
                "Any creature that succeeds at its save at the initial save but remains in the cloud must continue to save each round on your turn. " +
                "If the creature is undisturbed this condition lasts as long as a creature is in the cloud and for 1d4+1 rounds after it leaves. " +
                "Distracted creatures that are damaged immediately snap out of the condition. Distracted creatures receive additional saving throws each round to break the condition if there are enemies within 10 feet. " +
                "Creatures who are intoxicated by the cloud and break free from its effects from damage or extra saves are not effected in further rounds.",
                "c80b7411bc744737974fc0418df733ca",//fresh guid
                mindFog.Icon,
                AbilityType.Spell,
                UnitCommand.CommandType.Standard,
                AbilityRange.Medium,
                Helpers.roundsPerLevelDuration,
                Helpers.fortNegates,
                Helpers.Create<AbilityEffectRunActionOnClickedTarget>(a => a.Action = Helpers.CreateActionList(spawn_area)),
                //Helpers.CreateRunActions(SavingThrowType.Reflex, Helpers.CreateConditionalSaved(null, apply_entangled)),
                Helpers.CreateSpellComponent(SpellSchool.Conjuration),
                Helpers.CreateAbilityTargetsAround(20.Feet(), TargetType.Any)
                );
            euphoric_cloud.setMiscAbilityParametersRangedDirectional();
            euphoric_cloud.AvailableMetamagic = Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken | Metamagic.Extend | (Metamagic)MetamagicFeats.MetamagicExtender.Persistent;
            //MindFog eabf94e4edc6e714cabd96aa69f8b207 Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility
            //StinkingCloud   68a9e6d7256f1354289a39003a46d826 Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility
            //StinkingCloudAfterBuff  fa039d873ee3f3e42abaf19877abaae1 Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff
            //StinkingCloudArea   aa2e0a0fe89693f4e9205fd52c5ba3e5 Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbilityAreaEffect
            //StinkingCloudBuff   f85351ee696d98246ae5dc182b410447 Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff

            euphoric_cloud.AddToSpellList(Helpers.wizardSpellList, 2);
            euphoric_cloud.AddToSpellList(Helpers.druidSpellList, 2);
            euphoric_cloud.AddToSpellList(Helpers.magusSpellList, 2);

            euphoric_cloud.AddSpellAndScroll("61bacc43652d76c42b60d965b65cd741");//mind fog scroll image
        }

        public static void createMagesDisjunction()
        {
            BlueprintAbility greaterAreaDispel = library.Get<BlueprintAbility>("b9be852b03568064b8d2275a6cf9e2de");
            AbilitySpawnFx greaterDispelFx = greaterAreaDispel.GetComponent<AbilitySpawnFx>(); 

            var shadowEvocIcon = library.Get<BlueprintAbility>("237427308e48c3341b3d532b9d3a001f").Icon; //Shadow Evoc

            AbilityExecuteActionOnCast castExecution = new AbilityExecuteActionOnCast
            {
                Actions = Helpers.CreateActionList(Helpers.Create<ContextActionDispellMagicAreasAndSummons>())
            };
            mages_disjunction = Helpers.CreateAbility("MagesDisjunctionAbility",
                                                      "Mage's Disjunction",
                                                      "You cause disjunctive magic to surge through each creature within range. " +
                                                      "All spell effects on these creatures are dispelled." +
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
            var apply_init_buff = Common.createContextActionApplyBuff(init_buff, Helpers.CreateContextDuration(3, DurationRate.Rounds), is_from_spell: true);

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
            var buff = library.CopyAndAdd<BlueprintBuff>("6477ae917b0ec7a4ca76bc9f36b023ac", "SuggestionBuff", "729fa06959ad4a0ca1e761b955b6ff62"); //rainbow pattern then new guid
            var litany_of_eloquence = library.Get<BlueprintAbility>("c9198d9dfd2515d4ba98335b57bb66c7"); //litany of eloquence
            var hold_monster = library.Get<BlueprintAbility>("41e8a952da7a5c247b3ec1c2dbb73018");
            var checker_fact = hold_monster.GetComponents<AbilityTargetHasNoFactUnless>().ToArray();
            //foreach(BlueprintComponent bc in hold_monster.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log($"Hold_monster comoponent {bc.name} with type {bc.GetType().ToString()}");
            //}
            //foreach(AbilityTargetHasNoFactUnless athnfu in checker_fact)
            //{
            //    Main.logger.Log($"HasFactUnless from HoldMonster {athnfu.name}");
            //    Main.logger.Log($"HasFactUnless unless fact name {athnfu.UnlessFact.name} description {athnfu.UnlessFact.Description} and type {athnfu.UnlessFact.GetType().ToString()}");
            //    foreach(BlueprintUnitFact checkedFact in athnfu.CheckedFacts)
            //    {
            //        Main.logger.Log($"Checked fact name {checkedFact.name} description {checkedFact.Description} and type {checkedFact.GetType().ToString()}");
            //    }
            //}
            


            var does_not_work = hold_monster.GetComponent<AbilityTargetHasFact>();
            buff.SetDescription("");
            var apply_buff = CallOfTheWild.Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), is_from_spell: true);
            //var action = Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionHasFacts(false, checker_fact[0].CheckedFacts), Common.createContextConditionCasterHasFact(checker_fact[0].UnlessFact, has: false)),
            //                                        null,
            //                                        Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Common.createContextConditionHasFacts(false, checker_fact[1].CheckedFacts), Common.createContextConditionCasterHasFact(checker_fact[1].UnlessFact, has: false)),
            //                                                                  null,
            //                                                                    Helpers.CreateConditional(Common.createContextConditionHasFacts(false, does_not_work.CheckedFacts),
            //                                                                                            null,
            //                                                                                            apply_buff
            //                                                                                            )
            //                                                                 )
            //                                        );

            buff.SetIcon(litany_of_eloquence.Icon);
            buff.SetNameDescription("Suggestion", "This creature is under the compulsive effects of a Suggestion spell. They are lost in thought and they will take no actions until they are harmed.");

            //Main.logger.Log($"Passes first array point");

            suggestion = Helpers.CreateAbility("SuggestionAbility",
                                      "Suggestion",
                                      "You suggest to a single creature that they should avoid combat and turn their thoughts inward. The spell magically influences the creature to follow the suggestion. They are fascinated by the effect for the duration or until they are harmed.",
                                      "3682197d956543e2a694e163b5fdcc6c",
                                      //"",
                                      litany_of_eloquence.Icon,
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
            //suggestion.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(apply_buff));
            suggestion.SpellResistance = true;
            suggestion.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Piercing;


            //Main.logger.Log($"Finished creating suggestion");
            //Creating Suggestion, Mass          

            suggestion_mass = Helpers.CreateAbility("SuggestionMassAbility",
                "Suggestion, Mass",
                "This spell functions like Suggestion, except several creatures may be affected. \n" +suggestion.Description,
                "033e70b6c72f46cab793eab5ff9f3a87",
                //"",
                litany_of_eloquence.Icon,
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
            //suggestion_mass.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(apply_buff));
            suggestion_mass.SpellResistance = true;
            suggestion_mass.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Reach | Metamagic.Quicken | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Persistent | (Metamagic)CallOfTheWild.MetamagicFeats.MetamagicExtender.Piercing;

            NewSpells.suggestion.AddToSpellList(Helpers.bardSpellList, 2);//bard spell list
            NewSpells.suggestion.AddToSpellList(Helpers.wizardSpellList, 3);//wizard spell list
            NewSpells.suggestion_mass.AddToSpellList(Helpers.bardSpellList, 5);
            NewSpells.suggestion_mass.AddToSpellList(Helpers.wizardSpellList, 6);
            NewSpells.suggestion.AddSpellAndScroll("71289f8d77db10e4d90174c902e1b6eb"); //scroll of euphoric tranquility
            NewSpells.suggestion_mass.AddSpellAndScroll("71289f8d77db10e4d90174c902e1b6eb");  //scroll of euphoric tranquility
        }

        public static void UpdateEarPiercingScream()
        {
            var apply_dazed = Common.createContextActionApplyBuff(Common.dazed_non_mind_affecting, Helpers.CreateContextDuration(1));
            var scream = library.Get<BlueprintAbility>("8e7cfa5f213a90549aadd18f8f6f4664");
            AbilityEffectRunAction aera = scream.GetComponent<AbilityEffectRunAction>();
            foreach(GameAction ga in aera.Actions.Actions)
            {
                ContextActionConditionalSaved cacs = ga as ContextActionConditionalSaved;
                if(cacs!=null)
                {
                    cacs.Failed = Helpers.CreateActionList(apply_dazed);
                }
            }
        }

        public static void CreateUpdatedSilenceSpell()
        {
            //              SilenceBuff 7ce2f7b5b6904bb9aef6ee2e942e8ff9 Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff
            //              SilenceAreaEffect   69eb7248d6e84292bb9d3287e90b637d Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbilityAreaEffect
            //              SilenceAbility  9bd3a1c01eb04104bf5e21a7d6330b4d Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility

            CallOfTheWild.NewSpells.silence.LocalizedSavingThrow = Helpers.CreateString($"{CallOfTheWild.NewSpells.silence.name}.SavingThrow", "");
            CallOfTheWild.NewSpells.silence.SpellResistance = false;
            CallOfTheWild.NewSpells.silence.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten | Metamagic.Quicken;

            BlueprintAbilityAreaEffect silence_area = library.Get<BlueprintAbilityAreaEffect>("69eb7248d6e84292bb9d3287e90b637d");
            AbilityAreaEffectRunAction new_action = Helpers.CreateAreaEffectRunAction(unitEnter: Common.createContextActionApplyBuff(CallOfTheWild.NewSpells.silence_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false),
                                                  unitExit: Helpers.CreateConditional(Common.createContextConditionHasBuffFromCaster(CallOfTheWild.NewSpells.silence_buff),
                                                                                      Common.createContextActionRemoveBuffFromCaster(CallOfTheWild.NewSpells.silence_buff))
                                                                                      );
            silence_area.ReplaceComponent<AbilityEffectRunAction>(new_action);

        }
        

        public static void outputSpellInfoToLog()
        {

            //BlueprintAbility forcedRepentence = library.Get<BlueprintAbility>("cc0aeb74b35cb7147bff6c53538bbc76");
            //foreach(BlueprintComponent bc in forcedRepentence.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log($"Forced Repentence component {bc.name} of type {bc.GetType().ToString()}");
            //    SpellListComponent slc = bc as SpellListComponent;
            //    if (slc != null)
            //    {
            //        Main.logger.Log($"Spell list component asset guid {slc.SpellList.AssetGuid}");
            //    }
            //    SpellComponent sc = bc as SpellComponent;
            //    if(sc !=null)
            //    {
            //        Main.logger.Log($"Spell Component {sc.School}");
            //    }
            //    ContextRankConfig crc = bc as ContextRankConfig;
            //    if(crc !=null)
            //    {
            //        Main.logger.Log($"Context rank config class level {crc.IsBasedOnClassLevel} archetype {crc.RequiresArchetype} feature rank {crc.IsBasedOnFeatureRank} stat bonus {crc.IsBasedOnStatBonus} feature list {crc.IsFeatureList} custom prop {crc.IsBasedOnCustomProperty} division prg {crc.IsDivisionProgression} progressionstart {crc.IsDivisionProgressionStart} Type {crc.Type} ");
            //    }
            //}

            //BlueprintAbility colorspray = library.Get<BlueprintAbility>("91da41b9793a4624797921f221db653c");
            //foreach(BlueprintComponent bc in colorspray.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log($"Color Spray component {bc.name} with type {bc.GetType().ToString()}");
            //    SpellDescriptorComponent sd = bc as SpellDescriptorComponent;
            //    if (sd != null)
            //    {
            //        Main.logger.Log($"Spell descriptor component {sd.Descriptor.Value}");
            //    }
            //    SpellListComponent sc = bc as SpellListComponent;
            //    if(sc !=null)
            //    {
            //        Main.logger.Log($"");
            //    }
            //}

            //BlueprintBuff stinkingCloudAfterBuff = library.Get<BlueprintBuff>("fa039d873ee3f3e42abaf19877abaae1");
            //foreach (BlueprintComponent bc in stinkingCloudAfterBuff.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log($"Stinking cloud after buff component {bc.name} with type {bc.GetType().ToString()}");
            //    AddCondition ac = bc as AddCondition;
            //    if(ac != null )
            //    {
            //        Main.logger.Log($"The condition inflicted is {ac.Condition} ");
            //    }
            //}

            //BlueprintAbility dazzle = library.Get<BlueprintAbility>("f0f8e5b9808f44e4eadd22b138131d52");
            //foreach (BlueprintComponent bc in dazzle.GetComponents<BlueprintComponent>())
            //{
            //    var aera = bc as AbilityEffectRunAction;
            //    if (aera != null)
            //    {
            //        foreach (GameAction ga in aera.Actions.Actions)
            //        {
            //            Main.logger.Log($"The ability effect run action from flare name {ga.name} and type {ga.GetType().ToString()} ");
            //            ContextActionConditionalSaved cacs = ga as ContextActionConditionalSaved;
            //            if (cacs != null)
            //            {
            //                foreach (GameAction conditionalAction in cacs.Failed.Actions)
            //                {
            //                    Main.logger.Log($"Context Action Conditional Saved failed action {conditionalAction.name} of type {conditionalAction.GetType().ToString()}");
            //                    ContextActionApplyBuff caab = conditionalAction as ContextActionApplyBuff;
            //                    if (caab != null)
            //                        Main.logger.Log($"The name of the buff applied is {caab.Buff.name} and has type {caab.Buff.GetType().ToString()}");
            //                }

            //            }
            //        }
            //    }
            //    Main.logger.Log($"Flare spell Blueprint Component of type {bc.GetType().ToString()} with name {bc.name} ");
            //}

            //var dominate_monster = library.Get<BlueprintAbility>("3c17035ec4717674cae2e841a190e757");
            //foreach(BlueprintComponent bc in dominate_monster.GetComponents<BlueprintComponent>())
            //{
            //    if(bc as )
            //}

            //BlueprintCharacterClass sorcerer_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");
            //foreach (BlueprintComponent bc in sorcerer_class.ComponentsArray)
            //{
            //    Main.logger.Log($"Sorcerer class component {bc.name} with type {bc.GetType().ToString()}");
            //    PrerequisiteNoClassLevel pncl = bc as PrerequisiteNoClassLevel;
            //    if (pncl != null)
            //        Main.logger.Log($" Sorcerer's can't have class levels in {pncl.CharacterClass.LocalizedName.ToString()}");
            //}
            //BlueprintCharacterClass cleric_Class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            //foreach (BlueprintComponent bc in cleric_Class.ComponentsArray)
            //{
            //    Main.logger.Log($"Cleric class component {bc.name} with type {bc.GetType().ToString()}");
            //    PrerequisiteNoClassLevel pncl = bc as PrerequisiteNoClassLevel;
            //    if (pncl != null)
            //        Main.logger.Log($" Cleric's can't have class levels in {pncl.CharacterClass.LocalizedName.ToString()}");
            //    PrerequisiteNoFeature noFeature = bc as PrerequisiteNoFeature;
            //    if (noFeature != null)
            //        Main.logger.Log($"Clerics can't have {noFeature.Feature.Name} with description {noFeature.Feature.Description}");
            //}

            //BlueprintAbilityAreaEffect createPitAoe = library.Get<BlueprintAbilityAreaEffect>("cf742a1d377378e4c8799f6a3afff1ba"); //create pit guid
            //foreach(BlueprintComponent bc in createPitAoe.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log($"Create pit aoe component {bc.name} and type {bc.GetType().ToString()}");
            //    AreaEffectPit aep = bc as AreaEffectPit;
            //    if(aep!=null)
            //    {
            //        foreach (GameAction ga in aep.OnFallAction.Actions)
            //            Main.logger.Log($"AreaEffectPit action list OnFallAction action {ga.name} with type {ga.GetType().ToString()}");
            //        foreach (GameAction ga in aep.EveryRoundAction.Actions)
            //            Main.logger.Log($"AreaEffectPit action list EveryRoundAction action {ga.name} with type {ga.GetType().ToString()}");
            //        foreach (GameAction ga in aep.OnEndedActionForUnitsInside.Actions)
            //            Main.logger.Log($"AreaEffectPit action list OnEndedActionForUnitsInside action {ga.name} with type {ga.GetType().ToString()}");
            //        foreach (BlueprintUnitFact buf in aep.ImmunityFacts)
            //            Main.logger.Log($"Immunity fact for areaeffectpit {buf.name} with type {buf.GetType().ToString()} "); 
            //    }
            //}
            //Units in a pit have a UnityPartInPit which has the function 
            //ResilientSphere would presumably want something that makes the target IsUnTargetable and AddConditions for CantMove and CantAct. I'm not sure how IsUntargetable should work in terms of applying

            //public void CaptureUnit()
            //{
            //    if (this.m_Captured)
            //        return;
            //    this.Owner.State.IsUntargetable.Retain();
            //    this.Owner.State.AddCondition(UnitCondition.CantMove, (Buff)null);
            //    this.Owner.State.AddCondition(UnitCondition.CantAct, (Buff)null);
            //    this.m_Captured = true;
            //}
            //UnitPartInPit upip = new UnitPartInPit;
            //upip.Owner

            //Euphoric cloud should work like stinking cloud except that it applies fascinated and when it applies the condition it also gives a "recently Euphoric" buff for caster level rounds
            //When the everyRound and onEnter checks happen to poison the target, first check the condition, that they don't have "recently Euphoric" don't make the save check if they have that buff
            //stinking cloud
            //BlueprintAbilityAreaEffect scArea = library.Get<BlueprintAbilityAreaEffect>("aa2e0a0fe89693f4e9205fd52c5ba3e5"); //stinking cloud area
            //AbilityAreaEffectRunAction actionsInCloud = scArea.GetComponent<AbilityAreaEffectRunAction>();
            //foreach (GameAction ga in actionsInCloud.UnitEnter.Actions)
            //{
            //    Main.logger.Log($"Stinking Cloud area unit Enter action {ga.name} and type {ga.GetType().ToString() }");
            //    Conditional c = ga as Conditional;
            //    if (c != null)
            //    {
            //        foreach (Condition con in c.ConditionsChecker.Conditions)
            //        {
            //            Main.logger.Log($"Condition name {con.name} and type {con.GetType().ToString()}");
            //            ContextConditionHasFact factCondition = con as ContextConditionHasFact;
            //            if (factCondition != null)
            //            {
            //                Main.logger.Log($"The conditional fact {factCondition.Fact.name}, the not value is {factCondition.Not} and its type {factCondition.Fact.GetType().ToString()}");
            //            }
            //        }
            //        foreach (GameAction internalGa in c.IfTrue.Actions)
            //        {
            //            Main.logger.Log($" Conditional if true action name {internalGa.name} and type {internalGa.GetType().ToString()}");
            //        }
            //        foreach (GameAction internalGa in c.IfFalse.Actions)
            //        {
            //            Main.logger.Log($" Conditional if false action name {internalGa.name} and type {internalGa.GetType().ToString()}");
            //        }
            //    }
            //}
            //foreach (GameAction ga in actionsInCloud.Round.Actions)
            //{
            //    Main.logger.Log($"Stinking Cloud area Round action {ga.name} and type {ga.GetType().ToString() }");
            //    Conditional c = ga as Conditional;
            //    if (c != null)
            //    {
            //        foreach (Condition con in c.ConditionsChecker.Conditions)
            //        {
            //            Main.logger.Log($"Condition name {con.name} and type {con.GetType().ToString()}");
            //            ContextConditionHasFact factCondition = con as ContextConditionHasFact;
            //            if (factCondition != null)
            //            {
            //                Main.logger.Log($"The conditional fact {factCondition.Fact.name}, the not value is {factCondition.Not} and its type {factCondition.Fact.GetType().ToString()} ");
            //            }
            //        }
            //        foreach (GameAction internalGa in c.IfTrue.Actions)
            //        {
            //            Main.logger.Log($" Conditional if true action name {internalGa.name} and type {internalGa.GetType().ToString()}");
            //        }
            //        foreach (GameAction internalGa in c.IfFalse.Actions)
            //        {
            //            Main.logger.Log($" Conditional if false action name {internalGa.name} and type {internalGa.GetType().ToString()}");
            //        }
            //    }
            //}
            //foreach (GameAction ga in actionsInCloud.UnitMove.Actions)
            //{
            //    Main.logger.Log($"Stinking Cloud area unit Move action {ga.name} and type {ga.GetType().ToString() }");
            //}
            //foreach (GameAction ga in actionsInCloud.UnitExit.Actions)
            //{
            //    Main.logger.Log($"Stinking Cloud area unit Exit action {ga.name} and type {ga.GetType().ToString() }");
            //    Conditional c = ga as Conditional;
            //    if (c != null)
            //    {
            //        foreach (Condition con in c.ConditionsChecker.Conditions)
            //        {
            //            Main.logger.Log($"Condition name {con.name} and type {con.GetType().ToString()}");
            //        }
            //        foreach (GameAction internalGa in c.IfTrue.Actions)
            //        {
            //            Main.logger.Log($" Conditional if true action name {internalGa.name} and type {internalGa.GetType().ToString()}");
            //        }
            //        foreach (GameAction internalGa in c.IfFalse.Actions)
            //        {
            //            Main.logger.Log($" Conditional if false action name {internalGa.name} and type {internalGa.GetType().ToString()}");
            //        }
            //    }
            //}
            //BlueprintBuff Stinkingbuff = library.Get<BlueprintBuff>("f85351ee696d98246ae5dc182b410447");
            //foreach (BlueprintComponent bc in Stinkingbuff.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log($"     Stinking cloud buff {bc.name} has type {bc.GetType().ToString()}");
            //}
            //BlueprintBuff stinkingAfterBuff = library.Get<BlueprintBuff>("fa039d873ee3f3e42abaf19877abaae1");
            //foreach (BlueprintComponent bc in stinkingAfterBuff.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log($"Stinking cloud after buff {bc.name} has type {bc.GetType().ToString()}");
            //}

            ////rainbowpattern buff
            //BlueprintBuff rainbowpatternBuff = library.Get<BlueprintBuff>("6477ae917b0ec7a4ca76bc9f36b023ac");
            //foreach (BlueprintComponent bc in rainbowpatternBuff.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log($"Rainbow pattern buff component {bc.name} has type {bc.GetType().ToString()}");
            //    AddCondition ac = bc as AddCondition;
            //    if (ac != null)
            //        Main.logger.Log($" the condition is {ac.Condition}");
            //    SpellDescriptorComponent sdc = bc as SpellDescriptorComponent;
            //    if (sdc != null)
            //        Main.logger.Log($"  Spell Descriptor value {sdc.Descriptor.Value}");
            //    AddIncomingDamageTrigger aidt = bc as AddIncomingDamageTrigger;
            //    if (aidt != null)
            //    {
            //        foreach (GameAction ga in aidt.Actions.Actions)
            //            Main.logger.Log($" Damage trigger actions {ga.name} of type {ga.GetType().ToString()}");
            //    }
            //    AddFactContextActions afca = bc as AddFactContextActions;
            //    if (afca != null)
            //    {
            //        foreach (GameAction ga in afca.Activated.Actions)
            //            Main.logger.Log($" Activated action {ga.name} and {ga.GetType().ToString()}");
            //        foreach (GameAction ga in afca.Deactivated.Actions)
            //            Main.logger.Log($" Deactivated action {ga.name} and {ga.GetType().ToString()}");
            //        foreach (GameAction ga in afca.NewRound.Actions)
            //            Main.logger.Log($" New round action {ga.name} and {ga.GetType().ToString()}");
            //    }
            //}

            //BlueprintBuff holdPersonBuff = library.Get<BlueprintBuff>("11cb2fe4fe9c44b448cfe1788ae1ab59");
            //foreach (BlueprintComponent bc in holdPersonBuff.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log($"Hold person component {bc.name} with type {bc.GetType().ToString()}");
            //}

            //BlueprintBuff dominatePersonbuff = library.Get<BlueprintBuff>("c0f4e1c24c9cd334ca988ed1bd9d201f");
            //foreach (BlueprintComponent bc in dominatePersonbuff.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log($"Dominate person component {bc.name} with type {bc.GetType().ToString()}");
            //    AddFactContextActions afca = bc as AddFactContextActions;
            //    if(afca != null)
            //    {
            //        foreach (GameAction action in afca.Activated.Actions)
            //            Main.logger.Log($"Activated action {action.name} and type {action.GetType().ToString()}");
            //        foreach (GameAction action in afca.Deactivated.Actions)
            //            Main.logger.Log($"Deactivated action {action.name} and type {action.GetType().ToString()}");
            //        foreach (GameAction action in afca.NewRound.Actions)
            //            Main.logger.Log($"New Round action {action.name} and type {action.GetType().ToString()}");
            //    }
            //}

            //BlueprintBuff confusionBuff = library.Get<BlueprintBuff>("886c7407dc629dc499b9f1465ff382df");
            //foreach(BlueprintComponent bc in confusionBuff.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log($"Confusion buff component {bc.name} and type {bc.GetType().ToString()}");
            //}
        }

    }
}
