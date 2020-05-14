using CallOfTheWild;
using Kingmaker;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CowWithHatsCustomSpellsMod
{
    public static class CreationFunctions
    {

        public static AddCondition CreateAddCondition(UnitCondition cond)
        {
            var c = Helpers.Create<AddCondition>();
            c.Condition = cond;
            return c;
        }

        public static ContextConditionHasFact CreateContextConditionHasFact(BlueprintUnitFact fact)
        {
            var c = Helpers.Create<ContextConditionHasFact>();
            c.Fact = fact;
            return c;
        }

        public static ContextActionRemoveBuffSingleStack CreateContextRemoveSingleBuffStack(BlueprintBuff targetBuff)
        {
            var c = Helpers.Create<ContextActionRemoveBuffSingleStack>();
            c.TargetBuff = targetBuff;
            return c;
        }

        public static BuffSkillBonus CreateBuffSkillBonus(int value, StatType stat, ModifierDescriptor descriptor)
        {
            BuffSkillBonus bsb = Helpers.Create<BuffSkillBonus>();             
            bsb.Value = value;
            bsb.Stat = stat;
            bsb.Descriptor = descriptor;
            return bsb;
        }

    }

    //run action appears to be being called multiple times when it doesn't hit any units. Worth investigating
    public class ContextActionDispellMagicAreasAndSummons : ContextAction
    {

        public override string GetCaption()
        {
            return "Dispell Area Effects";
        }

        public override void RunAction()
        {
            var areas = Game.Instance.State.AreaEffects;
            var units = GameHelper.GetTargetsAround(this.Target.Point, 1.Feet(), false, false);

            foreach (UnitEntityData entity in units)
            {
                if (entity.Descriptor.HasFact(Game.Instance.BlueprintRoot.SystemMechanics.SummonedUnitBuff))
                {
                    Common.AddBattleLogMessage($"{entity.CharacterName} dismissed");
                    entity.Descriptor.RemoveFact(Game.Instance.BlueprintRoot.SystemMechanics.SummonedUnitBuff);
                }
                List<Buff> buffsToRemove = new List<Buff>();
                foreach (Buff b in entity.Buffs.Enumerable)
                {
                    if (b.IsFromSpell && !b.IsNotDispelable)
                    {
                        buffsToRemove.Add(b);
                    }
                }
                foreach(Buff b in buffsToRemove)
                {
                    Common.AddBattleLogMessage($"Dispelled {b.Name} from {entity.CharacterName}");
                    b.Remove();
                }
                foreach (AreaEffectEntityData area in areas)
                {
                    if (area.Context.SourceAbility != null
                        && area.Context.SourceAbility.IsSpell
                        && (area.Context.AssociatedBlueprint as BlueprintBuff == null)
                        && Helpers.GetField<TimeSpan?>(area, "m_Duration").HasValue
                        && area.View?.Shape != null
                        && area.View.Shape.Contains(entity.Position, 0.0f)
                        )
                    {
                        if (!area.IsEnded)
                            Common.AddBattleLogMessage($"Disjunctive energy has fractured the magic of {area.Context.SourceAbility.Name}");
                        area.ForceEnd();
                        //area.ForceEnd();
                    }
                }
            }
        }

    }
}
