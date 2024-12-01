
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EFT;
using EFT.HealthSystem;
using System.Reflection;
using System.Timers;
using Comfort.Common;
using EFT.UI.Health;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using Random = System.Random;
using notSoRealistic.Utils;
namespace notSoRealistic.MyPatches
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
        private static bool Prefix(EPhysicalCondition c, ref bool __result)
        {

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
        private static bool Prefix(EPhysicalCondition c, ref bool __result)
        {

            if (c == EPhysicalCondition.UsingMeds && Plugin.CanSprintUsingMeds.Value)
            {
                __result = false;
                return false;
                
            }

            return true;

        }
    }
   
    public class ExpiredFoodPatch : ModulePatch // all patches must inherit ModulePatch
    {
        private static readonly Random Random = new Random();

        private static readonly String[] DrinkablesWithContamination =
        {
            "5c0fa877d174af02a012e1cf", "5e8f3423fd7471236e6e3b64", "5448fee04bdc2dbc018b4567",
            "5751496424597720a27126da", "575062b524597720a31c09a1", "5751435d24597720a27126d1",
            "60b0f93284c20f0feb453da7", "57514643245977207f2c2d09", "60098b1705871270cd5352a1",
            "57513f07245977207e26a311", "57513f9324597720a7128161", "575146b724597720a27126d5",
            "544fb62a4bdc2dfb738b4568", "57513fcc24597720a31c09a6"
        };

        private static readonly String[] UpdatedDrinkablesWithContamination = DrinkablesWithContamination
            .Select(drink => drink + " Name")
            .ToArray();

        private static readonly String[] DrinkablesDecreaceContamination =
        {
            "5d403f9186f7743cac3f229b", "5d1b376e86f774252519444e", "5d40407c86f774318526545a",
            "5d1b33a686f7742523398398"
        };

        private static readonly String[] UpdatedDrinkablesDecreaceContamination = DrinkablesDecreaceContamination
            .Select(drink => drink + " Name")
            .ToArray();
        
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.MedsController), nameof(Player.MedsController.Class1158.SetOnUsedCallback));
        }

        [PatchPostfix]
        private static void Postfix()
        {
            var profileInfo = Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession()?.Profile;
            
            if (profileInfo == null) return;

            var currentPoison = profileInfo.Health.Poison.Current;
            
            var drinkContamination = Random.Next(4, 8); 
            var drinkContaminationDecreace = 20f;
            var foodContamination = Random.Next(1, 5); 
            
            
            var chance = Plugin.ExpiredFoodChance.Value;
            var roll = Random.Next(0, 100);
            
            if (UpdatedDrinkablesDecreaceContamination.Contains(PlayerInfo.medsController.Item.Name))
            {
                currentPoison -= drinkContaminationDecreace;
                profileInfo.Health.Poison.Current = Mathf.Clamp(currentPoison, 0f, 100f);
            }
            
            if (roll < chance)
            {
                // Lógica a ser executada caso a chance seja atingida
                if (UpdatedDrinkablesWithContamination.Contains(PlayerInfo.medsController.Item.Name))
                {
                    currentPoison += drinkContamination;
                    profileInfo.Health.Poison.Current = Mathf.Clamp(currentPoison, 0f, 100f);
                }
                
            }
            else
            {
                Logger.LogInfo($"Evento não disparado (chance: {chance}%, roll: {roll})");
            }
            
        }
    }
    
    public class changeToxicationHUD : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HealthParametersPanel), nameof(HealthParametersPanel.method_0));
        }
        
        private static Color GetToxicityColor(float toxicityLevel)
        {
            if (toxicityLevel < 20)
            {
                return new Color(160f / 255f, 214f / 255f, 136f / 255f); // Green
            }
            else if (toxicityLevel <= 50)
            {
                return new Color(218f / 255f,150f / 255f,2f / 255f); // Yellow
            }
            else
            {
                return new Color(175f / 255f,3f / 255f,3f / 255f); // Red
            }
        }

        [PatchPostfix]
        private static void Postfix(HealthParametersPanel __instance)
        {
            //HealthParameterPanel _radiation = (HealthParameterPanel)AccessTools.Field(typeof(HealthParametersPanel), "_radiation").GetValue(__instance);
            var HUD = __instance.gameObject;
            
            var profileInfo = Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession()?.Profile;

            if (profileInfo == null) return;
            
            var currentPoison = profileInfo.Health.Poison.Current;
            
            if (HUD.transform.childCount <= 0) return;
            
            var poison = HUD.transform.Find("Poisoning")?.gameObject;
            
            if (!poison) return;
            
            var current = poison.transform.Find("Current")?.gameObject;
            if (!current) return;
            
            var toxicityLevel = Mathf.Round(currentPoison);
            var currentUI = current.GetComponent<CustomTextMeshProUGUI>();
            currentUI.text = toxicityLevel.ToString();
            // Define the color based on toxicity level
            currentUI.color = GetToxicityColor(toxicityLevel);
            currentUI.fontSize = 28f;
        }
    }
    
   
    public class ExodrineBuffPatch : ModulePatch
    {
        
        //private static HashSet<Player> _playersWithPendingFracture = new HashSet<Player>();
        
        // Dictionary to store the timer for each player
        private static Dictionary<Player, Timer> _playerTimers = new Dictionary<Player, Timer>();

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
