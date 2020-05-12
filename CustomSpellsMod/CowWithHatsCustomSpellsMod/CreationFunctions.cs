using CallOfTheWild;
using Kingmaker;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
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
                    if (b.IsFromSpell)
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
                //Common.AddBattleLogMessage($"Dispelling energy has fractured the magic of {name}");

            //foreach (AreaEffectEntityData area in areas)
            //{
            //    if (area.Context.SourceAbility != null
            //        && area.Context.SourceAbility.IsSpell
            //        && (area.Context.AssociatedBlueprint as BlueprintBuff == null)
            //        && Helpers.GetField<TimeSpan?>(area, "m_Duration").HasValue
            //        && area.View?.Shape != null
            //        && area.View.Shape.Contains(this.Target.Point, 0.0f)
            //        )
            //    {
            //        //Common.AddBattleLogMessage($"Dispelled {area.Context.SourceAbility.Name} area effect");
            //        area.ForceEnd();
            //    }
            //}

            //List<string> areaNames = new List<String>();
            //foreach (AreaEffectEntityData area in areas)
            //{
            //    bool isInList = false;
            //    foreach (string name in areaNames)
            //    {
            //        if (areaNames.Contains(name))
            //        {
            //            isInList = true;
            //            Common.AddBattleLogMessage($"{name} area effect is already listed");
            //        }
            //    }
            //    if (!isInList)
            //        areaNames.Add(area.Context.SourceAbility.Name);
            //    area.ForceEnd();
            //}
            //foreach (string name in areaNames)
            //{
            //    Common.AddBattleLogMessage($"Dispelled {name} area effect");
            //}
        }
    }
}
