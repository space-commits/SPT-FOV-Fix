using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.UI;
using HarmonyLib;
using RealismMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static EFT.Player;

namespace FOVFix
{

    public class ValueWatcher<T>
    {
        private T _watchedValue;
        public T WatchedValue
        {
            get => _watchedValue;
            set
            {
                if (!Equals(_watchedValue, value))
                {
                    _watchedValue = value;
                    OnValueChanged?.Invoke(value);
                    Utils.Logger.LogWarning("Value changed: " + value);
                }
            }
        }

        public event Action<T> OnValueChanged;
    }

    //to-do: redo everything and put it in here
    public class FovController
    {
        private Player _player = null;
        public AnimatedTextPanel OpticPanel = null;

        ValueWatcher<bool> ADSWatcher = null;
        ValueWatcher<float> ScopeFOVWatcher = null;
        ValueWatcher<bool> ToggleZoomWatcher = null;
        ValueWatcher<bool> ScopeWatcher = null;

        public float CurrentScopeFOV = 0f;
        public bool IsToggleZoom = false;
        public bool IsPistol = false;

        public FovController()
        {
            ADSWatcher = new ValueWatcher<bool>();
            ADSWatcher.OnValueChanged += newValue => ChangeMainCamFOV();
            ScopeFOVWatcher = new ValueWatcher<float>();
            ScopeFOVWatcher.OnValueChanged += newValue => HandleScopeFOVChange();
            ToggleZoomWatcher = new ValueWatcher<bool>();
            ToggleZoomWatcher.OnValueChanged += newValue => HandleToggleZoomChange();
            ScopeWatcher = new ValueWatcher<bool>();
            ScopeWatcher.OnValueChanged += newValue => HandleToggleZoomChange();
        }


        /*     public void ZoomScope(float currentZoom)
             {
                 if (OpticPanel != null) OpticPanel.Show(Math.Round(currentZoom, 1) + "x");
                 Camera[] cams = Camera.allCameras;
                 foreach (Camera cam in cams)
                 {
                     if (cam.name == "BaseOpticCamera(Clone)")
                     {
                         cam.fieldOfView = Plugin.BaseScopeFOV.Value / Mathf.Pow(currentZoom, Plugin.MagPowerFactor.Value);
                     }
                 }

                 DoFov();
             }*/


        /*     public void DoFov()
             {
                 ProceduralWeaponAnimation pwa = _player.ProceduralWeaponAnimation;
                 if (pwa == null) return;

                 FirearmController fc = _player.HandsController as FirearmController;
                 float baseFOV = _player.ProceduralWeaponAnimation.Single_2;
                 int aimIndex = _player.ProceduralWeaponAnimation.AimIndex;

                 if (fc == null)
                 {
                     float targetFOV = baseFOV * TargetToggleZoomMulti;
                     CameraClass.Instance.SetFov(targetFOV, 1f, !IsAiming);
                     return;
                 }

                 if (_player != null && fc?.Weapon != null)
                 {
                     if (pwa.PointOfView == EPointOfView.FirstPerson && !pwa.Sprint && aimIndex < pwa.ScopeAimTransforms.Count)
                     {
                         float zoom = 1;
                         if (_player.ProceduralWeaponAnimation.CurrentAimingMod != null)
                         {
                             zoom = _player.ProceduralWeaponAnimation.CurrentAimingMod.GetCurrentOpticZoom();
                             CurrentScopeTempID = _player.ProceduralWeaponAnimation.CurrentAimingMod.Item.TemplateId;
                         }
                         bool isOptic = pwa.CurrentScope.IsOptic;
                         IsOptic = isOptic;
                         float zoomMulti = !isOptic ? Plugin.NonOpticFOVMulti.Value : Plugin.EnableVariableZoom.Value ? Utils.GetADSFoVMulti(CurrentZoom) : Utils.GetADSFoVMulti(zoom);
                         float sightFOV = baseFOV * zoomMulti * Plugin.GlobalADSMulti.Value;
                         float fov = pwa.IsAiming ? sightFOV : baseFOV;
                         float targetFOV = fov * TargetToggleZoomMulti;
                         CameraClass.Instance.SetFov(targetFOV, 1f, !IsAiming);

                     }
                 }
                 else if (pwa.PointOfView == EPointOfView.FirstPerson && !pwa.Sprint && aimIndex < pwa.ScopeAimTransforms.Count)
                 {
                     float sightFOV = baseFOV * Plugin.RangeFinderADSMulti.Value * Plugin.GlobalADSMulti.Value;
                     float fov = IsAiming ? sightFOV : baseFOV;

                     CameraClass.Instance.SetFov(fov, 1f, !IsAiming);
                 }
             }*/




        public void ChangeMainCamFOV()
        {
            if (IsPWANull()) return;
            var pwa = _player.ProceduralWeaponAnimation;
            var fc = _player.HandsController as FirearmController;

            float baseFOV = _player.ProceduralWeaponAnimation.Single_2;
            int aimIndex = _player.ProceduralWeaponAnimation.AimIndex;
            float toggleZoomMulti = 1f;
            float magnificationModifier = 1f;


            if (fc == null || !pwa.IsAiming)
            {
                toggleZoomMulti = IsToggleZoom ? Plugin.UnaimedToggleZoomMulti.Value : 1f;
            }

            bool fcIsNull = fc != null && fc?.Weapon != null;
            if (fcIsNull && pwa.PointOfView == EPointOfView.FirstPerson && !pwa.Sprint && pwa.IsAiming && aimIndex < pwa.ScopeAimTransforms.Count)
            {
                var hasOptic = pwa?.CurrentScope != null &&  pwa.CurrentScope.IsOptic;
                toggleZoomMulti = !IsToggleZoom ? 1f : hasOptic ? Plugin.OpticToggleZoomMulti.Value : Plugin.NonOpticToggleZoomMulti.Value;
                magnificationModifier = hasOptic ? Plugin.GlobalADSMulti.Value : Plugin.NonOpticFOVMulti.Value; //in future once scope FOV is standardized, will need to use current scope FOV to figure out what equiivalent scope mag it is, and use that to modify main cam FOV
            }

            float zoom = baseFOV * magnificationModifier * toggleZoomMulti;
            Utils.Logger.LogWarning($"base fov: {baseFOV}, scope: {magnificationModifier}, toggle: {toggleZoomMulti} ");
            CameraClass.Instance.SetFov(zoom, 1f, !pwa.IsAiming);
        }

        public bool IsPWANull()
        {
            ProceduralWeaponAnimation pwa = _player.ProceduralWeaponAnimation;
            return pwa == null;
        }

        public void HandleScopeFOVChange() 
        {
            ChangeMainCamFOV();
        }

        public void HandleToggleZoomChange()
        {
            ChangeMainCamFOV();
        }

        public void HandleScopeChange()
        {
            ChangeMainCamFOV();
        }

        public void CheckAiming() 
        {
            if (IsPWANull()) return;
            ADSWatcher.WatchedValue = _player.ProceduralWeaponAnimation.IsAiming;
        }

        public void CheckScopeFOV()
        {
            if (IsPWANull()) return;
            ScopeFOVWatcher.WatchedValue = CurrentScopeFOV;
        }

        public void CheckScope()
        {
            if (IsPWANull() || !Utils.WeaponReady) return;
            var pwa = _player.ProceduralWeaponAnimation;
            if (pwa.AimIndex < pwa.ScopeAimTransforms.Count) ScopeWatcher.WatchedValue = pwa.CurrentScope.IsOptic;
        }

        public void CheckToggleZoom()
        {
            ToggleZoomWatcher.WatchedValue = IsToggleZoom;
        }

        public void UpdateToggleZoom()
        {
            if (Plugin.ToggleZoomOnHoldBreath.Value) DoHoldBreahtToggleZoom();
            else DoKeybindToggleZoom();
        }

        public void DoHoldBreahtToggleZoom() 
        {
            if (_player.Physical.HoldingBreath) IsToggleZoom = true;
            else if (!_player.Physical.HoldingBreath) IsToggleZoom = false;
        }

        public void DoKeybindToggleZoom() 
        {
            if (Plugin.HoldToggleZoom.Value)
            {
                if (Input.GetKey(Plugin.ToggleZoomKeybind.Value.MainKey) && Plugin.ToggleZoomKeybind.Value.Modifiers.All(Input.GetKey)) IsToggleZoom = true;
                if (!Input.GetKey(Plugin.ToggleZoomKeybind.Value.MainKey)) IsToggleZoom = false;
            }
            else if (Input.GetKeyDown(Plugin.ToggleZoomKeybind.Value.MainKey) && Plugin.ToggleZoomKeybind.Value.Modifiers.All(Input.GetKey)) IsToggleZoom = !IsToggleZoom;
        }

        public void UpateScopeFOV()
        {
            if (CameraClass.Instance?.OpticCameraManager?.Camera != null) CurrentScopeFOV = CameraClass.Instance.OpticCameraManager.Camera.fieldOfView;
        }

        public void ControllerUpdate()
        {
            Utils.CheckIsReady();
            if (!Utils.IsReady) return;

            if (_player == null) _player = Singleton<GameWorld>.Instance.MainPlayer;
            if (_player != null)
            {
                UpateScopeFOV();
                UpdateToggleZoom();
                CheckAiming();
                CheckScopeFOV();
                CheckToggleZoom();
                CheckScope();
            }





            /*      


                    if (Plugin.ToggleZoomOnHoldBreath.Value)
                    {
                        if (_player.Physical.HoldingBreath && !CalledToggleZoomBreath)
                        {
                            CalledToggleZoomBreath = true;
                            SetToggleZoomMulti();
                            DoFov();
                        }
                        else if (!_player.Physical.HoldingBreath && CalledToggleZoomBreath)
                        {
                            CalledToggleZoomBreath = false;
                            SetToggleZoomMulti();
                            DoFov();
                        }
                    }

                    if (Plugin.HoldZoom.Value)
                    {
                        if (Input.GetKey(Plugin.ZoomKeybind.Value.MainKey) && Plugin.ZoomKeybind.Value.Modifiers.All(Input.GetKey) && !CalledToggleZoom && !(!IsAiming && Plugin.UnaimedToggleZoomMulti.Value == 1f))
                        {
                            CalledToggleZoom = true;
                            SetToggleZoomMulti();
                            DoFov();
                        }
                        if (!Input.GetKey(Plugin.ZoomKeybind.Value.MainKey) && CalledToggleZoom)
                        {
                            CalledToggleZoom = false;
                            SetToggleZoomMulti();
                            DoFov();
                        }
                    }
                    else
                    {
                        if (Input.GetKeyDown(Plugin.ZoomKeybind.Value.MainKey) && Plugin.ZoomKeybind.Value.Modifiers.All(Input.GetKey))
                        {
                            CalledToggleZoom = !CalledToggleZoom;
                            SetToggleZoomMulti();
                            DoFov();
                        }
                    }

                    if (!IsAiming && Plugin.UnaimedToggleZoomMulti.Value == 1f && CalledToggleZoom) 
                    {
                        CalledToggleZoom = false;
                        SetToggleZoomMulti();
                        DoFov();
                    }

                    if (IsAiming && !CalledADSZoom)
                    {
                        SetToggleZoomMulti();
                        DoFov();
                        CalledADSZoom = true;
                    }
                    else if (!IsAiming && CalledADSZoom)
                    {
                        SetToggleZoomMulti();
                        CalledADSZoom = false;
                    }*/


            /*    if (!Utils.IsReady && !_haveResetDict)
                {
                    WeaponScopeValues.Clear();
                    _haveResetDict = true;
                }*/
        }

    }
}
