using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using FistVR;
using HarmonyLib;
using UnityEngine;

// TODO: Change 'YourName' to your name. 
namespace LarpOfGod
{
    // TODO: Change 'YourPlugin' to the name of your plugin
    [BepInEx.BepInPlugin(NoMoreMagicVests.Id, "NoMoreMagicVests", "1.0.4")]
    [BepInProcess("h3vr.exe")]
    public partial class NoMoreMagicVests : BaseUnityPlugin
    {
        public const string Id = "larpofgod.nomoremagicvests";
        public static string Name => "NoMoreMagicVests";
        public static string Version => "1.0.4";
        /* == Quick Start == 
         * Your plugin class is a Unity MonoBehaviour that gets added to a global game object when the game starts.
         * You should use Awake to initialize yourself, read configs, register stuff, etc.
         * If you need to use Update or other Unity event methods those will work too.
         *
         * Some references on how to do various things:
         * Adding config settings to your plugin: https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/4_configuration.html
         * Hooking / Patching game methods: https://harmony.pardeike.net/articles/patching.html
         * Also check out the Unity documentation: https://docs.unity3d.com/560/Documentation/ScriptReference/index.html
         * And the C# documentation: https://learn.microsoft.com/en-us/dotnet/csharp/
         */
        public static ConfigEntry<float> damageAbsorbtionMultiplier;
        [HarmonyPatch(typeof(SosigWearable), nameof(SosigWearable.Damage))]
        class Patch
        {
            static bool Prefix(ref Damage d, ref SosigWearable __instance)
            {
                bool flag = false;
                if (d.Class == FistVR.Damage.DamageClass.Projectile)
                {
                    if (__instance.S != null)
                    {
                        if (__instance.WeakPointVolumes.Count > 0)
                        {
                            for (int i = 0; i < __instance.WeakPointVolumes.Count; i++)
                            {
                                if (__instance.TestVolumeBool(__instance.WeakPointVolumes[i].WeakPointBox, d.point))
                                {
                                    float num = 1f;
                                    if (__instance.S.IsDamResist || __instance.S.IsDamMult)
                                    {
                                        num = __instance.S.BuffIntensity_DamResistHarm;
                                    }
                                    if (__instance.S.IsFragile)
                                    {
                                        num *= 100f;
                                    }
                                    __instance.S.SetLastIFFDamageSource(d.Source_IFF);
                                    if (d.Source_IFF != __instance.S.E.IFFCode && d.Source_IFF > -1)
                                    {
                                        __instance.S.SetLastDamageReceivedClass(d.Class);
                                    }
                                    __instance.S.ProcessDamage(d.Dam_Piercing * __instance.WeakPointVolumes[i].PiercingDamageTransmission * __instance.S.DamMult_Piercing * num, 0f, d.Dam_Blunt * __instance.WeakPointVolumes[i].BluntDamageTransmission * __instance.S.DamMult_Blunt * num, 0f, d.point, __instance.L);
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (!flag && __instance.BluntDamageTransmission > 0.01f)
                        {
                            float num2 = 1f;
                            if (__instance.S.IsDamResist || __instance.S.IsDamMult)
                            {
                                num2 = __instance.S.BuffIntensity_DamResistHarm;
                            }
                            if (__instance.S.IsFragile)
                            {
                                num2 *= 100f;
                            }
                            __instance.S.SetLastIFFDamageSource(d.Source_IFF);
                            if (d.Source_IFF != __instance.S.E.IFFCode && d.Source_IFF > -1)
                            {
                                __instance.S.SetLastDamageReceivedClass(d.Class);
                            }
                            d.Dam_Piercing *= 0;
                            d.Dam_Blunt = d.Dam_TotalKinetic;
                            d.Dam_Blunt *= __instance.BluntDamageTransmission / damageAbsorbtionMultiplier.Value;
                            __instance.L.Damage(d);
                        }
                    }
                    float num3 = 1f;
                    if (__instance.L.BodyPart == SosigLink.SosigBodyPart.Head)
                    {
                        num3 = 1f;
                    }
                    else if (__instance.L.BodyPart == SosigLink.SosigBodyPart.Torso)
                    {
                        num3 = 0.4f;
                    }
                    else if (__instance.L.BodyPart == SosigLink.SosigBodyPart.UpperLink)
                    {
                        num3 = 0.1f;
                    }
                    else if (__instance.L.BodyPart == SosigLink.SosigBodyPart.LowerLink)
                    {
                        num3 = 0.1f;
                    }
                    float num4 = d.Dam_Blunt * num3;
                    float num5 = Mathf.Lerp(0f, 10f, num4 / 4000f);
                    __instance.S.Concuss(num5);
                    return false;
                }
                if (d.Class == FistVR.Damage.DamageClass.Melee && __instance.L != null)
                {
                    __instance.L.Damage(d);
                }
                return false;
            }
        }
        private void Awake()
        {
            Logger = base.Logger;
            damageAbsorbtionMultiplier = Config.Bind("General", "damageAbsorbtionMultiplier", 2f, "Higher values mean armor absorbs more damage (bottom capped at 1)");
            damageAbsorbtionMultiplier.Value = Mathf.Max(1, damageAbsorbtionMultiplier.Value);
            Harmony.CreateAndPatchAll(typeof(Patch));
            // Your plugin's ID, Name, and Version are available here.
            Logger.LogMessage($"Hello, world! Sent from {Id} {Name} {Version}");
        }

        // The line below allows access to your plugin's logger from anywhere in your code, including outside of this file.
        // Use it with 'YourPlugin.Logger.LogInfo(message)' (or any of the other Log* methods)
        internal new static ManualLogSource Logger { get; private set; }
    }
}
