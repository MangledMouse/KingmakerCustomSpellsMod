using CallOfTheWild;
using CallOfTheWild.DismissSpells;
using CallOfTheWild.NewMechanics;
using CallOfTheWild.TeamworkMechanics;
using CowWithHatsCustomSpellsMod.AlliedSpellcasterFix;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace CowWithHatsCustomSpellsMod
{
    class Core
    {
        static LibraryScriptableObject library => Main.library;
        // All classes (including prestige classes).
        public static List<BlueprintCharacterClass> classes;
        public static BlueprintArchetype eldritchScionArchetype;

        static internal void preLoad()
        {
            NewSpells.load();
            //at the moment I am failing to remove the appropriate immunities from the enemy i feel like removing them from
            //BalanceUpdates.load();
            //Main.logger.Log("Preload reached");
        }

        static internal void postLoad()
        {
            ClassUpdates.load();
            
        }


        internal static void Load()
        {
            const String eldritchScionClassId = "f5b8c63b141b2f44cbb8c2d7579c34f5";
            classes = library.Root.Progression.CharacterClasses.Where(c => c.AssetGuid != eldritchScionClassId).ToList();
        }

        internal static void AddSpellReplacement()
        {
            SpellReplacement.Load();
        }

        static internal void UpdateDismiss()
        {
            BlueprintAbility dismissAbility = library.Get<BlueprintAbility>("5b21f6f7f14347948a2b77e3ae7e1fc4");
            dismissAbility.SetDescription("This ability allows caster to dismiss any area effect that they created at the target location, to dismiss a summoned or animated creature, or to release a dominated creature from control.");
            ContextActionDismissSpell dismiss = Helpers.Create<ExpandedContextActionDismissSpell>();
            dismissAbility.GetComponent<AbilityEffectRunAction>().Actions = Helpers.CreateActionList(dismiss);
            dismissAbility.ReplaceComponent<AbilityTargetCanDismiss>(Helpers.Create<ExpandedAbilityTargetCanDismiss>());
        }

        static internal void UpdateDominationEffects()
        {
            BlueprintBuff dominate_person = library.Get<BlueprintBuff>("c0f4e1c24c9cd334ca988ed1bd9d201f");
            AddControllableToChangeFaction(dominate_person.GetComponent<ChangeFaction>());
            BlueprintBuff control_undead = library.Get<BlueprintBuff>("21d20a30b93e4ae281a6d70d9ae1a64d");
            AddControllableToChangeFaction(control_undead.GetComponent<ChangeFaction>());
        }

        static internal void AddControllableToChangeFaction(ChangeFaction cf)
        {
            Helpers.SetField(cf, "m_AllowDirectControl", true);
        }

        internal static void RemoveSlumberNerf()
        {
            BlueprintAbility slumber_hex = library.Get<BlueprintAbility>("31f0fa4235ad435e95ebc89d8549c2ce");
            slumber_hex.RemoveComponents<AbilityTargetCasterHDDifference>();
            slumber_hex.SetNameDescription(slumber_hex.GetName(),
                "Effect: A witch can cause a creature within 30 feet to fall into a deep, magical sleep, as per the spell sleep. The creature receives a Will save to negate the effect. If the save fails, the creature falls asleep for a number of rounds equal to the witch’s level.\n"
                                                    + "The creature will not wake due to noise or light, but others can rouse it with a standard action. This hex ends immediately if the creature takes damage. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.");
            BlueprintAbility shaman_slumber_hex = library.Get<BlueprintAbility>("c04cde18e91e4f84898de92a372bc1e0");
            shaman_slumber_hex.RemoveComponents<AbilityTargetCasterHDDifference>();
            shaman_slumber_hex.SetNameDescription(shaman_slumber_hex.GetName(), 
            "Effect: A witch can cause a creature within 30 feet to fall into a deep, magical sleep, as per the spell sleep. The creature receives a Will save to negate the effect. If the save fails, the creature falls asleep for a number of rounds equal to the witch’s level.\n"
                                                    + "The creature will not wake due to noise or light, but others can rouse it with a standard action. This hex ends immediately if the creature takes damage. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.");

            //hexstrike 82cad6f016c04bf49fa851f5e9e10953

            BlueprintAbility hexstrike_slumber = library.Get<BlueprintAbility>("82cad6f016c04bf49fa851f5e9e10953");
            hexstrike_slumber.SetNameDescription(hexstrike_slumber.GetName(),
                "Effect: A witch can cause a creature within 30 feet to fall into a deep, magical sleep, as per the spell sleep. The creature receives a Will save to negate the effect. If the save fails, the creature falls asleep for a number of rounds equal to the witch’s level.\n"
                                                    + "The creature will not wake due to noise or light, but others can rouse it with a standard action. This hex ends immediately if the creature takes damage. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.");

            BlueprintFeature slumber_hex_feature = library.Get<BlueprintFeature>("c086eeb69a4442df9c4bb8469a2c362d");
            slumber_hex_feature.SetNameDescription(slumber_hex.GetName(),
                "Effect: A witch can cause a creature within 30 feet to fall into a deep, magical sleep, as per the spell sleep. The creature receives a Will save to negate the effect. If the save fails, the creature falls asleep for a number of rounds equal to the witch’s level.\n"
                                                    + "The creature will not wake due to noise or light, but others can rouse it with a standard action. This hex ends immediately if the creature takes damage. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.");
            BlueprintFeature shaman_slumber_hex_feature = library.Get<BlueprintFeature>("ee7a8e5dc78a4d6c9e44d88affe47088");
            shaman_slumber_hex_feature.SetNameDescription(shaman_slumber_hex.GetName(),
                "Effect: A witch can cause a creature within 30 feet to fall into a deep, magical sleep, as per the spell sleep. The creature receives a Will save to negate the effect. If the save fails, the creature falls asleep for a number of rounds equal to the witch’s level.\n"
                                                    + "The creature will not wake due to noise or light, but others can rouse it with a standard action. This hex ends immediately if the creature takes damage. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.");

            
        }

        internal static void UpdateSilence()
        {
            NewSpells.CreateUpdatedSilenceSpell();
        }

        internal static void FixEarpierce()
        {
            NewSpells.UpdateEarPiercingScream();
        }

        internal static void ChangeInspireRage()
        {
            BlueprintBuff inspiredRageBuff = library.Get<BlueprintBuff>("77038b4555324455b1d110fbe2fbc0ef");
            inspiredRageBuff.RemoveComponents<SpellDescriptorComponent>();
            BlueprintBuff inspireRageEffectBuff = library.Get<BlueprintBuff>("78a69c73d07842c0b46aa9038c40114c");
            inspireRageEffectBuff.RemoveComponents<SpellDescriptorComponent>();
            BlueprintAbilityAreaEffect inspireRageArea = library.Get<BlueprintAbilityAreaEffect>("5421df42956841e7a0499568080421b7");
            inspireRageArea.RemoveComponents<SpellDescriptorComponent>();

            BlueprintBuff controlledInspiredRageDexBuff = library.Get<BlueprintBuff>("ed0106d16661497a8147af6175a55dc8");
            controlledInspiredRageDexBuff.RemoveComponents<SpellDescriptorComponent>();
            BlueprintBuff controlledInspiredRageStrBuff = library.Get<BlueprintBuff>("de4a813aeee84a329e3b8ed10e9caeec");
            controlledInspiredRageStrBuff.RemoveComponents<SpellDescriptorComponent>();
            BlueprintBuff controlledInspiredRageConBuff = library.Get<BlueprintBuff>("cd2956555ab04d46ad0525947fe19846");
            controlledInspiredRageConBuff.RemoveComponents<SpellDescriptorComponent>();


            //This messes with ability funcitoning at all. Will keep it here because it has the IDs of relevant blueprints to the buff in case there are other changes that need to be made
            //BlueprintBuff controlledRageDexBuff = library.Get<BlueprintBuff>("ed0106d16661497a8147af6175a55dc8");
            //controlledRageDexBuff.RemoveComponents<SpellDescriptorComponent>();
            //BlueprintBuff controlledRageStrBuff = library.Get<BlueprintBuff>("de4a813aeee84a329e3b8ed10e9caeec");
            //controlledRageStrBuff.RemoveComponents<SpellDescriptorComponent>();
            //BlueprintBuff controlledRageConBuff = library.Get<BlueprintBuff>("cd2956555ab04d46ad0525947fe19846");
            //controlledRageConBuff.RemoveComponents<SpellDescriptorComponent>();

            BlueprintBuff insightfulContemplationBuff = library.Get<BlueprintBuff>("6b4882478b094ccf99f17fea891d9d14");
            insightfulContemplationBuff.RemoveComponents<SpellDescriptorComponent>();
            BlueprintBuff insightfulContemplationAbilityBuff = library.Get<BlueprintBuff>("ebe4fa61b8594e5e9dc084495fa86c2f");
            insightfulContemplationAbilityBuff.RemoveComponents<SpellDescriptorComponent>();
            BlueprintAbilityAreaEffect insightfulContemplationAbilityArea = library.Get<BlueprintAbilityAreaEffect>("6b8a0589e9af4227a22b9ea5a1098559");
            insightfulContemplationAbilityArea.RemoveComponents<SpellDescriptorComponent>();

        }


        //SkaldCourtPoetInsigtfulContemplationBuff	6b4882478b094ccf99f17fea891d9d14	Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff
        //SkaldCourtPoetInsightfulContemplationAbilityBuff ebe4fa61b8594e5e9dc084495fa86c2f Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff
        //SkaldCourtPoetInsightfulContemplationAbilityArea    6b8a0589e9af4227a22b9ea5a1098559 Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbilityAreaEffect
        public static BlueprintFeatureSelection tweakedFeat;
        internal static void UpdateAmiri()
        {
            var amiri_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("b3f29faef0a82b941af04f08ceb47fa2");
            amiri_companion.Strength = 16;//+2
            amiri_companion.Dexterity = 13;
            amiri_companion.Constitution = 14;
            amiri_companion.Intelligence = 10;
            amiri_companion.Wisdom = 12;
            amiri_companion.Charisma = 10;
            var amiri1_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("df943986ee329e84a94360f2398ae6e6");
            var amiri_class_level = amiri1_feature.GetComponent<AddClassLevels>();
            amiri_class_level.Archetypes = new BlueprintArchetype[] { };
            amiri_class_level.RaceStat = Kingmaker.EntitySystem.Stats.StatType.Strength;
            amiri_class_level.Selections[0].Features[1] = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5"); //power attack feature
            amiri_class_level.Selections[0].Features[2] = library.Get<BlueprintFeature>("57299a78b2256604dadf1ab9a42e2873"); //weapon focus bastard sword

            //Main.logger.Log("All features of amiri add class levels component");
            //foreach (BlueprintFeature bf in amiri_class_level.Selections[0].Features)
            //{
            //    Main.logger.Log("A feature with name " + bf.name + " and type " + bf.GetType().ToString());
            //}

            amiri_class_level.Skills = new StatType[] { StatType.SkillPersuasion, StatType.SkillAthletics, StatType.SkillLoreNature, StatType.SkillPerception };

            //There is an error in tweak or treat breaks Amiri's human feat selection with 
            
        }

        internal static void FixForTweakOrTreatHumanCompanionFeats()
        {
            tweakedFeat = library.TryGet<BlueprintFeatureSelection>("c8a37ee56f164659baaa312027b9c42f");
            if (tweakedFeat != null)
            {
                UpdateForTweakOrTreat();
            }
        }

        internal static void UpdateForTweakOrTreat()
        {
            Main.logger.Log("Tweak or Treat mod found. Fixing human companions across versions");

            //foreach (BlueprintUnitFact fact in amiri_companion.AddFacts)
            //{
            //    Main.logger.Log("Amiri units add facts " + fact.AssetGuid + " with name " + fact.name);
            //}
            //AddFacts PowerAttackFact = CallOfTheWild.Helpers.CreateAddFact(powerAttackFeat);

            var amiri_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("b3f29faef0a82b941af04f08ceb47fa2");
            BlueprintFeature powerAttackFeat = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5");
            BlueprintFeature toughness = library.Get<BlueprintFeature>("d09b20029e9abfe4480b356c92095623");
            BlueprintUnitFact[] amiriAddFacts = new BlueprintUnitFact[amiri_companion.AddFacts.Length + 1];
            for (int i = 0; i < amiri_companion.AddFacts.Length; i++)
            {
                amiriAddFacts[i] = amiri_companion.AddFacts[i];
            }
            if (Main.settings.iconic_amiri || CallOfTheWild.Main.settings.update_companions)
                amiriAddFacts[amiriAddFacts.Length - 1] = powerAttackFeat;
            else
                amiriAddFacts[amiriAddFacts.Length - 1] = toughness;
            amiri_companion.AddFacts = amiriAddFacts;
            //foreach (BlueprintUnitFact fact in amiri_companion.AddFacts)
            //{
            //    Main.logger.Log("Amiri units add facts " + fact.AssetGuid + " with name " + fact.name);
            //}

            BlueprintFeature selectiveChannelFeature = library.Get<BlueprintFeature>("fd30c69417b434d47b6b03b9c1f568ff");
            BlueprintFeature impInit = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74");
            var tristain_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("f6c23e93512e1b54dba11560446a9e02");
            BlueprintUnitFact[] tristAddFacts = new BlueprintUnitFact[tristain_companion.AddFacts.Length + 1];
            for (int i = 0; i < tristain_companion.AddFacts.Length; i++)
                tristAddFacts[i] = tristain_companion.AddFacts[i];
            if (CallOfTheWild.Main.settings.update_companions)
            {
                tristAddFacts[tristAddFacts.Length - 1] = impInit;
            }
            else
            {
                tristAddFacts[tristAddFacts.Length - 1] = selectiveChannelFeature;
            }
            tristain_companion.AddFacts = tristAddFacts;

            var ekun_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("d5bc1d94cd3e5be4bbc03f3366f67afc");
            BlueprintUnitFact[] ekunAddFacts = new BlueprintUnitFact[ekun_companion.AddFacts.Length + 1];
            for (int i = 0; i < ekun_companion.AddFacts.Length; i++)
                ekunAddFacts[i] = ekun_companion.AddFacts[i];
            ekunAddFacts[ekunAddFacts.Length - 1] = impInit;
            ekun_companion.AddFacts = ekunAddFacts;

            

            BlueprintFeature dodgeFeature = library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5");
            BlueprintFeature shieldFocus = library.Get<BlueprintFeature>("ac57069b6bf8c904086171683992a92a");
            var valerie_compainion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("54be53f0b35bf3c4592a97ae335fe765");
            BlueprintUnitFact[] valAddFacts = new BlueprintUnitFact[valerie_compainion.AddFacts.Length + 1];
            for (int i = 0; i < valerie_compainion.AddFacts.Length; i++)
            {
                valAddFacts[i] = valerie_compainion.AddFacts[i];
            }
            if (CallOfTheWild.Main.settings.update_companions)
            {
                valAddFacts[valAddFacts.Length - 1] = shieldFocus;
            }
            else
                valAddFacts[valAddFacts.Length - 1] = dodgeFeature;
            valerie_compainion.AddFacts = valAddFacts;

            BlueprintFeature pointBlankShotFeat = library.Get<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab");
            var cephal_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("77c5eb949dffb9f45abcc7a78a2d281f");
            BlueprintUnitFact[] cephalAddFacts = new BlueprintUnitFact[cephal_companion.AddFacts.Length + 1];
            for(int i = 0; i < cephal_companion.AddFacts.Length; i++)
            {
                cephalAddFacts[i] = cephal_companion.AddFacts[i];
            }
            if (CallOfTheWild.Main.settings.update_companions)
            {
                cephalAddFacts[cephalAddFacts.Length - 1] = impInit;
            }
            else
                cephalAddFacts[cephalAddFacts.Length - 1] = pointBlankShotFeat;
            cephal_companion.AddFacts = cephalAddFacts;

            BlueprintFeature twoWeaponFighting = library.Get<BlueprintFeature>("ac8aaf29054f5b74eb18f2af950e752d"); 
            var varn_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("e83a03d50fedd35449042ce73f1b6908");
            BlueprintUnitFact[] varnAddFacts = new BlueprintUnitFact[varn_companion.AddFacts.Length + 1];
            for(int i =0;i<varn_companion.AddFacts.Length;i++)
                varnAddFacts[i] = varn_companion.AddFacts[i];
            varnAddFacts[varnAddFacts.Length-1] = twoWeaponFighting;
            varn_companion.AddFacts = valAddFacts;

        }

        internal static void UpdateExtendableSpells()
        {
            //These 4 are now redundant with the base Call of the Wild mod
            CallOfTheWild.NewSpells.daze_mass.AvailableMetamagic = CallOfTheWild.NewSpells.daze_mass.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
            CallOfTheWild.NewSpells.command.AvailableMetamagic = CallOfTheWild.NewSpells.command.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
            CallOfTheWild.NewSpells.synaptic_pulse.AvailableMetamagic = CallOfTheWild.NewSpells.synaptic_pulse.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
            CallOfTheWild.NewSpells.synaptic_pulse_greater.AvailableMetamagic = CallOfTheWild.NewSpells.synaptic_pulse_greater.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;

            //This is not offered by the base mod but it is possible there is something that makes it more complicated about it because of the aura it makes
            CallOfTheWild.NewSpells.babble.AvailableMetamagic = CallOfTheWild.NewSpells.babble.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
        }

        internal static void UpdateAlliedSpellcaster()
        {
            int fix_range = 2;
            var allied_spell_caster = library.Get<BlueprintFeature>("9093ceeefe9b84746a5993d619d7c86f");
            allied_spell_caster.GetComponent<AlliedSpellcaster>().Radius = fix_range;
            allied_spell_caster.ReplaceComponent<AlliedSpellcasterSameSpellBonus>(Helpers.Create<AlliedSpellcasterSameSpellBonusExpanded>(a => { a.Radius = fix_range; a.AlliedSpellcasterFact = allied_spell_caster; }));

            //foreach(BlueprintComponent bc in allied_spell_caster.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log("Allied spell caster component " + bc.name + " with type " + bc.GetType().ToString());
            //}

            allied_spell_caster.RemoveComponents<AlliedSpellcaster>();
        }

        internal static void FixFeyThoughts()
        {
            BlueprintFeature feyThoughtsBluff = library.TryGet<BlueprintFeature>("3393b744356648bf8c87021fd24680fb");
            feyThoughtsBluff.AddComponent(Helpers.Create<CallOfTheWild.NewMechanics.AddBonusToSkillCheckIfNoClassSkill>(a => { a.skill = StatType.SkillPersuasion; a.check = StatType.CheckBluff; }));
            BlueprintFeature feyThoughtsDiplomacy = library.TryGet<BlueprintFeature>("557db4b7510d44418fc654e1734ac0c2");
            feyThoughtsDiplomacy.AddComponent(Helpers.Create<CallOfTheWild.NewMechanics.AddBonusToSkillCheckIfNoClassSkill>(a => { a.skill = StatType.SkillPersuasion; a.check = StatType.CheckDiplomacy; }));
            //int i = feyThoughtsBluff.GetComponents<BlueprintComponent>().Count();
            //Main.logger.Log("Fey Thoughts Bluff has " + i + " components");
            //foreach (BlueprintComponent bc in feyThoughtsBluff.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log("Component with name " + bc.name + " and to string value " + bc.ToString());
            //    AddClassSkill acs = bc as AddClassSkill;
            //    if (acs != null)
            //    {
            //        Main.logger.Log("Add Class Skill skill " + acs.Skill.ToString());
            //    }
            //}
            //Main.logger.Log("Fey Thoughts Diplomacy components");
            //foreach (BlueprintComponent bc in feyThoughtsDiplomacy.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log("Component with name " + bc.name + " and to string value " + bc.ToString());
            //    AddClassSkill acs = bc as AddClassSkill;
            //    if (acs != null)
            //    {
            //        Main.logger.Log("Add Class Skill skill " + acs.Skill.ToString());
            //    }
            //}

            //Main.logger.Log("For reference, Fast talker, which works in favored class");
            //BlueprintFeature bf = library.TryGet<BlueprintFeature>("a56e300d308641669ed3b6fd27d862e4");
            //foreach(BlueprintComponent bc in bf.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log("Component with name " + bc.name + " and to string value " + bc.ToString());
            //    AddClassSkill acs = bc as AddClassSkill;
            //    if (acs != null)
            //    {
            //        Main.logger.Log("Add Class Skill skill " + acs.Skill.ToString());
            //    }
            //}
        }
    }

        //[HarmonyPatch(typeof(UnitAttack), "OnAction")]
        //static class UnitAttack_OnAction_Patch
        //{
        //    private static void Postfix(UnitAttack __instance)
        //    {
        //        if (!Main.settings.confusion_output)
        //        {
        //            return;
        //        }
        //        UnitEntityData unit = __instance.Executor;
        //    if (unit.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Confusion))
        //    {
        //        UnitPartConfusion part = unit.Ensure<UnitPartConfusion>();
        //        switch (part.State)
        //        {
        //            case ConfusionState.ActNormally:
        //                CallOfTheWild.Common.AddBattleLogMessage($"{unit.CharacterName} sees clearly through its confusion and acts normally");
        //                break;
        //            case ConfusionState.AttackNearest:
        //                CallOfTheWild.Common.AddBattleLogMessage($"{unit.CharacterName} lashes out at the nearest creature due to its confusion");
        //                break;
        //            case ConfusionState.DoNothing:
        //                CallOfTheWild.Common.AddBattleLogMessage($"{unit.CharacterName} stays still out of confusion");
        //                break;
        //            case ConfusionState.SelfHarm:
        //                CallOfTheWild.Common.AddBattleLogMessage($"{unit.CharacterName} hurts itself in its confusion");
        //                break;
        //            default:
        //                CallOfTheWild.Common.AddBattleLogMessage($"{unit.CharacterName} is not confused");
        //                break;
        //        }
        //    }
        //}
        //}

        //[HarmonyPatch(typeof(UnitSelfHarm), "OnAction")]
        //static class UnitSelfHarm_OnAction_Patch
        //{
        //    private static void Postfix(UnitSelfHarm __instance)
        //    {
        //        if (!Main.settings.confusion_output)
        //        {
        //            return;
        //        }
        //        if (__instance.Target != null)
        //            CallOfTheWild.Common.AddBattleLogMessage($"{__instance.Target.Unit.CharacterName} hurts itself in its confusion");
        //    }
        //}

        //[HarmonyPatch(typeof(UnitDoNothing), "OnAction")]
        //static class DoNothing_OnAction_Patch
        //{
        //    private static void Postfix(UnitDoNothing __instance)
        //    {
        //        if (!Main.settings.confusion_output)
        //        {
        //            return;
        //        }
        //        if (__instance.Target != null)
        //            CallOfTheWild.Common.AddBattleLogMessage($"{__instance.Target.Unit.CharacterName} stays still out of confusion");
        //    }
        //}


        //to avoid transpiler issues
        //we can write a postfix for the UnitPartConfusion State with this syntax
        //[HarmonyPatch(typeof(UnitPartConfusion), "State", MethodType.Setter)]
        //Then grab the __instance of the UnitPartConfusion and output to the battlelog on a switch of the ConfusionState State
        //which looks like 

        //[HarmonyPatch(typeof(UnitPartConfusion), "State", MethodType.Setter)]
        //static class UnitPartConfusion_StateSetter_Patch
        //{
        //    private static void Postfix(UnitPartConfusion __instance)
        //    {
        //        if(!Main.settings.confusion_output)
        //        {
        //            return;
        //        }
        //        switch (__instance.State)
        //        {
        //            case ConfusionState.ActNormally:
        //                CallOfTheWild.Common.AddBattleLogMessage($"{__instance.Owner.CharacterName} sees clearly through its confusion and acts normally");
        //                break;
        //            case ConfusionState.AttackNearest:
        //                CallOfTheWild.Common.AddBattleLogMessage($"{__instance.Owner.CharacterName} lashes out at the nearest creature due to its confusion");
        //                break;
        //            case ConfusionState.DoNothing:
        //                CallOfTheWild.Common.AddBattleLogMessage($"{__instance.Owner.CharacterName} stays still out of confusion");
        //                break;
        //            case ConfusionState.SelfHarm:
        //                CallOfTheWild.Common.AddBattleLogMessage($"{__instance.Owner.CharacterName} hurts itself in its confusion");
        //                break;
        //            default:
        //                CallOfTheWild.Common.AddBattleLogMessage($"{__instance.Owner.CharacterName} is not confused");
        //                break;
        //        }
        //    }
        //}    
}