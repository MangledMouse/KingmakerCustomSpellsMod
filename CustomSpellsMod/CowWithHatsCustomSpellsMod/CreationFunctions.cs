using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
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
            BuffSkillBonus bsb = new BuffSkillBonus();
            bsb.Value = value;
            bsb.Stat = stat;
            bsb.Descriptor = descriptor;
            return bsb;
        }
    }
}
