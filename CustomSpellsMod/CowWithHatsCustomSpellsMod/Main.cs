using UnityModManagerNet;
using System;
using System.Reflection;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Designers.Mechanics.Buffs;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using CallOfTheWild;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Common;
using Harmony12;

namespace CowWithHatsCustomSpellsMod
{
    internal class Main
    {
        internal class Settings
        {
            internal bool domination_dismissal;
            internal bool domination_gives_control;
            internal bool silence_update;
            //internal bool confusion_output;
            internal Settings()
            {
                using (StreamReader settings_file = File.OpenText("Mods/CowWithHatsCustomSpellsMod/settings.json"))
                using (JsonTextReader reader = new JsonTextReader(settings_file))
                {
                    JObject jo = (JObject)JToken.ReadFrom(reader);
                    domination_dismissal = (bool)jo["domination_dismissal"];
                    domination_gives_control = (bool)jo["domination_gives_control"];
                    silence_update = (bool)jo["silence_update"];
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

        //Patches the action Bar Manager to have it show up for summons, and maybe dominated targets
        //[HarmonyLib.HarmonyPatch(typeof(ActionBarManager), "CheckTurnPanelView")]
        //internal static class ActionBarManager_CheckTurnPanelView_Patch
        //{
        //    private static void Postfix(ActionBarManager __instance)
        //    {
        //        Main.logger.Log($"Patching Show Turn Panel");
        //        HarmonyLib.Traverse.Create(__instance).Method("ShowTurnPanel").GetValue();
        //    }
        //}

        //[Harmony12.HarmonyPatch(typeof(Helpers.GuidStorage), "dump")]
        //LibraryScriptableObject_LoadDictionary_Patch
        [Harmony12.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
        [Harmony12.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary", new Type[0])]
        [Harmony12.HarmonyAfter("CallOfTheWild")] //These want to launch after Call of the Wild so that Call's stuff exists and can be editted
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
                        Core.postLoad();
                        if (settings.domination_dismissal)
                        {
                            Core.UpdateDismiss();
                        }
                        if(settings.domination_gives_control)
                        {
                            Core.UpdateDominationEffects();
                        }
                        if(settings.silence_update)
                        {
                            Core.UpdateSilence();
                        }
                        //see Core.cs for Post fix related to settings.confusion_output it is dealt with in a new class after Core
                        alreadyRan = true;
                    }
                    else
                    {
                        //Main.logger.Log("That should work now");
                    }
                    //CallOfTheWild.Helpers.GuidStorage.load(Properties.Resources.blueprints, allow_guid_generation);

                    //logger.Log("Made it to post load");
                    

#if DEBUG
                    string guid_file_name = @"./Mods/CowWithHatsCustomSpellsMod/blueprints.txt";
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
    }


    public class ActionBarFix
    {
        [HarmonyPatch(typeof(ActionBarManager), "CheckTurnPanelView")]
        internal static class ActionBarManager_CheckTurnPanelView_Patch
        {
            private static void Postfix(ActionBarManager __instance)
            {
                Traverse.Create((object)__instance).Method("ShowTurnPanel", Array.Empty<object>()).GetValue();
            }
        }
    }
}

