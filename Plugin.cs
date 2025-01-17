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
    [BepInPlugin("com.vinihns.notSoRealistic", "Not So Realistic!", "1.1.0")]



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

        
        
    }
    
}
