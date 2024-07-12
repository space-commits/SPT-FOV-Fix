using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static EFT.Player;

namespace FOVFix
{
    //to-do: redo everything and put it in here
    public class FovController
    {
        public Dictionary<string, List<Dictionary<string, float>>> WeaponScopeValues = new Dictionary<string, List<Dictionary<string, float>>>();

        public Vector2 CamPanRotation = Vector2.zero;
        public bool IsRotating = false;
        private Vector2 _targetRotation = Vector2.zero;

        public bool DidToggleForFirstPlane = false;

        private bool _haveResetDict = false;

        public bool IsFixedMag = false;
        public bool CanToggle = false;
        public bool CanToggleButNotFixed = false;
        public bool IsFucky = false;
        public float MinZoom = 1f;
        public float MaxZoom = 1f;
        public float CurrentZoom = 1f;

        public string CurrentWeapInstanceID = "";
        public string CurrentScopeInstanceID = "";
        public string CurrentScopeTempID = "";

        public bool IsOptic;
        public bool HasRAPTAR = false;
        public bool CalledToggleZoom = false;
        public bool CalledToggleZoomBreath = false;
        public bool IsToggleZoom = false;
        public bool IsAiming = false;
        public bool ChangeSight = false;
        public bool ToggledMagnification = false;
        public bool CalledADSZoom = false;

        public float TargetToggleZoomMulti = 1f;
        public bool ToggleZoomActive = false;

        private Player _player = null;
        private OpticCratePanel _opticPanel = null;

        public FovController()
        {
        }

        private void UpdateStoredMagnificaiton(string weapID, string scopeID, float currentZoom)
        {
            if (WeaponScopeValues.ContainsKey(CurrentWeapInstanceID))
            {
                List<Dictionary<string, float>> scopes = WeaponScopeValues[CurrentWeapInstanceID];
                foreach (Dictionary<string, float> scopeDict in scopes)
                {
                    if (scopeDict.ContainsKey(CurrentScopeInstanceID))
                    {
                        scopeDict[CurrentScopeInstanceID] = currentZoom;
                        break;
                    }
                }
            }
        }

        public void HandleZoomInput(float zoomIncrement, bool toggleZoom = false)
        {
            float zoomBefore = CurrentZoom;
            CurrentZoom =
                !toggleZoom ? Mathf.Clamp(CurrentZoom + zoomIncrement, MinZoom, MaxZoom) :
                zoomIncrement;
            UpdateStoredMagnificaiton(CurrentWeapInstanceID, CurrentScopeInstanceID, CurrentZoom);
            ZoomScope(CurrentZoom);
            if (zoomBefore != CurrentZoom)
            {
                DidToggleForFirstPlane = true;
            }
        }

        public void ZoomScope(float currentZoom)
        {
            if (_opticPanel != null) _opticPanel.Show(Math.Round(currentZoom, 1) + "x");
            Camera[] cams = Camera.allCameras;
            foreach (Camera cam in cams)
            {
                if (cam.name == "BaseOpticCamera(Clone)")
                {
                    cam.fieldOfView = Plugin.BaseScopeFOV.Value / Mathf.Pow(currentZoom, Plugin.MagPowerFactor.Value);
                }
            }

            DoFov();
        }

        public void SetToggleZoomMulti() 
        {
            bool isOptic = _player.HandsController as FirearmController != null && _player?.ProceduralWeaponAnimation != null && _player?.ProceduralWeaponAnimation?.CurrentScope != null && _player.ProceduralWeaponAnimation.CurrentScope.IsOptic;
            bool cancelledZoom = (!CalledToggleZoom && !Plugin.ToggleZoomOnHoldBreath.Value) || (!CalledToggleZoomBreath && Plugin.ToggleZoomOnHoldBreath.Value);
            TargetToggleZoomMulti = cancelledZoom || _player.IsInventoryOpened ? 1f : isOptic && IsAiming ? Plugin.OpticToggleZoomMulti.Value : IsAiming ? Plugin.NonOpticToggleZoomMulti.Value : Plugin.UnaimedToggleZoomMulti.Value;
        }

        public void DoFov()
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
        }

        public void ControllerUpdate()
        {
            Utils.CheckIsReady();
            if (Utils.IsReady)
            {
                if (_opticPanel == null)
                {
                    GameObject opticObj = GameObject.Find("OpticCratePanel");
                    if (opticObj != null)
                    {
                        OpticCratePanel opticCrate;
                        if (opticObj.TryGetComponent<OpticCratePanel>(out opticCrate)) _opticPanel = opticCrate;
                    }
      
                }
                if(_player == null) _player = Singleton<GameWorld>.Instance.MainPlayer;
            }

            if (Utils.IsReady && _player != null)
            {
                _haveResetDict = false;

                if (Plugin.EnableVariableZoom.Value && !IsFixedMag && IsOptic && (!CanToggle || CanToggleButNotFixed) && IsAiming)
                {
                    if (Plugin.UseSmoothZoom.Value)
                    {
                        if (Input.GetKey(Plugin.VariableZoomOut.Value.MainKey) && Plugin.VariableZoomOut.Value.Modifiers.All(Input.GetKey))
                        {
                            HandleZoomInput(-Plugin.SmoothZoomSpeed.Value);
                        }
                        if (Input.GetKey(Plugin.VariableZoomIn.Value.MainKey) && Plugin.VariableZoomIn.Value.Modifiers.All(Input.GetKey))
                        {
                            HandleZoomInput(Plugin.SmoothZoomSpeed.Value);
                        }
                    }
                    else
                    {
                        if (Input.GetKeyDown(Plugin.VariableZoomOut.Value.MainKey) && Plugin.VariableZoomOut.Value.Modifiers.All(Input.GetKey))
                        {
                            HandleZoomInput(-Plugin.ZoomSteps.Value);
                        }
                        if (Input.GetKeyDown(Plugin.VariableZoomIn.Value.MainKey) && Plugin.VariableZoomIn.Value.Modifiers.All(Input.GetKey))
                        {
                            HandleZoomInput(Plugin.ZoomSteps.Value);
                        }
                    }
                    if (Plugin.UseMouseWheel.Value)
                    {
                        if ((Input.GetKey(Plugin.MouseWheelBind.Value.MainKey) && Plugin.UseMouseWheelPlusKey.Value) || (!Plugin.UseMouseWheelPlusKey.Value && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.R) && !Input.GetKey(KeyCode.C)))
                        {
                            float scrollDelta = Input.mouseScrollDelta.y * Plugin.ZoomSteps.Value;
                            if (scrollDelta != 0f)
                            {
                                HandleZoomInput(scrollDelta);
                            }
                        }
                    }
                }

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
                }
            }

            if (!Utils.IsReady && !_haveResetDict)
            {
                WeaponScopeValues.Clear();
                _haveResetDict = true;
            }
        }

    }
}
