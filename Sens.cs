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

                Camera mainCamera = GameObject.Find("FPS Camera").GetComponent<Camera>();
                Camera scopeCamera;
                float aimedFOV = Plugin.BaseScopeFOV.Value;
                if (GameObject.Find("BaseOpticCamera(Clone)") != null)
                {
                    scopeCamera = GameObject.Find("BaseOpticCamera(Clone)").GetComponent<Camera>();
                    if (Plugin.IsOptic)
                    {
                        aimedFOV = scopeCamera.fieldOfView;
                    }
                }

                float hipFOV = Mathf.Deg2Rad * Camera.VerticalToHorizontalFieldOfView(Singleton<SharedGameSettingsClass>.Instance.Game.Settings.FieldOfView, mainCamera.aspect);
                float realAimedFOV = Mathf.Deg2Rad * Camera.VerticalToHorizontalFieldOfView(aimedFOV, mainCamera.aspect);
                float exponent = 100f / Plugin.MouseSensFactor.Value; 
                float tanRatio = (float)(Mathf.Tan(realAimedFOV / 2) / Mathf.Tan(hipFOV / 2));
                float baseSens = Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity;
                float newSens = (float)Math.Pow(tanRatio, exponent) * baseSens;

                Plugin.AimingSens = newSens;
                if (!Plugin.RecoilStandaloneIsPresent && !Plugin.RealismModIsPresent) 
                {
                    ____aimingSens = newSens;
                }
            }
        }
    }
}
