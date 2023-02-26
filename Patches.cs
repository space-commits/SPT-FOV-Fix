using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using System.Linq;
using EFT.InventoryLogic;
using Comfort.Common;

namespace FOVFix
{
    public class FreeLookPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("Look", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(Player __instance, float deltaLookY, float deltaLookX, bool withReturn = true)
        {

            bool bool_1 = (bool)AccessTools.Field(typeof(Player), "bool_1").GetValue(__instance);
            bool bool_2 = (bool)AccessTools.Field(typeof(Player), "bool_2").GetValue(__instance);
            bool bool_3 = (bool)AccessTools.Field(typeof(Player), "bool_3").GetValue(__instance);
            bool bool_4 = (bool)AccessTools.Field(typeof(Player), "bool_4").GetValue(__instance);
            bool bool_5 = (bool)AccessTools.Field(typeof(Player), "bool_5").GetValue(__instance);
            bool bool_6 = (bool)AccessTools.Field(typeof(Player), "bool_6").GetValue(__instance);

            float float_0 = (float)AccessTools.Field(typeof(Player), "float_0").GetValue(__instance);
            float float_1 = (float)AccessTools.Field(typeof(Player), "float_1").GetValue(__instance);

            bool isAiming = __instance.HandsController != null && __instance.HandsController.IsAiming && !__instance.IsAI;
            EFTHardSettings instance = EFTHardSettings.Instance;
            Vector2 vector = instance.MOUSE_LOOK_HORIZONTAL_LIMIT;
            Vector2 mouse_LOOK_VERTICAL_LIMIT = instance.MOUSE_LOOK_VERTICAL_LIMIT;
            if (isAiming)
            {
                vector *= instance.MOUSE_LOOK_LIMIT_IN_AIMING_COEF;
            }
            Vector3 eulerAngles = __instance.ProceduralWeaponAnimation.HandsContainer.CameraTransform.eulerAngles;
            if (eulerAngles.x >= 50f && eulerAngles.x <= 90f && __instance.MovementContext.IsSprintEnabled)
            {
                mouse_LOOK_VERTICAL_LIMIT.y = 0f;
            }
            AccessTools.Field(typeof(Player), "float_0").SetValue(__instance, Mathf.Clamp(float_0 - deltaLookY, vector.x, vector.y));
            AccessTools.Field(typeof(Player), "float_1").SetValue(__instance, Mathf.Clamp(float_1 + deltaLookX, mouse_LOOK_VERTICAL_LIMIT.x, mouse_LOOK_VERTICAL_LIMIT.y));
            float x2 = (float_1 > 0f) ? (float_1 * (1f - float_0 / vector.y * (float_0 / vector.y))) : float_1;
            if (bool_4)
            {
                AccessTools.Field(typeof(Player), "bool_3").SetValue(__instance, false);
                AccessTools.Field(typeof(Player), "bool_4").SetValue(__instance, false);
            }
            if (bool_1)
            {
                AccessTools.Field(typeof(Player), "bool_2").SetValue(__instance, false);
                AccessTools.Field(typeof(Player), "bool_1").SetValue(__instance, false);
                AccessTools.Field(typeof(Player), "bool_3").SetValue(__instance, true);
                deltaLookY = 0f;
                deltaLookX = 0f;
            }
            if (Math.Abs(deltaLookY) >= 1E-45f && Math.Abs(deltaLookX) >= 1E-45f)
            {
                AccessTools.Field(typeof(Player), "bool_2").SetValue(__instance, true);
            }
            if (!bool_2 && withReturn)
            {
                if (Mathf.Abs(float_0) > 0.01f)
                {
                    AccessTools.Field(typeof(Player), "float_0").SetValue(__instance, Mathf.Lerp(float_0, 0f, Time.deltaTime * 15f));
                }
                else
                {
                    AccessTools.Field(typeof(Player), "float_0").SetValue(__instance, 0f);
                }
                if (Mathf.Abs(float_1) > 0.01f)
                {
                    AccessTools.Field(typeof(Player), "float_1").SetValue(__instance, Mathf.Lerp(float_1, 0f, Time.deltaTime * 15f));
                }
                else
                {
                    AccessTools.Field(typeof(Player), "float_1").SetValue(__instance, 0f);
                }
            }
            if (!bool_3 && float_0 != 0f && float_1 != 0f)
            {
                AccessTools.Field(typeof(Player), "bool_5").SetValue(__instance, true);
            }
            else
            {
                AccessTools.Field(typeof(Player), "bool_5").SetValue(__instance, false);
            }
            if (float_0 == 0f && float_1 == 0f)
            {
                AccessTools.Field(typeof(Player), "bool_4").SetValue(__instance, true);
            }
            __instance.HeadRotation = new Vector3(x2, float_0, 0f);
            __instance.ProceduralWeaponAnimation.SetHeadRotation(__instance.HeadRotation);
            return false;
        }
    }

    public class OnWeaponParametersChangedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ShotEffector).GetMethod("OnWeaponParametersChanged", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(ShotEffector __instance)
        {
            IWeapon _weapon = (IWeapon)AccessTools.Field(typeof(ShotEffector), "_weapon").GetValue(__instance);
            if (_weapon.Item.Owner.ID.StartsWith("pmc") || _weapon.Item.Owner.ID.StartsWith("scav"))
            {
                Plugin.HasRAPTAR = false;

                if (!_weapon.IsUnderbarrelWeapon) 
                {
                    Weapon weap = _weapon.Item as Weapon;
                    Mod[] weapMods = weap.Mods;
                    foreach (Mod mod in weapMods)
                    {
                        if (mod.TemplateId == "61605d88ffa6e502ac5e7eeb")
                        {
                            Plugin.HasRAPTAR = true;
                        }
                    }
                }

            }
        }
    }


    public class TacticalRangeFinderControllerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TacticalRangeFinderController).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {

            if (Plugin.HasRAPTAR == false && Plugin.disableRangeF.Value == false)
            {
                CameraClass.Instance.OpticCameraManager.Camera.fieldOfView = Plugin.rangeFinderFOV.Value;
            }

        }
    }


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
        protected override MethodBase GetTargetMethod(
            )
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

    //haven't seen this get called yet, so far redundant.
    public class CalcDistancePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.CameraControl.OpticSight).GetMethod("CalcDistance", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(EFT.CameraControl.OpticSight __instance)
        {

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

    public class method_20Patch : ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.Animations.ProceduralWeaponAnimation).GetMethod("method_20", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPostfix]
        private static void PatchPostfix(ref EFT.Animations.ProceduralWeaponAnimation __instance)
        {
;
            Player.FirearmController firearmController = (Player.FirearmController)AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "firearmController_0").GetValue(__instance);
            float baseFOV = (float)AccessTools.Property(typeof(EFT.Animations.ProceduralWeaponAnimation), "Single_2").GetValue(__instance);

            if (firearmController != null)
            {
                Player player = (Player)AccessTools.Field(typeof(EFT.Player.FirearmController), "_player").GetValue(firearmController);

                if (!player.IsAI)
                {
                    __instance.CameraSmoothTime = Plugin.CameraSmoothTime.Value;

                    if (__instance.PointOfView == EPointOfView.FirstPerson)
                    {
                        int AimIndex = (int)AccessTools.Property(typeof(EFT.Animations.ProceduralWeaponAnimation), "AimIndex").GetValue(__instance);

                        if (!__instance.Sprint && AimIndex < __instance.ScopeAimTransforms.Count)
                        {
                            bool isAiming = (bool)AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "bool_1").GetValue(__instance);

                            float zoom = 1;
                            if (player.ProceduralWeaponAnimation.CurrentAimingMod != null)
                            {
                                zoom = player.ProceduralWeaponAnimation.CurrentAimingMod.GetCurrentOpticZoom();

                            }
                             
                            float sightFOV = baseFOV * Helper.getADSFoVMulti(zoom) * Plugin.globalADSMulti.Value;
                            float fov = __instance.IsAiming ? sightFOV : baseFOV;
                            bool isOptic = __instance.CurrentScope.IsOptic;

                            if (Plugin.EnableExtraZoomOptic.Value == true && isOptic == true && Plugin.DoZoom == true)
                            {
                                float zoomedFOV = sightFOV * Plugin.OpticExtraZoom.Value;
                                CameraClass.Instance.SetFov(zoomedFOV, 1f, true);
                                return;
                            }
                            if (Plugin.EnableExtraZoomNonOptic.Value == true && isOptic == false && Plugin.DoZoom == true)
                            {
                                float zoomedFOV = sightFOV * Plugin.NonOpticExtraZoom.Value;
                                CameraClass.Instance.SetFov(zoomedFOV, 1f, true);
                                return;
                            }

                            CameraClass.Instance.SetFov(fov, 1f, !isAiming);
                        }
                        /*                       var method_0 = AccessTools.Method(typeof(EFT.Animations.ProceduralWeaponAnimation), "method_0");

                                               method_0.Invoke(__instance, new object[] { });*/
                    }
                }
            }
            else 
            {
                if (__instance.PointOfView == EPointOfView.FirstPerson)
                {
                    int AimIndex = (int)AccessTools.Property(typeof(EFT.Animations.ProceduralWeaponAnimation), "AimIndex").GetValue(__instance);
                    if (!__instance.Sprint && AimIndex < __instance.ScopeAimTransforms.Count)
                    {

                        bool bool_1 = (bool)AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "bool_1").GetValue(__instance);
                       
                        float sightFOV = baseFOV * Plugin.rangeFinderADSMulti.Value * Plugin.globalADSMulti.Value;
                        float fov = __instance.IsAiming ? sightFOV : baseFOV;

                        CameraClass.Instance.SetFov(fov, 1f, !bool_1);
                    }
                }
            }
        }
    }
}
 