/*using System;
using System.Collections.Generic;
using Jotunn.Managers;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace Altars_of_Creation;

public class WarpAltarManager: MonoBehaviour, Hoverable, Interactable
{
    // These are the fields that need to be used throughout the class
    private string name = "Offering Box";
    private static Container container;
    
    private static int tierOneItemPrice = Altars_of_CreationPlugin.TierOnePrice.Value;
    private static int tierTwoItemPrice = Altars_of_CreationPlugin.TierTwoPrice.Value;
    private static int tierThreeItemPrice = Altars_of_CreationPlugin.TierThreePrice.Value;
    
    public static List<string> interiorLoot1;
    public static List<string> interiorLoot2;
    public static List<string> interiorLoot3;
    
    // This method is called when the class starts. It is used to initialize everything required for the class.
    private void Awake()
    {
        Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Offering manager called Awake");
        container = gameObject.AddComponent<Container>();
        if (container != null)
        {
            container.m_inventory.m_name = name;
            container.m_inventory.m_height = 1;
            container.m_inventory.m_width = 1;
        }
        AddCustomButtonToInventoryGUI();
    }
    
    public static void SetupInteriorLootLists(string locationName, string filePath)
    {
        interiorLoot1 = Common.WarpLootManager.LoadLootConfig(locationName, "interiorLootTier1", filePath);
        interiorLoot2 = Common.WarpLootManager.LoadLootConfig(locationName, "interiorLootTier2", filePath);
        interiorLoot3 = Common.WarpLootManager.LoadLootConfig(locationName, "interiorLootTier3", filePath);
    }
    
    private static int CalculateInteriorTier(Inventory inventory)
    {
        int tier = -1;
        if (inventory.NrOfItems() > 0)
        {
            var itemInContainer = inventory.GetItem(0);
            var itemName = itemInContainer.m_shared.m_name;
            var itemStackSize = itemInContainer.m_stack;
            if (itemName == "$item_coins")
            {
                // Debugging statement to check what the user input into the container
                string logMessage = String.Format("Item: {0}, Stack: {1}", itemName, itemStackSize);
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug(logMessage);
                
                switch (itemStackSize)
                {
                    case var _ when itemStackSize == tierOneItemPrice:
                        tier = 1;
                        break;
                    case var _ when itemStackSize == tierTwoItemPrice:
                        tier = 2;
                        break;
                    case var _ when itemStackSize == tierThreeItemPrice:
                        tier = 3;
                        break;
                    default:
                        tier = 1; // Consider if you want the default case to be 3 or another value.
                        break;
                }
                
                inventory.RemoveAll();
                var churchKey = ItemManager.Instance.GetItem("ChurchKey").ItemPrefab;
                if (churchKey != null)
                {
                    inventory.AddItem(churchKey, 1);   
                }
                else
                {
                    Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Failed to get church key");
                }
            }
            else
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Offering insufficient. Add coins."); 
            }
        }
        else
        {
            
            MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Offering insufficient."); 
        }
        return tier;
    }
    
    private static void OnOfferingButtonClick()
    {
        Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Button was clicked!");

        var inventory = container.GetInventory();
        if (inventory == null)
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Inventory is null");
            return;
        }

        var tier = CalculateInteriorTier(inventory);
        if (tier == -1)
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("No items added to the offering container");
            return;
        }
        
        var interiorCreatureSpawnerList = Common.WarpCreatureManager.GetSceneInteriorCreatureSpawners();
        if (interiorCreatureSpawnerList.Count == 0)
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Failed to get interior creature spawners");
            return;
        }
        Common.WarpCreatureManager.UpdateCreaturesLevel(interiorCreatureSpawnerList, tier);
        
        var interiorContainerList = Common.WarpLootManager.GetSceneInteriorContainers();
        if (interiorContainerList.Count == 0)
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Failed to get interior creature spawners");
            return;
        }
        UpdateInteriorContainerTier(interiorContainerList, tier);
        ShrinkChests(interiorContainerList,tier);

        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Offering Accepted");
    }
    
    public string GetHoverText()
    {
        return "Press E to open";
    }

    public string GetHoverName()
    {
        return name;
    }

    // This method is used for when the player is interacting with the box
    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (container != null)
        {
            InventoryGui.instance.Show(container);
            
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Player is interacting with the box");
            return true;
        }
        Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Player is interacting with the box but the container is null");
        return false;
    }
    
    private static Button CreateAbstractButton(InventoryGui instance, string name, Transform parent)
    {
        var gamepad = instance.m_takeAllButton.GetComponent<UIGamePad>();
        bool wasEnabled = gamepad.enabled;

        gamepad.enabled = false;
        var button = UnityEngine.Object.Instantiate(instance.m_takeAllButton, parent);
        gamepad.enabled = wasEnabled;

        button.name = name;
        button.onClick.RemoveAllListeners();

        return button;
    }
    
    private static void AddCustomButtonToInventoryGUI()
    {
        var inventoryGui = InventoryGui.instance;
        if (inventoryGui != null)
        {
            // Assuming you want to add the button to the same parent as the 'Take All' button
            var parent = inventoryGui.m_takeAllButton.transform.parent;
            var customButton = CreateAbstractButton(inventoryGui, "CustomButton", parent);

            // Setup your button here (e.g., setting text, adding click listeners)
            customButton.onClick.AddListener(OnOfferingButtonClick);
            
            var buttonText = customButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Make Offering";
            }

            RectTransform buttonRect = customButton.GetComponent<RectTransform>();
            // Note: Adjust the values according to your needs
            buttonRect.anchoredPosition = new Vector2(217, 100); // Moves the button to the right (x) and down (y)
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

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        throw new System.NotImplementedException();
    }
}*/