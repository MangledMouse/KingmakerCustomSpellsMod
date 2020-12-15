using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CowWithHatsCustomSpellsMod
{
    class SpellReplacementHelpers
    {
        public static T CreateParamSelection<T>(String name, String displayName, String description, String guid, Sprite icon,
          FeatureGroup group, params BlueprintComponent[] components) where T : BlueprintParametrizedFeature
        {
            var feat = Create<T>();
            SetFeatureInfo(feat, name, displayName, description, guid, icon, group, components);
            return feat;
        }

        public static void SetFeatureInfo(BlueprintFeature feat, String name, String displayName, String description, String guid, Sprite icon,
            FeatureGroup group, params BlueprintComponent[] components)
        {
            feat.name = name;
            feat.SetComponents(components);
            feat.Groups = new FeatureGroup[] { group };
            feat.SetNameDescriptionIcon(displayName, description, icon);
            Main.library.AddAsset(feat, guid);
        }

        public static T Create<T>(Action<T> init = null) where T : ScriptableObject
        {
            var result = ScriptableObject.CreateInstance<T>();
            if (init != null) init(result);
            return result;
        }

        internal static String MergeIds(String guid1, String guid2, String guid3 = null)
        {
            // Parse into low/high 64-bit numbers, and then xor the two halves.
            ulong low = ParseGuidLow(guid1);
            ulong high = ParseGuidHigh(guid1);

            low ^= ParseGuidLow(guid2);
            high ^= ParseGuidHigh(guid2);

            if (guid3 != null)
            {
                low ^= ParseGuidLow(guid3);
                high ^= ParseGuidHigh(guid3);
            }

            return high.ToString("x16") + low.ToString("x16");
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

        // Parses the lowest 64 bits of the Guid (which corresponds to the last 16 characters).
        static ulong ParseGuidLow(String id) => ulong.Parse(id.Substring(id.Length - 16), NumberStyles.HexNumber);

        // Parses the high 64 bits of the Guid (which corresponds to the first 16 characters).
        static ulong ParseGuidHigh(String id) => ulong.Parse(id.Substring(0, id.Length - 16), NumberStyles.HexNumber);

    }
}
