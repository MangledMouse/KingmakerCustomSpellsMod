using CallOfTheWild;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UI.LevelUp.Phase;

namespace CowWithHatsCustomSpellsMod.AlliedSpellcasterFix
{
    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class AlliedSpellcasterSameSpellBonusExpanded : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>, IGlobalRulebookHandler<RuleSpellResistanceCheck>, IConcentrationBonusProvider, IRulebookHandler<RuleSpellResistanceCheck>, IGlobalRulebookSubscriber
    {
        public BlueprintUnitFact AlliedSpellcasterFact;
        public int Radius;
        //public bool isCaster = false;

        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            //Main.logger.Log("Event about to trigger for " + this.Owner.Unit.CharacterName);
            //isCaster = true;
            var spell = evt.Spell;
            foreach (UnitEntityData unitEntityData in GameHelper.GetTargetsAround(this.Owner.Unit.Position, (float)this.Radius, true, false))
            {
                if ((unitEntityData.Descriptor.HasFact(this.AlliedSpellcasterFact) || (bool)this.Owner.State.Features.SoloTactics) && unitEntityData != this.Owner.Unit && !unitEntityData.IsEnemy(this.Owner.Unit)
                     && hasSpell(unitEntityData.Descriptor, evt.Spell))
                {
                    evt.AddBonusCasterLevel(1);
                    evt.AddBonusConcentration(2);
                    break;
                }
            }
        }


        public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
        {
            Main.logger.Log("Caster with Allied Spellcaster " + this.Owner.Unit.CharacterName);
            foreach (UnitEntityData unitEntityData in GameHelper.GetTargetsAround(this.Owner.Unit.Position, (float)this.Radius, true, false))
            {
                if ((unitEntityData.Descriptor.HasFact(this.AlliedSpellcasterFact) || (bool)this.Owner.State.Features.SoloTactics) && unitEntityData != this.Owner.Unit && !unitEntityData.IsEnemy(this.Owner.Unit)
                     && hasSpell(unitEntityData.Descriptor, evt.Ability))
                {
                    evt.AdditionalSpellPenetration += 2;
                    //Main.logger.Log("Allied character possessing Allied Spellcaster " + unitEntityData.CharacterName);
                    //Main.logger.Log("Current Bonus " + evt.AdditionalSpellPenetration);
                    break;
                }
            }
        }

        public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
        {
            //if (isCaster)
            //{
            //    Main.logger.Log("Event just triggered for " + this.Owner.Unit.CharacterName);
            //}
            //isCaster = false;
        }

        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }

        public int GetStaticConcentrationBonus()
        {
            return 0;
        }

        private bool hasSpell(UnitDescriptor unit, BlueprintAbility spell)
        {
            var duplicates = SpellDuplicates.getDuplicates(spell);
            foreach (var sb in unit.Spellbooks)
            {
                foreach (var d in duplicates)
                {
                    if (sb.CanSpend(d))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
