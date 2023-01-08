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
                case <=12:
                    return Plugin.twelveADSMulti.Value;
                case > 12:
                    return Plugin.highADSMulti.Value;
                default:
                    return 1;
            }
        }

 /*       public static float getCameraPosMulti(float zoom)
        {
            switch (zoom)
            {
                case <= 1:
                    return Plugin.oneCameraPOSMulti.Value;
                case <= 2:
                    return Plugin.twoCameraPOSMulti.Value;
                case <= 3:
                    return Plugin.threeCameraPOSMulti.Value;
                case <= 4:
                    return Plugin.fourCameraPOSMulti.Value;
                case <= 6:
                    return Plugin.sixCameraPOSMulti.Value;
                case <= 12:
                    return Plugin.twelveCameraPOSMulti.Value;
                case > 12:
                    return Plugin.highCameraPOSMulti.Value;
                default:
                    return 1;
            }
        }*/
/*
        public static float getOpticFOVMulti(float zoom)
        {
            switch (zoom)
            {
                case <= 1:
                    return Plugin.oneOpticFOVMulti.Value;
                case <= 2:
                    return Plugin.twoOpticFOVMulti.Value;
                case <= 3:
                    return Plugin.threeOpticFOVMulti.Value;
                case <= 4:
                    return Plugin.fourOpticFOVMulti.Value;
                case <= 6:
                    return Plugin.sixOpticFOVMulti.Value;
                case <= 12:
                    return Plugin.twelveOpticFOVMulti.Value;
                case > 12:
                    return Plugin.highOpticFOVMulti.Value;
                default:
                    return 1;
            }
        }*/





    }
}
