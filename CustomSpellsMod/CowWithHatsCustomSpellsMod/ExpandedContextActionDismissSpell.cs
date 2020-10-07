using CallOfTheWild;
using CallOfTheWild.DismissSpells;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
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

    public class ExpandedContextActionDismissSpell : ContextActionDismissSpell
    {
        public override void RunAction()
        {
            //relevant dominate buffs
            //guid for dominatepersonbuff c0f4e1c24c9cd334ca988ed1bd9d201f
            UnitEntityData unit = GameHelper.GetTargetsAround(this.Target.Point, 1.Feet().Meters * 0.1f, false, false).FirstOrDefault();

            if (unit != null && unit.IsPlayerFaction)
            {
                List<Buff> buffsToRemove = new List<Buff>();
                foreach(Buff buff in unit.Buffs.Enumerable)
                {
                    //UnitEntityData entity = buff.MaybeContext?.MaybeCaster;
                    //if(entity !=null)
                    //{
                    //    Common.AddBattleLogMessage($"{buff.Name} has caster {entity.CharacterName}");
                    //}
                    if (buff.Blueprint.Name == "Domination" && buff.MaybeContext?.MaybeCaster == this.Context.MaybeCaster)
                    {
                        buffsToRemove.Add(buff);
                    }
                }
                if (buffsToRemove.Count > 0)
                {
                    ChangeFaction fcf = new ChangeFaction();
                    //fcf.Fact.MaybeContext?.MaybeCaster
                    buffsToRemove[0].Remove();
                    Common.AddBattleLogMessage($"{unit.CharacterName} released from domination");
                    return;
                }
            }
            base.RunAction();
        }
    }

    public class ExpandedAbilityTargetCanDismiss : BlueprintComponent, IAbilityTargetChecker
    {
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            var unit = GameHelper.GetTargetsAround(target.Point, 1.Feet().Meters * 0.1f, false, false).FirstOrDefault();
            if (unit != null)
            {
                var summoner = unit.Get<UnitPartSummonedMonster>()?.Summoner;

                bool isDominated = false;
                if(unit.IsPlayerFaction)
                {
                    //isDominated = true;
                    foreach (Buff buff in unit.Buffs.Enumerable)
                    {
                        if (buff.Blueprint.Name == "Domination" && buff.MaybeContext?.MaybeCaster == caster)
                        {
                            isDominated = true;
                        }
                    }
                }

                return summoner == caster || isDominated;
            }

            var area = Game.Instance.State.AreaEffects.Where(a => a.Context.SourceAbility != null && a.Context.MaybeCaster == caster
                                                             && ((a.Context.AssociatedBlueprint as BlueprintBuff) == null)
                                                             && Helpers.GetField<TimeSpan?>(a, "m_Duration").HasValue
                                                             && a.View?.Shape != null && a.View.Shape.Contains(target.Point, 0.0f)).FirstOrDefault();

            return area != null;
        }
    }
}
