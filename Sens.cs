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
                float baseSens = Singleton<SharedGameSettingsClass>.Instance.Control.Settings.MouseAimingSensitivity;
                float newSens = Mathf.Max(baseSens * (1f - ((Plugin.CurrentZoom - 1f) / Plugin.MouseSensFactor.Value)), Plugin.MouseSensLowerLimit.Value);
                Plugin.AimingSens = newSens;
                if (!Plugin.isRealismModPresent) 
                {
                    ____aimingSens = newSens;
                }
            }
        }
    }
}
