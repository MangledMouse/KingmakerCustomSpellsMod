using CallOfTheWild;
using CallOfTheWild.DismissSpells;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace CowWithHatsCustomSpellsMod
{
    class Core
    {
        static LibraryScriptableObject library => Main.library;

        static internal void preLoad()
        {
            NewSpells.load();
            //Main.logger.Log("Preload reached");
        }

        static internal void postLoad()
        {
            ClassUpdates.load();
            //UpdateDismiss();
        }

        static internal void UpdateDismiss()
        {
            BlueprintAbility dismissAbility = library.Get<BlueprintAbility>("5b21f6f7f14347948a2b77e3ae7e1fc4");
            dismissAbility.SetDescription("This ability allows caster to dismiss any area effect that they created at the target location, to dismiss a summoned or animated creature, or to release a dominated creature from control.");
            ContextActionDismissSpell dismiss = Helpers.Create<ExpandedContextActionDismissSpell>();
            dismissAbility.GetComponent<AbilityEffectRunAction>().Actions = Helpers.CreateActionList(dismiss);
            dismissAbility.ReplaceComponent<AbilityTargetCanDismiss>(Helpers.Create<ExpandedAbilityTargetCanDismiss>());
        }
    }
}