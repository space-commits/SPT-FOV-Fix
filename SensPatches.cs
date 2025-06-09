using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;

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
                    calledToggleZoom && isAiming && isOptic && Plugin.OpticToggleZoomMulti.Value != 1f ? Plugin.ToggleZoomOpticSensMulti.Value :
                    calledToggleZoom && isAiming && Plugin.NonOpticToggleZoomMulti.Value != 1f ? Plugin.ToggleZoomAimSensMulti.Value :
                    calledToggleZoom && Plugin.UnaimedToggleZoomMulti.Value != 1f ? Plugin.ToggleZoomUnAimSensMulti.Value : 1f;

                float scopeFOVMulti = isOptic && isAiming ? Utils.GetZoomSensValue(Plugin.FovController.CurrentScopeFOV) : Plugin.NonOpticSensMulti.Value;
                newSens = Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity * toggleZoomMulti * scopeFOVMulti;
                ____aimingSens = newSens;
            }
        }
    }

    public class ScopeSensitivityPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(SightComponent).GetMethod("get_GetCurrentSensitivity");
        }

        [PatchPrefix]
        public static bool PatchPrefix(SightComponent __instance, ref float __result)
        {
            if (Plugin.ChangeMouseSens.Value)
            {
                __result = 1f;
                return false;
            }
            return true;
        }
    }
}
