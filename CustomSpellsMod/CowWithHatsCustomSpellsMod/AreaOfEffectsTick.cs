using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects;
using System;
using System.Reflection;

namespace CowWithHatsCustomSpellsMod
{
    [HarmonyPatch(typeof(AreaEffectEntityData))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] {
        typeof(AreaEffectView),
        typeof(MechanicsContext),
        typeof(BlueprintAbilityAreaEffect),
        typeof(TargetWrapper),
        typeof(TimeSpan),
        typeof(TimeSpan?),
        typeof(bool),
    })]
    internal class AreaOfEffectsTick
    {
        // Cache the field info once for performance
        private static readonly FieldInfo TimeToNextRoundField = typeof(AreaEffectEntityData)
            .GetField("m_TimeToNextRound", BindingFlags.NonPublic | BindingFlags.Instance);

        static AreaOfEffectsTick()
        {
            //Main.logger.Log("[AreaOfEffectsTick] Patch class loaded.");
        }


        [HarmonyPostfix]
        public static void Postfix(AreaEffectEntityData __instance)
        {
            Main.logger.Log("[AreaOfEffectsTick] Constructor patched.");
            if (!Main.settings.address_multitick_bug)
            {
              //  Main.logger.Log("Not addressing multitick bug");
                return;
            }


            //Main.logger.Log("[AreaOfEffectsTick] Patching constructor...");

            var runAction = __instance.Blueprint.GetComponent<AbilityAreaEffectRunAction>();

            // Check runAction
            if (runAction == null)
            {
              //  Main.logger.Log("[AreaOfEffectsTick] runAction is null.");
                return;
            }
            else
            {
                //Main.logger.Log("[AreaOfEffectsTick] runAction found.");
            }

            // Check UnitEnter
            if (runAction.UnitEnter == null)
            {
                //Main.logger.Log("[AreaOfEffectsTick] runAction.UnitEnter is null.");
                return;
            }
            else
            {
                //Main.logger.Log("[AreaOfEffectsTick] runAction.UnitEnter found.");
            }

            // Check HasActions
            if (!runAction.UnitEnter.HasActions)
            {
                //Main.logger.Log("[AreaOfEffectsTick] runAction.UnitEnter.HasActions is false.");
                return;
            }
            else
            {
                //Main.logger.Log("[AreaOfEffectsTick] runAction.UnitEnter.HasActions is true.");
            }

            try
            {
                // Set m_TimeToNextRound = 6f using reflection
                TimeToNextRoundField?.SetValue(__instance, 6f);
                //Main.logger.Log($"[AreaOfEffectsTick] Updated m_TimeToNextRound = 6f for {__instance.Blueprint?.name ?? "unknown"}");
            }
            catch (Exception ex)
            {
                Main.logger.Log($"[AreaOfEffectsTick] Failed to set m_TimeToNextRound: {ex}");
            }
        }
    }
}
