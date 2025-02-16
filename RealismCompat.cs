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
        public bool RealismAltRifle { get; private set; } = false;
        public bool RealismAltPistol { get; private set; } = false;
        public float StanceBlenderValue { get; private set; } = 0f;
        public float StanceBlenderTarget { get; private set; } = 0f;
        public bool StancesAreEnabled { get; private set; } = false;
        public bool DoPatrolStanceAdsSmoothing { get; private set; } = false;
        public bool StopCameraMovmentForCollision { get; private set; } = false;
        public float CameraMovmentForCollisionSpeed { get; private set; } = 1f;
        public bool IsColliding { get; private set; } = false;

        public void Update() 
        {
            HasShoulderContact = RealismMod.WeaponStats.HasShoulderContact;
            IsMachinePistol = RealismMod.WeaponStats.IsMachinePistol;
            RealismAltRifle = RealismMod.PluginConfig.EnableAltRifle.Value;
            RealismAltPistol = RealismMod.PluginConfig.EnableAltPistol.Value;
            StanceBlenderTarget = RealismMod.StanceController.StanceBlender.Target;
            StanceBlenderValue = RealismMod.StanceController.StanceBlender.Value;
            StancesAreEnabled = RealismMod.Plugin.ServerConfig.enable_stances;
            DoPatrolStanceAdsSmoothing = !RealismMod.StanceController.FinishedUnPatrolStancing;
            DoPatrolStanceAdsSmoothing = !RealismMod.StanceController.FinishedUnPatrolStancing;
            StopCameraMovmentForCollision = RealismMod.StanceController.StopCameraMovement;
            IsColliding = RealismMod.StanceController.IsColliding;
            CameraMovmentForCollisionSpeed = RealismMod.StanceController.CameraMovmentForCollisionSpeed;
        }

    }
}
