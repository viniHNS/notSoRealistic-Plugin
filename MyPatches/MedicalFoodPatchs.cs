
using System.Collections;
using EFT;
using EFT.HealthSystem;
using System.Reflection;
using Comfort.Common;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using notSoRealistic.Utils;
namespace notSoRealistic.MyPatches

// ReSharper disable InconsistentNaming
{
    
    internal partial struct PlayerInfo
    {
        internal static GameWorld gameWorld
        {
            get => Singleton<GameWorld>.Instance;
        }

        internal static Player.FirearmController FC
        {
            get => player.HandsController as Player.FirearmController;
        }
        
        internal static Player.MedsController medsController
        {
            get => player.HandsController as Player.MedsController;
        }

        internal static Player player
        {
            get => gameWorld.MainPlayer;
        }

        internal static Player.ItemHandsController itemHandsController
        {
            get => player.HandsController as Player.ItemHandsController;
        }

        internal static Player.UsableItemController usableItemController
        {
            get => itemHandsController as Player.UsableItemController;
        }
    }
    
    public class CanWalkInSurgeryPatch : ModulePatch // all patches must inherit ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.SetPhysicalCondition));
        }

        [PatchPrefix]
        private static bool Prefix(EPhysicalCondition c, ref bool __result, Player ____player)
        {

            // If this player instance is not the main player, don't continue the rest of the method
            if (!____player.IsYourPlayer || PlayerInfo.player is HideoutPlayer)
            {
                return true;
            }
            
            
            
            if (c == EPhysicalCondition.HealingLegs && Plugin.CanWalkInSurgery.Value)
            {
                __result = false;
                return false;
            }

            return true;

        }
    }

    public class RunPatch : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.SetPhysicalCondition));
        }

        [PatchPrefix]
        private static bool Prefix(EPhysicalCondition c, ref bool __result, Player ____player)
        {
            // If this player instance is not the main player, don't continue the rest of the method
            if (!____player.IsYourPlayer || PlayerInfo.player is HideoutPlayer)
            {
                return true;
            }
            
            Plugin.LogSource.LogWarning("_PLAYER => " + ____player.Profile.Info.Nickname);

            if (c == EPhysicalCondition.UsingMeds && Plugin.CanSprintUsingMeds.Value)
            {
                __result = false;
                return false;
                
            }

            return true;

        }
    }
   
    
    public class ExodrineBuffPatch : ModulePatch
    {
        
        //private static HashSet<Player> _playersWithPendingFracture = new HashSet<Player>();
        
        // Dictionary to store the timer for each player
        // private static Dictionary<Player, Timer> _playerTimers = new Dictionary<Player, Timer>();

        protected override MethodBase GetTargetMethod()
        {
            //It's not perfect, but will do the job :) 
            return AccessTools.Method(typeof(ActiveHealthController),
                nameof(ActiveHealthController.method_18));
        }

        [PatchPostfix]
        private static void Postfix()
        {
            var exodrine = "67352773c7d52bfd03002b6f Name";
            
            if (!PlayerInfo.player)
            {
                Logger.LogError("PlayerInfo.player is null.");
                return;
            }

            if (!PlayerInfo.medsController)
            {
                Logger.LogError("PlayerInfo.medsController is null.");
                return;
            }

            if (PlayerInfo.medsController.Item == null)
            {
                Logger.LogError("PlayerInfo.medsController.Item is null.");
                return;
            }
            if (PlayerInfo.medsController.Item.Name == exodrine )
            {
                PlayerInfo.player.StartCoroutine(DelayExodrine(PlayerInfo.player, 306f));
            }
            
        }
        
        private static IEnumerator DelayExodrine(Player player, float time)
        {
            
            yield return new WaitForSeconds(time);
            
            var actualHpRightLeg = PlayerInfo.player.ActiveHealthController.GetBodyPartHealth(EBodyPart.RightLeg);
            var actualHpLeftLeg = PlayerInfo.player.ActiveHealthController.GetBodyPartHealth(EBodyPart.LeftLeg);
            
            PlayerInfo.player.ActiveHealthController.ChangeHealth(EBodyPart.LeftLeg, -actualHpLeftLeg.Current, GClass2788.StimulatorUse);
            PlayerInfo.player.ActiveHealthController.ChangeHealth(EBodyPart.RightLeg, -actualHpRightLeg.Current, GClass2788.StimulatorUse);
            
            PlayerInfo.player.ActiveHealthController.DestroyBodyPart(EBodyPart.RightLeg, EDamageType.Stimulator);
            PlayerInfo.player.ActiveHealthController.DestroyBodyPart(EBodyPart.LeftLeg, EDamageType.Stimulator);
            TarkovEffects.ApplyBleeding(player, EBodyPart.Stomach, "HeavyBleeding");
            TarkovEffects.ApplyEffect(player, "Tremor", EBodyPart.Head, 0f, 120f, 10f, 1);
            
        }
    }
}
