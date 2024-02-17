using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using HarmonyLib;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

namespace Altars_of_Creation;

public class WarpCreatureManager: MonoBehaviour
{
    public static GameObject GetCreaturePrefab(string prefabName)
    {   
        GameObject creaturePrefab = PrefabManager.Cache.GetPrefab<GameObject>(prefabName);
        if (creaturePrefab != null)
        {
            return creaturePrefab;
        }
        else
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Prefab not found for name:" + prefabName);
            return null;
        }
    }

    public static List<CreatureSpawner> GetExteriorCreatureSpawners(GameObject location)
    {
        List<CreatureSpawner> locationExteriorSpawners = new List<CreatureSpawner>();
        
        CreatureSpawner[] allSpawners = location.GetComponentsInChildren<CreatureSpawner>();
        
        foreach (var spawner in allSpawners)
        {
            if (spawner.transform.parent != null && spawner.transform.position.y <= 5000)
            {
                locationExteriorSpawners.Add(spawner);
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Exterior creature spawner found in " + location + "with name: " + spawner.transform.parent.name);
            }
        }

        return locationExteriorSpawners;
    }
    
    public static List<CreatureSpawner> GetInteriorCreatureSpawners(GameObject location)
    {
        List<CreatureSpawner> locationInteriorSpawners = new List<CreatureSpawner>();
        
        CreatureSpawner[] allSpawners = location.GetComponentsInChildren<CreatureSpawner>();

        foreach (var spawner in allSpawners)
        {
            if (spawner.transform.parent != null && spawner.transform.position.y >= 5000)
            {
                locationInteriorSpawners.Add(spawner);
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Interior creature spawner found in " + location + " with name: " + spawner.transform.parent.name);
            }
        }

        return locationInteriorSpawners;
    }
    
    public static List<CreatureSpawner> GetSceneInteriorCreatureSpawners()
    {
        List<CreatureSpawner> locationInteriorSpawners = new List<CreatureSpawner>();
        
        var allSceneObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        
        foreach (var rootObject in allSceneObjects)
        {
            if (rootObject.name.StartsWith("MWL_RuinsCathedral1_Interior_Spawner") && rootObject.transform.position.y >= 5000 && !locationInteriorSpawners.Any(spawner => spawner.gameObject.name == rootObject.name))
            {
                Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Found object with name matching criteria. Name: " + rootObject.name);
                CreatureSpawner spawner = rootObject.GetComponent<CreatureSpawner>();
                if (spawner != null)
                {
                    locationInteriorSpawners.Add(spawner);
                    Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Found interior creature spawner with name: " + spawner);
                }
                else
                {
                    Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Failed the creature spawner from object with name: " + rootObject);
                }
            }
        }

        return locationInteriorSpawners;
    }
    
    public static List<string> CreateCreatureList(string filePath, string locationName, int creatureCount)
    {
        List<string> locationCreatureList = new List<string>();
        
        // Read the YAML content from the file
        string yamlContent = File.ReadAllText(filePath);
        if (yamlContent != null)
        {
            // Load the YAML stream
            var yaml = new YamlStream();
            yaml.Load(new StringReader(yamlContent));
        
            // Get the mapping node of the YAML document
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        
            // Check if the location name key exists in the YAML document
            if (mapping.Children.ContainsKey(new YamlScalarNode(locationName)))
            {
                // Get the creatures node list from the mapping
                var creaturesNode = (YamlSequenceNode)mapping.Children[new YamlScalarNode(locationName)]["creatures"];
                int creaturesInNode = creaturesNode.Children.Count;

                // Generate a random start index
                int randomNumber = Random.Range(0, creaturesInNode-1);
            
                // Add creatures to the list, looping if necessary
                for (int i = 0; i < creatureCount; i++)
                {
                    // Calculate the index taking into account the loop
                    int index = (randomNumber + i) % creaturesInNode;
                    var creature = (YamlScalarNode)creaturesNode.Children[index];
                    locationCreatureList.Add(creature.Value);
                }
            }
        }
        return locationCreatureList;
    }
    
    public static void AddCreaturetoSpawner(CreatureSpawner creatureSpawner, string creaturePrefab)
    {
        var creature = GetCreaturePrefab(creaturePrefab);
        if (creature != null)
        {
            creatureSpawner.m_creaturePrefab = creature;
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("Creature with name " + creaturePrefab + " was added to " + creatureSpawner.transform.parent.name);
 
        }
        else
        {
            Altars_of_CreationPlugin.Altars_of_CreationLogger.LogError("Creature not found for name: " + creaturePrefab);
 
        }
    }

    public static void AddCreaturestoSpawnerList(List<CreatureSpawner> exteriorCreatureSpawnerList, List<string> exteriorCreatureList)
    {
        int creatureIndex = 0;

        foreach (var spawner in exteriorCreatureSpawnerList)
        {
            // Use modulo to loop back to the start of the creature list if necessary
            string creatureName = exteriorCreatureList[creatureIndex % exteriorCreatureList.Count];
            
            // Add the creature to the spawner
            AddCreaturetoSpawner(spawner, creatureName);
            
            // Increment the creatureIndex to get the next creature for the next spawner
            creatureIndex++;
        }
    }
       
    public static void UpdateCreaturesLevel(List<CreatureSpawner> creatureSpawners, int minLevel)
    {
        foreach (var spawner in creatureSpawners)
        {
            ZNetView nview = spawner.Spawn();
            // These checks might be overkill and could be simplified a bit
            if (!nview || !nview.gameObject || !nview.gameObject.TryGetComponent(out Character character))
            {
                continue;
            }
            character.SetLevel(minLevel);
        }
    }
}