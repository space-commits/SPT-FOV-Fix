﻿using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using System.Linq;
using EFT.InventoryLogic;
using Comfort.Common;
using static EFT.Player;
using EFT.Animations;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections;
using static CC_Vintage;
using BepInEx.Logging;
using UnityEngine.Experimental.GlobalIllumination;

namespace FOVFix
{

    public class OperationSetScopeModePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController.GClass1528).GetMethod("SetScopeMode", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool PatchPrefix(Player.FirearmController.GClass1528 __instance, GStruct144[] scopeStates)
        {
            if (__instance.CanChangeScopeStates(scopeStates))
            {
                if (Plugin.CanToggle || !Plugin.IsOptic)
                {
                    FirearmsAnimator fAnimator = (FirearmsAnimator)AccessTools.Field(typeof(Player.FirearmController.GClass1528), "firearmsAnimator_0").GetValue(__instance);
                    fAnimator.ModToggleTrigger();
                }
                GClass1596 gclass1596_0 = (GClass1596)AccessTools.Field(typeof(Player.FirearmController.GClass1528), "gclass1596_0").GetValue(__instance);
                gclass1596_0.UpdateScopesMode();
            }
            return false;
        }
    }

    public class ChangeAimingModePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController).GetMethod("ChangeAimingMode", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void PatchPrefix(Player.FirearmController __instance)
        {
            Plugin.ChangeSight = true;
        }
    }

    public class SetScopeModePatch : ModulePatch
    {
        private static bool canToggle = false;
        private static bool isFixedMag = false;
        private static bool isOptic = false;
        private static bool isFucky = false;
        private static bool canToggleButNotFixed = false;

        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController).GetMethod("SetScopeMode", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool PatchPrefix(Player.FirearmController __instance, GStruct144[] scopeStates)
        {
            if (Plugin.IsOptic) 
            {
                Plugin.ChangeSight = true;

                Player player = (Player)AccessTools.Field(typeof(EFT.Player.FirearmController), "_player").GetValue(__instance);
                ProceduralWeaponAnimation pwa = player.ProceduralWeaponAnimation;
                isOptic = pwa.CurrentScope.IsOptic;

                Mod currentAimingMod = (player.ProceduralWeaponAnimation.CurrentAimingMod != null) ? player.ProceduralWeaponAnimation.CurrentAimingMod.Item as Mod : null;
                SightComponent sightComp = player.ProceduralWeaponAnimation.CurrentAimingMod;
                SightModClass sightModClass = currentAimingMod as SightModClass;
                GInterface248 inter = (GInterface248)AccessTools.Field(typeof(EFT.InventoryLogic.SightComponent), "ginterface248_0").GetValue(sightModClass.Sight);

                canToggle = currentAimingMod.Template.ToolModdable;
                isFixedMag = currentAimingMod.Template.HasShoulderContact;
                canToggleButNotFixed = canToggle && !isFixedMag;

                float minZoom = 1f;
                float maxZoom = 1f;

                if (isFixedMag)
                {
                    minZoom = inter.Zooms[0][0];
                    maxZoom = minZoom;
                }
                else if (canToggleButNotFixed && inter.Zooms[0].Length > 2)
                {
                    minZoom = inter.Zooms[0][0];
                    maxZoom = inter.Zooms[0][2];
                }
                else
                {
                    minZoom = inter.Zooms[0][0];
                    maxZoom = inter.Zooms[0][1];
                }

                isFucky = (minZoom < 2 && sightComp.SelectedScopeIndex == 0 && sightComp.SelectedScopeMode == 0 && !isFixedMag && !canToggle);
                if (!canToggle && !Plugin.IsFucky)
                {
                    return false;
                }

                return true;
            }
            return true;
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance)
        {
            Player player = (Player)AccessTools.Field(typeof(EFT.Player.FirearmController), "_player").GetValue(__instance);
            if (isOptic)
            {
                if (!canToggle)
                {
                    return;
                }

                if (isFixedMag)
                {
                    float currentToggle = player.ProceduralWeaponAnimation.CurrentAimingMod.GetCurrentOpticZoom();
                    Plugin.CurrentZoom = currentToggle;
                    Plugin.ZoomScope(currentToggle);
                }
            }
        }
    }



    public class IsAimingPatch : ModulePatch
    {
        private static bool hasSetFov = false;
        private static float adsTimer = 0f;

        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController).GetMethod("get_IsAiming", BindingFlags.Instance | BindingFlags.Public);
        }

        private static Item getContainedItem (Slot x)
        {
            return x.ContainedItem;
        }

        private static string getSightComp(SightComponent x)
        {
            return x.Item.Name;
        }

        private static bool hasScopeAimBone(SightComponent sightComp, Player player)
        {
            List<ProceduralWeaponAnimation.GClass2075> scopeAimTransforms = player.ProceduralWeaponAnimation.ScopeAimTransforms;
            for (int i = 0; i < scopeAimTransforms.Count; i++)
            {
                if (scopeAimTransforms[i].Mod != null && scopeAimTransforms[i].Mod.Equals(sightComp))
                {
                    return true;
                }
            }
            return false;
        }

        private static GStruct144[] getScopeModeFullList(Weapon weapon, Player player)
        {
            //you can thank BSG for this monstrosity 
            IEnumerable<SightComponent> sightEnumerable = Enumerable.OrderBy<SightComponent, string>(Enumerable.Select<Slot, Item>(weapon.AllSlots, new Func<Slot, Item>(getContainedItem)).GetComponents<SightComponent>(), new Func<SightComponent, string>(getSightComp));
            List<GStruct144> sightStructList = new List<GStruct144>();
            int aimIndex = weapon.AimIndex.Value;
            int index = 0;
            foreach (SightComponent sightComponent in sightEnumerable) 
            {
                if (hasScopeAimBone(sightComponent, player)) 
                {
                    for (int i = 0; i < sightComponent.ScopesCount; i++)
                    {
                        int sightMode = (sightComponent.ScopesSelectedModes.Length != sightComponent.ScopesCount) ? 0 : sightComponent.ScopesSelectedModes[i];
                        int scopeCalibrationIndex = (sightComponent.ScopesCurrentCalibPointIndexes.Length != sightComponent.ScopesCount) ? 0 : sightComponent.ScopesCurrentCalibPointIndexes[i];
                        sightStructList.Add(new GStruct144
                        {
                            Id = sightComponent.Item.Id,
                            ScopeIndexInsideSight = i,
                            ScopeMode = ((index == aimIndex) ? (sightMode + 1) : sightMode),
                            ScopeCalibrationIndex = scopeCalibrationIndex
                        });
                        index++;
                    }
                }
            }
            return sightStructList.ToArray();
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance, bool __result)
        {
            Player player = (Player)AccessTools.Field(typeof(EFT.Player.ItemHandsController), "_player").GetValue(__instance);
            if (player.IsYourPlayer) 
            {
                Plugin.IsAiming = __result;

                if (Plugin.EnableVariableZoom.Value && Plugin.IsAiming && (!hasSetFov || Plugin.ChangeSight))
                {
                    Plugin.ChangeSight = false;
                    ProceduralWeaponAnimation pwa = player.ProceduralWeaponAnimation;
                    if (pwa.CurrentScope.IsOptic)
                    {
                        Plugin.IsOptic = true;
                        adsTimer += Time.deltaTime;

                        if (adsTimer >= 0.5f)
                        {
                            hasSetFov = true;
                            Mod currentAimingMod = (pwa.CurrentAimingMod != null) ? pwa.CurrentAimingMod.Item as Mod : null;
                            SightModClass sightModClass = currentAimingMod as SightModClass;
                            SightComponent sightComp = player.ProceduralWeaponAnimation.CurrentAimingMod;
                            GInterface248 inter = (GInterface248)AccessTools.Field(typeof(EFT.InventoryLogic.SightComponent), "ginterface248_0").GetValue(sightModClass.Sight);
                            Plugin.IsFixedMag = currentAimingMod.Template.HasShoulderContact;
                            Plugin.CanToggle = currentAimingMod.Template.ToolModdable;
                            Plugin.CanToggleButNotFixed = Plugin.CanToggle && !Plugin.IsFixedMag;
                            float minZoom = 1f;
                            float maxZoom = 1f;

                            if (Plugin.IsFixedMag)
                            {
                                minZoom = inter.Zooms[0][0];
                                maxZoom = minZoom;
                            }
                            else if (Plugin.CanToggleButNotFixed && inter.Zooms[0].Length > 2)
                            {
                                minZoom = inter.Zooms[0][0];
                                maxZoom = inter.Zooms[0][2];
                            }
                            else if (inter.Zooms[0][0] > inter.Zooms[0][1])
                            {
                                maxZoom = inter.Zooms[0][0];
                                minZoom = inter.Zooms[0][1];
                            }
                            else 
                            {
                                minZoom = inter.Zooms[0][0];
                                maxZoom = inter.Zooms[0][1];
                            }

                            Plugin.IsFucky = (minZoom < 2 && sightComp.SelectedScopeIndex == 0 && sightComp.SelectedScopeMode == 0 && !Plugin.IsFixedMag && !Plugin.CanToggle && currentAimingMod.TemplateId != "5b2388675acfc4771e1be0be");
                            if (Plugin.IsFucky)
                            {
                                __instance.SetScopeMode(getScopeModeFullList(__instance.Item, player));
                            }

                            Plugin.MinZoom = minZoom;
                            Plugin.MaxZoom = maxZoom;

                            Plugin.CurrentWeapID = __instance.Item.Id.ToString();
                            Plugin.CurrentScopeID = pwa.CurrentAimingMod.Item.Id.ToString();

                            bool weapExists = true;
                            bool scopeExists = false;
                            float rememberedZoom = minZoom;

                            if (!Plugin.WeaponScopeValues.ContainsKey(Plugin.CurrentWeapID))
                            {
                                weapExists = false;
                                Plugin.WeaponScopeValues[Plugin.CurrentWeapID] = new List<Dictionary<string, float>>();
                            }

                            List<Dictionary<string, float>> scopes = Plugin.WeaponScopeValues[Plugin.CurrentWeapID];
                            foreach (Dictionary<string, float> scopeDict in scopes)
                            {
                                if (scopeDict.ContainsKey(Plugin.CurrentScopeID))
                                {
                                    rememberedZoom = scopeDict[Plugin.CurrentScopeID];
                                    scopeExists = true;
                                    break;
                                }
                            }

                            if (!scopeExists)
                            {
                                Dictionary<string, float> newScope = new Dictionary<string, float>
                                        {
                                           { Plugin.CurrentScopeID, minZoom }
                                        };
                                Plugin.WeaponScopeValues[Plugin.CurrentWeapID].Add(newScope);
                            }

                            bool isElcan = Plugin.IsFixedMag && Plugin.CanToggle;

                            if (!isElcan && (Plugin.IsFixedMag || !weapExists || !scopeExists))
                            {
                                Plugin.CurrentZoom = minZoom;
                                Plugin.ZoomScope(minZoom);
                            }

                            if (weapExists && scopeExists)
                            {
                                Plugin.CurrentZoom = rememberedZoom;
                                Plugin.ZoomScope(rememberedZoom);
                            }

                            if (isElcan)
                            {
                                float currentToggle = player.ProceduralWeaponAnimation.CurrentAimingMod.GetCurrentOpticZoom();
                                Plugin.CurrentZoom = currentToggle;
                                Plugin.ZoomScope(currentToggle);
                            }
                        }
                    }
                    else
                    {
                        Plugin.CurrentZoom = 1f;
                    }
                }
                else if (!Plugin.IsAiming)
                {
                    adsTimer = 0f;
                    hasSetFov = false;
                }
            }
        }
    }


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
            bool mouseLookControl = (bool)AccessTools.Field(typeof(Player), "bool_2").GetValue(__instance);
            bool isResettingLook = (bool)AccessTools.Field(typeof(Player), "bool_3").GetValue(__instance);
            bool bool_4 = (bool)AccessTools.Field(typeof(Player), "bool_4").GetValue(__instance);
            bool isLooking = (bool)AccessTools.Field(typeof(Player), "bool_5").GetValue(__instance);
            bool bool_6 = (bool)AccessTools.Field(typeof(Player), "bool_6").GetValue(__instance);

            float lookZ = (float)AccessTools.Field(typeof(Player), "float_0").GetValue(__instance);
            float verticalLimit = (float)AccessTools.Field(typeof(Player), "float_1").GetValue(__instance);

            bool isAiming = __instance.HandsController != null && __instance.HandsController.IsAiming && !__instance.IsAI;
            EFTHardSettings instance = EFTHardSettings.Instance;
            Vector2 vector = new Vector2(-60f, 60f);
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
            AccessTools.Field(typeof(Player), "float_0").SetValue(__instance, Mathf.Clamp(lookZ - deltaLookY, vector.x, vector.y));
            AccessTools.Field(typeof(Player), "float_1").SetValue(__instance, Mathf.Clamp(verticalLimit + deltaLookX, mouse_LOOK_VERTICAL_LIMIT.x, mouse_LOOK_VERTICAL_LIMIT.y));
            float x2 = (verticalLimit > 0f) ? (verticalLimit * (1f - lookZ / vector.y * (lookZ / vector.y))) : verticalLimit;
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
            if (!mouseLookControl && withReturn)
            {
                if (Mathf.Abs(lookZ) > 0.01f)
                {
                    AccessTools.Field(typeof(Player), "float_0").SetValue(__instance, Mathf.Lerp(lookZ, 0f, Time.deltaTime * 15f));
                }
                else
                {
                    AccessTools.Field(typeof(Player), "float_0").SetValue(__instance, 0f);
                }
                if (Mathf.Abs(verticalLimit) > 0.01f)
                {
                    AccessTools.Field(typeof(Player), "float_1").SetValue(__instance, Mathf.Lerp(verticalLimit, 0f, Time.deltaTime * 15f));
                }
                else
                {
                    AccessTools.Field(typeof(Player), "float_1").SetValue(__instance, 0f);
                }
            }
            if (!isResettingLook && lookZ != 0f && verticalLimit != 0f)
            {
                AccessTools.Field(typeof(Player), "bool_5").SetValue(__instance, true);
            }
            else
            {
                AccessTools.Field(typeof(Player), "bool_5").SetValue(__instance, false);
            }
            if (lookZ == 0f && verticalLimit == 0f)
            {
                AccessTools.Field(typeof(Player), "bool_4").SetValue(__instance, true);
            }
            __instance.HeadRotation = new Vector3(x2, lookZ, 0f);
            __instance.ProceduralWeaponAnimation.SetHeadRotation(__instance.HeadRotation);
            return false;
        }
    }


    public class LerpCameraPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.Animations.ProceduralWeaponAnimation).GetMethod("LerpCamera", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(EFT.Animations.ProceduralWeaponAnimation __instance, float dt, float ___float_10, float ___float_9, float ___float_16, Player.ValueBlender ___valueBlender_0, Vector3 ___vector3_2, Vector3 ___vector3_7, Vector3 ____vCameraTarget, Player.ValueBlenderDelay ___valueBlenderDelay_0, Quaternion ___quaternion_3, Quaternion ___quaternion_4)
        {
            Player.FirearmController firearmController = (Player.FirearmController)AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "firearmController_0").GetValue(__instance);
            float Single_1 = Singleton<SharedGameSettingsClass>.Instance.Game.Settings.HeadBobbing;

            float camZ = firearmController != null && __instance.IsAiming == true && !Plugin.IsOptic && firearmController.Item.WeapClass == "pistol" ? ____vCameraTarget.z - Plugin.PistolOffset.Value : __instance.IsAiming == true && !Plugin.IsOptic ? ____vCameraTarget.z - Plugin.NonOpticOffset.Value : __instance.IsAiming == true && Plugin.IsOptic == true ? ____vCameraTarget.z - Plugin.OpticPosOffset.Value : ____vCameraTarget.z;

            Vector3 localPosition = __instance.HandsContainer.CameraTransform.localPosition;
            Vector2 a = new Vector2(localPosition.x, localPosition.y);
            Vector2 b = new Vector2(____vCameraTarget.x, ____vCameraTarget.y);
            float num = __instance.IsAiming ? (___float_9 * __instance.CameraSmoothBlender.Value * ___float_10) : Plugin.CameraSmoothOut.Value;
            Vector2 vector = Vector2.Lerp(a, b, dt * num);
            float num2 = localPosition.z;
            float num3 = Plugin.IsOptic ? Plugin.OpticSmoothTime.Value * dt : firearmController != null && firearmController.Item.WeapClass == "pistol" ? Plugin.PistolSmoothTime.Value * dt : Plugin.CameraSmoothTime.Value * dt;
            float num4 = __instance.IsAiming ? (1f + __instance.HandsContainer.HandsPosition.GetRelative().y * 100f + __instance.TurnAway.Position.y * 10f) : Plugin.CameraSmoothOut.Value;
            num2 = Mathf.Lerp(num2, camZ, num3 * num4);
            Vector3 localPosition2 = new Vector3(vector.x, vector.y, num2) + __instance.HandsContainer.CameraPosition.GetRelative();
            if (___float_16 > 0f)
            {
                float value = ___valueBlender_0.Value;
                if (__instance.IsAiming && value > 0f)
                {
                    __instance.HandsContainer.SwaySpring.ApplyVelocity(___vector3_2 * value);
                }
            }
            __instance.HandsContainer.CameraTransform.localPosition = localPosition2;
            Quaternion b2 = __instance.HandsContainer.CameraAnimatedFP.localRotation * __instance.HandsContainer.CameraAnimatedTP.localRotation;
            __instance.HandsContainer.CameraTransform.localRotation = Quaternion.Lerp(___quaternion_3, b2, Single_1 * (1f - ___valueBlenderDelay_0.Value)) * Quaternion.Euler(__instance.HandsContainer.CameraRotation.Get() + ___vector3_7) * ___quaternion_4;

            return false;
        }
    }

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

                            float zoomMulti = Plugin.EnableVariableZoom.Value ? Utils.getADSFoVMulti(Plugin.CurrentZoom) : Utils.getADSFoVMulti(zoom);
                            float sightFOV = baseFOV * zoomMulti * Plugin.GlobalADSMulti.Value;
                            float fov = __instance.IsAiming ? sightFOV : baseFOV;
                            bool isOptic = __instance.CurrentScope.IsOptic;
                            Plugin.IsOptic = isOptic;

                            if (Plugin.DoZoom)
                            {
                                float zoomFactor = isOptic ? Plugin.OpticExtraZoom.Value : Plugin.NonOpticExtraZoom.Value;
                                float zoomedFOV = fov * zoomFactor;
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
                       
                        float sightFOV = baseFOV * Plugin.RangeFinderADSMulti.Value * Plugin.GlobalADSMulti.Value;
                        float fov = __instance.IsAiming ? sightFOV : baseFOV;

                        CameraClass.Instance.SetFov(fov, 1f, !bool_1);
                    }
                }
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
                if (Plugin.TrueOneX.Value == true && __instance.TemplateCamera.fieldOfView >= 24)
                {
                    return false;
                }
                __instance.TemplateCamera.fieldOfView *= Plugin.GlobalOpticFOVMulti.Value;
                __instance.name = "DONE";
            }
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

            if (Plugin.HasRAPTAR == false)
            {
                CameraClass.Instance.OpticCameraManager.Camera.fieldOfView = Plugin.RangeFinderFOV.Value;
            }

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

}