using Harmony12;
using System.Reflection;
using CallOfTheWild;
using Kingmaker.Blueprints.Classes;

namespace CowWithHatsCustomSpellsMod
{
    [HarmonyPatch]
    public static class OverrideEvilEyeHexCreation
    {
        static MethodBase TargetMethod()
        {
            return typeof(CallOfTheWild.Witch)
        .GetMethod("createEvilEye", BindingFlags.NonPublic | BindingFlags.Static);
        }

        static void Postfix()
        {
            var evil_eyeField = AccessTools.Field(typeof(CallOfTheWild.Witch), "evil_eye");
            if(evil_eyeField != null )
            {
                BlueprintFeature evil_eye = (BlueprintFeature)evil_eyeField.GetValue(null);
                if(evil_eye != null)
                {
                    evil_eye.SetDescription(
                "The witch can cause doubt to creep into the mind of a foe within 30 feet that she can see.\n"
                + "Effect: The target takes a –2 penalty on one of the following(witch’s choice): AC, attack rolls or saving throws. This hex lasts for a number of rounds equal to 3 + the witch’s Intelligence modifier. A Will save reduces this to just 1 round.\n"
                + "This is a mind-affecting effect. At 8th level the penalty increases to –4.");
                }
            }
        }
    }
}
