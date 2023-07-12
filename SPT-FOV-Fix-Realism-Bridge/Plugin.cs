/*
using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;

namespace Bridge
{

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("FOVFix", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("RealismMod", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static bool CheckIsReady()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var sessionResultPanel = Singleton<SessionResultPanel>.Instance;

            if (gameWorld == null || gameWorld.AllPlayers == null || gameWorld.AllPlayers.Count <= 0 || sessionResultPanel != null)
            {
                return false;
            }
            return true;
        }

        void Update()
        {
            if (CheckIsReady()) 
            {
                RealismMod.Plugin.StartingAimSens = FOVFix.Plugin.AimingSens;
            }

        }
    }
}

*/