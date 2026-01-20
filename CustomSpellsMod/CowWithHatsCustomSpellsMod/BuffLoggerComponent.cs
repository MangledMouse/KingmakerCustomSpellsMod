using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Buffs;

namespace CowWithHatsCustomSpellsMod
{
    public class BuffLoggerComponent : BuffLogic
    {
        public override void OnFactActivate()
        {
            base.OnFactActivate();
            var unit = Owner;
            Main.logger.Log($"BuffLoggerComponent: Buff '{this.Fact?.Name}' applied to {unit.CharacterName}. Listing all active buffs:");
            foreach (var activeBuff in unit.Buffs)
            {
                Main.logger.Log($"- {activeBuff.Blueprint.name} ({activeBuff.Blueprint.AssetGuid})");
            }
        }

        public override void OnFactDeactivate()
        {
            base.OnFactDeactivate();
            var unit = Owner;
            Main.logger.Log($"BuffLoggerComponent: Buff '{this.Fact?.Name}' removed from {unit.CharacterName}.");
        }
    }
}