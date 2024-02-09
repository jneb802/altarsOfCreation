using System.Collections.Generic;
using System.Linq;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Altars_of_Creation;

public class WarpLootManager: MonoBehaviour
{
    
    public static List<string> meadowsLoot2 = new List<string> { "FineWood", "Bronze", "CopperOre", "Coins" };
    public static List<string> interiorLoot1 = new List<string> { "Copper", "Tin", "Ruby"};
    public static List<string> interiorLoot2 = new List<string> { "Copper", "Bronze", "SurtlingCore"};
    public static List<string> interiorLoot3 = new List<string> { "Bronze", "IronScrap", "SurtlingCore" };
    
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
                    Debug.LogWarning("Prefab for " + itemName + " not found");
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
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogInfo("Successfully added the Container component to " + childName);
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
                dropTable.m_dropMin = 3;
                dropTable.m_dropMax = 3;
                
                foreach (var itemName in itemNames)
                {

                    GameObject itemPrefab = PrefabManager.Cache.GetPrefab<GameObject>(itemName);

                    if (itemPrefab != null)
                    {
                        DropTable.DropData dropData = new DropTable.DropData
                        {
                            m_item = itemPrefab,
                            m_stackMin = 5,
                            m_stackMax = 6,
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
}