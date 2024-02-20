﻿/*using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Configs;
using ServerSync;
using UnityEngine;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using Paths = BepInEx.Paths;

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

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        
        public static ConfigEntry<int> MWL_RuinsCathedral1_Quantity_Config = null!;
        public static ConfigEntry<int> TierOnePrice = null!;
        public static ConfigEntry<int> TierTwoPrice = null!;
        public static ConfigEntry<int> TierThreePrice = null!;
        public static ConfigEntry<Toggle> UseCustomLocationCreatureListYAML = null!;
        
        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public void Awake()
        {
            // Uncomment the line below to use the LocalizationManager for localizing your mod.
            //Localizer.Load(); // Use this to initialize the LocalizationManager (for more information on LocalizationManager, see the LocalizationManager documentation https://github.com/blaxxun-boop/LocalizationManager#example-project).
            bool saveOnSet = Config.SaveOnConfigSet;
            Config.SaveOnConfigSet =
                false; // This and the variable above are used to prevent the config from saving on startup for each config entry. This is speeds up the startup process.

            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();

            if (saveOnSet)
            {
                Config.SaveOnConfigSet = saveOnSet;
                Config.Save();
            }

            WarpAssetManager.LoadAssets();
            WarpItemManager.CreateChurchKey();
            WarpTextManager.AddlocalizationsEnglish();
            
            ZoneManager.OnVanillaLocationsAvailable += WarpLocationManager.AddLocation_MWL_RuinsCathedral1;
            
            MWL_RuinsCathedral1_Quantity_Config = config("1 - Location Spawn Quantities", "MWL_RuinsCathedral1", 10, "Amount of this location the game will attempt to place during world generation");
            TierOnePrice = config("2 - Dungeon Tier Prices", "Tier 1 Price", 200, "Amount of offering item (default is coins) the player must offer to trigger dungeon tier 1");
            TierTwoPrice = config("2 - Dungeon Tier Prices", "Tier 2 Price", 350, "Amount of offering item (default is coins) the player must offer to trigger dungeon tier 2");
            TierThreePrice = config("2 - Dungeon Tier Prices", "Tier 3 Price", 500, "Amount of offering item (default is coins) the player must offer to trigger dungeon tier 3");
            UseCustomLocationCreatureListYAML = config("3 - Custom Location Creature List", "Use Custom Location Loot List", Toggle.Off, "When Off, location will spawn default creatures. When On, location will spawn creatures from the MWL_AltarsOfCreation_Creatures.yml file");
            
        }

        private void OnDestroy()
        {
            Config.Save();
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


        #region ConfigOptions

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

        class AcceptableShortcuts : AcceptableValueBase
        {
            public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
            {
            }

            public override object Clamp(object value) => value;
            public override bool IsValid(object value) => true;

            public override string ToDescriptionString() =>
                "# Acceptable values: " + string.Join(", ", UnityInput.Current.SupportedKeyCodes);
        }

        #endregion
        
        
        
    }

    public static class KeyboardExtensions
    {
        public static bool IsKeyDown(this KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKeyDown(shortcut.MainKey) &&
                   shortcut.Modifiers.All(Input.GetKey);
        }

        public static bool IsKeyHeld(this KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKey(shortcut.MainKey) &&
                   shortcut.Modifiers.All(Input.GetKey);
        }
    }
}*/