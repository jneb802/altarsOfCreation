using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Extensions;
using Jotunn.Managers;
using UnityEngine;

namespace Altars_of_Creation;

public class WarpItemManager
{
    
    public static void AddChurchKeytoChild(GameObject parentGameObject, string childName)
    {
        // Find the child GameObject by name
        Transform childTransform = parentGameObject.transform.Find(childName);

        // Check if the child was found
        if (childTransform != null)
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Found the churchgate gameobject");
            // Get the the SpawnArea component on the child GameObject
            Door childDoor = childTransform.gameObject.GetComponent<Door>();

            if (childDoor != null)
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("childDoor is not null");
                var churchKey = ItemManager.Instance.GetItem("ChurchKey").ItemPrefab;
                if (churchKey != null)
                {
                    childDoor.m_keyItem = churchKey.GetComponent<ItemDrop>();
                }
                else
                {
                    Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Failed to get church key");
                }
            }
            else
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Failed to get door component");
            }
                    
        }
        else
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Child GameObject (" + childName + ") not found in parent GameObject (" + parentGameObject + ")");
        }
    }
    
    public static void CreateChurchKey()
    {
        // Create and add a custom item
        ItemConfig keyConfig = new ItemConfig();
        keyConfig.Amount = 1;
        
        // Prefab did not use mocked refs so no need to fix them
        var churchKeyItem = new CustomItem(WarpAssetManager.assetBundle, "ChurchKey", fixReference: false, keyConfig);
        ItemManager.Instance.AddItem(churchKeyItem);
        
        Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("ChurchKey has been added");
    }
    
    [HarmonyPatch(typeof(Door), nameof(Door.Interact))]
    private static class DoorPatch
    {
        private static void Postfix(Door __instance, ref bool __result, Humanoid character)
        {
            if (__result)
            {
                var inventory = character.GetInventory();
                if (inventory != null)
                {
                    if (inventory.ContainsItemByName("$warp_churchkey"))
                    {
                        inventory.RemoveItem("$warp_churchkey", 1);
                    }
                }
            }
        }
    } 
}