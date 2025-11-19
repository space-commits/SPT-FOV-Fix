using System;
using System.Collections.Generic;
using System.Text;
using RealismMod;

namespace FOVFix
{
    public class RealismCompat
    {
        public bool HasShoulderContact { get; private set; } = false;
        public bool IsMachinePistol { get; private set; } = false;
        public bool DoAltPistol { get; private set; } = false;
        public float StanceBlenderValue { get; private set; } = 0f;
        public float StanceBlenderTarget { get; private set; } = 0f;
        public bool StancesAreEnabled { get; private set; } = false;
        public bool DoPatrolStanceAdsSmoothing { get; private set; } = false;
        public bool StopCameraMovmentForCollision { get; private set; } = false;
        public float CameraMovmentForCollisionSpeed { get; private set; } = 1f;
        public bool IsColliding { get; private set; } = false;
        public bool IsLeftShoulder { get; private set; } = false;
        public bool IsResettingShoulder { get; private set; } = false;
        public bool IsFiringMovement { get; private set; } = false;
        public bool DoAltRifle { get; private set; } = false;

        public void Update() 
        {
            HasShoulderContact = RealismMod.WeaponStats.HasShoulderContact;
            IsMachinePistol = RealismMod.WeaponStats.IsMachinePistol;
            DoAltPistol = RealismMod.PluginConfig.EnableAltPistol.Value;
            StanceBlenderTarget = RealismMod.StanceController.StanceBlender.Target;
            StanceBlenderValue = RealismMod.StanceController.StanceBlender.Value;
            StancesAreEnabled = RealismMod.Plugin.ServerConfig.enable_stances;
            DoPatrolStanceAdsSmoothing = !RealismMod.StanceController.FinishedUnPatrolStancing;
            StopCameraMovmentForCollision = RealismMod.StanceController.StopCameraMovement;
            IsColliding = RealismMod.StanceController.IsColliding;
            CameraMovmentForCollisionSpeed = RealismMod.StanceController.CameraMovmentForCollisionSpeed;
            IsLeftShoulder = RealismMod.StanceController.IsLeftShoulder;
            IsResettingShoulder = RealismMod.StanceController.IsLeftStanceResetState;
            DoAltRifle = RealismMod.PluginConfig.EnableAltRifle.Value;
        }
    }
}
