using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Pandorai.Creatures;
using Pandorai.Items;
using Pandorai.Sounds;
using Pandorai.Structures;
using Pandorai.Tilemaps;

namespace Pandorai.Persistency
{
    static class PersistencyLoader
    {
        public const string SettingsFilePath = "settings.json";
        public const string SavedGameFilePath = "savedGame.json";

        public static void LoadSettings()
        {
            var fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), SettingsFilePath);
            if(!File.Exists(fullPath))
            {
                return;
            }

            var json = File.ReadAllText(fullPath);
            var settings = JsonConvert.DeserializeObject<Settings>(json);
            SoundManager.MusicVolume = settings.MusicVolume;
            SoundManager.SoundsVolume = settings.SoundsVolume;
        }

        public static void SaveSettings()
        {
            var settings = new Settings();
            settings.MusicVolume = SoundManager.MusicVolume;
            settings.SoundsVolume = SoundManager.SoundsVolume;

            var json = JsonConvert.SerializeObject(settings);
            var fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), SettingsFilePath);
            File.WriteAllText(fullPath, json);
        }

        public static bool IsSavedGame()
        {
            var fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), SavedGameFilePath);
            return File.Exists(fullPath);
        }
        
        public static void SaveGame()
        {
            var gameState = new GameState();

            // tiles
            gameState.Tiles = new TileState[Main.Game.Map.Tiles.GetLength(0), Main.Game.Map.Tiles.GetLength(1)];
            for (int x = 0; x < gameState.Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < gameState.Tiles.GetLength(1); y++)
                {
                    gameState.Tiles[x, y] = new TileState
                    {
                        Tile = Main.Game.Map.Tiles[x, y],
                    };
                }
            }

            // creatures
            foreach (var creature in Main.Game.CreatureManager.Creatures)
            {
                gameState.Creatures.Add(new CreatureState
                {
                    Creature = creature,
                });
            }

            // structures
            foreach (var structure in StructureManager.Structures)
            {
                gameState.Structures.Add(new StructureState
                {
                    Structure = structure,
                });
            }

            // items
            foreach (var item in ItemManager.Items)
            {
                gameState.Items.Add(new ItemState
                {
                    Item = item,
                });
            }

            // save file
            var json = JsonConvert.SerializeObject(gameState);
            var fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), SavedGameFilePath);
            File.WriteAllText(fullPath, json);
        }

        public static void LoadSavedGame()
        {
            var fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), SavedGameFilePath);
            if(!File.Exists(fullPath))
            {
                throw new FileNotFoundException("There is no saved game file");
            }

            var json = File.ReadAllText(fullPath);
            var gameState = JsonConvert.DeserializeObject<GameState>(json);

            // tiles
            Main.Game.Map.Tiles = new Tile[gameState.Tiles.GetLength(0), gameState.Tiles.GetLength(1)];
            for (int x = 0; x < gameState.Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < gameState.Tiles.GetLength(1); y++)
                {
                    Main.Game.Map.Tiles[x, y] = gameState.Tiles[x, y].Tile;
                }
            }

            // creatures
            foreach (var creatureState in gameState.Creatures)
            {
                creatureState.Creature.Stats = creatureState.Creature.Stats.Clone(creatureState.Creature);
                creatureState.Creature.Inventory.Owner = creatureState.Creature;
                Main.Game.CreatureManager.Creatures.Add(creatureState.Creature);
            }

            // structures
            foreach (var structureState in gameState.Structures)
            {
                var tile = Main.Game.Map.GetTile(structureState.Structure.Tile.Index);
                tile.MapObject = new MapObject(ObjectType.Interactive, 0)
                {
                    Structure = structureState.Structure,
                };
                structureState.Structure.Tile.Tile = tile;

                StructureManager.Structures.Add(structureState.Structure);
            }

            // items
            foreach (var itemState in gameState.Items)
            {
                var tile = Main.Game.Map.GetTile(itemState.Item.TileIndex);
                tile.MapObject = new MapObject(ObjectType.Collectible, 0)
                {
                    Item = itemState.Item,
                };
                ItemManager.Items.Add(itemState.Item);
            }
        }
    }
}