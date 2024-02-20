using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Extensions;
using ServerSync;
using UnityEngine;
using Jotunn.Managers;
using Paths = BepInEx.Paths;
using Jotunn.Utils;
using YamlDotNet.RepresentationModel;

namespace Altars_of_Creation
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Altars_of_CreationPlugin : BaseUnityPlugin
    {
        internal const string ModName = "MWL_Altars_of_Creation";
        internal const string ModVersion = "1.0.0";
        internal const string Author = "warpalicious";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource Altars_of_CreationLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);
        
        public void Awake()
        {
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();

            WarpAssetManager.LoadAssets();
            WarpItemManager.CreateChurchKey();
            WarpTextManager.AddlocalizationsEnglish();

            ZoneManager.OnVanillaLocationsAvailable += AddLocation_MWL_RuinsCathedral1;

            bool saveOnSet = Config.SaveOnConfigSet;
            Config.SaveOnConfigSet =
                false; // This and the variable above are used to prevent the config from saving on startup for each config entry. This is speeds up the startup process.
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
            
            if (saveOnSet)
            {
                Config.SaveOnConfigSet = saveOnSet;
                Config.Save();
            }
            
            MWL_RuinsCathedral1_Quantity_Config = config("1 - Location Spawn Quantities", "MWL_RuinsCathedral1", 10,
                "Amount of this location the game will attempt to place during world generation");
            TierOnePrice = config("2 - Dungeon Tier Prices", "Tier 1 Price", 200,
                "Amount of offering item (default is coins) the player must offer to trigger dungeon tier 1");
            TierTwoPrice = config("2 - Dungeon Tier Prices", "Tier 2 Price", 350,
                "Amount of offering item (default is coins) the player must offer to trigger dungeon tier 2");
            TierThreePrice = config("2 - Dungeon Tier Prices", "Tier 3 Price", 500,
                "Amount of offering item (default is coins) the player must offer to trigger dungeon tier 3");
            UseCustomLocationCreatureListYAML = config("3 - Custom Location YAML files",
                "Use Custom Location Creature List", Toggle.Off,
                "When Off, location will spawn default creatures. When On, location will spawn creatures from the warpalicious.MWL_Altars_of_Creation_Creatures.yml file BepinEx config folder");
            UseCustomLocationLootListYAML = config("3 - Custom Location YAML files",
                "Use Custom Location Loot List", Toggle.Off,
                "When Off, location will use default loot. When On, location will use custom loot from the warpalicious.MWL_Altars_of_Creation_LootLists.yml file in BepinEx config folder");
            
            SetupCreatureYAML();
            SetupLootYAML();
        }

        public static string creatureYAMLContent;
        public static string lootYAMLContent;
        
        public static void SetupCreatureYAML()
        { 
            if (UseCustomLocationCreatureListYAML.Value == Toggle.On)
            {
                var creatureYAMLFilePath = Path.Combine(Paths.ConfigPath, "warpalicious.MWL_Altars_of_Creation_Creatures.yml");
                creatureYAMLContent = File.ReadAllText(creatureYAMLFilePath);
                Altars_of_CreationLogger.LogInfo("Successfully loaded warpalicious.MWL_Altars_of_Creation_Creatures.yml file from BepinEx config folder");
            }
            else
            {
                creatureYAMLContent = AssetUtils.LoadTextFromResources("warpalicious.MWL_Altars_of_Creation_Creatures.yml");
            }
        }
        
        public static void SetupLootYAML()
        { 
            if (UseCustomLocationLootListYAML.Value == Toggle.On)
            {
                var lootYAMLFilePath = Path.Combine(Paths.ConfigPath, "warpalicious.MWL_Altars_of_Creation_LootLists.yml");
                lootYAMLContent = File.ReadAllText(lootYAMLFilePath);
                Altars_of_CreationLogger.LogInfo("Successfully loaded warpalicious.MWL_Altars_of_Creation_LootLists.yml file from BepinEx config folder");
            }
            else
            {
                lootYAMLContent = AssetUtils.LoadTextFromResources("warpalicious.MWL_Altars_of_Creation_LootLists.yml");
            }
        }
        
        public static void AddLocation_MWL_RuinsCathedral1()
        {
            var MWL_RuinsCathedral1_GameObject = WarpAssetManager.assetBundle.LoadAsset<GameObject>("MWL_RuinsCathedral1");
            
            GameObject MWL_RuinsCathedral1_Container = ZoneManager.Instance.CreateLocationContainer(MWL_RuinsCathedral1_GameObject);
            
            var exteriorCreatureList = WarpCreatureManager.CreateCreatureList("MWL_RuinsCathedral1",7,creatureYAMLContent);
            var interiorCreatureList = WarpCreatureManager.CreateCreatureList("MWL_RuinsCathedral1",20,creatureYAMLContent);
            
            var exteriorCreatureSpawnerList = WarpCreatureManager.GetExteriorCreatureSpawners(MWL_RuinsCathedral1_Container);
            var interiorCreatureSpawnerList = WarpCreatureManager.GetInteriorCreatureSpawners(MWL_RuinsCathedral1_Container);

            WarpCreatureManager.AddCreaturestoSpawnerList(exteriorCreatureSpawnerList,exteriorCreatureList);
            WarpCreatureManager.AddCreaturestoSpawnerList(interiorCreatureSpawnerList,interiorCreatureList);
            
            var exteriorLoot = WarpLootManager.LoadLootConfig("MWL_RuinsCathedral1", "exteriorLoot", lootYAMLContent);
            WarpAltarManager.SetupInteriorLootLists("MWL_RuinsCathedral1", lootYAMLContent);
            
            DropTable Cathedral1DropTable = WarpLootManager.CreateDropTable(exteriorLoot, 2, 3);
            WarpLootManager.AddContainerToChild(MWL_RuinsCathedral1_Container.gameObject.transform.FindDeepChild("Unity").gameObject, "loot_chest_wood1", Cathedral1DropTable);
            WarpLootManager.AddContainerToChild(MWL_RuinsCathedral1_Container.gameObject.transform.FindDeepChild("Unity").gameObject, "loot_chest_wood2", Cathedral1DropTable);
            WarpLootManager.AddContainerToChild(MWL_RuinsCathedral1_Container.gameObject.transform.FindDeepChild("Unity").gameObject, "loot_chest_wood3", Cathedral1DropTable);
            
            WarpAltarManager.AddOfferingManagerToChildContainer(MWL_RuinsCathedral1_Container.gameObject.transform.FindDeepChild("Unity").gameObject, "offeringBox");
            
            WarpItemManager.AddChurchKeytoChild(MWL_RuinsCathedral1_Container.gameObject.transform.FindDeepChild("Blueprint").gameObject, "churchgate");
            
            CustomLocation MWL_RuinsCathedral1_Location = 
                new CustomLocation(MWL_RuinsCathedral1_Container, fixReference: true,
                    new LocationConfig
                        {
                            Biome = Heightmap.Biome.Meadows,
                            Quantity = Altars_of_CreationPlugin.MWL_RuinsCathedral1_Quantity_Config.Value,
                            Priotized = true,
                            ExteriorRadius = 32,
                            ClearArea = true,
                            RandomRotation = false,
                            Group = "Ruins_large",
                            MinDistanceFromSimilar = 1028,
                            MaxTerrainDelta = 2f,
                            MinAltitude = 5,
                            MinDistance = 1500,
                            MaxDistance = 5000,
                            InteriorRadius = 64,
                            InForest = true,
                            ForestTresholdMin = 1.2f,
                            ForestTrasholdMax = 2,
                            HasInterior = true,
                            InteriorEnvironment = "Crypt",
                        });

            MWL_RuinsCathedral1_Location.Location.m_discoverLabel = "Kristnir Cathedral";
            MWL_RuinsCathedral1_Location.Location.m_applyRandomDamage = false;
            MWL_RuinsCathedral1_Location.Location.m_noBuild = false;
            
            ZoneManager.Instance.AddCustomLocation(MWL_RuinsCathedral1_Location);
            
            ZoneManager.OnVanillaLocationsAvailable -= AddLocation_MWL_RuinsCathedral1;
        }

        private void OnDestroy()
        {
            Config.Save();
        }
    
        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        public static ConfigEntry<int> MWL_RuinsCathedral1_Quantity_Config = null!;
        public static ConfigEntry<int> TierOnePrice = null!;
        public static ConfigEntry<int> TierTwoPrice = null!;
        public static ConfigEntry<int> TierThreePrice = null!;
        public static ConfigEntry<Toggle> UseCustomLocationCreatureListYAML = null!;
        public static ConfigEntry<Toggle> UseCustomLocationLootListYAML = null!;

        public enum Toggle
        {
            On = 1,
            Off = 0
        }
        
        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Altars_of_CreationLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                Altars_of_CreationLogger.LogError($"There was an issue loading your {ConfigFileName}");
                Altars_of_CreationLogger.LogError("Please check your config entries for spelling and format!");
            }
        }

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order = null!;
            [UsedImplicitly] public bool? Browsable = null!;
            [UsedImplicitly] public string? Category = null!;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
        }
    }
}