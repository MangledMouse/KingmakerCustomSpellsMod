using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CowWithHatsCustomSpellsMod
{
    class Buffs
    {
        public class ContextSkillBuff : BuffLogic
        {
            public ContextValue value;
        }
    }
}

/*
 
namespace Kingmaker.Designers.Mechanics.Buffs
{
  [ComponentName("BuffMechanics/Bonus to Stealth skill")]
  [AllowedOn(typeof (BlueprintUnitFact))]
  [AllowMultipleComponents]
  public class BuffSkillBonus : OwnedGameLogicComponent<UnitDescriptor>
  {
    public StatType Stat;
    public ModifierDescriptor Descriptor;
    public int Value;
    private ModifiableValue.Modifier m_SkillModifier;

    public override void OnTurnOn()
    {
      base.OnTurnOn();
      this.m_SkillModifier = this.Owner.Stats.GetStat(this.Stat).AddModifier(this.Value, (GameLogicComponent) this, this.Descriptor);
    }

    public override void OnTurnOff()
    {
      base.OnTurnOff();
      this.m_SkillModifier.Remove();
      this.m_SkillModifier = (ModifiableValue.Modifier) null;
    }
  }
}
 */
