﻿using CallOfTheWild;
using CallOfTheWild.DismissSpells;
using CallOfTheWild.NewMechanics;
using CallOfTheWild.TeamworkMechanics;
using CowWithHatsCustomSpellsMod.AlliedSpellcasterFix;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;
using static CallOfTheWild.MetamagicFeats;

namespace CowWithHatsCustomSpellsMod
{
    class Core
    {
        static LibraryScriptableObject library => Main.library;
        // All classes (including prestige classes).
        public static List<BlueprintCharacterClass> classes;
        public static BlueprintArchetype eldritchScionArchetype;
        static BlueprintCharacterClass cleric_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
        static BlueprintCharacterClass inquisitor_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");
        static public BlueprintProgression law_archon_domain;
        static public BlueprintProgression law_archon_domain_secondary;
        static BlueprintFeatureSelection cleric_domain_selection = library.Get<BlueprintFeatureSelection>("48525e5da45c9c243a343fc6545dbdb9");
        static BlueprintFeatureSelection cleric_secondary_domain_selection = library.Get<BlueprintFeatureSelection>("43281c3d7fe18cc4d91928395837cd1e");


        static internal void preLoad()
        {
            NewSpells.load();
            //at the moment I am failing to remove the appropriate immunities from the enemy i feel like removing them from
            //BalanceUpdates.load();
            //Main.logger.Log("Preload reached");
        }

        static internal void postLoad()
        {
            ClassUpdates.load();
            
        }


        internal static void Load()
        {
            const String eldritchScionClassId = "f5b8c63b141b2f44cbb8c2d7579c34f5";
            classes = library.Root.Progression.CharacterClasses.Where(c => c.AssetGuid != eldritchScionClassId).ToList();
        }

        internal static void AddSpellReplacement()
        {
            SpellReplacement.Load();
        }

        static internal void UpdateDismiss()
        {
            BlueprintAbility dismissAbility = library.Get<BlueprintAbility>("5b21f6f7f14347948a2b77e3ae7e1fc4");
            dismissAbility.SetDescription("This ability allows caster to dismiss any area effect that they created at the target location, to dismiss a summoned or animated creature, or to release a dominated creature from control.");
            ContextActionDismissSpell dismiss = Helpers.Create<ExpandedContextActionDismissSpell>();
            dismissAbility.GetComponent<AbilityEffectRunAction>().Actions = Helpers.CreateActionList(dismiss);
            dismissAbility.ReplaceComponent<AbilityTargetCanDismiss>(Helpers.Create<ExpandedAbilityTargetCanDismiss>());
        }

        static internal void UpdateDominationEffects()
        {
            BlueprintBuff dominate_person = library.Get<BlueprintBuff>("c0f4e1c24c9cd334ca988ed1bd9d201f");
            AddControllableToChangeFaction(dominate_person.GetComponent<ChangeFaction>());
            BlueprintBuff control_undead = library.Get<BlueprintBuff>("21d20a30b93e4ae281a6d70d9ae1a64d");
            AddControllableToChangeFaction(control_undead.GetComponent<ChangeFaction>());
        }

        static internal void AddControllableToChangeFaction(ChangeFaction cf)
        {
            Helpers.SetField(cf, "m_AllowDirectControl", true);
        }

        internal static void RemoveSlumberNerf()
        {
            BlueprintAbility slumber_hex = library.Get<BlueprintAbility>("31f0fa4235ad435e95ebc89d8549c2ce");
            slumber_hex.RemoveComponents<AbilityTargetCasterHDDifference>();
            slumber_hex.SetNameDescription(slumber_hex.GetName(),
                "Effect: A witch can cause a creature within 30 feet to fall into a deep, magical sleep, as per the spell sleep. The creature receives a Will save to negate the effect. If the save fails, the creature falls asleep for a number of rounds equal to the witch’s level.\n"
                                                    + "The creature will not wake due to noise or light, but others can rouse it with a standard action. This hex ends immediately if the creature takes damage. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.");
            BlueprintAbility shaman_slumber_hex = library.Get<BlueprintAbility>("c04cde18e91e4f84898de92a372bc1e0");
            shaman_slumber_hex.RemoveComponents<AbilityTargetCasterHDDifference>();
            shaman_slumber_hex.SetNameDescription(shaman_slumber_hex.GetName(), 
            "Effect: A shaman can cause a creature within 30 feet to fall into a deep, magical sleep, as per the spell sleep. The creature receives a Will save to negate the effect. If the save fails, the creature falls asleep for a number of rounds equal to the witch’s level.\n"
                                                    + "The creature will not wake due to noise or light, but others can rouse it with a standard action. This hex ends immediately if the creature takes damage. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.");

            BlueprintAbility divine_scourge_slumber_hex = library.Get<BlueprintAbility>("59a600c4da4341fc8e635be2786c760b");
            divine_scourge_slumber_hex.RemoveComponents<AbilityTargetCasterHDDifference>();
            divine_scourge_slumber_hex.SetNameDescription(divine_scourge_slumber_hex.GetName(),
            "Effect: A divine scourge can cause a creature within 30 feet to fall into a deep, magical sleep, as per the spell sleep. The creature receives a Will save to negate the effect. If the save fails, the creature falls asleep for a number of rounds equal to the witch’s level.\n"
                                                    + "The creature will not wake due to noise or light, but others can rouse it with a standard action. This hex ends immediately if the creature takes damage. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.");

            //hexstrike 82cad6f016c04bf49fa851f5e9e10953

            BlueprintAbility hexstrike_slumber = library.Get<BlueprintAbility>("82cad6f016c04bf49fa851f5e9e10953");
            hexstrike_slumber.SetNameDescription(hexstrike_slumber.GetName(),
                "Effect: A witch can cause a creature within 30 feet to fall into a deep, magical sleep, as per the spell sleep. The creature receives a Will save to negate the effect. If the save fails, the creature falls asleep for a number of rounds equal to the witch’s level.\n"
                                                    + "The creature will not wake due to noise or light, but others can rouse it with a standard action. This hex ends immediately if the creature takes damage. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.");

            BlueprintFeature slumber_hex_feature = library.Get<BlueprintFeature>("c086eeb69a4442df9c4bb8469a2c362d");
            slumber_hex_feature.SetNameDescription(slumber_hex.GetName(),
                "Effect: A witch can cause a creature within 30 feet to fall into a deep, magical sleep, as per the spell sleep. The creature receives a Will save to negate the effect. If the save fails, the creature falls asleep for a number of rounds equal to the witch’s level.\n"
                                                    + "The creature will not wake due to noise or light, but others can rouse it with a standard action. This hex ends immediately if the creature takes damage. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.");
            BlueprintFeature shaman_slumber_hex_feature = library.Get<BlueprintFeature>("ee7a8e5dc78a4d6c9e44d88affe47088");
            shaman_slumber_hex_feature.SetNameDescription(shaman_slumber_hex.GetName(),
                "Effect: A shaman can cause a creature within 30 feet to fall into a deep, magical sleep, as per the spell sleep. The creature receives a Will save to negate the effect. If the save fails, the creature falls asleep for a number of rounds equal to the witch’s level.\n"
                                                    + "The creature will not wake due to noise or light, but others can rouse it with a standard action. This hex ends immediately if the creature takes damage. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.");

            BlueprintFeature divine_scourge_slumber_hex_feature = library.Get<BlueprintFeature>("4192c07404ac434fa9b7633a35d3ae81");
            shaman_slumber_hex_feature.SetNameDescription(divine_scourge_slumber_hex.GetName(),
                "Effect: A divine scourge can cause a creature within 30 feet to fall into a deep, magical sleep, as per the spell sleep. The creature receives a Will save to negate the effect. If the save fails, the creature falls asleep for a number of rounds equal to the witch’s level.\n"
                                                    + "The creature will not wake due to noise or light, but others can rouse it with a standard action. This hex ends immediately if the creature takes damage. Whether or not the save is successful, a creature cannot be the target of this hex again for 1 day.");

        }

        internal static void UpdateSilence()
        {
            NewSpells.CreateUpdatedSilenceSpell();
        }

        internal static void FixEarpierce()
        {
            NewSpells.UpdateEarPiercingScream();
        }

        internal static void ChangeInspireRage()
        {
            BlueprintBuff inspiredRageBuff = library.Get<BlueprintBuff>("77038b4555324455b1d110fbe2fbc0ef");
            inspiredRageBuff.RemoveComponents<SpellDescriptorComponent>();
            BlueprintBuff inspireRageEffectBuff = library.Get<BlueprintBuff>("78a69c73d07842c0b46aa9038c40114c");
            inspireRageEffectBuff.RemoveComponents<SpellDescriptorComponent>();
            BlueprintAbilityAreaEffect inspireRageArea = library.Get<BlueprintAbilityAreaEffect>("5421df42956841e7a0499568080421b7");
            inspireRageArea.RemoveComponents<SpellDescriptorComponent>();

            BlueprintBuff controlledInspiredRageDexBuff = library.Get<BlueprintBuff>("ed0106d16661497a8147af6175a55dc8");
            controlledInspiredRageDexBuff.RemoveComponents<SpellDescriptorComponent>();
            BlueprintBuff controlledInspiredRageStrBuff = library.Get<BlueprintBuff>("de4a813aeee84a329e3b8ed10e9caeec");
            controlledInspiredRageStrBuff.RemoveComponents<SpellDescriptorComponent>();
            BlueprintBuff controlledInspiredRageConBuff = library.Get<BlueprintBuff>("cd2956555ab04d46ad0525947fe19846");
            controlledInspiredRageConBuff.RemoveComponents<SpellDescriptorComponent>();


            //This messes with ability funcitoning at all. Will keep it here because it has the IDs of relevant blueprints to the buff in case there are other changes that need to be made
            //BlueprintBuff controlledRageDexBuff = library.Get<BlueprintBuff>("ed0106d16661497a8147af6175a55dc8");
            //controlledRageDexBuff.RemoveComponents<SpellDescriptorComponent>();
            //BlueprintBuff controlledRageStrBuff = library.Get<BlueprintBuff>("de4a813aeee84a329e3b8ed10e9caeec");
            //controlledRageStrBuff.RemoveComponents<SpellDescriptorComponent>();
            //BlueprintBuff controlledRageConBuff = library.Get<BlueprintBuff>("cd2956555ab04d46ad0525947fe19846");
            //controlledRageConBuff.RemoveComponents<SpellDescriptorComponent>();

            BlueprintBuff insightfulContemplationBuff = library.Get<BlueprintBuff>("6b4882478b094ccf99f17fea891d9d14");
            insightfulContemplationBuff.RemoveComponents<SpellDescriptorComponent>();
            BlueprintBuff insightfulContemplationAbilityBuff = library.Get<BlueprintBuff>("ebe4fa61b8594e5e9dc084495fa86c2f");
            insightfulContemplationAbilityBuff.RemoveComponents<SpellDescriptorComponent>();
            BlueprintAbilityAreaEffect insightfulContemplationAbilityArea = library.Get<BlueprintAbilityAreaEffect>("6b8a0589e9af4227a22b9ea5a1098559");
            insightfulContemplationAbilityArea.RemoveComponents<SpellDescriptorComponent>();

        }


        //SkaldCourtPoetInsigtfulContemplationBuff	6b4882478b094ccf99f17fea891d9d14	Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff
        //SkaldCourtPoetInsightfulContemplationAbilityBuff ebe4fa61b8594e5e9dc084495fa86c2f Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff
        //SkaldCourtPoetInsightfulContemplationAbilityArea    6b8a0589e9af4227a22b9ea5a1098559 Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbilityAreaEffect
        public static BlueprintFeatureSelection tweakedFeat;
        internal static void UpdateAmiri()
        {
            var amiri_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("b3f29faef0a82b941af04f08ceb47fa2");
            amiri_companion.Strength = 16;//+2
            amiri_companion.Dexterity = 13;
            amiri_companion.Constitution = 14;
            amiri_companion.Intelligence = 10;
            amiri_companion.Wisdom = 12;
            amiri_companion.Charisma = 10;
            var amiri1_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("df943986ee329e84a94360f2398ae6e6");
            var amiri_class_level = amiri1_feature.GetComponent<AddClassLevels>();
            amiri_class_level.Archetypes = new BlueprintArchetype[] { };
            amiri_class_level.RaceStat = Kingmaker.EntitySystem.Stats.StatType.Strength;
            amiri_class_level.Selections[0].Features[1] = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5"); //power attack feature
            amiri_class_level.Selections[0].Features[2] = library.Get<BlueprintFeature>("57299a78b2256604dadf1ab9a42e2873"); //weapon focus bastard sword

            //Main.logger.Log("All features of amiri add class levels component");
            //foreach (BlueprintFeature bf in amiri_class_level.Selections[0].Features)
            //{
            //    Main.logger.Log("A feature with name " + bf.name + " and type " + bf.GetType().ToString());
            //}

            amiri_class_level.Skills = new StatType[] { StatType.SkillPersuasion, StatType.SkillAthletics, StatType.SkillLoreNature, StatType.SkillPerception };

            //There is an error in tweak or treat breaks Amiri's human feat selection with 
            
        }

        internal static void FixForTweakOrTreatHumanCompanionFeats()
        {
            tweakedFeat = library.TryGet<BlueprintFeatureSelection>("c8a37ee56f164659baaa312027b9c42f");
            if (tweakedFeat != null)
            {
                UpdateForTweakOrTreat();
            }
        }

        internal static void UpdateForTweakOrTreat()
        {
            Main.logger.Log("Tweak or Treat mod found. Fixing human companions across versions");

            //foreach (BlueprintUnitFact fact in amiri_companion.AddFacts)
            //{
            //    Main.logger.Log("Amiri units add facts " + fact.AssetGuid + " with name " + fact.name);
            //}
            //AddFacts PowerAttackFact = CallOfTheWild.Helpers.CreateAddFact(powerAttackFeat);

            var amiri_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("b3f29faef0a82b941af04f08ceb47fa2");
            BlueprintFeature powerAttackFeat = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5");
            BlueprintFeature toughness = library.Get<BlueprintFeature>("d09b20029e9abfe4480b356c92095623");
            BlueprintUnitFact[] amiriAddFacts = new BlueprintUnitFact[amiri_companion.AddFacts.Length + 1];
            for (int i = 0; i < amiri_companion.AddFacts.Length; i++)
            {
                amiriAddFacts[i] = amiri_companion.AddFacts[i];
            }
            if (Main.settings.iconic_amiri || CallOfTheWild.Main.settings.update_companions)
                amiriAddFacts[amiriAddFacts.Length - 1] = powerAttackFeat;
            else
                amiriAddFacts[amiriAddFacts.Length - 1] = toughness;
            amiri_companion.AddFacts = amiriAddFacts;
            //foreach (BlueprintUnitFact fact in amiri_companion.AddFacts)
            //{
            //    Main.logger.Log("Amiri units add facts " + fact.AssetGuid + " with name " + fact.name);
            //}

            BlueprintFeature selectiveChannelFeature = library.Get<BlueprintFeature>("fd30c69417b434d47b6b03b9c1f568ff");
            BlueprintFeature impInit = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74");
            var tristain_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("f6c23e93512e1b54dba11560446a9e02");
            BlueprintUnitFact[] tristAddFacts = new BlueprintUnitFact[tristain_companion.AddFacts.Length + 1];
            for (int i = 0; i < tristain_companion.AddFacts.Length; i++)
                tristAddFacts[i] = tristain_companion.AddFacts[i];
            if (CallOfTheWild.Main.settings.update_companions)
            {
                tristAddFacts[tristAddFacts.Length - 1] = impInit;
            }
            else
            {
                tristAddFacts[tristAddFacts.Length - 1] = selectiveChannelFeature;
            }
            tristain_companion.AddFacts = tristAddFacts;

            BlueprintFeature pointBlankShotFeat = library.Get<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab");
            BlueprintFeature preciseShotFeat = library.Get<BlueprintFeature>("8f3d1e6b4be006f4d896081f2f889665");
            BlueprintFeature longbowFocusFeat = library.Get<BlueprintFeature>("f641e7c569328614c87e0270ac5325dd");
            BlueprintFeature boonCompanion = library.Get<BlueprintFeature>("8fc01f06eab4dd946baa5bc658cac556");
            var ekun_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("d5bc1d94cd3e5be4bbc03f3366f67afc");
            //Main.logger.Log("Ekun companion add facts BEFORE I mess with him.");
            //for (int i = 0; i < ekun_companion.AddFacts.Length; i++)
            //{
            //    Main.logger.Log("Ekun Companion has add fact with name " + ekun_companion.AddFacts[i].Name + " and content " + ekun_companion.AddFacts[i].ToString() + " and type " + ekun_companion.AddFacts[i].GetType().ToString());
            //    BlueprintFeature bf = ekun_companion.AddFacts[i] as BlueprintFeature;
            //    if (bf != null)
            //    {
            //        Main.logger.Log("blueprint feature");
            //        foreach (BlueprintComponent bc in bf.ComponentsArray)
            //        {
            //            Main.logger.Log("A blueprint component with name " + bc.name + " and to string value " + bc.ToString() + " and type " + bc.GetType());
            //            AddFacts af = bc as AddFacts;
            //            if (af != null)
            //            {
            //                foreach (BlueprintUnitFact buf in af.Facts)
            //                    Main.logger.Log("A blueprintunitfact with name " + buf.name + " and to string value " + buf.ToString() + " and type " + buf.GetType());
            //            }
            //            AddClassLevels acl = bc as AddClassLevels;
            //            if (acl != null)
            //            {
            //                foreach (SelectionEntry se in acl.Selections)
            //                {
            //                    Main.logger.Log("A selection in this add class level " + se.ToString());
            //                    foreach (BlueprintFeature features in se.Features)
            //                    {
            //                        Main.logger.Log("A feature in this selection " + features.name);
            //                    }
            //                }
            //            }
            //        }

            //    }
            //}

            
            BlueprintUnitFact[] ekunAddFacts = new BlueprintUnitFact[ekun_companion.AddFacts.Length + 4];
            for (int i = 0; i < ekun_companion.AddFacts.Length; i++)
                ekunAddFacts[i] = ekun_companion.AddFacts[i];
            ekunAddFacts[ekunAddFacts.Length - 1] = pointBlankShotFeat;
            ekunAddFacts[ekunAddFacts.Length - 2] = preciseShotFeat;
            ekunAddFacts[ekunAddFacts.Length - 3] = longbowFocusFeat;
            ekunAddFacts[ekunAddFacts.Length - 4] = boonCompanion;
            ekun_companion.AddFacts = ekunAddFacts;

            //int theLength = ekun_companion.AddFacts.Length + 1;
            //Main.logger.Log("This is the length I am setting the new array to " + theLength);
            //BlueprintUnitFact[] ekunAddFacts = new BlueprintUnitFact[ekun_companion.AddFacts.Length + 1];
            //for (int i = 0; i < ekun_companion.AddFacts.Length; i++)
            //    ekunAddFacts[i] = ekun_companion.AddFacts[i];
            ////ekunAddFacts[ekunAddFacts.Length - 1] = powerAttackFeat; 
            //ekunAddFacts[ekunAddFacts.Length - 1] = impInit;
            //ekun_companion.AddFacts = ekunAddFacts;

            //Main.logger.Log("Ekun companion add facts AFTER I mess with him.");
            //for (int i = 0; i < ekun_companion.AddFacts.Length; i++)
            //{
            //    Main.logger.Log("Ekun Companion has add fact with name "+ ekun_companion.AddFacts[i].Name + " and content " + ekun_companion.AddFacts[i].ToString());
            //}


            //Main.logger.Log("Valerie companion start");
           // Main.logger.Log("Valerie companion add facts BEFORE I mess with her.");
            
            BlueprintFeature dodgeFeature = library.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5");
            BlueprintFeature shieldFocus = library.Get<BlueprintFeature>("ac57069b6bf8c904086171683992a92a");
            var valerie_compainion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("54be53f0b35bf3c4592a97ae335fe765");
            //for (int i = 0; i < valerie_compainion.AddFacts.Length; i++)
            //{
            //    Main.logger.Log("Valerie Companion has add fact with name " + valerie_compainion.AddFacts[i].Name + " and content " + valerie_compainion.AddFacts[i].ToString() + " and type " + valerie_compainion.AddFacts[i].GetType().ToString());

            //}
            BlueprintUnitFact[] valAddFacts = new BlueprintUnitFact[valerie_compainion.AddFacts.Length + 1];
            for (int i = 0; i < valerie_compainion.AddFacts.Length; i++)
            {
                valAddFacts[i] = valerie_compainion.AddFacts[i];
            }
            if (CallOfTheWild.Main.settings.update_companions)
            {
                valAddFacts[valAddFacts.Length - 1] = shieldFocus;
            }
            else
                valAddFacts[valAddFacts.Length - 1] = dodgeFeature;
            valerie_compainion.AddFacts = valAddFacts;
            //Main.logger.Log("Valerie companion add facts AFTER I mess with her.");
            //for (int i = 0; i < valerie_compainion.AddFacts.Length; i++)
            //{
            //    Main.logger.Log("Valerie Companion has add fact with name " + valerie_compainion.AddFacts[i].Name + " and content " + valerie_compainion.AddFacts[i].ToString() + " and type " + valerie_compainion.AddFacts[i].GetType().ToString());
            //}
           // Main.logger.Log("Valerie companion finish");

           // Main.logger.Log("Cephal companion start");
            
            var cephal_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("77c5eb949dffb9f45abcc7a78a2d281f");
            BlueprintUnitFact[] cephalAddFacts = new BlueprintUnitFact[cephal_companion.AddFacts.Length + 1];
            BlueprintFeature metamgicExtend = library.Get<BlueprintFeature>("f180e72e4a9cbaa4da8be9bc958132ef");
            for (int i = 0; i < cephal_companion.AddFacts.Length; i++)
            {
                cephalAddFacts[i] = cephal_companion.AddFacts[i];
            }
            if (CallOfTheWild.Main.settings.update_companions)
            {
                cephalAddFacts[cephalAddFacts.Length - 1] = impInit;
            }
            else
                cephalAddFacts[cephalAddFacts.Length - 1] = metamgicExtend;
            cephal_companion.AddFacts = cephalAddFacts;
           // Main.logger.Log("Cephal companion start");

          //  Main.logger.Log("Maegar companion start");
            BlueprintFeature ironWill = library.Get<BlueprintFeature>("175d1577bb6c9a04baf88eec99c66334"); 
            var varn_companion = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("e83a03d50fedd35449042ce73f1b6908");
            BlueprintUnitFact[] varnAddFacts = new BlueprintUnitFact[varn_companion.AddFacts.Length + 1];
            for(int i =0;i<varn_companion.AddFacts.Length;i++)
                varnAddFacts[i] = varn_companion.AddFacts[i];
            varnAddFacts[varnAddFacts.Length-1] = ironWill;
            varn_companion.AddFacts = varnAddFacts;
            //Main.logger.Log("Maegar companion end");
        }

        internal static void UpdateExtendableSpells()
        {
            //These 4 are now redundant with the base Call of the Wild mod
            CallOfTheWild.NewSpells.daze_mass.AvailableMetamagic = CallOfTheWild.NewSpells.daze_mass.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
            CallOfTheWild.NewSpells.command.AvailableMetamagic = CallOfTheWild.NewSpells.command.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
            CallOfTheWild.NewSpells.synaptic_pulse.AvailableMetamagic = CallOfTheWild.NewSpells.synaptic_pulse.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
            CallOfTheWild.NewSpells.synaptic_pulse_greater.AvailableMetamagic = CallOfTheWild.NewSpells.synaptic_pulse_greater.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;

            //This is not offered by the base mod but it is possible there is something that makes it more complicated about it because of the aura it makes
            CallOfTheWild.NewSpells.babble.AvailableMetamagic = CallOfTheWild.NewSpells.babble.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
        }

        internal static void UpdateAlliedSpellcaster()
        {
            int fix_range = 2;
            var allied_spell_caster = library.Get<BlueprintFeature>("9093ceeefe9b84746a5993d619d7c86f");
            allied_spell_caster.GetComponent<AlliedSpellcaster>().Radius = fix_range;
            allied_spell_caster.ReplaceComponent<AlliedSpellcasterSameSpellBonus>(Helpers.Create<AlliedSpellcasterSameSpellBonusExpanded>(a => { a.Radius = fix_range; a.AlliedSpellcasterFact = allied_spell_caster; }));

            //foreach(BlueprintComponent bc in allied_spell_caster.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log("Allied spell caster component " + bc.name + " with type " + bc.GetType().ToString());
            //}

            allied_spell_caster.RemoveComponents<AlliedSpellcaster>();
        }

        internal static void FixFeyThoughts()
        {
            BlueprintFeature feyThoughtsBluff = library.TryGet<BlueprintFeature>("3393b744356648bf8c87021fd24680fb");
            feyThoughtsBluff.AddComponent(Helpers.Create<CallOfTheWild.NewMechanics.AddBonusToSkillCheckIfNoClassSkill>(a => { a.skill = StatType.SkillPersuasion; a.check = StatType.CheckBluff; }));
            BlueprintFeature feyThoughtsDiplomacy = library.TryGet<BlueprintFeature>("557db4b7510d44418fc654e1734ac0c2");
            feyThoughtsDiplomacy.AddComponent(Helpers.Create<CallOfTheWild.NewMechanics.AddBonusToSkillCheckIfNoClassSkill>(a => { a.skill = StatType.SkillPersuasion; a.check = StatType.CheckDiplomacy; }));
        }

        internal static void FixStudiedCombat()
        {
            BlueprintAbility studiedCombatAbillity = library.TryGet<BlueprintAbility>("aadd0473232c44ebb2d7eb3fbd233d92");
            BlueprintAbility studiedCombatNoCdAbillity = library.TryGet<BlueprintAbility>("30d78c01ff964e5881315644c9633ba2");
            BlueprintAbility studiedCombatWrapperAbility = library.TryGet<BlueprintAbility>("306956961f804de09ff4d51372f3533b");
            
            BlueprintAbility studiedCombatQuickened = library.TryGet<BlueprintAbility>("2fc11151c2a1462d8fa87c2b42df1d22");
            BlueprintAbility studiedCombatQuickenedNoCd = library.TryGet<BlueprintAbility>("ef8e87a38ec54aa994d632be06ac09ba");
            if (studiedCombatAbillity == null
                || studiedCombatNoCdAbillity == null
                || studiedCombatWrapperAbility == null
                || studiedCombatQuickened == null
                || studiedCombatQuickenedNoCd == null)
            {
                Main.logger.Log("Failed to find a studied combat component");
            }
            else
            {
                studiedCombatAbillity.Range = AbilityRange.Medium;
                studiedCombatNoCdAbillity.Range = AbilityRange.Medium;
                studiedCombatWrapperAbility.Range = AbilityRange.Medium;
                studiedCombatQuickened.Range = AbilityRange.Medium;
                studiedCombatNoCdAbillity.Range = AbilityRange.Medium;
                Main.logger.Log("Studied Combat range should have been adjusted");
            }
            

        }


        internal static void FixCuriosityInvocation()
        {

            BlueprintBuff EnableCuriosityPatronAspectBuff = library.TryGet<BlueprintBuff>("8feee126c56b4bcf96c57a40da892683");
            BlueprintActivatableAbility EnableCuriosityPatronAspectBuffToggleAbility = library.TryGet<BlueprintActivatableAbility>("50ab43c81b06445c8f81cdc801a869ff");

            List<BlueprintBuff> patronCuriosityBuffs = new List<BlueprintBuff>();
            
            BlueprintBuff curiosityPatronAspectBuff = library.TryGet<BlueprintBuff>("76f185a31cad48eeb6dc00aa82c96235");
            curiosityPatronAspectBuff.SetDescription("The DCs of the invoker’s hexes and patron spells increase by 1. These DCs increase by an additional 1 at 8th level and 16th level.");
            EnableCuriosityPatronAspectBuff.SetDescription("The DCs of the invoker’s hexes and patron spells increase by 1. These DCs increase by an additional 1 at 8th level and 16th level.");
            EnableCuriosityPatronAspectBuffToggleAbility.SetDescription("The DCs of the invoker’s hexes and patron spells increase by 1. These DCs increase by an additional 1 at 8th level and 16th level.");

            patronCuriosityBuffs.Add(curiosityPatronAspectBuff);
            
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("b22516831d554cda90a9fd02d6ce9043")); //WitchAgilityPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("f0a41f4ce4644f4087282cec2be1a128")); //WitchAncestorsPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("0d879099dc47471ca8d71293d720c230")); //WitchAnimalPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("cf215da90d484546bb1042e1776f4b2d")); //WitchAutumnPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("7bb8000787f04b3685100710379f3897")); //WitchDeathPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("7315cdf49ffa4250b84f35e510432958")); //WitchDevotionPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("246ecf0cd5b5443b9bcc082b2e8e5b47")); //WitchElementsPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("a08a50e7e319431d9c0a2eb77e45b4ca")); //WitchEnchantmentPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("138cc3c95ee349ed850416d5db0201ba")); //WitchEndurancePatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("6dbbecd3443c43509c22c124a2f55f48")); //WitchHealingPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("7b0f7b8946c8455db7979b18e4724abf")); //WitchLightPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("2fe57bf3437641138db2b6992025056d")); //WitchMercyPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("17404f1539bb46768bd28845a4c0fd0f")); //WitchMountainPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("cbefffc320034eb184581d7fa5b89d8c")); //WitchPlaguePatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("9459de5d2a5f482ba2abdb7c52d26f8a")); //WitchProtectionPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("4eeb10c67b194ec38047561210d529ff")); //WitchShadowPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("9c626421b2224badaa006e7652c20308")); //WitchSpringPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("5d0732fde9be4a6d8a3c322be54b05a1")); //WitchStormsPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("720669699cd84187b15eba9f27e35dd7")); //WitchStrengthPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("96fd6593b5a64de59d4dbcb49beedb4c")); //WitchSummerPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("c5ce6d23f6034f0bbe05848c6127249f")); //WitchTransformationPatronFeatureCurioistyBuff
            patronCuriosityBuffs.Add(library.TryGet<BlueprintBuff>("21337dae00ba418c83dba601c4178488")); //WitchWinterPatronFeatureCurioistyBuff

            (int, int)[] statProgression = new (int, int)[] { (7, 1), (15, 2), (20, 3) };

            foreach (BlueprintBuff buff in patronCuriosityBuffs)
            {
                ContextRankConfig crc = buff.GetComponent<ContextRankConfig>();
                if(crc != null)
                {
                    ContextRankConfig newCrc = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                    progression: ContextRankProgression.Custom,
                                                                    customProgression: statProgression
                                                                    );
                    buff.ReplaceComponent<ContextRankConfig>(newCrc);
                }
                else
                {
                    Main.logger.Log("Failed to find " + buff.Name + " context rank");
                }
            }
        }

        internal static void fixExplosionRing()
        {
        //    BlueprintItemEquipmentRing explosionRing = library.TryGet<BlueprintItemEquipmentRing>("a4aa862b9e7771d4fae3402aa905ae3b");
        //    BlueprintEquipmentEnchantment[] explosionRingEnchants = (BlueprintEquipmentEnchantment[])(Helpers.GetField(explosionRing, "m_Enchantments"));
        //    if (explosionRingEnchants != null)
        //    {
        //        Main.logger.Log($"Explosion Ring Enchants");
        //        foreach (BlueprintEquipmentEnchantment enchantment in explosionRingEnchants)
        //        {
        //            Main.logger.Log($"Explosion Ring enchant {enchantment.name} and toString value {enchantment.ToString()} and {enchantment.Description}");
        //            Main.logger.Log($"{enchantment.name} components");
        //            foreach (BlueprintComponent bc in enchantment.GetComponents<BlueprintComponent>())
        //            {
        //                Main.logger.Log($"Component name {bc.name} with toString value {bc.ToString()} and type {bc.GetType().ToString()}");
        //                AddUnitFeatureEquipment aufe = bc as AddUnitFeatureEquipment;
        //                if (aufe != null)
        //                {
        //                    Main.logger.Log($"Feature {aufe.name} ");
        //                    if(aufe.Enchantment!=null)
        //                    {
        //                        Main.logger.Log($"enchantment {aufe.Enchantment.Name} and enchantment descrpition {aufe.Enchantment.Description} ");
        //                    }
                        
        //                }
        //            }
        //        }
        //    }

        //    BlueprintItemEquipmentRing acerbicRing = library.TryGet<BlueprintItemEquipmentRing>("1f34a6b309907a44681c689709976bff");
        //    BlueprintEquipmentEnchantment[] acerbicRingEnchants = (BlueprintEquipmentEnchantment[])(Helpers.GetField(acerbicRing, "m_Enchantments"));
        //    if (acerbicRingEnchants != null)
        //    {
        //        Main.logger.Log($"Acerbic Ring Enchants");
        //        foreach (BlueprintEquipmentEnchantment enchantment in acerbicRingEnchants)
        //        {
        //            Main.logger.Log($"Acerbic Ring enchant {enchantment.name} and toString value {enchantment.ToString()} and {enchantment.Description}");
        //            Main.logger.Log($"{enchantment.name} components");
        //            foreach(BlueprintComponent bc in enchantment.GetComponents<BlueprintComponent>())
        //            {
        //                Main.logger.Log($"Component name {bc.name} with toString value {bc.ToString()} and type {bc.GetType().ToString()}");
        //                //AddUnitFeatureEquipment aufe = bc as AddUnitFeatureEquipment;
        //                if (aufe != null)
        //                {
        //                    Main.logger.Log($"Feature {aufe.name} ");
        //                    if (aufe.Enchantment != null)
        //                    {
        //                        Main.logger.Log($"enchantment {aufe.Enchantment.Name} and enchantment descrpition {aufe.Enchantment.Description} ");
        //                    }

        //                }
        //            }
        //        }
        //    }
        }

        internal static void fixUndeadAnatomy()
        {
//UndeadAnatomyIAbility   8d535e198bb44ba2b6cf6ea603753fe4 Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility
//UndeadAnatomyIIAbility  e6161d07f6154750b236b2df94ffa019 Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility
//UndeadAnatomyIIIAbility c4529ff389fb45cfa06dd562daa5af61 Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility
//ScrollOfUndeadAnatomyIAbility   ed5e68f786820a9a39d04ebb08403a7b Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable
//ScrollOfUndeadAnatomyIIAbility  861b2be9fb2306683d2992c29fcaa586 Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable
//ScrollOfUndeadAnatomyIIIAbility a45fa91d84cd04f72f72f57fd190aafe Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable

            BlueprintItemEquipmentUsable undeadAnatomy1Scroll = library.Get<BlueprintItemEquipmentUsable>("ed5e68f786820a9a39d04ebb08403a7b");
            CopyScroll undeadAnatomyScroll1 = undeadAnatomy1Scroll.GetComponent<CopyScroll>();
            undeadAnatomyScroll1.CustomSpell = library.Get<BlueprintAbility>("8d535e198bb44ba2b6cf6ea603753fe4");

            BlueprintItemEquipmentUsable undeadAnatomy2Scroll = library.Get<BlueprintItemEquipmentUsable>("861b2be9fb2306683d2992c29fcaa586");
            CopyScroll undeadAnatomyScroll2 = undeadAnatomy2Scroll.GetComponent<CopyScroll>();
            undeadAnatomyScroll2.CustomSpell = library.Get<BlueprintAbility>("e6161d07f6154750b236b2df94ffa019");

            BlueprintItemEquipmentUsable undeadAnatomy3Scroll = library.Get<BlueprintItemEquipmentUsable>("a45fa91d84cd04f72f72f57fd190aafe");
            CopyScroll undeadAnatomyScroll3 = undeadAnatomy3Scroll.GetComponent<CopyScroll>();
            undeadAnatomyScroll3.CustomSpell = library.Get<BlueprintAbility>("c4529ff389fb45cfa06dd562daa5af61");

            //foreach (BlueprintComponent bc in undeadAnatomy1Scroll.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log($"component {bc.name} has type {bc.GetType()}");
            //    CopyScroll cs = bc as CopyScroll;
            //    if (cs != null)
            //    {
            //        cs.CustomSpell = library.Get<BlueprintAbility>("8d535e198bb44ba2b6cf6ea603753fe4");
            //        Main.logger.Log($"The custom spell is {cs.CustomSpell.name} ");
            //    }
            //}

            //BlueprintItemEquipmentUsable undeadAnatomy1Scroll = library.Get<BlueprintItemEquipmentUsable>("ed5e68f786820a9a39d04ebb08403a7b");
            //undeadAnatomy1Scroll.Ability.AddSpellAndScroll
        }

        internal static void CreateLawArchonSubdomain()
        {
            Main.logger.Log("Made it to Create Law Archon Subdomain method");
            var icon = Helpers.GetIcon("a0bc0525895932b42bfd47f1544e6e35"); //archons aura buff

            var resource = Helpers.CreateAbilityResource("LawArchonSubdomainAuraOfMenaceResource", 
                "Aura Of Menace", 
                "At 8th level, you can emit a 30-foot aura of menace as a standard action. Enemies in this aura take a –2 penalty to AC and on attacks and saves as long as they remain inside the aura. You can use this ability for a number of rounds per day equal to your cleric level. These rounds do not need to be consecutive.", 
                "e7982a5bfeb0456d975988a212a45a9a", 
                icon);
            resource.SetIncreasedByLevel(0, 1, new BlueprintCharacterClass[] { cleric_class, inquisitor_class });
            Main.logger.Log("Created subdomain aura resource");

            var buff = library.Get<BlueprintBuff>("a0bc0525895932b42bfd47f1544e6e35");

            var area_buff = Common.createBuffAreaEffect(buff, 30.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>()), "LawArchonSubdomainArea");
            area_buff.SetNameDescriptionIcon("Aura of Menace",
                                             "At 8th level, you can emit a 30-foot aura of menace as a standard action. Enemies in this aura take a –2 penalty to AC and on attacks and saves as long as they remain inside the aura. You can use this ability for a number of rounds per day equal to your cleric level. These rounds do not need to be consecutive.",
                                             icon);
            Main.logger.Log("Created area buff");

            var toggle = Common.buffToToggle(area_buff, Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard, false,
                                             Helpers.CreateActivatableResourceLogic(resource, ActivatableAbilityResourceLogic.ResourceSpendType.NewRound));


            var law_domain = library.Get<BlueprintProgression>("a723d11a5ae5df0488775e31fac9117d");
            var law_domain_secondary = library.Get<BlueprintProgression>("0d9749df9d68ded438ecdf8527085963");
            var good_domain = library.Get<BlueprintProgression>("243ab3e7a86d30243bdfe79c83e6adb4");
            var good_domain_secondary = library.Get<BlueprintProgression>("efc4219c7894afc438180737adc0b7ac");
            var good_archon_domain = library.Get<BlueprintProgression>("3db8e899931f4cd8a32904512f024b08");
            var good_archon_domain_secondary = library.Get<BlueprintProgression>("d1457e93a5f84860a8fe4f2b5b9f0201");

            var law_domain_greater = library.Get<BlueprintFeature>("3dc5e2b315ff07f438582a2468beb1fb");
            var feature = Common.ActivatableAbilityToFeature(toggle, false);
            feature.AddComponent(resource.CreateAddAbilityResource());
            var spell_list = library.CopyAndAdd<BlueprintSpellList>("57b0bbdc1114ee846945f1808b13cff7", "ArchonLawSubdomainSpellList", "");
            Common.excludeSpellsFromList(spell_list, a => false);

            law_archon_domain = createSubdomain("ArchonLawSubdomain", "Archon (Law) Subdomain",
                                   "You follow a strict and ordered code of laws, and in so doing, achieve enlightenment.\n" +
                                   "Touch of Law: You can touch a willing creature as a standard action, infusing it with the power of divine order and allowing it to treat all attack rolls, skill checks, ability checks, and saving throws for 1 round as if the natural d20 roll resulted in an 11. You can use this ability a number of times per day equal to 3 + your Wisdom modifier.\n" +
                                   $"{toggle.Name}: {toggle.Description}\n" +
                                   "Domain Spells: Divine Favor, Protection from Chaos Communal, Prayer, Protection From Energy Communal, Dominate Person, Summon Monster VI, Dictum, Shield Of Law, Dominate Monster.",
                                   law_domain,
                                   new BlueprintFeature[] { law_domain_greater },
                                   new BlueprintFeature[] { feature },
                                   spell_list
                                   );
            var good_domain_allowed = library.Get<BlueprintFeature>("882521af8012fc749930b03dc18a69de");
            Common.replaceDomainSpell(law_archon_domain, library.Get<BlueprintAbility>("9d5d2d3ffdd73c648af3eb3e585b1113"), 1); //divine favor
            Common.replaceDomainSpell(law_archon_domain, library.Get<BlueprintAbility>("e740afbab0147944dab35d83faa0ae1c"), 6); //summon monster 6
            law_archon_domain.AddComponents(Helpers.PrerequisiteNoFeature(law_domain), Helpers.PrerequisiteNoFeature(good_domain), Helpers.PrerequisiteFeature(good_domain_allowed));

            law_archon_domain_secondary = library.CopyAndAdd(law_archon_domain, "ArchonLawSubdomainSecondaryProgression", "");
            law_archon_domain_secondary.RemoveComponents<LearnSpellList>();

            law_archon_domain_secondary.AddComponents(Helpers.PrerequisiteNoFeature(law_archon_domain),
                                                 Helpers.PrerequisiteNoFeature(law_domain),
                                                 Helpers.PrerequisiteNoFeature(law_domain_secondary));
            law_archon_domain.AddComponents(Helpers.PrerequisiteNoFeature(law_archon_domain_secondary));

            law_domain.AddComponents(Helpers.PrerequisiteNoFeature(law_archon_domain), Helpers.PrerequisiteNoFeature(law_archon_domain_secondary));
            law_domain_secondary.AddComponents(Helpers.PrerequisiteNoFeature(law_archon_domain), Helpers.PrerequisiteNoFeature(law_archon_domain_secondary));
            good_domain.AddComponents(Helpers.PrerequisiteNoFeature(law_archon_domain), Helpers.PrerequisiteNoFeature(law_archon_domain_secondary));
            good_domain_secondary.AddComponents(Helpers.PrerequisiteNoFeature(law_archon_domain), Helpers.PrerequisiteNoFeature(law_archon_domain_secondary));
            good_archon_domain.AddComponents(Helpers.PrerequisiteNoFeature(law_archon_domain), Helpers.PrerequisiteNoFeature(law_archon_domain_secondary));
            good_archon_domain_secondary.AddComponents(Helpers.PrerequisiteNoFeature(law_archon_domain), Helpers.PrerequisiteNoFeature(law_archon_domain_secondary));

            cleric_domain_selection.AllFeatures = AddToFeatureArray(cleric_domain_selection.AllFeatures, good_archon_domain);
            cleric_secondary_domain_selection.AllFeatures = AddToFeatureArray(cleric_secondary_domain_selection.AllFeatures, good_archon_domain_secondary);
            Main.logger.Log("finished creating domain");

        }
        static BlueprintProgression createSubdomain(string name,
                                                   string display_name,
                                                   string description,
                                                   BlueprintProgression base_progression,
                                                   BlueprintFeature[] old_features,
                                                   BlueprintFeature[] new_features,
                                                   BlueprintSpellList spell_list = null)
        {
            bool[] features_replaced = new bool[old_features.Length];
            List<LevelEntry> new_level_entries = new List<LevelEntry>();
            var progression = library.CopyAndAdd(base_progression, name + "Progression", "");
            progression.SetNameDescription(display_name, description);

            foreach (var le in base_progression.LevelEntries)
            {
                var features = le.Features.ToArray();

                for (int i = 0; i < old_features.Length; i++)
                {
                    if (!features_replaced[i])
                    {
                        if (features.Contains(old_features[i]))
                        {
                            features_replaced[i] = true;
                            features = features.RemoveFromArray(old_features[i]);
                            if (new_features[i] != null)
                            {
                                features = AddToFeatureBaseArray(features, new_features[i]);
                            }
                        }
                    }
                }

                if (!features.Empty())
                {
                    new_level_entries.Add(Helpers.LevelEntry(le.Level, features));
                }
            }

            progression.LevelEntries = new_level_entries.ToArray();

            if (spell_list != null)
            {
                progression.ReplaceComponent<LearnSpellList>(l => l.SpellList = spell_list);
            }

            features_replaced = new bool[old_features.Length];
            List<UIGroup> ui_groups = new List<UIGroup>();
            foreach (var uig in base_progression.UIGroups)
            {
                var features = uig.Features.ToArray();

                for (int i = 0; i < old_features.Length; i++)
                {
                    if (!features_replaced[i])
                    {
                        if (features.Contains(old_features[i]))
                        {
                            features_replaced[i] = true;
                            features = features.RemoveFromArray(old_features[i]);
                            if (new_features[i] != null)
                            {
                                features = AddToFeatureBaseArray(features, new_features[i] );
                            }
                        }
                    }
                }

                if (!features.Empty())
                {
                    ui_groups.Add(Helpers.CreateUIGroup(features));
                }
            }

            progression.UIGroups = ui_groups.ToArray();
            //add domain spells
            var f0 = progression.LevelEntries[0].Features[0];
            var comp = f0.GetComponent<AddFeatureOnClassLevel>();
            if (comp != null)
            {
                f0 = library.CopyAndAdd(f0, name + f0.name, "");
                f0.RemoveComponent(comp);
            }

            var give_spells = Helpers.CreateFeature(name + "SpellListFeature",
                                                "",
                                                "",
                                                "",
                                                null,
                                                FeatureGroup.None,
                                                Helpers.Create<AddSpecialSpellList>(a => { a.CharacterClass = cleric_class; a.SpellList = spell_list; })
                                                );

            give_spells.IsClassFeature = true;
            give_spells.HideInUI = true;

            f0.AddComponent(Helpers.CreateAddFeatureOnClassLevel(give_spells, 1, new BlueprintCharacterClass[] { cleric_class }));
            f0.SetNameDescription(progression);
            progression.LevelEntries[0].Features[0] = f0;
            if (base_progression.UIGroups.Length > 0)
            {
                progression.UIGroups[0].Features.Add(f0);
            }

            return progression;
        }

        internal static BlueprintFeature[] AddToFeatureArray(BlueprintFeature[] features, BlueprintFeature feature)
        {
            var len = features.Length;
            var result = new BlueprintFeature[len];
            Array.Copy(features, result, len);
            result[len] = feature;
            return result;
        }

        internal static BlueprintFeatureBase[] AddToFeatureBaseArray(BlueprintFeatureBase[] features, BlueprintFeatureBase value)
        {
            var len = features.Length;
            var result = new BlueprintFeatureBase[len];
            Array.Copy(features, result, len);
            result[len] = value;
            return result;
        }

        internal static void RemovePlanarFocusNerf()
        {
            BlueprintArchetype bloodhunterArchetype = library.Get<BlueprintArchetype>("48e4ff30d3524dbc90a7933107645fd0");
            BlueprintCharacterClass ranger_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");
            BlueprintArchetype naturalist= library.Get<BlueprintArchetype>("5794865f6aa541ce9c09c8e682cea799");
            BlueprintCharacterClass summoner = library.Get<BlueprintCharacterClass>("0f4c4ada51334b43a802350c5c0b85f5");
            BlueprintCharacterClass hunter = library.Get<BlueprintCharacterClass>("32486dcfda61462fbfd66b5644786b39");
            BlueprintCharacterClass inquisitor = library.Get<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");
            BlueprintArchetype sacredHuntsman = library.Get<BlueprintArchetype>("46eb929c8b6d7164188eb4d9bcd0a012");

            RemoveFireNerf(library.Get<BlueprintUnitFact>("ccf0e83ae87f4edfb81fa6fbc3343a5e"), new BlueprintCharacterClass[]{ hunter, inquisitor}, sacredHuntsman,0); //PlanarFocusFireBuff
            RemoveFireNerf(library.Get<BlueprintUnitFact>("2e5fda59676445b4b6e66d49ec84ca10"), new BlueprintCharacterClass[]{ hunter, inquisitor }, sacredHuntsman, 0); //PlanarFocusFire
            BlueprintUnitFact EnablePlanarFocusFireBuff = library.Get<BlueprintUnitFact>("4814d6c8d1f84f1ea570b17b1ec4e604");
            EnablePlanarFocusFireBuff.RemoveComponents<AddWeaponEnergyDamageDice>();
            RemoveFireNerf(EnablePlanarFocusFireBuff, new BlueprintCharacterClass[]{ hunter, inquisitor }, sacredHuntsman, 0); //EnablePlanarFocusFireBuff

            RemoveFireNerf(library.Get<BlueprintUnitFact>("4c02a5cb966344fba103cd328be359bb"), new BlueprintCharacterClass[] { summoner }, naturalist, 2); //NaturalistPlanarFocusFireBuff
            RemoveFireNerf(library.Get<BlueprintUnitFact>("7aba8cf50d73431591480fe7ea01de90"), new BlueprintCharacterClass[] { summoner }, naturalist, 2); //NaturalistPlanarFocusFire
            BlueprintUnitFact EnableNaturalistPlanarFocusFireBuff = library.Get<BlueprintUnitFact>("bafd7cf574fd42b29748d8612bbdc08b");
            EnableNaturalistPlanarFocusFireBuff.RemoveComponents<AddWeaponEnergyDamageDice>();
            RemoveFireNerf(EnableNaturalistPlanarFocusFireBuff, new BlueprintCharacterClass[] { summoner }, naturalist, 2); //EnableNaturalistPlanarFocusFireBuff

            RemoveFireNerf(library.Get<BlueprintUnitFact>("ce14a5a676914248b65110923656f872"), new BlueprintCharacterClass[] { ranger_class }, bloodhunterArchetype, 0); //BloodHunterPlanarFocusFireBuff
            RemoveFireNerf(library.Get<BlueprintUnitFact>("02798c08308b4ebfbf57236ed8dfd51c"), new BlueprintCharacterClass[] { ranger_class }, bloodhunterArchetype, 0); //BloodHunterPlanarFocusFire
            BlueprintUnitFact EnableBloodHunterPlanarFocusFireBuff = library.Get<BlueprintUnitFact>("c2bed503f18f41969ee61bf04e99316b");
            EnableBloodHunterPlanarFocusFireBuff.RemoveComponents<AddWeaponEnergyDamageDice>();
            RemoveFireNerf(EnableBloodHunterPlanarFocusFireBuff, new BlueprintCharacterClass[] { ranger_class }, bloodhunterArchetype, 0); //EnableBloodHunterPlanarFocusFireBuff

            RemoveColdNerf(library.Get<BlueprintUnitFact>("465e5323e25b4bcd863f28a6847bab55"), new BlueprintCharacterClass[] { hunter, inquisitor }, sacredHuntsman, 0); //PlanarFocusColdBuff
            RemoveColdNerf(library.Get<BlueprintUnitFact>("051ed3f6fc36400c991bbbde652a4682"), new BlueprintCharacterClass[] { hunter, inquisitor }, sacredHuntsman, 0); //PlanarFocusCold
            BlueprintUnitFact EnablePlanarFocusColdBuff = library.Get<BlueprintUnitFact>("e20b8a26714249c9b7dc5ef06753ba12");
            EnablePlanarFocusColdBuff.RemoveComponents<ContextRankConfig>();
            RemoveColdNerf(EnablePlanarFocusColdBuff, new BlueprintCharacterClass[] { hunter, inquisitor }, sacredHuntsman, 0); //EnablePlanarFocusColdBuff

            RemoveColdNerf(library.Get<BlueprintUnitFact>("1ee60fa7ec0b4973aa826d1c2a814207"), new BlueprintCharacterClass[] { summoner }, naturalist, 2); //NaturalistPlanarFocusColdBuff
            RemoveColdNerf(library.Get<BlueprintUnitFact>("41eb01770de04378b25a488174f35b35"), new BlueprintCharacterClass[] { summoner }, naturalist, 2); //NaturalistPlanarFocusCold
            BlueprintUnitFact EnableNaturalistPlanarFocusColdBuff = library.Get<BlueprintUnitFact>("7c289ee04b7040e9916864e23426a29d");
            EnableNaturalistPlanarFocusColdBuff.RemoveComponents<ContextRankConfig>();
            RemoveColdNerf(EnableNaturalistPlanarFocusColdBuff, new BlueprintCharacterClass[] { summoner }, naturalist, 2); //EnableNaturalistPlanarFocusColdBuff

            RemoveColdNerf(library.Get<BlueprintUnitFact>("3abd65d42bf64d5dae4abd2b67e71cec"), new BlueprintCharacterClass[] { ranger_class }, bloodhunterArchetype, 0); //BloodHunterPlanarFocusColdBuff
            RemoveColdNerf(library.Get<BlueprintUnitFact>("93d7eadefe84425d9d395dbdcded42d4"), new BlueprintCharacterClass[] { ranger_class }, bloodhunterArchetype, 0); //BloodHunterPlanarFocusCold
            BlueprintUnitFact EnableBloodHunterPlanarFocusColdBuff = library.Get<BlueprintUnitFact>("f5a37f47a5af4f10b413f74024f85aad");
            EnableBloodHunterPlanarFocusColdBuff.RemoveComponents<ContextRankConfig>();
            RemoveColdNerf(EnableBloodHunterPlanarFocusColdBuff, new BlueprintCharacterClass[] { ranger_class }, bloodhunterArchetype, 0); //EnableBloodHunterPlanarFocusColdBuff

            BlueprintFeature planarFocusFeature = library.Get<BlueprintFeature>("e1f63b9863824046bc25dad606cebe36");//PlanarFocusFeature
            planarFocusFeature.SetDescription("When you use your animal focus class feature, you can choose any of the following new aspects unless they conflict with your alignment." + Environment.NewLine + "Planar Focus: Air - You gain limited levitation ability that gives you immunity to difficult terrain and ground based effects." + Environment.NewLine + "Planar Focus: Chaos - Your form shifts subtly, making it difficult for others to aim precise attacks against you. You gain a 25% chance to negate extra damage from critical hits and precision damage from attacks made against you (such as from sneak attacks). Only chaotic characters can use this planar focus." + Environment.NewLine + "Planar Focus: Cold - Creatures that attack you with natural attacks or melee weapons take 1d4 points of cold damage for every 2 class levels you possess." + Environment.NewLine + "Planar Focus: Earth - You gain +2 bonus to CMB when performing bull rush maneuver, and a +2 bonus to CMD when defending against it. You also receive +2 enhancement bonus to your natural armor." + Environment.NewLine + "Planar Focus: Evil - You gain a +1 profane bonus to AC and on saves against attacks made and effects created by good outsiders. This bonus increases to +2 at 12th level. Only evil characters can use this planar focus." + Environment.NewLine + "Planar Focus: Fire - Your natural attacks and melee weapons deal 1d6 points of fire damage for every 4 class levels you possess." + Environment.NewLine + "Planar Focus: Good - You gain a +1 sacred bonus to AC and on saves against attacks made or effects created by evil outsiders. This bonus increases to +2 at 12th level. Only good characters can use this planar focus." + Environment.NewLine + "Planar Focus: Law - You gain immunity to polymorph spells." + Environment.NewLine + "Planar Focus: Shadow - You gain a +5 bonus on Stealth and Trickery checks.");
            BlueprintFeature naturalistPlanarFocusFeature = library.Get<BlueprintFeature>("02f252423252409486b795f753e551ba");//NaturalistPlanarFocusFeature
            naturalistPlanarFocusFeature.SetDescription("When you use your animal focus class feature, you can choose any of the following new aspects unless they conflict with your alignment." + Environment.NewLine + "Planar Focus: Air - You gain limited levitation ability that gives you immunity to difficult terrain and ground based effects." + Environment.NewLine + "Planar Focus: Chaos - Your form shifts subtly, making it difficult for others to aim precise attacks against you. You gain a 25% chance to negate extra damage from critical hits and precision damage from attacks made against you (such as from sneak attacks). Only chaotic characters can use this planar focus." + Environment.NewLine + "Planar Focus: Cold - Creatures that attack you with natural attacks or melee weapons take 1d4 points of cold damage for every 2 class levels you possess." + Environment.NewLine + "Planar Focus: Earth - You gain +2 bonus to CMB when performing bull rush maneuver, and a +2 bonus to CMD when defending against it. You also receive +2 enhancement bonus to your natural armor." + Environment.NewLine + "Planar Focus: Evil - You gain a +1 profane bonus to AC and on saves against attacks made and effects created by good outsiders. This bonus increases to +2 at 12th level. Only evil characters can use this planar focus." + Environment.NewLine + "Planar Focus: Fire - Your natural attacks and melee weapons deal 1d6 points of fire damage for every 4 class levels you possess." + Environment.NewLine + "Planar Focus: Good - You gain a +1 sacred bonus to AC and on saves against attacks made or effects created by evil outsiders. This bonus increases to +2 at 12th level. Only good characters can use this planar focus." + Environment.NewLine + "Planar Focus: Law - You gain immunity to polymorph spells." + Environment.NewLine + "Planar Focus: Shadow - You gain a +5 bonus on Stealth and Trickery checks.");
            BlueprintFeature bloodHunterPlanarFocusFeature = library.Get<BlueprintFeature>("fd4705d7e43b4057a7d0670cb45fda2c");//BloodHunterPlanarFocusFeature
            bloodHunterPlanarFocusFeature.SetDescription("When you use your animal focus class feature, you can choose any of the following new aspects unless they conflict with your alignment." + Environment.NewLine + "Planar Focus: Air - You gain limited levitation ability that gives you immunity to difficult terrain and ground based effects." + Environment.NewLine + "Planar Focus: Chaos - Your form shifts subtly, making it difficult for others to aim precise attacks against you. You gain a 25% chance to negate extra damage from critical hits and precision damage from attacks made against you (such as from sneak attacks). Only chaotic characters can use this planar focus." + Environment.NewLine + "Planar Focus: Cold - Creatures that attack you with natural attacks or melee weapons take 1d4 points of cold damage for every 2 class levels you possess." + Environment.NewLine + "Planar Focus: Earth - You gain +2 bonus to CMB when performing bull rush maneuver, and a +2 bonus to CMD when defending against it. You also receive +2 enhancement bonus to your natural armor." + Environment.NewLine + "Planar Focus: Evil - You gain a +1 profane bonus to AC and on saves against attacks made and effects created by good outsiders. This bonus increases to +2 at 12th level. Only evil characters can use this planar focus." + Environment.NewLine + "Planar Focus: Fire - Your natural attacks and melee weapons deal 1d6 points of fire damage for every 4 class levels you possess." + Environment.NewLine + "Planar Focus: Good - You gain a +1 sacred bonus to AC and on saves against attacks made or effects created by evil outsiders. This bonus increases to +2 at 12th level. Only good characters can use this planar focus." + Environment.NewLine + "Planar Focus: Law - You gain immunity to polymorph spells." + Environment.NewLine + "Planar Focus: Shadow - You gain a +5 bonus on Stealth and Trickery checks.");
        }

        internal static void RemoveColdNerf(BlueprintUnitFact buf, BlueprintCharacterClass[] allowed_classes, BlueprintArchetype allowed_archetype, int delay)
        {
            ContextRankConfig crf = buf.GetComponent<ContextRankConfig>();
            if (crf != null)
            {
                Main.logger.Log($"Replacing Context Rank Config from {buf.name} with updated cold step of 2 instead of 4");
                ContextRankConfig updatedcrf = Helpers.CreateContextRankConfig(ContextRankBaseValueTypeExtender.MasterMaxClassLevelWithArchetype.ToContextRankBaseValueType(),
                                ContextRankProgression.StartPlusDivStep,
                                AbilityRankType.DamageDice,
                                startLevel: delay,
                                stepLevel: 2,
                                classes: allowed_classes, archetype: allowed_archetype);
                buf.ReplaceComponent<ContextRankConfig>(updatedcrf);
            }
            buf.SetDescription("Creatures that attack you with natural attacks or melee weapons take 1d4 points of cold damage for every 2 class levels you possess.");
            
        }

        internal static void RemoveFireNerf(BlueprintUnitFact buf, BlueprintCharacterClass[] allowed_classes, BlueprintArchetype allowed_archetype, int delay)
        {
            AddWeaponEnergyDamageDice awedd = buf.GetComponent<AddWeaponEnergyDamageDice>();
            if(awedd != null)
            {
                Main.logger.Log($"Replacing Weapon Energy damage dice from {buf.name} with updated fire damage dice of d6 instead of d3");
                AddWeaponEnergyDamageDice updatedAwedd = Common.createAddWeaponEnergyDamageDiceBuff(Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.DamageDice)),
                                                                                            DamageEnergyType.Fire,
                                                                                            AttackType.Melee, AttackType.Touch);
                buf.ReplaceComponent<AddWeaponEnergyDamageDice>(updatedAwedd);
            }
            buf.SetDescription("Your natural attacks and melee weapons deal 1d6 points of fire damage for every 4 class levels you possess");
        }

        internal static void FixSheetLightningNotToWorkOnNonLiving()
        {
            var chain_lightning = library.Get<BlueprintAbility>("645558d63604747428d55f0dd3a4cb58");
            var dazed = Common.dazed_non_mind_affecting;
            var dazzled = library.Get<BlueprintBuff>("df6d1025da07524429afbae248845ecc");

            var apply_dazed = Common.createContextActionApplyBuff(dazed, Helpers.CreateContextDuration(1), is_from_spell: true);
            var apply_dazzled = Common.createContextActionApplyBuff(dazzled, Helpers.CreateContextDuration(1), is_from_spell: true);
            var deal_damage = Helpers.CreateActionDealDamage(DamageEnergyType.Electricity, Helpers.CreateContextDiceValue(DiceType.Zero, 0, 1));


            //ContextActionConditionalSaved save_result = Helpers.CreateConditionalSaved(apply_dazzled, apply_dazed);
            //ContextActionSavingThrow context_saved = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(save_result));
            //Common.addConditionalDCIncrease(context_saved, Helpers.CreateConditionsCheckerOr(Helpers.Create<CallOfTheWild.NewMechanics.ContextConditionTargetHasMetalArmor>()), 2);

            //Conditional dazingEffect = Helpers.CreateConditional(Common.createContextConditionHasFacts(false, CallOfTheWild.Common.undead, CallOfTheWild.Common.construct), null, context_saved);

            //AbilityEffectRunAction theEffect = Helpers.CreateRunActions(dazingEffect, deal_damage);

            GameAction[] dazzle_and_damage = new GameAction[] { apply_dazzled, deal_damage };
            GameAction[] daze_and_damage = new GameAction[] { apply_dazed, deal_damage };
            GameAction[] just_damage = new GameAction[] { deal_damage };
            Conditional save_passed = Helpers.CreateConditional(Common.createContextConditionHasFacts(false, CallOfTheWild.Common.undead, CallOfTheWild.Common.construct), just_damage, dazzle_and_damage);
            Conditional save_failed = Helpers.CreateConditional(Common.createContextConditionHasFacts(false, CallOfTheWild.Common.undead, CallOfTheWild.Common.construct), just_damage, daze_and_damage);
            ContextActionConditionalSaved save_result_redux = Helpers.CreateConditionalSaved(save_passed, save_failed);
            ContextActionSavingThrow context_saved_redux = Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(save_result_redux));
            Common.addConditionalDCIncrease(context_saved_redux, Helpers.CreateConditionsCheckerOr(Helpers.Create<CallOfTheWild.NewMechanics.ContextConditionTargetHasMetalArmor>()), 2);

            CallOfTheWild.NewSpells.sheet_lightning.RemoveComponent(CallOfTheWild.NewSpells.sheet_lightning.GetComponent<AbilityEffectRunAction>());
            //CallOfTheWild.NewSpells.sheet_lightning.AddComponent(theEffect);
            CallOfTheWild.NewSpells.sheet_lightning.AddComponent(Helpers.CreateRunActions(context_saved_redux));
        }

        internal static void FixBurstOfRadiance()
        {
            var dazzled = library.Get<BlueprintBuff>("df6d1025da07524429afbae248845ecc");
            var blind = library.Get<BlueprintBuff>("187f88d96a0ef464280706b63635f2af");
            var balance_fixes_in_cotw = CallOfTheWild.Main.settings.balance_fixes;

            ContextDiceValue contextDiceValueIfBalanceFixes = Helpers.CreateContextDiceValue(DiceType.D4);
            if (balance_fixes_in_cotw)
                contextDiceValueIfBalanceFixes = Helpers.CreateContextDiceValue(DiceType.D6);

            ContextActionDealDamage damage = Helpers.CreateActionDealDamage(DamageEnergyType.Divine, contextDiceValueIfBalanceFixes, isAoE: true);
            var apply_damage = Helpers.CreateConditional(Helpers.CreateContextConditionAlignment(AlignmentComponent.Evil),
                                                         damage);
            var duration = Helpers.CreateContextDuration(0, diceType: DiceType.D4, diceCount: 1);

            var apply_blind = Common.createContextActionApplyBuff(blind, duration, is_from_spell: true);
            var apply_dazzled = Common.createContextActionApplyBuff(dazzled, duration, is_from_spell: true);

            GameAction[] dazzle_and_damage = new GameAction[] { apply_damage, apply_dazzled};
            GameAction[] blind_and_damage = new GameAction[] { apply_damage, apply_blind };

            var effect = Helpers.CreateConditionalSaved(dazzle_and_damage, blind_and_damage);

            CallOfTheWild.NewSpells.burst_of_radiance.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(SavingThrowType.Reflex, effect));
        }

        internal static void FixSpellPerfectionMissingGreaterElementalFocus()
        {
            BlueprintParametrizedFeature spell_perfection = library.Get<BlueprintParametrizedFeature>("bf89280c487c4539abcccd0d55a9a56e");
            foreach(BlueprintComponent bc in spell_perfection.GetComponents<BlueprintComponent>())
            {
                //Main.logger.Log($"Blueprint component from spell perfection {bc.name} with string value {bc.ToString()} and type {bc.GetType().ToString()}");
                SpellPerfectionDoubleFeatBonuses perfection_doubling_thing = bc as SpellPerfectionDoubleFeatBonuses;
                if (perfection_doubling_thing !=null)
                {
                    BlueprintUnitFact[] things_to_double = perfection_doubling_thing.spell_parameters_feats;
                    perfection_doubling_thing.spell_parameters_feats = perfection_doubling_thing.spell_parameters_feats.AddToArray(library.Get<BlueprintFeatureSelection>("1c17446a3eb744f438488711b792ca4d").AllFeatures); //elemental focus greater

                }
            }

        }

        internal static void FixBruisingIntellect()
        {
            BlueprintFeature bruisingIntellectTrait = library.TryGet<BlueprintFeature>("4e21c192f2ea4f1a8b567dd216deb3b4");
            if (bruisingIntellectTrait == null)
                return;


            //foreach (BlueprintComponent component in bruisingIntellectTrait.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log($"Blueprint component from bruising intellect {component.name} with string value {component.ToString()} and type {component.GetType().ToString()}");
            //    AddBonusToSkillCheckIfNoClassSkill bonus = component as AddBonusToSkillCheckIfNoClassSkill;
            //    if(bonus != null)
            //    {
            //        Main.logger.Log($"AddBonusToSkillCheckIfNoClassSkill {bonus.name.ToString()} has skill {bonus.skill} and check {bonus.check.ToString()}");
            //    }
            //}//brusiing intellect
            

            //foreach (BlueprintComponent component in library.Get<BlueprintFeature>("17726c5b51754680b20a2852d3de67f6").GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log($"Blueprint component from omen {component.name} with string value {component.ToString()} and type {component.GetType().ToString()}");
            //}//omen

            bruisingIntellectTrait.RemoveComponents<AddBonusToSkillCheckIfNoClassSkill>();
            bruisingIntellectTrait.AddComponent(Helpers.Create<CallOfTheWild.NewMechanics.AddBonusToSkillCheckIfNoClassSkill>(a => { a.skill = StatType.SkillPersuasion; a.check = StatType.CheckIntimidate; }));

            
            //foreach (BlueprintComponent component in bruisingIntellectTrait.GetComponents<BlueprintComponent>())
            //{
            //    Main.logger.Log($"Blueprint component from bruising intellect {component.name} with string value {component.ToString()} and type {component.GetType().ToString()}");
            //    AddBonusToSkillCheckIfNoClassSkill bonus = component as AddBonusToSkillCheckIfNoClassSkill;
            //    if (bonus != null)
            //    {
            //        Main.logger.Log($"AddBonusToSkillCheckIfNoClassSkill {bonus.name.ToString()} has skill {bonus.skill} and check {bonus.check.ToString()}");
            //    }
            //}//brusiing intellect
        }

        internal static void fixSpellTypos()
        {
            BlueprintAbility ballLightningSpell = library.TryGet<BlueprintAbility>("8f45d4110fed47b2bb1b5a2303175fc6");
            bool balanceChanges = ballLightningSpell.Description.ToCharArray()[167] == "8".ToCharArray()[0];
            fixBallLightningFactDescription(ballLightningSpell, balanceChanges);
            BlueprintBuff ballLighningBuff = library.Get<BlueprintBuff>("b41745f7e8074151b454c88eafd028b9");
            fixBallLightningFactDescription(ballLighningBuff, balanceChanges);

            BlueprintAbility touchOfGracelessness = library.Get<BlueprintAbility>("5d38c80a819e8084ba19b29a865312c2");
            BlueprintAbility touchOfGracelessNessCast = library.Get<BlueprintAbility>("ad10bfec6d7ae8b47870e3a545cc8900");
            //Main.logger.Log(touchOfGracelessness.Description.ToString());
            string descriptionOfTouch = $"With a single touch you reduce a creature to a fumbling clown.\n"
                                                + $"The target is slowed to half its movement speed and the target takes a penalty to its Dexterity equal to 1d6+1 per two caster levels (maximum 1d6+5). This penalty cannot drop the target's Dexterity score below 1.\n"
                                                + $"A successful Fortitude saves halves the penalty to Dexterity and negates the movement speed penalty.";
            touchOfGracelessness.SetDescription(descriptionOfTouch);
            touchOfGracelessNessCast.SetDescription(descriptionOfTouch);
            //Main.logger.Log(touchOfGracelessness.Description.ToString());

            BlueprintAbility sleetStorm = library.Get<BlueprintAbility>("95574c36b2b4487499757994c8d2d472");
            CallOfTheWild.Common.addSpellDescriptor(sleetStorm, SpellDescriptor.Cold);

            BlueprintFeature swiftConsume = library.Get<BlueprintFeature>("85cb0dc4e5ba491f87dfe4e089e48681");
            swiftConsume.SetDescription("The arcanist can use the consume spells class feature as swift action instead of as move action.");

            BlueprintBuff knowThyEnemyBuff = library.Get<BlueprintBuff>("60453c28f24d445db6122ec0ef84a3ee");
            knowThyEnemyBuff.SetDescription("When the lore warden succeeds at a Knowledge or Lore check to identify a creature’s abilities and weaknesses, she can also use a standard action to grant herself a +2 insight bonus on all attack and weapon damage rolls made against that enemy. This bonus lasts for a number of rounds equal to half her class level (minimum 2 rounds), or until the lore warden uses this ability against a different creature. At 11th level, she also gains a +2 bonus to her AC against the creature when using this ability. At 19th level, the insight bonus increases to +3.");
            BlueprintAbility knowThyEnemyAbility = library.Get<BlueprintAbility>("22c26399e80c479585620cb4fa3fb8b7");
            knowThyEnemyAbility.SetDescription("When the lore warden succeeds at a Knowledge or Lore check to identify a creature’s abilities and weaknesses, she can also use a standard action to grant herself a +2 insight bonus on all attack and weapon damage rolls made against that enemy. This bonus lasts for a number of rounds equal to half her class level (minimum 2 rounds), or until the lore warden uses this ability against a different creature. At 11th level, she also gains a +2 bonus to her AC against the creature when using this ability. At 19th level, the insight bonus increases to +3.");
            BlueprintFeature knowThyEnemyFeature = library.Get<BlueprintFeature>("33df5e9a2e574ba08c0a25a66f8c3735");
            knowThyEnemyFeature.SetDescription("When the lore warden succeeds at a Knowledge or Lore check to identify a creature’s abilities and weaknesses, she can also use a standard action to grant herself a +2 insight bonus on all attack and weapon damage rolls made against that enemy. This bonus lasts for a number of rounds equal to half her class level (minimum 2 rounds), or until the lore warden uses this ability against a different creature. At 11th level, she also gains a +2 bonus to her AC against the creature when using this ability. At 19th level, the insight bonus increases to +3.");
        }

        private static void fixBallLightningFactDescription( BlueprintUnitFact buf, bool balanceChanges)
        {
            //buf.SetDescription(buf.Description.Insert(464, "d")); //this cannot be handled directly because the string for the description is populated from an internal variable
            string dieAmount;
            if (balanceChanges)
            {
                dieAmount = "8";
            }
            else
                dieAmount = "6";
            buf.SetDescription($"You create a globe of lightning that flies in whichever direction you indicate.\n"
                                                            + $"If the globe enters a space with a creature, it stops moving for the round and deals 6d{dieAmount} points of electricity damage to that creature, though a successful Reflex save negates the damage. Creatures wearing metal armor take a –4 penalty on this saving throw.\n"
                                                            + $"For every 4 caster levels above 7th, the damage increase by an additional 3d{dieAmount} (9d{dieAmount} at 11th, 12d{dieAmount} at 15th, to the maximum of 15d{dieAmount} at 19th)\n"
                                                            + "The globe moves as long as you actively direct it (as a move action); otherwise it stays at rest.");
        }

        internal static void AddBardicInspirationToDawnflowerAnchorite()
        {
            //DawnflowerAnchoriteChannelDomainProgressionFeature	e58a52b73cbf476b8cd1f8f214fbd7e7	Kingmaker.Blueprints.Classes.BlueprintFeature
            //AnimalCompanionRank 1670990255e4fe948a863bafd5dbda5d Kingmaker.Blueprints.Classes.BlueprintFeature
            //BardicPerformanceResourceFact	b92bfc201c6a79e49afd0b5cfbfc269f	Kingmaker.Blueprints.Classes.BlueprintFeature
            var channel_energy_domain_progression = library.Get<BlueprintFeature>("e58a52b73cbf476b8cd1f8f214fbd7e7");
            channel_energy_domain_progression.SetDescription(
                "The character adds his Dawnflower anchorite class levels to his effective class level in the corresponding class for the purpose of determining the effects of the following features: "
                                                                      + "Domains, Channel Energy, Bardic Performance, Animal Companion and Wildshape."
                );
            var bardicPerformance = library.Get<BlueprintFeature>("5d4308fa344af0243b2dd3b1e500b2cc");
            for (int i = 1; i <= 10; i++)
            {
                channel_energy_domain_progression.AddComponent(Helpers.CreateAddFeatureOnClassLevelIfHasFact(bardicPerformance, i, getDawnflowerAcnchoriteArray(), bardicPerformance));
            }
        }

        private static BlueprintCharacterClass[] getDawnflowerAcnchoriteArray()
        {
            return new BlueprintCharacterClass[] { library.Get<BlueprintCharacterClass>("3bdfed13c1b747b38b4193faf0213423") };
        }

        internal static void fixCorpseCompanion()
        {
            BlueprintArchetype corpse_companion_archetype = library.Get<BlueprintArchetype>("883543606e074042bd6f40d4f54641ad");
            //corpse_companion_archetype.GetRemoveEntry(12);

            BlueprintFeature companion_proficiency = library.Get<BlueprintFeature>("cd81defa0e924f7f8cfd90040c41bd2d");
            BlueprintFeature str_dex_bonus = library.Get<BlueprintFeature>("228e0457e3634577ae60963c6a5d4a4e");
            BlueprintFeature size_increase1 = library.Get<BlueprintFeature>("cf825092b0a94437942938464c4e7f18");
            BlueprintFeature size_increase2 = library.Get<BlueprintFeature>("6c3cb94a6ef14b02b35f98bcadaaa433");

            corpse_companion_archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, companion_proficiency),
                                                              Helpers.LevelEntry(2, str_dex_bonus),
                                                              Helpers.LevelEntry(4, str_dex_bonus),
                                                              Helpers.LevelEntry(6, str_dex_bonus),
                                                              Helpers.LevelEntry(7, size_increase1),
                                                              Helpers.LevelEntry(8, str_dex_bonus),
                                                              Helpers.LevelEntry(10, str_dex_bonus),
                                                              Helpers.LevelEntry(12, str_dex_bonus),
                                                              Helpers.LevelEntry(14, str_dex_bonus),
                                                              Helpers.LevelEntry(15, size_increase2),
                                                              Helpers.LevelEntry(16, str_dex_bonus),
                                                              Helpers.LevelEntry(18, str_dex_bonus),
                                                              Helpers.LevelEntry(20, str_dex_bonus)
                                                            };
        }

        internal static void fixTemporalCelerity()
        {
            //TemporalCelerityOracleRevelationInitiative2Feature  39c20686344c450d9936701fd5536232 Kingmaker.Blueprints.Classes.BlueprintFeature
            //TemporalCelerityOracleRevelationInitiative3Feature  fc1d098d17364ac09f7bc96df139894c Kingmaker.Blueprints.Classes.BlueprintFeature
            //WarSightOracleRevelationInitiative2Feature  5634f09095884fea8e21a1734d3d5302 Kingmaker.Blueprints.Classes.BlueprintFeature
            //WarSightOracleRevelationInitiative3Feature  d939c0b2b6dd46e2baad4bd7eb4abf4a Kingmaker.Blueprints.Classes.BlueprintFeature
            //TemporalCelerityShamanRevelationInitiative2Feature  9b6950738c6c4c759d6731e1185d2b53 Kingmaker.Blueprints.Classes.BlueprintFeature
            //TemporalCelerityShamanRevelationInitiative3Feature  27e65f06c7644db7bf49726043ddf61b Kingmaker.Blueprints.Classes.BlueprintFeature

            BlueprintFeature tempCelerityOracleRevelationInitiative2Feature = library.Get<BlueprintFeature>("39c20686344c450d9936701fd5536232");
            setd20amount(1, tempCelerityOracleRevelationInitiative2Feature);
            BlueprintFeature tempCelerityOracleRevelationInitiative3Feature = library.Get<BlueprintFeature>("fc1d098d17364ac09f7bc96df139894c");
            setd20amount(2, tempCelerityOracleRevelationInitiative3Feature);

            BlueprintFeature warSightOracleRevelationInitiative2Feature = library.Get<BlueprintFeature>("5634f09095884fea8e21a1734d3d5302");
            setd20amount(1, warSightOracleRevelationInitiative2Feature);
            BlueprintFeature warSightOracleRevelationInitiative3Feature = library.Get<BlueprintFeature>("d939c0b2b6dd46e2baad4bd7eb4abf4a");
            setd20amount(2, warSightOracleRevelationInitiative3Feature);

            BlueprintFeature temporalCelerityShamanRevelationInitiative2Feature = library.Get<BlueprintFeature>("9b6950738c6c4c759d6731e1185d2b53");
            setd20amount(1, temporalCelerityShamanRevelationInitiative2Feature);
            BlueprintFeature temporalCelerityShamanRevelationInitiative3Feature = library.Get<BlueprintFeature>("27e65f06c7644db7bf49726043ddf61b");
            setd20amount(1, temporalCelerityShamanRevelationInitiative3Feature);
        }

        internal static void setd20amount(int amount, BlueprintFeature theRevelation)
        {
            ModifyD20 d20component = theRevelation.GetComponent<ModifyD20>();
            if (d20component != null)
            {
                d20component.RollsAmount = amount;
            }
        }

        internal static void AllowPersistentSpellOnAllDamagingSpells()
        {
            //((b.AvailableMetamagic & Metamagic.Maximize) != 0)).Cast<BlueprintAbility>().ToArray()
            BlueprintFeature persistent_metamagic = library.Get<BlueprintFeature>("09b10675689147459bd485b28f156768");
            var spells = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(b => b.IsSpell && b.EffectOnEnemy == AbilityEffectOnUnit.Harmful && ((b.AvailableMetamagic & Metamagic.Maximize) != 0) && ((b.AvailableMetamagic & (Metamagic)MetamagicExtender.Persistent) == 0)).Cast<BlueprintAbility>().ToArray();
            foreach (var s in spells)
            {
                s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicExtender.Persistent;
                if (s.Parent != null)
                {
                    s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicExtender.Persistent;
                }
            }
        }

        internal static void allowDazingOnAllDamagingSpells()
        {
            List<String> exemptions = new List<string>() { "Archon’s Trumpet", "Animate Dead", "Irresistible Dance", "Ghoul Touch", "Shadow Evocation, Greater (Archon’s Trumpet)" };
            BlueprintFeature dazing_metamagic = library.Get<BlueprintFeature>("b3078f42707c4500902b5f78e3c778ab");
            var spells = library.GetAllBlueprints().OfType<BlueprintAbility>().Where(b => b.IsSpell && b.EffectOnEnemy == AbilityEffectOnUnit.Harmful && ((b.AvailableMetamagic & Metamagic.Empower) != 0) && ((b.AvailableMetamagic & (Metamagic)MetamagicExtender.Dazing) == 0)).Cast<BlueprintAbility>().ToArray();
            foreach (var s in spells)
            {
                if (!exemptions.Contains(s.Name))
                {
                    s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicExtender.Dazing;
                    if (s.Parent != null)
                    {
                        s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicExtender.Dazing;
                    }
                    //Main.logger.Log($"Damaging spell that can't have dazing metamagic. Spell name: {s.Name}");
                }
            }
        }

        internal static void fixBloodhuntersFavorTargetShare()
        {
            BlueprintBuff bloodhunter_favored_target = library.Get<BlueprintBuff>("894596869b7841fba9d2c45ed80ed52d");
            ShareFavoredEnemies sfe = bloodhunter_favored_target.GetComponent<ShareFavoredEnemies>();
            sfe.Half = false;
        }

        internal static void fixFieryShuriken()
        {
            BlueprintAbility fiery_shuriken = CallOfTheWild.NewSpells.fiery_shiriken;
            CallOfTheWild.Common.addSpellDescriptor(fiery_shuriken, SpellDescriptor.Fire);
            BlueprintAbility fire_shuriken_1projectile = library.Get<BlueprintAbility>("ee048804323f4369ad2d6b6e53b760e3");
            CallOfTheWild.Common.addSpellDescriptor(fire_shuriken_1projectile, SpellDescriptor.Fire);
            BlueprintAbility fire_shuriken_all_projectile = library.Get<BlueprintAbility>("65d6a7e056994a13a9f5386a008d17e5");
            CallOfTheWild.Common.addSpellDescriptor(fire_shuriken_all_projectile, SpellDescriptor.Fire);

            BlueprintAbility fireS1 = library.Get<BlueprintAbility>("cef68859a5114e29ab68b0212cca484a");
            CallOfTheWild.Common.addSpellDescriptor(fireS1, SpellDescriptor.Fire);
            BlueprintAbility fireS2 = library.Get<BlueprintAbility>("68d836bc43e14ef0ac77eec3a5fb4276");
            CallOfTheWild.Common.addSpellDescriptor(fireS2, SpellDescriptor.Fire);
            BlueprintAbility fireS3 = library.Get<BlueprintAbility>("903ce6f303854073af1c86dc9d5ae3bc");
            CallOfTheWild.Common.addSpellDescriptor(fireS3, SpellDescriptor.Fire);
            BlueprintAbility fireS4 = library.Get<BlueprintAbility>("2a1af589198a4f6abac719873118ceb7");
            CallOfTheWild.Common.addSpellDescriptor(fireS4, SpellDescriptor.Fire);
            BlueprintAbility fireS5 = library.Get<BlueprintAbility>("eba75b742e1449edb9a3dc63de41a99c");
            CallOfTheWild.Common.addSpellDescriptor(fireS5, SpellDescriptor.Fire);
            BlueprintAbility fireS6 = library.Get<BlueprintAbility>("34030ceaba244521a2c56aff51b83fb5");
            CallOfTheWild.Common.addSpellDescriptor(fireS6, SpellDescriptor.Fire);
            BlueprintAbility fireS7 = library.Get<BlueprintAbility>("7645eb17fbc348eebbd2d5929e35c93f");
            CallOfTheWild.Common.addSpellDescriptor(fireS7, SpellDescriptor.Fire);
        }

        internal static void fixDivineScourgeHexesDurations()
        {
            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Charisma, progression: ContextRankProgression.StartPlusDivStep,
                                                    min: 3, startLevel: -2, stepLevel: 1, type: AbilityRankType.DamageBonus);
            switchRankConfigForNewConfig(library.Get<BlueprintAbility>("09e4d54cf7a2423680284dbbf74d953a"), context_rank_config); //DivineScourgeEvilEyeACHexAbility
            switchRankConfigForNewConfig(library.Get<BlueprintAbility>("ed8dccf0ead04c5facf14f87348857c6"),context_rank_config); //SplitHexDivineScourgeEvilEyeACHexAbility
            switchRankConfigForNewConfig(library.Get<BlueprintAbility>("3a45043726fc4c6082ef38ef65f1dcbc"), context_rank_config);//DivineScourgeEvilEyeACHexAbilityHexStikeAbility

            switchRankConfigForNewConfig(library.Get<BlueprintAbility>("f55ec19e1f6340a69688516b2d380669"), context_rank_config);//DivineScourgeEvilEyeAttackHexAbility
            switchRankConfigForNewConfig(library.Get<BlueprintAbility>("78d43d85575642c2ad333fdc1263f8cf"), context_rank_config);//SplitHexDivineScourgeEvilEyeAttackHexAbility
            switchRankConfigForNewConfig(library.Get<BlueprintAbility>("7eb73a8b670c4f269338d80634e4f2c9"), context_rank_config);//DivineScourgeEvilEyeAttackHexAbilityHexStikeAbility

            switchRankConfigForNewConfig(library.Get<BlueprintAbility>("362d36290eff40ab82feb51a13034192"), context_rank_config);//DivineScourgeEvilEyeSavesHexAbility
            switchRankConfigForNewConfig(library.Get<BlueprintAbility>("362d36290eff40ab82feb51a13034192"), context_rank_config);//SplitHexDivineScourgeEvilEyeSavesHexAbility
            switchRankConfigForNewConfig(library.Get<BlueprintAbility>("362d36290eff40ab82feb51a13034192"), context_rank_config);//DivineScourgeEvilEyeSavesHexAbilityHexStikeAbility

            var retribution_context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus,
                                                                                stat: StatType.Charisma,
                                                                                progression: ContextRankProgression.AsIs,
                                                                                type: AbilityRankType.DamageBonus, min: 1);
            switchRankConfigForNewConfig(library.Get<BlueprintAbility>("5b1526595e834bcab5d3d1dd1c1cfe8d"), context_rank_config);//DivineScourgeRetributionHexAbility
            switchRankConfigForNewConfig(library.Get<BlueprintAbility>("b536d525ff2541bea5d9a63f872d1a47"), context_rank_config);//SplitHexDivineScourgeRetributionHexAbility
            switchRankConfigForNewConfig(library.Get<BlueprintAbility>("5b1526595e834bcab5d3d1dd1c1cfe8d"), context_rank_config);//DivineScourgeRetributionHexAbilityHexStikeAbility
        }

        internal static void switchRankConfigForNewConfig(BlueprintAbility ability, ContextRankConfig rankConfig)
        {
            ability.RemoveComponents<ContextRankConfig>();
            ability.AddComponent(rankConfig);
        }
    }

    //[HarmonyPatch(typeof(UnitAttack), "OnAction")]
    //static class UnitAttack_OnAction_Patch
    //{
    //    private static void Postfix(UnitAttack __instance)
    //    {
    //        if (!Main.settings.confusion_output)
    //        {
    //            return;
    //        }
    //        UnitEntityData unit = __instance.Executor;
    //    if (unit.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Confusion))
    //    {
    //        UnitPartConfusion part = unit.Ensure<UnitPartConfusion>();
    //        switch (part.State)
    //        {
    //            case ConfusionState.ActNormally:
    //                CallOfTheWild.Common.AddBattleLogMessage($"{unit.CharacterName} sees clearly through its confusion and acts normally");
    //                break;
    //            case ConfusionState.AttackNearest:
    //                CallOfTheWild.Common.AddBattleLogMessage($"{unit.CharacterName} lashes out at the nearest creature due to its confusion");
    //                break;
    //            case ConfusionState.DoNothing:
    //                CallOfTheWild.Common.AddBattleLogMessage($"{unit.CharacterName} stays still out of confusion");
    //                break;
    //            case ConfusionState.SelfHarm:
    //                CallOfTheWild.Common.AddBattleLogMessage($"{unit.CharacterName} hurts itself in its confusion");
    //                break;
    //            default:
    //                CallOfTheWild.Common.AddBattleLogMessage($"{unit.CharacterName} is not confused");
    //                break;
    //        }
    //    }
    //}
    //}

    //[HarmonyPatch(typeof(UnitSelfHarm), "OnAction")]
    //static class UnitSelfHarm_OnAction_Patch
    //{
    //    private static void Postfix(UnitSelfHarm __instance)
    //    {
    //        if (!Main.settings.confusion_output)
    //        {
    //            return;
    //        }
    //        if (__instance.Target != null)
    //            CallOfTheWild.Common.AddBattleLogMessage($"{__instance.Target.Unit.CharacterName} hurts itself in its confusion");
    //    }
    //}

    //[HarmonyPatch(typeof(UnitDoNothing), "OnAction")]
    //static class DoNothing_OnAction_Patch
    //{
    //    private static void Postfix(UnitDoNothing __instance)
    //    {
    //        if (!Main.settings.confusion_output)
    //        {
    //            return;
    //        }
    //        if (__instance.Target != null)
    //            CallOfTheWild.Common.AddBattleLogMessage($"{__instance.Target.Unit.CharacterName} stays still out of confusion");
    //    }
    //}


    //to avoid transpiler issues
    //we can write a postfix for the UnitPartConfusion State with this syntax
    //[HarmonyPatch(typeof(UnitPartConfusion), "State", MethodType.Setter)]
    //Then grab the __instance of the UnitPartConfusion and output to the battlelog on a switch of the ConfusionState State
    //which looks like 

    //[HarmonyPatch(typeof(UnitPartConfusion), "State", MethodType.Setter)]
    //static class UnitPartConfusion_StateSetter_Patch
    //{
    //    private static void Postfix(UnitPartConfusion __instance)
    //    {
    //        if(!Main.settings.confusion_output)
    //        {
    //            return;
    //        }
    //        switch (__instance.State)
    //        {
    //            case ConfusionState.ActNormally:
    //                CallOfTheWild.Common.AddBattleLogMessage($"{__instance.Owner.CharacterName} sees clearly through its confusion and acts normally");
    //                break;
    //            case ConfusionState.AttackNearest:
    //                CallOfTheWild.Common.AddBattleLogMessage($"{__instance.Owner.CharacterName} lashes out at the nearest creature due to its confusion");
    //                break;
    //            case ConfusionState.DoNothing:
    //                CallOfTheWild.Common.AddBattleLogMessage($"{__instance.Owner.CharacterName} stays still out of confusion");
    //                break;
    //            case ConfusionState.SelfHarm:
    //                CallOfTheWild.Common.AddBattleLogMessage($"{__instance.Owner.CharacterName} hurts itself in its confusion");
    //                break;
    //            default:
    //                CallOfTheWild.Common.AddBattleLogMessage($"{__instance.Owner.CharacterName} is not confused");
    //                break;
    //        }
    //    }
    //}    
}