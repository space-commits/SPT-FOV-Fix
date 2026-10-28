using RealismCommonLib.Integration;
using static RealismCommonLib.Plugin;

namespace FOVFix
{
    // All cross-mod reads go through here. Stance values are the "not installed" defaults when Stance-Overhaul is absent.
    internal static class CommonLibCompat
    {
        public static bool HasShoulderContact => WeaponStateInstance != null && WeaponStateInstance.HasShoulderContact;
        public static bool IsMachinePistol => WeaponStateInstance != null && WeaponStateInstance.IsMachinePistol;
        public static bool IsLeftShoulder => ModuleServices.Stance.IsLeftShoulder;

        // no equivalent in common lib or Stance-Overhaul yet (collision isn't ported)
        // public static bool StopCameraMovmentForCollision => false;
        // public static float CameraMovmentForCollisionSpeed => 1f;
    }
}
