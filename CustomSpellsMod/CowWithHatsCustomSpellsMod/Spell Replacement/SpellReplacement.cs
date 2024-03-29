﻿// Copyright (c) 2019 Jennifer Messerly (Updated more recently by CowWithHat)
// This code is licensed under MIT license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Linq;
using CallOfTheWild;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.FactLogic;
using Newtonsoft.Json;
using UnityEngine;

namespace CowWithHatsCustomSpellsMod
{
    static class SpellReplacement
    {
        static LibraryScriptableObject library => Main.library;

        static bool loaded = false;

        internal static void Load()
        {
            if (loaded) return;
            loaded = true;

            // Create feature selections that we can use to allows spontaneous casters to replace known spells.
            //
            // The idea for this came from how the devs implemented Mystic Theurge and Inquisitor. The spellbook
            // selection UI doesn't support 2 classes, so they used parameterized feature selections to choose
            // the inquisitor spells.
            //
            // A similar approach is used here to replace spells. This has the nice side effect that the spell is
            // removed before adding new spells.
            //
            // The progression is based on caster level, which cannot be implemented using a BlueprintProgression
            // directly. So instead we rely on the `HandleUpdateCasterLevel` method being called from our patches to
            // ApplySpellbook.Apply and SelectFeature.Apply (used for Prestigious Spellcaster).
            foreach (var characterClass in Helpers.classes)
            {
                BlueprintCharacterClass occultistClass = library.Get<BlueprintCharacterClass>("1b76f3c73aa84f91a1c65513fb23aa01");
                BlueprintArchetype relicHunterArchetype = library.Get<BlueprintArchetype>("ee3aefc7cb19461f95f5d254009664e0");
                if (characterClass == occultistClass)
                    break;
                //Main.logger.Log($"Spell Replacement Progression function started for {characterClass.name}");
                if(characterClass.Spellbook != null)
                    CreateSpellReplacementProgression(characterClass.Spellbook);
                foreach (var archetype in characterClass.Archetypes)
                {
                    //if (archetype == relicHunterArchetype)
                    //    break;
                    //Main.logger.Log($"Spell Replacement Progression function started for {archetype.name}");
                    if (archetype.ReplaceSpellbook != null)
                        CreateSpellReplacementProgression(archetype.ReplaceSpellbook);
                }
            }

            //as explained by elmindra in the previous comment, this is supposed to apply a patch to applyspellbook.apply and selectfeature.apply but one of those functions seems to no longer be present
            //there is error test in output_log.txt for it but i don't really understand exactly how it works
            ApplySpellbook_Apply_Patch.onApplySpellbook.Add((state, unit, previousCasterLevel) =>
            {
                var spellbook = unit.GetSpellbook(state.SelectedClass);
                if (spellbook != null && spellbook.CasterLevel != previousCasterLevel)
                {
                    HandleUpdateCasterLevel(unit, state, spellbook);
                }
            });
        }

        static void CreateSpellReplacementProgression(BlueprintSpellbook spellbook)
        {
            if (spellbook == null || !spellbook.Spontaneous || spellbook.SpellsKnown == null) return;

            var levelEntries = new Dictionary<int, BlueprintFeatureSelection>();

            // Replace a spell is optional, so provide a way to opt out.
            var keepAllSpellsFeat = CreateKeepAllSpellsFeat(spellbook);

            // The minimum level for spell replacement is 4.
            //
            // This creates an entry at every level, because it's possible that the caster level
            // increases won't follow a predictable pattern due to multiclassing.
            BlueprintFeatureSelection replaceFeat = null;
            int previousMaxSpell = 0;
            for (var level = 4; level <= 20; level++)
            {
                var spellsKnown = spellbook.SpellsKnown;
                int maxSpellLevel = spellsKnown.Levels[level].Count.Length - 2;
                if (maxSpellLevel < 1) continue; // can't replace spells yet.

                if (maxSpellLevel != previousMaxSpell)
                {
                    replaceFeat = CreateSpellReplacementFeature(spellbook, maxSpellLevel, keepAllSpellsFeat);
                    previousMaxSpell = maxSpellLevel;
                }
                levelEntries[level] = replaceFeat;
            }

            casterProgressions[spellbook] = levelEntries;
        }

        static BlueprintFeatureSelection CreateSpellReplacementFeature(BlueprintSpellbook spellbook, int maxSpellLevel, BlueprintFeature keepAllSpells)
        {
            var caster = spellbook.CharacterClass;

            var displayName = $"Replace Spell ({spellbook.CharacterClass.Name})";
            var description = GetDescription(spellbook);
            Sprite icon = null; // TODO: grab icon like the spellbook UI does

            var replaceFeat = Helpers.CreateParamSelection<ReplaceSpellSelection>(
                spellbook.name + $"ReplaceSpellParameterized{maxSpellLevel}",
                displayName,
                description,
                Helpers.MergeIds(spellbook.AssetGuid, guidsByLevel[maxSpellLevel]),
                icon,
                FeatureGroup.None,
                Helpers.Create<ReplaceSpellParametrized>(r => r.Spellbook = spellbook));
            replaceFeat.SpellcasterClass = caster;
            replaceFeat.SpellLevel = maxSpellLevel;
            replaceFeat.HideInUI = true;

            var selection = Helpers.CreateFeatureSelection(
                spellbook.name + $"ReplaceSpellSelection{maxSpellLevel}",
                displayName,
                description,
                Helpers.MergeIds(replaceFeat.AssetGuid, "e8c5583ddc2942cfb176582b2cc99254"),
                icon,
                FeatureGroup.None);
            selection.HideInUI = true;
            selection.SetFeatures(replaceFeat, keepAllSpells);
            //Log.Write(selection, showSelection : true);
            return selection;
        }

        static String GetDescription(BlueprintSpellbook spellbook)
        {
            var spellProgression = spellbook.GetCasterSpellProgression();
            bool isFullCaster = spellProgression == CasterSpellProgression.FullCaster;
            bool isThreeQuartersCaster = spellProgression == CasterSpellProgression.ThreeQuartersCaster;

            var firstSpell = isFullCaster ? 4 : (isThreeQuartersCaster ? 5 : 8);
            var replacementFrequency = isFullCaster ? 2 : 3;
            var secondSpell = firstSpell + replacementFrequency;
            var thirdSpell = firstSpell + replacementFrequency * 2;

            var className = spellbook.CharacterClass.Name.ToLower();
            return $"Upon reaching {firstSpell}th level, and at every {replacementFrequency} {className} level after that ({secondSpell}th, {thirdSpell}th, and so on), a {className} can choose to learn a new spell in place of one they already know. In effect, the {className} loses the old spell in exchange for the new one. The new spell's level must be the same as that of the spell being exchanged. A {className} may swap only a single spell at any given level, and must choose whether or not to swap the spell at the same time that they gain new spells known for the level.";
        }

        internal static bool CanReplaceSpellThisLevel(Spellbook spellbook)
        {
            var blueprint = spellbook.Blueprint;
            if (!blueprint.Spontaneous) return false;

            var casterLevel = spellbook.CasterLevel;
            var spellsKnown = blueprint.SpellsPerDay;
            switch (blueprint.GetCasterSpellProgression())
            {
                case CasterSpellProgression.FullCaster:
                    // Full caster gets to replace on level 4 and every 2 levels after.
                    return casterLevel >= 4 && casterLevel % 2 == 0;
                case CasterSpellProgression.ThreeQuartersCaster:
                    // 2/3 caster gets to replace on level 5 and every 3 levels after.
                    return casterLevel >= 5 && casterLevel % 3 == 2;
                case CasterSpellProgression.HalfCaster:
                    // 1/2 caster gets to replace on level 8 and every 3 levels after.
                    // (these classes aren't implemented yet, but it would be for a class like Bloodrager).
                    return casterLevel >= 8 && casterLevel % 3 == 2;
            }
            return false;
        }

        internal static void HandleUpdateCasterLevel(UnitDescriptor unit, LevelUpState state, Spellbook spellbook)
        {
            var casterLevel = spellbook.CasterLevel;
            if (CanReplaceSpellThisLevel(spellbook))
            {
                Dictionary<int, BlueprintFeatureSelection> levelEntries;
                if (!casterProgressions.TryGetValue(spellbook.Blueprint, out levelEntries))
                {
                    Log.Write($"Error: could not find spellbook {spellbook.Blueprint.name}" +
                        $", keys are: {String.Join(", ", casterProgressions.Keys.Select(k => k.name))}");
                    return;
                }

                BlueprintFeatureSelection feat;
                if (levelEntries.TryGetValue(casterLevel, out feat))
                {
                    state.AddSelection(null, feat, feat, casterLevel);
                }
            }
        }

        static BlueprintFeature CreateKeepAllSpellsFeat(BlueprintSpellbook spellbook)
        {
            var feat = Helpers.CreateFeature(spellbook.name + "KnownAllSpellsFeature",
                "Keep all spells", "Choose this to skip replacing a known spell this level.",
                Helpers.MergeIds(spellbook.AssetGuid, "b09dfa039e3e4893a8c1f5df5c7f8195"),
                spellbook.CharacterClass.Icon,
                FeatureGroup.None);
            feat.HideInUI = true;
            feat.Ranks = 10;
            return feat;
        }

        static String[] guidsByLevel = new String[] {
            "d3d362e8bb3d4bdea4ce3364dcdb0482",
            "55dbdf7da08640259b1b47e4f34376b7",
            "e2dbdac3ac654bd7955950d0755a2a7a",
            "a527cd61c5dc45ecb622a6ce09163be3",
            "6b4547d6aa0b4af8b90ee225660c715d",
            "c0b10917d6b3474f8287c7747e3051ed",
            "e9dd0530c88747fc8f0af6804b1e327e",
            "e42efccf6df746d9bd9cb985226cba8d",
            "cfce0551a012487f9069b4495c2db2d7",
        };

        static Dictionary<BlueprintSpellbook, Dictionary<int, BlueprintFeatureSelection>> casterProgressions = new Dictionary<BlueprintSpellbook, Dictionary<int, BlueprintFeatureSelection>>();
    }


    public class MaxLevelSpellFromSpellbook : CustomParamSelection
    {
        protected override IEnumerable<BlueprintScriptableObject> GetItems(UnitDescriptor beforeLevelUpUnit, UnitDescriptor previewUnit)
        {
            return GetAllItems();
        }

        protected override IEnumerable<BlueprintScriptableObject> GetAllItems()
        {
            var items = new List<BlueprintScriptableObject>();
            var spellsByLevel = SpellcasterClass.Spellbook.SpellList.SpellsByLevel;
            for (int level = 1; level <= SpellLevel && level < spellsByLevel.Length; level++)
            {
                items.AddRange(spellsByLevel[level].Spells);
            }
            return items.ToArray();
        }
    }

    public class ReplaceSpellSelection : MaxLevelSpellFromSpellbook
    {

        protected override IEnumerable<BlueprintScriptableObject> GetItems(UnitDescriptor beforeLevelUpUnit, UnitDescriptor previewUnit)
        {
            var spells = new List<BlueprintAbility>();

            var classData = previewUnit.Progression.GetClassData(SpellcasterClass);
            var classSpellbook = classData?.Spellbook;
            if (classSpellbook == null) return spells;

            var spellbook = previewUnit.GetSpellbook(classSpellbook);
            if (spellbook == null) return spells;

            // Bloodline/Mystery spells cannot legally be replaced.
            var grantedSpells = new HashSet<BlueprintAbility>(previewUnit.Progression.Features.Enumerable
                .SelectMany(f => f.Blueprint.GetComponents<AddKnownSpell>()).Select(a => a.Spell));

            for (int level = 1; level <= SpellLevel; level++)
            {
                foreach (var knownSpell in spellbook.GetKnownSpells(level))
                {
                    var spell = knownSpell.Blueprint;
                    if (grantedSpells.Contains(spell)) continue;
                    spells.Add(spell);
                }
            }
            return spells;
        }
    }

    [AllowedOn(typeof(BlueprintParametrizedFeature))]
    public class ReplaceSpellParametrized : ParametrizedFeatureComponent, ILevelUpCompleteUIHandler
    {
        public BlueprintSpellbook Spellbook;

        [JsonProperty]
        bool m_Applied = false;

        public override void OnFactActivate()
        {
            if (m_Applied) return;
            m_Applied = true;

            var spell = Param.Blueprint as BlueprintAbility;
            if (spell == null) return;

            var spellbook = Owner.GetSpellbook(Spellbook);
            if (spellbook == null) return;

            var spellLevel = spellbook.GetSpellLevel(spell);
            // If we're in the level-up UI, add a new known spell choice
            var levelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
            if (Owner == levelUp.Preview || Owner == levelUp.Unit)
            {
                var state = levelUp.State;
                var spellSelection = state.DemandSpellSelection(spellbook.Blueprint, spellbook.Blueprint.SpellList);
                int existingNewSpells = spellSelection.LevelCount[spellLevel]?.SpellSelections.Length ?? 0;

                Log.Write($"Adding spell selection to level {spellLevel}");
                spellSelection.SetLevelSpells(spellLevel, 1 + existingNewSpells);
            }
            // Remove the spell
            Log.Write($"Removing spell {spell.name}");
            spellbook.RemoveSpell(spell);
        }

        public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
        {
            if (unit.Descriptor == Owner)
            {
                // Remove this fact to free some memory. There is no reason to keep it after level-up.
                Owner.RemoveFact(Fact);
            }
        }

        public override void OnFactDeactivate()
        {
            Log.Write($"ReplaceSpellParametrized.OnFactDeactivate: {Spellbook.name}");
        }
    }
    // This intercepts ApplySpellbook.Apply, so we can update the spellbook whenever
    // Prestigious Spellcaster causes us to gain a casting level that otherwise would
    // not have been gained.
    //
    // I don't think there is any way around this, as there is no level-up event that
    // necessarily fires when ApplySpellbook.Apply and prestige class spellbook selection
    // happen. So we can't reliably handle taking Prestigious Spellcaster in advance, and
    // have it increase the spells known ofr that class.
    [Harmony12.HarmonyPatch(typeof(ApplySpellbook), "Apply", new Type[] { typeof(LevelUpState), typeof(UnitDescriptor) })]
    static class ApplySpellbook_Apply_Patch
    {
        internal static readonly List<Action<LevelUpState, UnitDescriptor, int>> onApplySpellbook = new List<Action<LevelUpState, UnitDescriptor, int>>();

        static int previousCasterLevel;

        static bool Prefix(ApplySpellbook __instance, LevelUpState state, UnitDescriptor unit)
        {
            try
            {
                previousCasterLevel = 0;
                // Note: we need to get the spellbook from class data, because spellbooks can get
                // replaced. For example, we took a Sorcerer level but the spellbook is actually
                // EmpyrealSorcerer's spellbook. Similar for prestige classes (e.g. we took a level
                // of Eldritch Knight, but the spellbook is actually the Wizard spellbook). 
                var spellbook = unit.GetSpellbook(state.SelectedClass);
                if (spellbook != null) previousCasterLevel = spellbook.CasterLevel;
            }
            catch (Exception e)
            {
                Log.Error($"Error in ApplySpellbook.Apply (prefix): {e}");
            }
            return true;
        }

        static void Postfix(ApplySpellbook __instance, LevelUpState state, UnitDescriptor unit)
        {
            try
            {
                foreach (var action in onApplySpellbook)
                {
                    action(state, unit, previousCasterLevel);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error in ApplySpellbook.Apply (postfix): {e}");
            }
        }
    }

    public abstract class CustomParamSelection : BlueprintParametrizedFeature, IFeatureSelection
    {
        FeatureUIData[] cached;

        public new IEnumerable<IFeatureSelectionItem> Items
        {
            get
            {
                try
                {
                    return cached ?? (cached = BlueprintsToItems(GetAllItems()).ToArray());
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    return cached = Array.Empty<FeatureUIData>();
                }
            }
        }
        IEnumerable<IFeatureSelectionItem> IFeatureSelection.Items => Items;

        public CustomParamSelection()
        {
            this.ParameterType = FeatureParameterType.Custom;
        }

        protected abstract IEnumerable<BlueprintScriptableObject> GetAllItems();

        protected abstract IEnumerable<BlueprintScriptableObject> GetItems(UnitDescriptor beforeLevelUpUnit, UnitDescriptor previewUnit);

        protected virtual bool CanSelect(UnitDescriptor unit, FeatureParam param) => true;

        bool IFeatureSelection.CanSelect(UnitDescriptor unit, LevelUpState state, FeatureSelectionState selectionState, IFeatureSelectionItem item)
        {
            if (HasNoSuchFeature)
            {
                var feat = item.Param.Value.Blueprint as BlueprintFeature;
                if (feat != null && unit.HasFact(feat)) return false;
            }
            return CanSelect(unit, item.Param.Value);
        }

        IEnumerable<IFeatureSelectionItem> IFeatureSelection.ExtractSelectionItems(UnitDescriptor beforeLevelUpUnit, UnitDescriptor previewUnit)
        {
            try
            {
                Log.Write("IFeatureSelection.ExtractSelectionItems");
                var items = GetItems(beforeLevelUpUnit, previewUnit);
                Log.Append($"GetItems(): {GetType().Name}, name: {name}, guid: {AssetGuid}");
                foreach (var b in items)
                {
                    Log.Append($"  - {b.name}");
                }
                Log.Flush();
                return BlueprintsToItems(items);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return Array.Empty<IFeatureSelectionItem>();
        }

        IEnumerable<FeatureUIData> BlueprintsToItems(IEnumerable<BlueprintScriptableObject> items)
        {
            return items.Select(scriptable =>
            {
                var fact = scriptable as BlueprintUnitFact;
                return new FeatureUIData(this, scriptable, fact?.Name, fact?.Description, fact?.Icon, scriptable.name);
            });
        }
    }

}