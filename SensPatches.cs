using Comfort.Common;
using EFT;
using RealismMod;
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
            if (Plugin.ChangeMouseSens.Value)
            {
                float newSens = 1f;
                bool isOptic = Plugin.FovController.OpticWatcher.WatchedValue;
                bool isAiming = Plugin.FovController.ADSWatcher.WatchedValue;
                bool calledToggleZoom = Plugin.FovController.IsToggleZoom;
                float toggleZoomMulti =
                    calledToggleZoom && isAiming && isOptic ? Plugin.ToggleZoomOpticSensMulti.Value :
                    calledToggleZoom && isAiming ? Plugin.ToggleZoomAimSensMulti.Value :
                    calledToggleZoom ? Plugin.ToggleZoomUnAimSensMulti.Value : 1f;

                float scopeFOVMulti = isOptic && isAiming ? Utils.GetZoomSensValue(Plugin.FovController.CurrentScopeFOV) : Plugin.NonOpticSensMulti.Value;
                newSens = Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity * toggleZoomMulti * scopeFOVMulti;
                ____aimingSens = newSens;
            }
        }
    }
}
