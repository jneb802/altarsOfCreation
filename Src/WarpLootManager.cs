using System.Collections.Generic;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Altars_of_Creation;

public class WarpLootManager
{
    
    public static List<string> meadowsLoot2 = new List<string> { "FineWood", "Bronze", "CopperOre", "Coins" };
    public static List<string> interiorLoot1 = new List<string> { "Copper", "Tin", "Ruby"};
    public static List<string> interiorLoot2 = new List<string> { "Copper", "Bronze", "Surtling Core"};
    public static List<string> interiorLoot3 = new List<string> { "Bronze", "Iron Scrap", "Surtling Core" };
    
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
                // Use Jotunn's PrefabManager to get the prefab for the item
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
        
        public static List<Container> GetInteriorContainers(GameObject location)
        {
            List<Container> locationInteriorContainers = new List<Container>();
        
            Container[] allContainers = location.FindDeepChild("Unity").GetComponentsInChildren<Container>();
        
            foreach (var container in allContainers)
            {
                if (container.transform.parent != null && container.transform.parent.position.y >= 5000)
                {
                    locationInteriorContainers.Add(container);
                    Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Interior container spawner found in " + location + " with name: " + container.transform.parent.name);

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