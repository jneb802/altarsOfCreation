using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Jotunn.Utils;
using UnityEngine;
using Jotunn.Extensions;
using Jotunn.Managers;
using Steamworks;

namespace Altars_of_Creation;

public static class WarpAssetManager
{
    private const string BundleName = "mwl_tyggrason";
    private const string PrefabName = "MWL_RuinsCathedral1";
    public static AssetBundle assetBundle;
    public static GameObject MWL_RuinsCathedral1_GameObject;
    
    public static void LoadAssets()
    {
        assetBundle = AssetUtils.LoadAssetBundleFromResources(
            BundleName,
            Assembly.GetExecutingAssembly()
        );
        if (assetBundle == null)
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Failed to load asset bundle with name: " + BundleName);
           
        }
    }

    private static bool LoadGameObject()
    {
        MWL_RuinsCathedral1_GameObject = assetBundle.LoadAsset<GameObject>(PrefabName);
        CheckGameObjects();
        return CheckGameObjects();
    }
    
    private static bool CheckGameObjects()
    {
        var prefabs = new Dictionary<string, GameObject>
        {
            {PrefabName, MWL_RuinsCathedral1_GameObject},
               
        };

        bool result = true;
                
        foreach (var prefab in prefabs)
        {
            if (!prefab.Value)
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError(prefab.Key + " is not loaded.");
                return false;
            }
            else
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug(prefab.Key + " is loaded.");
            }
        }

        return false;
    }
    
    
}