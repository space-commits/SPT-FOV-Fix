using System;
using System.Collections.Generic;
using System.Text;
using RealismMod;

namespace FOVFix
{
    public class RealismCompat
    {
        public bool HasShoulderContact = false;
        public bool IsMachinePistol = false;
        public bool RealismAltRifle = false;
        public bool RealismAltPistol = false;

        public void Update() 
        {
            HasShoulderContact = RealismMod.WeaponStats.HasShoulderContact;
            IsMachinePistol = RealismMod.WeaponStats.IsMachinePistol;
            RealismAltRifle = RealismMod.PluginConfig.EnableAltRifle.Value;
            RealismAltPistol = RealismMod.PluginConfig.EnableAltPistol.Value;
        }

    }
}
