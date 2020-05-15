using CallOfTheWild;
using Kingmaker;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
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
   
}
