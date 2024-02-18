using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Altars_of_Creation;

public class WarpLootManager: MonoBehaviour
{
    public static List<string> exteriorLoot;
    public static List<string> interiorLoot1;
    public static List<string> interiorLoot2;
    public static List<string> interiorLoot3;
    
    public static List<String> LoadLootConfig(string locationName, string lootListName)
    {
        var filePath = @"C:\\Users\\jneb8\\RiderProjects\\Altars of Creation\\CreatureLists\\warpalicious.MWL_LocationLootLists.yml";
        var yamlContent = File.ReadAllText(filePath);
        
        var yaml = new YamlStream();
        yaml.Load(new StringReader(yamlContent));
        var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        
        List<string> lootList = new List<string>();

        if (mapping.Children.ContainsKey(new YamlScalarNode(locationName)))
        {
            var locationNode = mapping.Children[new YamlScalarNode(locationName)] as YamlMappingNode;
            if (locationNode != null && locationNode.Children.ContainsKey(new YamlScalarNode(lootListName)))
            {
                var lootItems = (YamlSequenceNode)locationNode.Children[new YamlScalarNode(lootListName)];
                foreach (var item in lootItems)
                {
                    lootList.Add(((YamlScalarNode)item).Value);
                }
            }
        }
        else
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Failed to find location with name: " + locationName + " in LocationInteriorLootList");
        }

        return lootList;
    }

    public static void RunLootConfigs(string locationName)
    {
        exteriorLoot = LoadLootConfig(locationName, "exteriorLoot");
        interiorLoot1 = LoadLootConfig(locationName, "interiorLootTier1");
        interiorLoot2 = LoadLootConfig(locationName, "interiorLootTier2");
        interiorLoot3 = LoadLootConfig(locationName, "interiorLootTier3");
    }

    public static DropTable CreateDropTable(List<string> itemNames, int dropMin, int dropMax)
        {
            DropTable newDropTable = new DropTable
            {
                m_dropMin = dropMin,
                m_dropMax = dropMax,
                m_dropChance = 1.0f
            };

            foreach (var itemName in itemNames)
            {
                GameObject itemPrefab = PrefabManager.Cache.GetPrefab<GameObject>(itemName);

                if (itemPrefab != null)
                {
                    DropTable.DropData dropData = new DropTable.DropData
                    {
                        m_item = itemPrefab,
                        m_stackMin = 1,
                        m_stackMax = 3,
                        m_weight = 1.0f,
                        m_dontScale = false
                    };

                    newDropTable.m_drops.Add(dropData);
                }
                else
                {
                    Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Prefab for " + itemName + " not found");
                }
            }

            return newDropTable;
        }

        public static void AddContainerToChild(GameObject parentGameObject, string childName, DropTable dropTable)
        {
            // Find the child GameObject by name
            Transform childTransform = parentGameObject.transform.Find(childName);

            // Check if the child was found
            if (childTransform != null)
            {
                // Add the Container component to the child GameObject
                Container container = childTransform.gameObject.AddComponent<Container>();
                if (container != null)
                {
                    // Configure the Container properties
                    container.m_defaultItems = dropTable;
                    container.m_name = "Chest";
                    container.m_width = 4;
                    container.m_height = 2;
                }
            }
            else
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Child GameObject (" + childName + ") not found in parent GameObject (" + parentGameObject + ")");
            }
        }
        
        public static void AddOfferingManagerToChildContainer(GameObject parentGameObject, string childName)
        {
            // Find the child GameObject by name
            Transform childTransform = parentGameObject.transform.Find(childName);

            // Check if the child was found
            if (childTransform != null)
            {
                childTransform.gameObject.AddComponent<WarpAltarManager>();
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Successfully added the Container component to " + childName);
            }
            else
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Child GameObject (" + childName +
                                                                           ") not found in parent GameObject (" + parentGameObject + ")");
            }
        }
        
        public static List<Container> GetSceneInteriorContainers()
        {
            List<Container> locationInteriorContainers = new List<Container>();
        
            var allSceneObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        
            foreach (var rootObject in allSceneObjects)
            {
                if (rootObject.name.StartsWith("loot_chest_wood_interior") && rootObject.transform.position.y >= 5000)
                {
                    Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Found object with name matching criteria. Name: " + rootObject.name);
                    Container container = rootObject.GetComponent<Container>();
                    if (container != null)
                    {
                        locationInteriorContainers.Add(container);
                        Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Found container with name: " + container);
                    }
                    else
                    {
                        Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Failed the container from object with name: " + rootObject);
                    }
                }
            }

            return locationInteriorContainers;
        }

        public static void UpdateInteriorContainerTier(List<Container> containers, int tier)
        {
            var itemNames = interiorLoot1;
            switch (tier)
            {
                case 1:
                    itemNames = interiorLoot1;
                    break;
                case 2:
                    itemNames = interiorLoot2;
                    break;
                case 3:
                    itemNames = interiorLoot3;
                    break;
                default:
                    itemNames = interiorLoot1;
                    break;
            }

            foreach (var container in containers)
            {
                var dropTable = container.m_defaultItems;
                
                dropTable.m_oneOfEach = true;
                dropTable.m_dropMin = 1;
                dropTable.m_dropMax = 1;
                
                foreach (var itemName in itemNames)
                {

                    GameObject itemPrefab = PrefabManager.Cache.GetPrefab<GameObject>(itemName);

                    if (itemPrefab != null)
                    {
                        DropTable.DropData dropData = new DropTable.DropData
                        {
                            m_item = itemPrefab,
                            m_stackMin = 3,
                            m_stackMax = 8,
                            m_weight = 1.0f,
                            m_dontScale = false 
                        };
                        
                        dropTable.m_drops.Add(dropData);
                    }
                    else
                    {
                        Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Prefab for " + itemName +
                            " not found");
                    }

                    container.AddDefaultItems();
                }

            }
        }
        
        public static void ShrinkChests(List<Container> containers, int tier)
        {
            int chestsToShrink = 2;
            
            switch (tier)
            {
                case 1:
                    chestsToShrink = 2;
                    break;
                case 2:
                    chestsToShrink = 1;
                    break;
                case 3:
                    chestsToShrink = 0;
                    break;
                default:
                    chestsToShrink = 2;
                    break;
            }
            
            foreach (var container in containers)
            {
                if (chestsToShrink <= 0) break; 
                if (container.transform != null)
                {
                    var parentTransform = container.transform;
                    var scale = parentTransform.localScale;
                    scale.x = 0.01f;
                    scale.y = 0.01f;
                    scale.z = 0.01f;
                    parentTransform.localScale = scale;
                    chestsToShrink--;
                }
                else
                {
                    Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Parent transform for container not found");
                }
            }
            
        }
        
}
