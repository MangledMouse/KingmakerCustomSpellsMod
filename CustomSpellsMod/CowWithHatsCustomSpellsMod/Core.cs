using CallOfTheWild;
using CallOfTheWild.DismissSpells;
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

        static internal void preLoad()
        {
            NewSpells.load();
            //Main.logger.Log("Preload reached");
        }

        static internal void postLoad()
        {
            ClassUpdates.load();
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

        internal static void UpdateSilence()
        {
            NewSpells.CreateUpdatedSilenceSpell();
        }

        internal static void FixEarpierce()
        {
            NewSpells.UpdateEarPiercingScream();
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