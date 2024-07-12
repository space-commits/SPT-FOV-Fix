using Comfort.Common;
using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
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
        public static void PatchPrefix(Player.FirearmController __instance, ref float ____aimingSens)
        {
            float newSens = 1f;
            float toggleZoomMulti = Plugin.FovController.CalledToggleZoom && Plugin.FovController.IsAiming && Plugin.FovController.IsOptic ? Plugin.ToggleZoomOpticSensMulti.Value : Plugin.FovController.CalledToggleZoom && Plugin.FovController.IsAiming ? Plugin.ToggleZoomAimSensMulti.Value : Plugin.FovController.CalledToggleZoom ? Plugin.ToggleZoomMulti.Value : 1f;

            if (Plugin.UseBasicSensCalc.Value)
            {
                float magnificationMulti = Plugin.FovController.IsOptic && Plugin.FovController.IsAiming ? Utils.GetZoomSensValue(Plugin.FovController.CurrentZoom) : Plugin.NonOpticSensMulti.Value;
                newSens = Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity * toggleZoomMulti * magnificationMulti;
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

                float aimedFOV = !Plugin.FovController.IsOptic || scopeCamera == null ? Plugin.BaseScopeFOV.Value : scopeCamera.fieldOfView;
                float hipFOV = Mathf.Deg2Rad * Camera.VerticalToHorizontalFieldOfView(mainCamera.fieldOfView, mainCamera.aspect);
                float realAimedFOV = Mathf.Deg2Rad * Camera.VerticalToHorizontalFieldOfView(aimedFOV, mainCamera.aspect);
                float exponent = 100f / Plugin.MouseSensFactor.Value;
                float tanRatio = (float)(Mathf.Tan(realAimedFOV / 2) / Mathf.Tan(hipFOV / 2));
                float inGameSens = Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity;
                newSens = Mathf.Pow(tanRatio, exponent) * inGameSens * toggleZoomMulti;
            }

            ____aimingSens = newSens;
        }
    }
}
