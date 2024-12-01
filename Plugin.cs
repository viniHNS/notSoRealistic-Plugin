using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using notSoRealistic.MyPatches;
using notSoRealistic.Utils;
using System.Timers;
using UnityEngine;

namespace notSoRealistic
{
    // first string below is your plugin's GUID, it MUST be unique to any other mod. Read more about it in BepInEx docs. Be sure to update it if you copy this project.
    [BepInPlugin("com.vinihns.notSoRealistic", "Not So Realistic!", "1.0.0")]



    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        public static ConfigEntry<bool> CanWalkInSurgery;
        public static ConfigEntry<bool> CanSprintUsingMeds;
        public static ConfigEntry<bool> CanResolveMalfunctionsWithoutInspection;
        public static ConfigEntry<bool> RemoveBossMalfunctions;
        public static ConfigEntry<int> ExpiredFoodChance;

        private Profile profileInfo;

        private float timeSinceLastContusion = 0f;
        private float contusionInterval = 60f;

        private void Awake()
        {
            LogSource = Logger;
            LogSource.LogInfo("Adding little things");

            CanWalkInSurgery = Config.Bind("Medical Changes", "Can walk in surgery", true,
                "Allows the player to walk while performing surgical treatments.");
            CanSprintUsingMeds = Config.Bind("Medical Changes", "Can sprint using meds", true,
                "Allows sprinting while using meds—because pain can't always slow you down.");
            CanResolveMalfunctionsWithoutInspection = Config.Bind("Weapon Changes",
                "Can resolve malfunctions without inspection", true,
                "Allows the player to resolve weapon malfunctions without inspecting the weapon first.");
            RemoveBossMalfunctions = Config.Bind("Weapon Changes", "Remove boss malfunctions", true,
                "Removes weapon malfunctions caused by bosses.");
            ExpiredFoodChance = Config.Bind<int>("Other Changes", "Contaminated Food Chance", 0,
                new ConfigDescription("Chance to the food contaminate the player",
                    new AcceptableValueRange<int>(0, 100)));

            PatchManager.EnablePatches();
        }


        private void Update()
        {
            if (ExpiredFoodChance.Value <= 0) return;
            
            var gameWorld = Singleton<GameWorld>.Instance;

            if (!gameWorld || !gameWorld.MainPlayer) return;

            if (PlayerInfo.player is HideoutPlayer) return;

            profileInfo = Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession()?.Profile;

            if (profileInfo == null)
            {
                LogSource.LogError("profileInfo is null");
                return;
            }

            if (profileInfo.Health == null)
            {
                LogSource.LogError("profileInfo.Health is null");
                return;
            }

            if (profileInfo.Health.Poison == null)
            {
                LogSource.LogError("profileInfo.Health.Poison is null");
                return;
            }

            timeSinceLastContusion += Time.deltaTime;

            // Adjust contusion interval based on poison level
            if (profileInfo.Health.Poison.Current > 20 && profileInfo.Health.Poison.Current <= 40)
            {
                contusionInterval = 120f;
            }
            else if (profileInfo.Health.Poison.Current > 40 && profileInfo.Health.Poison.Current <= 80)
            {
                contusionInterval = 60f;
            }
            else if (profileInfo.Health.Poison.Current > 80)
            {
                contusionInterval = 30f;
            }
            else
            {
                contusionInterval = 120f;
            }

            if (!(timeSinceLastContusion >= contusionInterval)) return;

            if (profileInfo.Health.Poison.Current > 20 && profileInfo.Health.Poison.Current <= 40)
            {
                PlayerInfo.player.ActiveHealthController.DoContusion(5, 50);
                PlayerInfo.player.ActiveHealthController.DoStun(1, 0);
            }

            if (profileInfo.Health.Poison.Current > 40 && profileInfo.Health.Poison.Current <= 80)
            {
                PlayerInfo.player.ActiveHealthController.DoContusion(10, 50);
                PlayerInfo.player.ActiveHealthController.DoStun(1, 0);
                TarkovEffects.ApplyEffect(PlayerInfo.player, "Tremor", EBodyPart.Head, 0f, 45f, 10f, 1);
            }

            if (profileInfo.Health.Poison.Current > 80)
            {
                PlayerInfo.player.ActiveHealthController.DoContusion(10, 50);
                PlayerInfo.player.ActiveHealthController.DoStun(1, 0);
                TarkovEffects.ApplyEffect(PlayerInfo.player, "Tremor", EBodyPart.Head, 0f, 45f, 10f, 1);
                TarkovEffects.ApplyEffect(PlayerInfo.player, "TunnelVision", EBodyPart.Head, 0f, 20f, 10f, 1);
                TarkovEffects.ApplyBleeding(PlayerInfo.player, EBodyPart.Head, "LightBleeding");
            }

            timeSinceLastContusion = 0f;
        }
        
    }
    
}
