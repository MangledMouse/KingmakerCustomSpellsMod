using CallOfTheWild;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CowWithHatsCustomSpellsMod
{
    class NewComponents
    {
    }

    public class ContextActionDispellMagicAreasAndSummons : ContextAction
    {

        public override string GetCaption()
        {
            return "Dispell Area Effects";
        }

        public override void RunAction()
        {
            var areas = Game.Instance.State.AreaEffects;
            var units = GameHelper.GetTargetsAround(this.Target.Point, 1.Feet() *.1f, false, false);

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
                foreach (Buff b in buffsToRemove)
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

    public class ContextConditionHasFascinateBreaker : ContextCondition
    {
        protected override bool CheckCondition()
        {
            bool hasANearbyEnemy = false;
            UnitEntityData thisUnit = this.Target.Unit;

            var units = GameHelper.GetTargetsAround(thisUnit.Position, 10.Feet(), true, false);

            foreach (UnitEntityData unit in units)
            {
                if (thisUnit.IsEnemy(unit))
                {
                    hasANearbyEnemy = true;
                }
            }
            return hasANearbyEnemy;
        }

        protected override string GetConditionCaption()
        {
            return string.Empty;
        }
    }

}
