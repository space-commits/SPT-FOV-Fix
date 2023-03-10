using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using UnityEngine;

namespace FOVFix
{
  public static class Helper
  {

        public static string CompactCollimator = "55818acf4bdc2dde698b456b";
        public static string Collimator = "55818ad54bdc2ddc698b4569";
        public static string AssaultScope = "55818add4bdc2d5b648b456f";
        public static string Scope = "55818ae44bdc2dde698b456c";
        public static string IronSight = "55818ac54bdc2d5b648b456e";
        public static string SpecialScope = "55818aeb4bdc2ddc698b456a";

        public static string[] scopeTypes = new string[] { "55818acf4bdc2dde698b456b", "55818ad54bdc2ddc698b4569", "55818add4bdc2d5b648b456f", "55818ae44bdc2dde698b456c", "55818ac54bdc2d5b648b456e", "55818aeb4bdc2ddc698b456a" };

        public static void CheckIsReady()
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            SessionResultPanel sessionResultPanel = Singleton<SessionResultPanel>.Instance;

            if (gameWorld?.AllPlayers.Count > 0)
            {
                Player player = gameWorld.AllPlayers[0];
                if (player != null && player?.HandsController != null)
                {
                    Plugin.player = player; 
                    if (player?.HandsController?.Item != null && player?.HandsController?.Item is Weapon)
                    {
                        Plugin.WeaponReady = true;
                    }
                }
            }

            if (gameWorld == null || gameWorld.AllPlayers == null || gameWorld.AllPlayers.Count <= 0 || sessionResultPanel != null)
            {
                Plugin.IsReady = false;
            }
            Plugin.IsReady = true;
        }

        public static bool IsSight(Mod mod)
        {
            bool isScope = false;

            foreach (string id in scopeTypes) 
            {
                isScope = mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[id] ? true : false;
            }

            return isScope;
        }

        public static float getADSFoVMulti(float zoom) 
        {
            switch (zoom)
            {
                case <= 1:
                    return Plugin.oneADSMulti.Value;
                case <= 2:
                    return Plugin.twoADSMulti.Value;
                case <= 3:
                    return Plugin.threeADSMulti.Value;
                case <= 4:
                    return Plugin.fourADSMulti.Value;
                case <= 6:
                    return Plugin.sixADSMulti.Value;
                case <= 8:
                    return Plugin.eightADSMulti.Value;
                case <=12:
                    return Plugin.twelveADSMulti.Value;
                case <= 14:
                    return Plugin.fourteenADSMulti.Value;
                case > 14:
                    return Plugin.highADSMulti.Value;
                default:
                    return 1;
            }
        }
    }
}
