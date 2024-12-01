using EFT;
using Comfort.Common;
using System.Reflection;
using EFT.HealthSystem;
using HarmonyLib;
using static EFT.InventoryLogic.Weapon;
using SPT.Reflection.Patching;

namespace notSoRealistic.MyPatches
{
    public class CanResolveMalfunctionsWithoutInspectionPatch : ModulePatch // all patches must inherit ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WeaponMalfunctionStateClass), nameof(WeaponMalfunctionStateClass.IsKnownMalfType));
        }
        
        [PatchPostfix]
        private static void Postfix(ref bool __result)
        {
            __result = true;
            //Logger.LogWarning("Malfunctions can be resolved without inspection");
        }
        
    }
    
    public class RemoveBossMalfunctionsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ActiveHealthController), nameof(ActiveHealthController.AddMisfireEffect));
        }

        [PatchPrefix]
        static bool Prefix(ActiveHealthController __instance)
        {
            return !Plugin.RemoveBossMalfunctions.Value;
        }
    }
}