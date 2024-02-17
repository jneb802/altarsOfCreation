using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Extensions;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using System;

namespace Altars_of_Creation;

public class WarpLocationManager
{
    
    private static Dictionary<Type, Dictionary<string, UnityEngine.Object>> dictionaryCache = null;
    
    public static void ClearPrefabCache(Type t)
    {
        if (dictionaryCache == null)
        {
            var cacheField = AccessTools.Field(typeof(PrefabManager.Cache), "dictionaryCache") as FieldInfo;
            if (cacheField != null)
            {
                var dict = cacheField.GetValue(null) as Dictionary<Type, Dictionary<string, UnityEngine.Object>>;
                if (dict != null)
                {
                    dictionaryCache = dict;
                }
                else
                {
                    Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Couldn't get value of PrefabManager.Cache.dictionaryCache");
                }
            }
            else
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Couldn't find memberinfo for PrefabManager.Cache.dictionaryCache");
            }
        }

        if (dictionaryCache != null)
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Clearing cache for type: " + t);
            dictionaryCache.Remove(t);
        }
    }
    
    public static void AddLocation_MWL_RuinsCathedral1()
    {
        GameObject MWL_RuinsCathedral1_Container = ZoneManager.Instance.CreateLocationContainer(WarpAssetManager.MWL_RuinsCathedral1_GameObject);
        
        var exteriorCreatureList = WarpCreatureManager.CreateCreatureList(@"C:\Users\jneb8\RiderProjects\Altars of Creation\CreatureLists\LocationsCreatureList.yml","MWL_RuinsCathedral1",7);
        var interiorCreatureList = WarpCreatureManager.CreateCreatureList(@"C:\Users\jneb8\RiderProjects\Altars of Creation\CreatureLists\LocationsCreatureList.yml","MWL_RuinsCathedral1",20);
        
        var exteriorCreatureSpawnerList = WarpCreatureManager.GetExteriorCreatureSpawners(MWL_RuinsCathedral1_Container);
        var interiorCreatureSpawnerList = WarpCreatureManager.GetInteriorCreatureSpawners(MWL_RuinsCathedral1_Container);

        WarpCreatureManager.AddCreaturestoSpawnerList(exteriorCreatureSpawnerList,exteriorCreatureList);
        WarpCreatureManager.AddCreaturestoSpawnerList(interiorCreatureSpawnerList,interiorCreatureList);
        
        DropTable Cathedral1DropTable = WarpLootManager.CreateDropTable(WarpLootManager.meadowsLoot2, 2, 3);
        WarpLootManager.AddContainerToChild(MWL_RuinsCathedral1_Container.gameObject.transform.FindDeepChild("Unity").gameObject, "loot_chest_wood1", Cathedral1DropTable);
        WarpLootManager.AddContainerToChild(MWL_RuinsCathedral1_Container.gameObject.transform.FindDeepChild("Unity").gameObject, "loot_chest_wood2", Cathedral1DropTable);
        WarpLootManager.AddContainerToChild(MWL_RuinsCathedral1_Container.gameObject.transform.FindDeepChild("Unity").gameObject, "loot_chest_wood3", Cathedral1DropTable);
        
        WarpLootManager.AddOfferingManagerToChildContainer(MWL_RuinsCathedral1_Container.gameObject.transform.FindDeepChild("Unity").gameObject, "offeringBox");
        
        CustomLocation MWL_RuinsCathedral1_Location = 
            new CustomLocation(MWL_RuinsCathedral1_Container, fixReference: true,
                new LocationConfig
                    {
                        Biome = Heightmap.Biome.Meadows,
                        Quantity = Altars_of_CreationPlugin.MWL_RuinsCathedral1_Quantity_Config.Value,
                        Priotized = true,
                        ExteriorRadius = 32,
                        ClearArea = true,
                        RandomRotation = false,
                        Group = "Ruins_large",
                        MinDistanceFromSimilar = 1028,
                        MaxTerrainDelta = 2f,
                        MinAltitude = 5,
                        MinDistance = 1500,
                        MaxDistance = 5000,
                        InteriorRadius = 64,
                        InForest = false,
                        ForestTresholdMin = 1,
                        HasInterior = true,
                        InteriorEnvironment = "Crypt",
                    });

        MWL_RuinsCathedral1_Location.Location.m_discoverLabel = "Kristnir Cathedral";
        MWL_RuinsCathedral1_Location.Location.m_applyRandomDamage = false;
        MWL_RuinsCathedral1_Location.Location.m_noBuild = false;
        
        ClearPrefabCache(typeof(Material));
        ClearPrefabCache(typeof(Shader));
        
        ZoneManager.Instance.AddCustomLocation(MWL_RuinsCathedral1_Location);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddLocation_MWL_RuinsCathedral1;
    }
}