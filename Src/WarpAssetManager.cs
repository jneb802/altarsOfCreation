using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Jotunn.Utils;
using UnityEngine;
using Jotunn.Extensions;
using Jotunn.Managers;

namespace Altars_of_Creation;

public class WarpAssetManager
{
    public static AssetBundle mwl_tyggrason_bundle;
    public static GameObject MWL_RuinsCathedral1_GameObject;
    
    private static Dictionary<Type, Dictionary<string, UnityEngine.Object>> dictionaryCache = null;
    
    public static void LoadAssets()
    {
        
        mwl_tyggrason_bundle = AssetUtils.LoadAssetBundleFromResources("mwl_tyggrason", Assembly.GetExecutingAssembly());
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
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError(prefab.Key + " is not loaded.");
            }
            else
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug(prefab.Key + " is loaded.");
            }
        }
    }
    
    public static void ClearPrefabCache(Type t)
    {
        if (dictionaryCache == null)
        {
            var cacheField = AccessTools.Field(typeof(PrefabManager.Cache), "dictionaryCache") as FieldInfo;
            if (cacheField != null)
            {
                var dict = cacheField.GetValue(null) as Dictionary<Type, Dictionary<string, UnityEngine.Object>>;
                if (dict != null)
                {
                    dictionaryCache = dict;
                }
                else
                {
                    Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Couldn't get value of PrefabManager.Cache.dictionaryCache");
                }
            }
            else
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Couldn't find memberinfo for PrefabManager.Cache.dictionaryCache");
            }
        }

        if (dictionaryCache != null)
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Clearing cache for type: " + t);
            dictionaryCache.Remove(t);
        }
    }
}