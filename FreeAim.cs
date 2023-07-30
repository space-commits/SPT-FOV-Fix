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
using static UnityEngine.GraphicsBuffer;
using System.Collections;
using EFT.InventoryLogic;

namespace FOVFix
{
    public static class FreeAimController 
    {
        public static bool IsInDeadZone = false;

        public static bool DoPanCameraToAiming = false;
        public static bool DoPanCameraFromAiming = false;

        public static bool PanCamRight = false;
        public static bool PanCamLeft = false;
        public static bool PanCamUp = false;
        public static bool PanCamDown = false;

        public static Vector3 WeapRotation = Vector3.zero;  
    }


    public class FreeAimPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ProceduralWeaponAnimation).GetMethod("CalculateCameraPosition", BindingFlags.Instance | BindingFlags.Public);
        }

        private static Vector3 accumulatedRotation = Vector3.zero;
        private static bool wasAiming = false;
        private static bool doRotationReset  = false;

        private static void panCamera(bool panLeft, bool panRight, bool panUp, bool panDown)
        {
            FreeAimController.PanCamLeft = panLeft;
            FreeAimController.PanCamRight = panRight;
            FreeAimController.PanCamUp = panUp;
            FreeAimController.PanCamDown = panDown;
        }

        
        [PatchPostfix]
        private static void PatchPostfix(ProceduralWeaponAnimation __instance)
        {

            if (Plugin.IsReady && Plugin.WeaponReady)
            {
                Player.FirearmController firearmController = (Player.FirearmController)AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "firearmController_0").GetValue(__instance);

                try
                {
                    Player player = (Player)AccessTools.Field(typeof(EFT.Player.FirearmController), "_player").GetValue(firearmController);

                    if (player.IsYourPlayer)
                    {

                     /*   if (Plugin.isRotating)
                        {
                            __instance.HandsContainer.HandsRotation.Damping = 0.5f;
                            __instance.HandsContainer.HandsRotation.InputIntensity = 1f;
                            __instance.HandsContainer.HandsRotation.AccelerationMax = 10f;
                            __instance.HandsContainer.HandsRotation.ReturnSpeed = 0.5f;
                        }
                        else
                        {
                            __instance.HandsContainer.HandsRotation.Damping = 0.5f;
                            __instance.HandsContainer.HandsRotation.InputIntensity = 1f;
                            __instance.HandsContainer.HandsRotation.AccelerationMax = 10f;
                            __instance.HandsContainer.HandsRotation.ReturnSpeed = 1f;
                        }*/

                        __instance.HandsContainer.HandsRotation.Damping = 0.5f;
                        __instance.HandsContainer.HandsRotation.InputIntensity = 1f;
                        __instance.HandsContainer.HandsRotation.AccelerationMax = 10f;
                        __instance.HandsContainer.HandsRotation.ReturnSpeed = 1f;

                        float sensMulti = Plugin.IsAiming ? Plugin.FreeAimADSSens.Value : Plugin.FreeAimHipSens.Value;

                        float mouseX = Input.GetAxis("Mouse X");
                        float mouseY = Input.GetAxis("Mouse Y");

                        if (!doRotationReset) 
                        {
                            accumulatedRotation.x -= mouseY * sensMulti;
                            accumulatedRotation.z += mouseX * sensMulti;
                        }
             
                        float hipMult = Plugin.FreeAimHipDeadzoneMulti.Value;

                        if (!doRotationReset) 
                        {
                            accumulatedRotation.x = Mathf.Clamp(accumulatedRotation.x, -Plugin.DeadZoneXLimit.Value * hipMult, Plugin.DeadZoneXLimit.Value * hipMult);
                            accumulatedRotation.z = Mathf.Clamp(accumulatedRotation.z, -Plugin.DeadZoneYLimit.Value * hipMult, Plugin.DeadZoneYLimit.Value * hipMult);
                        }

                        if (!doRotationReset && (mouseX * sensMulti < Plugin.PanCamThreshold.Value && mouseX * sensMulti > -Plugin.PanCamThreshold.Value) && (mouseY * sensMulti < Plugin.PanCamThreshold.Value && mouseY * sensMulti > -Plugin.PanCamThreshold.Value))
                        {
         /*                   accumulatedRotation.x = Mathf.Clamp(accumulatedRotation.x, -Plugin.DeadZoneXLimit.Value * hipMult, Plugin.DeadZoneXLimit.Value * hipMult);
                            accumulatedRotation.z = Mathf.Clamp(accumulatedRotation.z, -Plugin.DeadZoneYLimit.Value * hipMult, Plugin.DeadZoneYLimit.Value * hipMult);*/
                            panCamera(false, false, false, false);
                        }
                        else if (!FreeAimController.IsInDeadZone)
                        {

                            Logger.LogWarning("making cam pan");

                            if (mouseY >= 0.3f)
                            {
                                panCamera(false, false, true, false);
                            }
                            else if (mouseY <= -0.3f)
                            {
                                panCamera(false, false, false, true);
                            }
                            else if (mouseX <= -0.3f)
                            {
                                panCamera(true, false, false, false);
                            }
                            else if (mouseX >= 0.3f)
                            {
                                panCamera(false, true, false, false);
                            }
                            else
                            {
                                panCamera(false, false, false, false);
                            }
                        }

          /*              if (!doRotationReset  && Plugin.isRotating) //looks a bit jank, need to maybe instead lerp accumulatedRotation
                        {
                            accumulatedRotation.x = Mathf.Clamp(accumulatedRotation.x, -Plugin.DeadZoneXLimit.Value * hipMult * 4, Plugin.DeadZoneXLimit.Value * hipMult * 4);
                            accumulatedRotation.z = Mathf.Clamp(accumulatedRotation.z, -Plugin.DeadZoneYLimit.Value * hipMult * 4, Plugin.DeadZoneYLimit.Value * hipMult * 4);
                        }*/

                        if (accumulatedRotation.x > -Plugin.DeadZoneXLimit.Value && accumulatedRotation.x < Plugin.DeadZoneXLimit.Value && accumulatedRotation.z > -Plugin.DeadZoneYLimit.Value && accumulatedRotation.z < Plugin.DeadZoneYLimit.Value)
                        {
                            FreeAimController.IsInDeadZone = true;
                        }
                        else 
                        {
                            FreeAimController.IsInDeadZone = false;
                        }

                        if (wasAiming != Plugin.IsAiming && Plugin.IsAiming && (Plugin.AlwaysResetOnAim.Value || !FreeAimController.IsInDeadZone))
                        {
                            Logger.LogWarning("aim");
                            FreeAimController.DoPanCameraToAiming = true;
                            doRotationReset = true;
                        }

                        if (wasAiming != Plugin.IsAiming && !Plugin.IsAiming && Plugin.ResetWhenUnaiming.Value)
                        {
                            Logger.LogWarning("un-aim");
                            FreeAimController.DoPanCameraFromAiming = true;
                            doRotationReset = true;
                        }

                        if (!FreeAimController.DoPanCameraToAiming && FreeAimController.DoPanCameraFromAiming && doRotationReset) 
                        {
                            Logger.LogWarning("reseting");
                            accumulatedRotation = Vector3.Lerp(accumulatedRotation, Vector3.zero, 0.1f);
                            Logger.LogWarning(__instance.HandsContainer.HandsRotation.Zero);
                            if (accumulatedRotation.x <= 0.01f && accumulatedRotation.x >= -0.01f && accumulatedRotation.z <= 0.01f && accumulatedRotation.z >= -0.01f)
                            {
                                Logger.LogWarning("reset");
                                doRotationReset = false;
                                accumulatedRotation = Vector3.zero;
                            }
                        }

                        __instance.HandsContainer.HandsRotation.Zero = Vector3.Lerp(__instance.HandsContainer.HandsRotation.Zero, accumulatedRotation, 1f);  // + __instance.RotationZeroSum

                        wasAiming = Plugin.IsAiming;
                        FreeAimController.WeapRotation = accumulatedRotation;
 
                        /*__instance.HandsContainer.DefaultAimPlane = __instance.HandsContainer.FarPlane;*/ //don't think this was doing anything, but in case it was leaving it here for now*/
                    }
                }
                catch
                {
                    return;
                }
            }
        }
    }

    public class CalculateScaleValueByFovPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("CalculateScaleValueByFov");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref float ___float_10)
        {
            ___float_10 = Plugin.FovScale.Value;
        }
    }


    public class InitTransformsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.Animations.ProceduralWeaponAnimation).GetMethod("InitTransforms", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(EFT.Animations.ProceduralWeaponAnimation __instance)
        {

            Player.FirearmController firearmController = (Player.FirearmController)AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "firearmController_0").GetValue(__instance);

            if (firearmController != null)
            {
                Player player = (Player)AccessTools.Field(typeof(EFT.Player.FirearmController), "_player").GetValue(firearmController);
                if (player.IsYourPlayer == true)
                {
                    /*__instance.HandsContainer.WeaponRoot.localPosition += new Vector3(Plugin.test1.Value, Plugin.test2.Value, Plugin.test3.Value);*/
                }
            }
        }
    }



    public class RotatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MovementState).GetMethod("Rotate", BindingFlags.Instance | BindingFlags.Public);
        }

        private static float rotationMulti = 0f;

        [PatchPrefix]
        private static bool Prefix(MovementState __instance, ref Vector2 deltaRotation, bool ignoreClamp)
        {
            GClass1603 MovementContext = (GClass1603)AccessTools.Field(typeof(MovementState), "MovementContext").GetValue(__instance);
            Player player = (Player)AccessTools.Field(typeof(GClass1603), "player_0").GetValue(MovementContext);

            if (player.IsYourPlayer) 
            {
                if (!Plugin.RealismModIsPresent)
                {
                    /*         if (FreeAimController.IsInDeadZone)
                             {
                                 if (!Plugin.FreeAimBlocksRotation.Value)
                                 {
                                     deltaRotation *= Plugin.FreeAimRotationReduction.Value;
                                     return true;
                                 }
                                 return false;
                             }
                             else
                             {
                                 if (FreeAimController.PanCamLeft)
                                 {
                                     deltaRotation.x += -1f * Plugin.CamRotationMulti.Value;
                                 }
                                 if (FreeAimController.PanCamRight)
                                 {
                                     deltaRotation.x += 1f * Plugin.CamRotationMulti.Value;
                                 }
                                 if (FreeAimController.PanCamUp)
                                 {
                                     deltaRotation.y += -1f * Plugin.CamRotationMulti.Value;
                                 }
                                 if (FreeAimController.PanCamDown)
                                 {
                                     deltaRotation.y += 1f * Plugin.CamRotationMulti.Value;
                                 }
                                 deltaRotation = MovementContext.ClampRotation(deltaRotation);
                                 MovementContext.Rotation += deltaRotation;
                                 return false;
                             }*/

                    /*            if (Input.GetKeyDown(KeyCode.PageUp))
                                {
                                    doRotation = true;
                                    targetX = Plugin.test2.Value;
                                }
                                if (Input.GetKeyDown(KeyCode.PageDown))
                                {
                                    doRotation = true;
                                    targetX = -Plugin.test2.Value;
                                }

                                if (doRotation && !Utils.FloatsAreAproxEqual(currentX, targetX, (int)Plugin.test3.Value))
                                {
                                    Logger.LogWarning("not =");
                                    currentX = Mathf.Lerp(currentX, targetX, Plugin.test1.Value);
                                }
                                else if (!Utils.FloatsAreAproxEqual(currentX, 0f, (int)Plugin.test3.Value)) 
                                {
                                    doRotation = false;
                                    Logger.LogWarning("=");
                                    currentX = Mathf.Lerp(currentX, 0f, Plugin.test1.Value);
                                }

                                Logger.LogWarning("currentX " + currentX);
                                Logger.LogWarning("targetX " + targetX);*/


                    if (!Plugin.AlwaysAllowRotation.Value)
                    {
                        if (FreeAimController.IsInDeadZone)
                        {
                            rotationMulti = Mathf.Lerp(rotationMulti, 0f, Plugin.DeadZoneRotateResetSpeed.Value);
                        }
                        else
                        {
                            rotationMulti = Mathf.Lerp(rotationMulti, Plugin.CamRotationMulti.Value, Plugin.RotateRampUpSpeed.Value);
                        }
                        deltaRotation *= rotationMulti;
                    }
                    else
                    {
                        deltaRotation *= Plugin.CamRotationMulti.Value;
                    }
                  
                    deltaRotation = MovementContext.ClampRotation(deltaRotation);
                    MovementContext.Rotation += Plugin.camPanRotation + deltaRotation;

                    return false; 
                }
            }
            return true;
        }
    }
}
