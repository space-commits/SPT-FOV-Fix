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

        public void Update() 
        {
            HasShoulderContact = RealismMod.WeaponStats.HasShoulderContact;
            IsMachinePistol = RealismMod.WeaponStats.IsMachinePistol;
            RealismAltRifle = RealismMod.PluginConfig.EnableAltRifle.Value;
            RealismAltPistol = RealismMod.PluginConfig.EnableAltPistol.Value;
            StanceBlenderTarget = RealismMod.StanceController.StanceBlender.Target;
            StanceBlenderValue = RealismMod.StanceController.StanceBlender.Value;
        }

    }
}
