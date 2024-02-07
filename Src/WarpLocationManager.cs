using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Extensions;
using Jotunn.Managers;
using PieceManager;

namespace Altars_of_Creation;

public class WarpLocationManager
{
    public static void AddLocation_MWL_RuinsCathedral1()
    {
        var exteriorCreatureList = WarpCreatureManager.CreateCreatureList(@"C:\Users\jneb8\RiderProjects\Altars of Creation\CreatureLists\LocationsCreatureList.yml","MWL_RuinsCathedral1",7);
        if (exteriorCreatureList != null)
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("exteriorCreatureList is not null");
            foreach (var item in exteriorCreatureList)
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("ExteriorList: Creature name list in includes" + item);
            }
        }
        else
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("exteriorCreatureList is null");
        }
        
        
        var interiorCreatureList = WarpCreatureManager.CreateCreatureList(@"C:\Users\jneb8\RiderProjects\Altars of Creation\CreatureLists\LocationsCreatureList.yml","MWL_RuinsCathedral1",20);
        if (interiorCreatureList != null)
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("interiorCreatureList is not null");
            foreach (var item in interiorCreatureList)
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("interiorCreatureList: Creature name list in includes" + item);
            }
        }
        else
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("interiorCreatureList is null");
        }

        var exteriorCreatureSpawnerList = WarpCreatureManager.GetExteriorCreatureSpawners(WarpAssetManager.MWL_RuinsCathedral1_GameObject);
        if (exteriorCreatureSpawnerList != null)
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("exteriorCreatureSpawnerList is not null");
            foreach (var item in exteriorCreatureSpawnerList)
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("exteriorCreatureSpawnerList: Creature name list in includes" + item);
            }
        }
        else
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("exteriorCreatureSpawnerList is null");
        }
        
        var interiorCreatureSpawnerList = WarpCreatureManager.GetInteriorCreatureSpawners(WarpAssetManager.MWL_RuinsCathedral1_GameObject);
        if (interiorCreatureSpawnerList != null)
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("interiorCreatureSpawnerList is not null");
            foreach (var item in interiorCreatureSpawnerList)
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("interiorCreatureSpawnerList: Creature name list in includes" + item);
            }
        }
        else
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("interiorCreatureSpawnerList is null");
        }

        WarpCreatureManager.AddCreaturestoSpawnerList(exteriorCreatureSpawnerList,exteriorCreatureList);
        WarpCreatureManager.AddCreaturestoSpawnerList(interiorCreatureSpawnerList,interiorCreatureList);
        
        DropTable Cathedral1DropTable = WarpLootManager.CreateDropTable(WarpLootManager.meadowsLoot2, 2, 3);
        WarpLootManager.AddContainerToChild(WarpAssetManager.MWL_RuinsCathedral1_GameObject.gameObject.transform.FindDeepChild("Unity").gameObject, "loot_chest_wood1", Cathedral1DropTable);
        WarpLootManager.AddContainerToChild(WarpAssetManager.MWL_RuinsCathedral1_GameObject.gameObject.transform.FindDeepChild("Unity").gameObject, "loot_chest_wood2", Cathedral1DropTable);
        DropTable Cathedral1InteriorDropTable = WarpLootManager.CreateDropTable(WarpLootManager.meadowsLoot2, 2, 3);
        WarpLootManager.AddContainerToChild(WarpAssetManager.MWL_RuinsCathedral1_GameObject.gameObject.transform.FindDeepChild("Unity").gameObject, "loot_chest_wood_interior1", Cathedral1InteriorDropTable);

        WarpLootManager.AddOfferingManagerToChildContainer(WarpAssetManager.MWL_RuinsCathedral1_GameObject.gameObject.transform.FindDeepChild("Unity").gameObject, "offeringBox");

        // All the items have correct materials and destruction effects (except chests) when I use only the ShaderSwap. So it seems that ShaderSwap will also swap materials.
        MaterialReplacer.RegisterGameObjectForShaderSwap(WarpAssetManager.MWL_RuinsCathedral1_GameObject.gameObject.transform.FindDeepChild("Blueprint").gameObject, MaterialReplacer.ShaderType.PieceShader);
        MaterialReplacer.RegisterGameObjectForShaderSwap(WarpAssetManager.MWL_RuinsCathedral1_GameObject.gameObject.transform.FindDeepChild("Vegetation").gameObject, MaterialReplacer.ShaderType.VegetationShader);
        MaterialReplacer.RegisterGameObjectForShaderSwap(WarpAssetManager.MWL_RuinsCathedral1_GameObject.gameObject.transform.FindDeepChild("Unity").gameObject, MaterialReplacer.ShaderType.Standard);
        MaterialReplacer.RegisterGameObjectForMatSwap(WarpAssetManager.MWL_RuinsCathedral1_GameObject.gameObject.transform.FindDeepChild("Blueprint").gameObject);
        MaterialReplacer.RegisterGameObjectForMatSwap(WarpAssetManager.MWL_RuinsCathedral1_GameObject.gameObject.transform.FindDeepChild("Vegetation").gameObject);

        CustomLocation MWL_RuinsCathedral1_Location = new CustomLocation(WarpAssetManager.MWL_RuinsCathedral1_GameObject, fixReference: false,
            new LocationConfig
                {
                    Biome = Heightmap.Biome.Meadows,
                    Quantity = 20,
                    Priotized = true,
                    ExteriorRadius = 50,
                    ClearArea = true,
                    RandomRotation = false,
                    Group = "Ruins_large",
                    MinDistanceFromSimilar = 512,
                    MaxTerrainDelta = 2f,
                    MinAltitude = 5,
                    MinDistance = 0,
                    MaxDistance = 5000,
                    InteriorRadius = 50,
                    HasInterior = true,
                    InteriorEnvironment = "Crypt",
                });

        MWL_RuinsCathedral1_Location.Location.m_discoverLabel = "Kristnir Cathedral";
        MWL_RuinsCathedral1_Location.Location.m_applyRandomDamage = false;
        MWL_RuinsCathedral1_Location.Location.m_noBuild = false;

        ZoneManager.Instance.AddCustomLocation(MWL_RuinsCathedral1_Location);
    }
}