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
                Camera mainCamera = null;
                Camera scopeCamera = null;
                Camera[] cams = Camera.allCameras;
                foreach(Camera cam in cams) 
                {
                    if (cam.name == "FPS Camera") 
                    {
                        mainCamera = cam;
                        break;
                    }
                    if (cam.name == "BaseOpticCamera(Clone)") 
                    {
                        scopeCamera = cam;
                        break;
                    }
                }

                float aimedFOV = scopeCamera == null ? Plugin.BaseScopeFOV.Value : scopeCamera.fieldOfView;
                float hipFOV = Mathf.Deg2Rad * Camera.VerticalToHorizontalFieldOfView(Singleton<SharedGameSettingsClass>.Instance.Game.Settings.FieldOfView, mainCamera.aspect);
                float realAimedFOV = Mathf.Deg2Rad * Camera.VerticalToHorizontalFieldOfView(aimedFOV, mainCamera.aspect);
                float exponent = 100f / Plugin.MouseSensFactor.Value;
                float tanRatio = (float)(Mathf.Tan(realAimedFOV / 2) / Mathf.Tan(hipFOV / 2));
                float inGameSens = Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity;
                float newSens = (float)Math.Pow(tanRatio, exponent) * inGameSens;

                Plugin.AimingSens = newSens;
                if (!Plugin.RecoilStandaloneIsPresent && !Plugin.RealismModIsPresent)
                {
                    ____aimingSens = newSens;
                }
            }


        }
    }
}
