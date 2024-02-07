using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Jotunn.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace Altars_of_Creation;

public class WarpAltarManager: MonoBehaviour, Hoverable, Interactable
{
    // These are the fields that need to be used throughout the class
    private string name = "Offering Box";
    private static Container container;
    
    //private static Inventory Inventory;

    public string offeringItem = "Coins";
    public int creatureLevelOnePrice = 250;
    public int creatureLevelTwoPrice = 100;

    //public static EffectList altarEffects = new EffectList();
    
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
    // This is the method that gets called when the player clicks the button
    private static void OnOfferingButtonClick()
    {
        Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Button was clicked!");

        var inventory = container.GetInventory();
        if (inventory != null)
        {
           var tier = CalculateInteriorTier(inventory);
           if (tier != null)
           {
               var customLocation = ZoneManager.Instance.GetCustomLocation("MWL_RuinsCathedral1");
               
               if (customLocation != null)
               {
                   var customLocationGameObject = customLocation.Prefab;

                   var interiorContainers = WarpLootManager.GetInteriorContainers(customLocationGameObject);
                   WarpLootManager.UpdateInteriorContainerTier(interiorContainers, tier);
                   
                   var interiorCreatureSpawnerList = WarpCreatureManager.GetInteriorCreatureSpawners(customLocationGameObject);
                   WarpCreatureManager.UpdateCreaturesLevel(interiorCreatureSpawnerList, tier);

                   MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Offering Accepted");
               }
           }
        }
    }

    private static int CalculateInteriorTier(Inventory inventory)
    {
        int tier = 1;
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
                    case 200:
                        tier = 1;
                        break;
                    case 350:
                        tier = 2;
                        break;
                    case 500:
                        tier = 3;
                        break;
                    default:
                        tier = 3;
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

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        throw new System.NotImplementedException();
    }
}