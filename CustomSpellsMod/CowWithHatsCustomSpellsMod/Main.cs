using UnityModManagerNet;
using System;
using System.Reflection;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using CallOfTheWild;
using Kingmaker.UI.ActionBar;
using Harmony12;

namespace CowWithHatsCustomSpellsMod
{
    internal class Main
    {
        internal class Settings
        {
            internal bool domination_dismissal;
            internal bool domination_gives_control;
            internal bool ear_pierce_daze_update;
            internal bool remove_slumber_nerf;
            internal bool change_inspire_rage_mind_affecting;
            internal bool spell_replacement;
            internal bool iconic_amiri;
            internal bool fix_allied_spellcaster;
            internal bool fix_fey_thoughts;
            internal bool fix_studied_combat;
            internal bool remove_curiousity_invocation_nerf;
            internal bool remove_planar_focus_nerf;
            internal bool bardic_to_dawnflower;
            internal bool persistent_on_damaging_spells;
            internal bool address_multitick_bug;

            //internal bool silence_update;
            //internal bool confusion_output;
            internal Settings()
            {
                using (StreamReader settings_file = File.OpenText("Mods/CowWithHatsCustomSpellsMod/settings.json"))
                using (JsonTextReader reader = new JsonTextReader(settings_file))
                {
                    JObject jo = (JObject)JToken.ReadFrom(reader);
                    domination_dismissal = (bool)jo["domination_dismissal"];
                    domination_gives_control = (bool)jo["domination_gives_control"];
                    ear_pierce_daze_update = (bool)jo["ear_pierce_daze_update"];
                    remove_slumber_nerf = (bool)jo["remove_slumber_nerf"];
                    change_inspire_rage_mind_affecting = (bool)jo["change_inspire_rage_mind_affecting"];
                    spell_replacement = (bool)jo["spell_replacement"];
                    iconic_amiri = (bool)jo["iconic_amiri"];
                    fix_allied_spellcaster = (bool)jo["fix_allied_spellcaster"];
                    fix_fey_thoughts = (bool)jo["fix_fey_thoughts"];
                    fix_studied_combat = (bool)jo["fix_studied_combat"];
                    remove_curiousity_invocation_nerf = (bool)jo["remove_curiousity_invocation_nerf"];
                    remove_planar_focus_nerf = (bool)jo["remove_planar_focus_nerf"];
                    persistent_on_damaging_spells = (bool)jo["persistent_on_damaging_spells"];
                    address_multitick_bug = (bool)jo["address_multitick_bug"];
                    //bardic_to_dawnflower = (bool)jo["bardic_to_dawnflower"];
                    //silence_update = (bool)jo["silence_update"];
                    //confusion_output = (bool)jo["confusion_output"];
                }
            }
        }

        static internal Settings settings = new Settings();
        internal static UnityModManagerNet.UnityModManager.ModEntry.ModLogger logger;
        public bool ranOnce = false;
        internal static Harmony12.HarmonyInstance harmony;
        internal static LibraryScriptableObject library;

        static readonly Dictionary<Type, bool> typesPatched = new Dictionary<Type, bool>();
        static readonly List<String> failedPatches = new List<String>();
        static readonly List<String> failedLoading = new List<String>();

        internal bool CheckIfRun()
        {
            if (!ranOnce)
            {
                ranOnce = true;
                return false;
            }
            else
                return true;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void DebugLog(string msg)
        {
            if (logger != null) logger.Log(msg);
        }
        internal static void DebugError(Exception ex)
        {
            if (logger != null) logger.Log(ex.ToString() + "\n" + ex.StackTrace);
        }
        internal static bool enabled;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                logger = modEntry.Logger;
                harmony = Harmony12.HarmonyInstance.Create(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());

            }
            catch (Exception ex)
            {
                DebugError(ex);
                throw ex;
            }
            return true;
        }

        //static class LibraryScriptableObject_LoadDictionary_Patch
        //{
        //    static bool Run = false;
        //    static void Postfix()
        //    {
        //        if (Run) return; Run = true;
        //        try
        //        {
        //            Main.DebugLog("Loading CowWithHat's Custom Spells");
        //            
        //        }
        //        catch(Exception ex)
        //        {
        //            Main.DebugError(ex);
        //        }
        //    }
        //}

        [Harmony12.HarmonyPatch(typeof(CallOfTheWild.NewSpells), "load")]
        static class LibraryScriptableObject_LoadDictionary_Patch_Before
        {
            static bool Run = false;
            static void Postfix()
            {
                if (Run) return; Run = true;
                Main.library = CallOfTheWild.NewSpells.library;
                try
                {
//#if DEBUG
//                    bool allow_guid_generation = true;
//#else
//                    bool allow_guid_generation = false; //no guids should be ever generated in release
//#endif
//                    CallOfTheWild.Helpers.GuidStorage.load(Properties.Resources.blueprints, allow_guid_generation);
                    Main.DebugLog("Loading CowWithHat's Custom Spells");
                    //logger.Log("Made it to pre load");
                    Core.preLoad();                    
                    //this might break some stuff
                    //CallOfTheWild.Helpers.GuidStorage.dump(@"./Mods/CowWithHatsCustomSpellsMod/loaded_blueprints.txt");
                }
                catch (Exception ex)
                {
                    Main.DebugError(ex);
                }
            }
        }



        //[Harmony12.HarmonyPatch(typeof(Helpers.GuidStorage), "dump")]
        //LibraryScriptableObject_LoadDictionary_Patch
        [Harmony12.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
        [Harmony12.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary", new Type[0])]
        [Harmony12.HarmonyAfter("RacesUnleashed", "DerringDo", "ZFavoredClass", "TweakOrTreat")]
        [Harmony12.HarmonyPriority(Harmony12.Priority.Low)] //These want to launch after Call of the Wild so that Call's stuff exists and can be editted
        static class LibraryScriptableObject_LoadDictionary_Patch_After
        {
            static bool alreadyRan = false;
            static void Postfix()
            {
                try
                {
                    Main.DebugLog("Loading CowWithHat's CustomSpellsMod after Call of the Wild");
                     
                    CallOfTheWild.LoadIcons.Image2Sprite.icons_folder = @"./Mods/CowWithHatsCustomSpellsMod/Icons/";
#if DEBUG                
                    bool allow_guid_generation = true;
#else
                    bool allow_guid_generation = false; //no guids should be ever generated in release
#endif
                    if (!alreadyRan)
                    {
                        //Main.logger.Log("Loaddictionary postfix is running");
                        Core.Load();
                        Core.postLoad();
                        if (settings.domination_dismissal)
                        {
                            Core.UpdateDismiss();
                        }
                        if(settings.domination_gives_control)
                        {
                            Core.UpdateDominationEffects();
                        }
                        if (settings.ear_pierce_daze_update)
                            Core.FixEarpierce();
                        if (settings.remove_slumber_nerf)
                        {
                            Core.RemoveSlumberNerf();
                        }
                        if(settings.change_inspire_rage_mind_affecting)
                        {
                            Core.ChangeInspireRage();
                        }
                        if(settings.spell_replacement)
                        {
                            Core.AddSpellReplacement();
                        }
                        if(settings.iconic_amiri)
                        {
                            Core.UpdateAmiri();
                        }
                        if (settings.fix_allied_spellcaster)
                        {
                            Core.UpdateAlliedSpellcaster();
                        }
                        if (settings.fix_studied_combat)
                        {
                            Core.FixStudiedCombat();
                        }
                        if(settings.remove_curiousity_invocation_nerf)
                        {
                            Core.FixCuriosityInvocation();
                        }
                        if(settings.remove_planar_focus_nerf)
                        {
                            Core.RemovePlanarFocusNerf();
                        }
                        if (settings.persistent_on_damaging_spells)
                        {
                            Core.AllowPersistentSpellOnAllDamagingSpells();
                        }
                        //Core.CreateLawArchonSubdomain();

                        //Core.fixExplosionRing();
                        //if(settings.silence_update)
                        //{
                        //    Core.UpdateSilence();
                        //}
                        //see Core.cs for Post fix related to settings.confusion_output it is dealt with in a new class after Core
                        //fix fey thoughts if tweak or treat is installed
                        BlueprintFeature feyThoughtsBluff = library.TryGet<BlueprintFeature>("3393b744356648bf8c87021fd24680fb");
                        if (feyThoughtsBluff !=null && settings.fix_fey_thoughts)
                        {
                            //HalfElfRacialFeatures.load();
                            if (settings.fix_fey_thoughts)
                                Core.FixFeyThoughts();
                        }


                        //if(settings.bardic_to_dawnflower)
                        //{
                        //    Core.AddBardicInspirationToDawnflowerAnchorite();
                        //}

                        Core.FixForTweakOrTreatHumanCompanionFeats();

                        Core.FixSheetLightningNotToWorkOnNonLiving();

                        Core.FixBurstOfRadiance();

                        Core.FixSpellPerfectionMissingGreaterElementalFocus();

                        Core.FixBruisingIntellect();

                        Core.fixSpellTypos();

                        Core.fixUndeadAnatomy();

                        Core.fixCorpseCompanion();

                        Core.fixTemporalCelerity();

                        Core.fixBloodhuntersFavorTargetShare();

                        Core.fixFieryShuriken();

                        Core.allowDazingOnAllDamagingSpells();

                        Core.fixDivineScourgeHexesDurations();

                        alreadyRan = true;
                    }
                    else
                    {
                        //Main.logger.Log("That should work now");
                    }
                    //CallOfTheWild.Helpers.GuidStorage.load(Properties.Resources.blueprints, allow_guid_generation);

                    //logger.Log("Made it to post load");
                    

#if DEBUG
                    //string guid_file_name = @"./Mods/CowWithHatsCustomSpellsMod/blueprints.txt";
                    //CallOfTheWild.Helpers.GuidStorage.dump(guid_file_name);
#endif
                    //CallOfTheWild.Helpers.GuidStorage.dump(@"./Mods/CowWithHatsCustomSpellsMod/loaded_blueprints.txt");
                }
                catch (Exception ex)
                {
                    Main.DebugError(ex);
                }
            }
        }

        internal static Exception Error(String message)
        {
            logger?.Log(message);
            return new InvalidOperationException(message);
        }


        //This is from Eldritch Arcana and may not be as useful as I'd hoped
        static Harmony12.HarmonyInstance harmonyInstance;
        // We don't want one patch failure to take down the entire mod, so they're applied individually.
        //
        // Also, in general the return value should be ignored. If a patch fails, we still want to create
        // blueprints, otherwise the save won't load. Better to have something be non-functional.
        internal static bool ApplyPatch(Type type, String featureName)
        {
            try
            {
                if (typesPatched.ContainsKey(type)) return typesPatched[type];

                var patchInfo = Harmony12.HarmonyMethodExtensions.GetHarmonyMethods(type);
                if (patchInfo == null || patchInfo.Count() == 0)
                {
                    Log.Error($"Failed to apply patch {type}: could not find Harmony attributes");
                    failedPatches.Add(featureName);
                    typesPatched.Add(type, false);
                    return false;
                }
                var processor = new Harmony12.PatchProcessor(harmonyInstance, type, Harmony12.HarmonyMethod.Merge(patchInfo));
                var patch = processor.Patch().FirstOrDefault();
                if (patch == null)
                {
                    Log.Error($"Failed to apply patch {type}: no dynamic method generated");
                    failedPatches.Add(featureName);
                    typesPatched.Add(type, false);
                    return false;
                }
                typesPatched.Add(type, true);
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to apply patch {type}: {e}");
                failedPatches.Add(featureName);
                typesPatched.Add(type, false);
                NullReferenceException nre = e as NullReferenceException;
                if(e!=null)
                {
                    logger.Log("Null reference exception message is as follows " + nre.Message + " for feature " + featureName + " of type " + type);
                }
                return false;
            }
        }

    }

    //need to comment out the ActionbarFix for 2.0.8 version of the mod
    public static class ActionBarFix
    {
        public static bool HasMethod(this object objectToCheck, string methodName)
        {
            var type = objectToCheck.GetType();
            return type.GetMethod(methodName) != null;
        }

        [HarmonyPatch(typeof(ActionBarManager), "CheckTurnPanelView")]
        internal static class ActionBarManager_CheckTurnPanelView_Patch
        {
            //private static bool Prepare(ActionBarManager __instance)
            //{
            //    bool hasCheckTurnPanelView = HasMethod(__instance, "CheckTurnPanelView");
            //    Main.logger.Log("Reached this point");
            //    return hasCheckTurnPanelView;
            //}
            private static void Postfix(ActionBarManager __instance)
            {
                Traverse.Create((object)__instance).Method("ShowTurnPanel", Array.Empty<object>()).GetValue();
            }
        }
    }
}

