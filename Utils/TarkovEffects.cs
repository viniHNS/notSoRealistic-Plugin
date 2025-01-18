
using System;
using System.Linq;
using EFT;
using EFT.HealthSystem;
using System.Reflection;
// ReSharper disable InconsistentNaming

namespace notSoRealistic.Utils
{
    public static class TarkovEffects
    {
        public static void ApplyEffect(Player player, string effectName, EBodyPart bodyPart, float? delayTime, float? duration, float? residueTime, float? strength)
        {
            // Effects: TunnelVision, Contusion, Tremor, LightBleeding, HeavyBleeding
    
            // Verificar se o player e o ActiveHealthController estão presentes
            if (!player || player.ActiveHealthController == null)
            {
                throw new InvalidOperationException("Player or ActiveHealthController is null.");
            }

            // Use Reflection to find the nested effect type
            var effectType = typeof(ActiveHealthController)
                .GetNestedType(effectName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (effectType == null)
            {
                throw new InvalidOperationException($"Effect type '{effectName}' not found in ActiveHealthController.");
            }

            // Retrieve the generic AddEffect method
            var addEffectMethod = GetEffectMethod();
            if (addEffectMethod == null)
            {
                throw new InvalidOperationException("AddEffect method not found in ActiveHealthController.");
            }

            // Make the method generic for the desired effect type and invoke it
            addEffectMethod.MakeGenericMethod(effectType)
                .Invoke(player.ActiveHealthController, new object[] { bodyPart, delayTime, duration, residueTime, strength, null });
        }

        private static MethodInfo GetEffectMethod()
        {
            // Use Reflection to find the AddEffect method with the appropriate signature
            return typeof(ActiveHealthController).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(method =>
                    method.IsGenericMethod &&
                    method.GetParameters().Length == 6 &&
                    method.GetParameters()[0].Name == "bodyPart" &&
                    method.GetParameters()[5].Name == "initCallback");
        }
        
        public static void ApplyBleeding(Player player, EBodyPart bodyPart, string bleedingTypeName)
        {
            // Retrieve the bleeding type class using Reflection
            var bleedingType = typeof(ActiveHealthController).GetNestedType(bleedingTypeName, BindingFlags.NonPublic);

            if (bleedingType == null) return;
            
            // Retrieve the DoBleed method with specific parameter types
            var doBleedMethod = typeof(ActiveHealthController).GetMethod(
                "DoBleed",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(EBodyPart) },
                null
            );

            if (doBleedMethod == null) return;
            
            // Use Reflection to call the generic method DoBleed with the correct type
            doBleedMethod.MakeGenericMethod(bleedingType)
                .Invoke(player.ActiveHealthController, new object[] { bodyPart });
        }
    }
    

    
}
