using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.CameraControl;
using EFT.InventoryLogic;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace FOVFix
{



        //this gets called when a new scene is created, so can't change fieldofview multi by current zoom level. 
        public class OpticSightAwakePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.CameraControl.OpticSight).GetMethod("Awake", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(EFT.CameraControl.OpticSight __instance)
        {
            __instance.TemplateCamera.gameObject.SetActive(false);

            if (__instance.name != "DONE")
            {
                if (Plugin.trueOneX.Value == true && __instance.TemplateCamera.fieldOfView >= 24) 
                {
                    return false;
                }
                __instance.TemplateCamera.fieldOfView *= Plugin.globalOpticFOVMulti.Value;
                __instance.name = "DONE";
            }
   
       

            return false;


        }
    }

    public class GetAnyOpticsDistanceToCameraPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ScopePrefabCache).GetMethod("GetAnyOpticsDistanceToCamera", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(ScopePrefabCache __instance, ref float __result)
        {
            ScopePrefabCache.ScopeModeInfo[] _scopeModeInfos = (ScopePrefabCache.ScopeModeInfo[])AccessTools.Field(typeof(ScopePrefabCache), "_scopeModeInfos").GetValue(__instance);
            float distanceMulti = Plugin.globalCameraPOSMulti.Value;

            if (_scopeModeInfos[__instance.CurrentModeId].OpticSight != null)
            {
                __result = _scopeModeInfos[__instance.CurrentModeId].OpticSight.DistanceToCamera * distanceMulti;
                return false;
            }
            __result = __instance.FirstOptic.DistanceToCamera * distanceMulti;
            return false;
        }
    }

    public class CalcDistancePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.CameraControl.OpticSight).GetMethod("CalcDistance", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(EFT.CameraControl.OpticSight __instance)
        {
            Logger.LogWarning("CalcDistancePatch");

            float distanceMulti = Plugin.globalCameraPOSMulti.Value;

            if (__instance.ScopeTransform != null)
            {
                __instance.DistanceToCamera = Vector3.Distance(__instance.ScopeTransform.position * distanceMulti, __instance.LensRenderer.transform.position * distanceMulti);
                return false;
            }
            Transform transform = __instance.transform.Find("mod_aim_camera");
            if (transform == null)
            {
                Debug.Log("Cant set distance for " + __instance.name);
                return false;
            }
            __instance.DistanceToCamera = Vector3.Distance(transform.position * distanceMulti, __instance.LensRenderer.transform.position * distanceMulti);
            return false;
        }
    }

    //better to do it in method_17Patch, as this method also sets FOV in general.
/*    public class SetFovPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CameraClass).GetMethod("SetFov", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(CameraClass __instance, ref float x, float time, Coroutine ___coroutine_0, bool applyFovOnCamera = true)
        {

            var _method_4 = AccessTools.Method(typeof(CameraClass), "method_4");
            float fov = x * Plugin.globalADSMulti.Value;

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
    }*/

    public class method_17Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.Animations.ProceduralWeaponAnimation).GetMethod("method_17", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPostfix]
        private static void PatchPostfix(ref EFT.Animations.ProceduralWeaponAnimation __instance)
        {
;
            Player.FirearmController firearmController = (Player.FirearmController)AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "firearmController_0").GetValue(__instance);

            if (firearmController != null)
            {
                Player player = (Player)AccessTools.Field(typeof(EFT.Player.FirearmController), "_player").GetValue(firearmController);
                if (!player.IsAI)
                {
                    if (__instance.PointOfView == EPointOfView.FirstPerson)
                    {
                        int AimIndex = (int)AccessTools.Property(typeof(EFT.Animations.ProceduralWeaponAnimation), "AimIndex").GetValue(__instance);
                       
                        if (!__instance.Sprint && AimIndex < __instance.ScopeAimTransforms.Count)
                        {
                            bool bool_1 = (bool)AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "bool_1").GetValue(__instance);
                            float Single_2 = (float)AccessTools.Property(typeof(EFT.Animations.ProceduralWeaponAnimation), "Single_2").GetValue(__instance);

                            float zoom = 1;
                            if (player.ProceduralWeaponAnimation.CurrentAimingMod != null) 
                            {
                                zoom = player.ProceduralWeaponAnimation.CurrentAimingMod.GetCurrentOpticZoom();

                            }

                            float baseFOV = Single_2;
                            float sightFOV = baseFOV * Helper.getADSFoVMulti(zoom) * Plugin.globalADSMulti.Value;

                            float fov = __instance.IsAiming ? sightFOV : baseFOV;
                            CameraClass.Instance.SetFov(fov, 1f, !bool_1);
                        }
                        var method_0 = AccessTools.Method(typeof(EFT.Animations.ProceduralWeaponAnimation), "method_0");

                        method_0.Invoke(__instance, new object[] { });
                    }
                }
            }

        }
    }
}
 