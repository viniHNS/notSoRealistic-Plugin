
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

namespace notSoRealistic.Utils
{
    
    public static class playerStats
    {
        public static Profile profile;

        static playerStats()
        {
            profile = Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession()?.Profile;
        }

        public static float Contamination
        {
            get => profile.Health.Poison.Current;
            set => profile.Health.Poison.Current = value;
        }
    }

    
    
    
}
