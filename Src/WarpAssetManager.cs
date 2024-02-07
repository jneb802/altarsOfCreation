using System;
using System.Collections.Generic;
using Jotunn.Utils;
using UnityEngine;
using Jotunn.Extensions;

namespace Altars_of_Creation;

public class WarpAssetManager
{
    public static AssetBundle mwl_tyggrason_bundle;
    public static GameObject MWL_RuinsCathedral1_GameObject;

    public static void LoadAssets()
    {
        mwl_tyggrason_bundle = AssetUtils.LoadAssetBundleFromResources("mwl_tyggrason");
        if (mwl_tyggrason_bundle != null)
        {
            LoadGameObject();
        }
    }

    public static void LoadGameObject()
    {
        MWL_RuinsCathedral1_GameObject = mwl_tyggrason_bundle.LoadAsset<GameObject>("MWL_RuinsCathedral1");
        CheckGameObjects();
        
    }
    
    public static void CheckGameObjects()
    {
        var prefabs = new Dictionary<string, GameObject>
        {
            {"MWL_RuinsCathedral1", MWL_RuinsCathedral1_GameObject},
               
        };
                
        foreach (var prefab in prefabs)
        {
            if (prefab.Value == null)
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError($"{prefab.Key} is not loaded.");
            }
            else
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug($"{prefab.Key} has been loaded.");
            }
        }
    }
}