using System.Reflection;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Extensions;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace Altars_of_Creation;

public class WarpLocationManager
{
    
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
        
        CustomLocation MWL_RuinsCathedral1_Location = new CustomLocation(MWL_RuinsCathedral1_Container, fixReference: true,
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
        
        WarpAssetManager.ClearPrefabCache(typeof(Material));
        WarpAssetManager.ClearPrefabCache(typeof(Shader));
        
        ZoneManager.Instance.AddCustomLocation(MWL_RuinsCathedral1_Location);
        
        ZoneManager.OnVanillaLocationsAvailable -= AddLocation_MWL_RuinsCathedral1;
    }
}