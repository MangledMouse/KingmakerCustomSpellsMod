﻿using CallOfTheWild;
using CallOfTheWild.DismissSpells;
using CallOfTheWild.NewMechanics;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Controllers.Units;
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
        }


        //internal static void UpdateAmiri()
        //{
        //    var amiri_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("b3f29faef0a82b941af04f08ceb47fa2");
        //    amiri_companion.Strength = 16;//+2
        //    amiri_companion.Dexterity = 13;
        //    amiri_companion.Constitution = 14;
        //    amiri_companion.Intelligence = 10;
        //    amiri_companion.Wisdom = 12;
        //    amiri_companion.Charisma = 10;
        //    var amiri1_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("df943986ee329e84a94360f2398ae6e6");
        //    var amiri_class_level = amiri1_feature.GetComponent<AddClassLevels>();
        //    amiri_class_level.Archetypes = new BlueprintArchetype[] { };
        //    amiri_class_level.RaceStat = Kingmaker.EntitySystem.Stats.StatType.Strength;
        //    amiri_class_level.Selections[0].Features[1] = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5");
        //    amiri_class_level.Skills = new StatType[] { StatType.SkillPersuasion, StatType.SkillAthletics, StatType.SkillLoreNature };
        //}

        internal static void UpdateExtendableSpells()
        {
            CallOfTheWild.NewSpells.daze_mass.AvailableMetamagic = CallOfTheWild.NewSpells.daze_mass.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
            CallOfTheWild.NewSpells.command.AvailableMetamagic = CallOfTheWild.NewSpells.command.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
            CallOfTheWild.NewSpells.babble.AvailableMetamagic = CallOfTheWild.NewSpells.babble.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
            CallOfTheWild.NewSpells.synaptic_pulse.AvailableMetamagic = CallOfTheWild.NewSpells.synaptic_pulse.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
            CallOfTheWild.NewSpells.synaptic_pulse_greater.AvailableMetamagic = CallOfTheWild.NewSpells.synaptic_pulse_greater.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
            //Check these

            //CallOfTheWild.NewSpells.sheet_lightning.AvailableMetamagic = CallOfTheWild.NewSpells.sheet_lightning.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
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