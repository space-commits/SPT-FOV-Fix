using Aki.Reflection.Patching;
using EFT.Animations;
using EFT;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using static EFT.Player;

namespace FOVFix
{
    public static class FreeAimController 
    {
        public static bool IsInDeadZone = false;
        public static bool PanCamRight = false;
        public static bool PanCamLeft = false;
        public static bool PanCamUp = false;
        public static bool PanCamDown = false;
    }


    public class FreeAimPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ProceduralWeaponAnimation).GetMethod("CalculateCameraPosition", BindingFlags.Instance | BindingFlags.Public);
        }


        private static Quaternion accumulatedRotation = Quaternion.identity;

        private static Vector3 accumulatedRotationVect = Vector3.zero;

        private static void panCamera(bool panLeft, bool panRight, bool panUp, bool panDown ) 
        {
            if (panLeft)
            {
                PanCamLeft = true;
                PanCamRight = false;
                PanCamUp = false;
                PanCamDown = false;
            }
            else if (panRight) 
            {
                PanCamLeft = false;
                PanCamRight = true;
                PanCamUp = false;
                PanCamDown = false;
            }
        }

        [PatchPostfix]
        private static void PatchPostfix(ProceduralWeaponAnimation __instance)
        {

            Player.FirearmController firearmController = (Player.FirearmController)AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "firearmController_0").GetValue(__instance);

            __instance.HandsContainer.HandsRotation.Damping = 0.5f;
            __instance.HandsContainer.HandsRotation.InputIntensity = 1f;
            __instance.HandsContainer.HandsRotation.AccelerationMax = 10f;
            __instance.HandsContainer.HandsRotation.ReturnSpeed = 1f;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            float sensMulti = Plugin.IsAiming ? Plugin.FreeAimADSSens.Value : Plugin.FreeAimHipSens.Value;

            accumulatedRotationVect.x -= mouseY * sensMulti;
            accumulatedRotationVect.z += mouseX * sensMulti;

            float hipMult = Plugin.FreeAimHipDeadzoneMulti.Value;

            accumulatedRotationVect.x = Mathf.Clamp(accumulatedRotationVect.x, -Plugin.DeadZoneXLimit.Value * hipMult, Plugin.DeadZoneXLimit.Value * hipMult);
            accumulatedRotationVect.z = Mathf.Clamp(accumulatedRotationVect.z, -Plugin.DeadZoneYLimit.Value * hipMult, Plugin.DeadZoneYLimit.Value * hipMult);

            __instance.HandsContainer.HandsRotation.Zero = accumulatedRotationVect;

            Logger.LogWarning("zero " + __instance.HandsContainer.HandsRotation.Zero);

            if (accumulatedRotationVect.x > -Plugin.DeadZoneXLimit.Value && accumulatedRotationVect.x < Plugin.DeadZoneXLimit.Value && accumulatedRotationVect.z > -Plugin.DeadZoneYLimit.Value && accumulatedRotationVect.z < Plugin.DeadZoneYLimit.Value)
            {
                FreeAimController.IsInDeadZone = true;
            }
            else
            {
                FreeAimController.IsInDeadZone = false;

                if (accumulatedRotationVect.x > -Plugin.DeadZoneXLimit.Value) 
                {
                 
                }
            }

            /*Logger.LogWarning("weaponroot " + __instance.HandsContainer.WeaponRootAnim.localRotation);*/

        }
    }

    public class RotatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MovementState).GetMethod("Rotate", BindingFlags.Instance | BindingFlags.Public);
        }

        private static Vector2 initialRotation = Vector3.zero;

        [PatchPrefix]
        private static bool Prefix(MovementState __instance, ref Vector2 deltaRotation, bool ignoreClamp)
        {
            if (!Plugin.RealismModIsPresent) 
            {
                GClass1603 MovementContext = (GClass1603)AccessTools.Field(typeof(MovementState), "MovementContext").GetValue(__instance);
                if (FreeAimController.IsInDeadZone)
                {
                    if (!Plugin.FreeAimBlocksRotation.Value)
                    {
                        deltaRotation *= Plugin.FreeAimHipDeadzoneMulti.Value;
                        return true;
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
