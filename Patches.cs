using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace FOVFix
{
    public class SetFovPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CameraClass).GetMethod("SetFov", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(CameraClass __instance, float time, Coroutine ___coroutine_0, bool applyFovOnCamera = true)
        {

            var _method_4 = AccessTools.Method(typeof(CameraClass), "method_4");
            float fov = Singleton<GClass1659>.Instance.Game.Settings.FieldOfView.Value;

            if (___coroutine_0 != null)
            {
                StaticManager.KillCoroutine(___coroutine_0);
            }
            if (__instance.Camera == null)
            {
                return false;
            }
            IEnumerator meth4Enumer = (IEnumerator)_method_4.Invoke(__instance, new object[] { fov, time });
            AccessTools.Property(typeof(CameraClass), "ApplyDovFovOnCamera").SetValue(__instance, applyFovOnCamera);
            ___coroutine_0 = StaticManager.BeginCoroutine(meth4Enumer);
            return false;
        }
    }
}
