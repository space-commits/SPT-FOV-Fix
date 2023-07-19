using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace FOVFix
{
    public class AimingSensitivityPatch : ModulePatch
    {
        public static float GetZoomSensValue(float magnificaiton) 
        {
            switch (magnificaiton)
            {
                case <= 1.5f:
                    return Plugin.OneSensMulti.Value;
                case <= 2:
                    return Plugin.TwoSensMulti.Value;
                case <= 3:
                    return Plugin.ThreeSensMulti.Value;
                case <= 4:
                    return Plugin.FourSensMulti.Value;
                case <= 5:
                    return Plugin.FiveSensMulti.Value;
                case <= 6:
                    return Plugin.SixSensMulti.Value;
                case <= 8:
                    return Plugin.EightSensMulti.Value;
                case <= 10:
                    return Plugin.TenSensMulti.Value;
                case <= 12:
                    return Plugin.TwelveSensMulti.Value;
                case > 12:
                    return Plugin.HighSensMulti.Value;
                default:
                    return 1;
            }
        }


        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController).GetMethod("get_AimingSensitivity");
        }
        [PatchPrefix]
        public static void PatchPrefix(ref float ____aimingSens)
        {
            if (Plugin.IsAiming)
            {
                /*  float baseSens = Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity;
                  float newSens = Mathf.Max(baseSens * (1f - ((Plugin.CurrentZoom - 1f) / Plugin.MouseSensFactor.Value)), Plugin.MouseSensLowerLimit.Value);
  */
                float newSens = 0.5f;

                if (Plugin.UseBasicSensCalc.Value)
                {
                    newSens = Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity *  GetZoomSensValue(Plugin.CurrentZoom);
                    Plugin.AimingSens = newSens;
                }
                else 
                {
                    Camera mainCamera = null;
                    Camera scopeCamera = null;
                    Camera[] cams = Camera.allCameras;
                    foreach (Camera cam in cams)
                    {
                        if (cam.name == "FPS Camera")
                        {
                            mainCamera = cam;
                            continue;
                        }
                        if (cam.name == "BaseOpticCamera(Clone)")
                        {
                            scopeCamera = cam;
                        }
                    }

                    float aimedFOV = !Plugin.IsOptic || scopeCamera == null ? Plugin.BaseScopeFOV.Value : scopeCamera.fieldOfView;
                    float hipFOV = Mathf.Deg2Rad * Camera.VerticalToHorizontalFieldOfView(mainCamera.fieldOfView, mainCamera.aspect);
                    float realAimedFOV = Mathf.Deg2Rad * Camera.VerticalToHorizontalFieldOfView(aimedFOV, mainCamera.aspect);
                    float exponent = 100f / Plugin.MouseSensFactor.Value;
                    float tanRatio = (float)(Mathf.Tan(realAimedFOV / 2) / Mathf.Tan(hipFOV / 2));
                    float inGameSens = Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity;
                    newSens = Mathf.Pow(tanRatio, exponent) * inGameSens;
                    Plugin.AimingSens = newSens;
                }
               
                if (!Plugin.RecoilStandaloneIsPresent && !Plugin.RealismModIsPresent)
                {
                    ____aimingSens = newSens;
                }
            }
        }
    }
}
