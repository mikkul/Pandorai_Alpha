using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pandorai.Sprites
{
	public static class TilesheetManager
	{
		public static List<TilesheetSprite> MapObjectSpritesheet = new List<TilesheetSprite>();
		public static List<TilesheetSprite> CreatureSpritesheet = new List<TilesheetSprite>();

		public static Texture2D MapSpritesheetTexture;
		public static Texture2D CreatureSpritesheetTexture;

		public static void AddMapSprite(int startX, int startY, int spriteWidth, int spriteHeight, int frameCount, float timePerFrame)
		{
			MapObjectSpritesheet.Add(new TilesheetSprite(new Rectangle(startX, startY, spriteWidth, spriteHeight), frameCount, timePerFrame));
		}

		public static void LoadMapSpritesheet()
		{
			AddMapSprite(0, 0, 64, 64, 0, 0); // floor 0
			AddMapSprite(64, 0, 64, 64, 1, 350); // floor variation with grass 1
			AddMapSprite(0, 64, 64, 64, 0, 0); // wall 2
			AddMapSprite(0, 128, 64, 64, 0, 0); // player 3
			AddMapSprite(0, 192, 64, 64, 6, 200); // blue key 4
			AddMapSprite(0, 256, 64, 64, 0, 0); // spider 5
			AddMapSprite(192, 0, 64, 64, 0, 0); // door 6
			AddMapSprite(0, 320, 64, 64, 3, 600); // transform spell 7
			AddMapSprite(0, 384, 64, 64, 0, 0); // tile deacal 8

			AddMapSprite(512, 320, 64, 64, 0, 0); // wall no neighbours 9
			AddMapSprite(256, 320, 64, 64, 0, 0); // wall right neighbour 10
			AddMapSprite(320, 320, 64, 64, 0, 0); // wall left and right neighbours 11
			AddMapSprite(384, 320, 64, 64, 0, 0); // wall left neighbour 12
			AddMapSprite(256, 448, 64, 64, 0, 0); // wall bottom neighbour 13
			AddMapSprite(256, 512, 64, 64, 0, 0); // wall bottom and top neighbours 14
			AddMapSprite(256, 576, 64, 64, 0, 0); // wall top neighbour 15
			AddMapSprite(  0, 448, 64, 64, 0, 0); // wall right, bottom and bottom-right neighbours 16
			AddMapSprite( 64, 448, 64, 64, 0, 0); // wall left, right, bottom, bottom-left, bottom-right neighbours 17
			AddMapSprite(128, 448, 64, 64, 0, 0); // wall left, bottom, bottom-left neighbours 18
			AddMapSprite(  0, 512, 64, 64, 0, 0); // wall top, bottom, right, top-right, bottom-right neighbours 19
			AddMapSprite( 64, 512, 64, 64, 0, 0); // wall all possible neighbours 20
			AddMapSprite(128, 512, 64, 64, 0, 0); // wall top, bottom, left, top-left, bottom-left neighbours 21
			AddMapSprite(  0, 576, 64, 64, 0, 0); // wall top, right, top-right neighbours 22
			AddMapSprite( 64, 576, 64, 64, 0, 0); // wall left, right, top, top-left, top-right neighbours 23
			AddMapSprite(128, 576, 64, 64, 0, 0); // wall top, left, top-left neighbours 24
			AddMapSprite(384, 448, 64, 64, 0, 0); // wall bottom and right neighbours 25
			AddMapSprite(512, 576, 64, 64, 0, 0); // wall top and left neighbours 26
			AddMapSprite(512, 384, 64, 64, 0, 0); // wall bottom and left neighbours 27
			AddMapSprite(320, 576, 64, 64, 0, 0); // wall top and right neighbours 28
			AddMapSprite(448, 128, 64, 64, 0, 0); // wall top, bottom, left neighbours 29
			AddMapSprite(384, 192, 64, 64, 0, 0); // wall left, right, bottom neighbours 30
			AddMapSprite(512, 192, 64, 64, 0, 0); // wall left, right, top neighbours 31
			AddMapSprite(448, 256, 64, 64, 0, 0); // wall top, bottom, right neighbours 32
			AddMapSprite(192, 448, 64, 64, 0, 0); // wall top, bottom, left, right neighbours 33
			AddMapSprite(384,  64, 64, 64, 0, 0); // wall top, bottom, left, right, top-right neighbours 34
			AddMapSprite(448,   0, 64, 64, 0, 0); // wall top, bottom, left, right, bottom-right neighbours 35
			AddMapSprite(512,  64, 64, 64, 0, 0); // wall top, bottom, left, right, top-left neighbours 36
			AddMapSprite(576,   0, 64, 64, 0, 0); // wall top, bottom, left, right, bottom-left neighbours 37
			AddMapSprite( 64, 128, 64, 64, 0, 0); // wall left, right, bottom, bottom-right neighbours 38
			AddMapSprite(128, 128, 64, 64, 0, 0); // wall left, right, bottom, bottom-left neighbours 39
			AddMapSprite(192, 128, 64, 64, 0, 0); // wall left, right, top, top-right neighbours 40
			AddMapSprite(256, 128, 64, 64, 0, 0); // wall left, right, top, top-left neighbours 41
			AddMapSprite( 64, 256, 64, 64, 0, 0); // wall top, bottom, left, bottom-left neighbours 42
			AddMapSprite(128, 256, 64, 64, 0, 0); // wall top, bottom, right, bottom-right neighbours 43
			AddMapSprite(192, 256, 64, 64, 0, 0); // wall top, bottom, left, top-left neighbours 44
			AddMapSprite(256, 256, 64, 64, 0, 0); // wall top, bottom, right, top-right neighbours 45
			AddMapSprite(448, 512, 64, 64, 0, 0); // wall top, bottom, left, right, top-right, bottom-left neighbours 46
			AddMapSprite(576, 512, 64, 64, 0, 0); // wall top, bottom, left, right, top-left, bottom-right neighbours 47
			AddMapSprite(256, 384, 64, 64, 0, 0); // wall everything except bottom-left neighbours 48
			AddMapSprite(320, 384, 64, 64, 0, 0); // wall everything except bottom-right neighbours 49
			AddMapSprite(384, 384, 64, 64, 0, 0); // wall everything except top-left neighbours 50
			AddMapSprite(448, 384, 64, 64, 0, 0); // wall everything except top-right neighbours 51
			AddMapSprite(576,  64, 64, 64, 0, 0); // wall everything except bottom-left and bottom-right neighbours 52
			AddMapSprite(576, 128, 64, 64, 0, 0); // wall everything except top-left and top-right neighbours 53
			AddMapSprite(576, 192, 64, 64, 0, 0); // wall everything except top-left and bottom-left neighbours 54
			AddMapSprite(576, 256, 64, 64, 0, 0); // wall everything except top-right and bottom-right neighbours 55

			AddMapSprite(320, 0, 64, 64, 0, 0); // stairs down 56
			AddMapSprite(384, 0, 64, 64, 0, 0); // stairs up 57

			AddMapSprite(64, 384, 64, 64, 0, 0); // health potion 58
			AddMapSprite(128, 384, 64, 64, 0, 0); // stone spike spell 59
			AddMapSprite(320, 256, 64, 64, 0, 0); // chest closed 60
			AddMapSprite(384, 256, 64, 64, 0, 0); // chest open 61

			AddMapSprite(320, 512, 64, 64, 0, 0); // torch left 62
			AddMapSprite(384, 512, 64, 64, 0, 0); // torch right 63
			AddMapSprite(384, 576, 64, 64, 0, 0); // torch top 64
			AddMapSprite(448, 576, 64, 64, 0, 0); // torch bottom 65

			AddMapSprite(320, 128, 64, 64, 0, 0); // grave 66
			AddMapSprite(384, 128, 64, 64, 0, 0); // warrior tomb 67

			AddMapSprite(192, 448, 64, 64, 0, 0); // corpse 68

			AddMapSprite(192, 512, 64, 64, 0, 0); // fireball spell 69

			AddMapSprite(448, 64, 64, 64, 0, 0); // stealth cape 70

			AddMapSprite(192, 320, 64, 64, 0, 0); // speed potion 71

			AddMapSprite(448, 192, 64, 64, 0, 0); // book stand 72

			AddMapSprite(576, 384, 64, 64, 0, 0); // mass destruction spell 73

			AddMapSprite(512, 256, 64, 64, 0, 0); // barrel 74
			AddMapSprite(576, 320, 64, 64, 0, 0); // barrel destroyed 75

			AddMapSprite(576, 384, 64, 64, 0, 0); // white gem 76
			AddMapSprite(576, 448, 64, 64, 0, 0); // gem pedestal 77

			AddMapSprite(512, 128, 64, 64, 0, 0); // power altar 78

			AddMapSprite(0, 64, 64, 64, 6, 200); // yellow key 79
			AddMapSprite(0, 640, 64, 64, 6, 200); // red key 80

			AddMapSprite(192, 576, 64, 64, 0, 0); // ice arrow spell 81

			AddMapSprite(384, 640, 64, 64, 0, 0); // empty rune 82
			AddMapSprite(448, 640, 64, 64, 0, 0); // fireball rune 83
			AddMapSprite(512, 640, 64, 64, 0, 0); // ice arrow rune 84
			AddMapSprite(576, 640, 64, 64, 0, 0); // mass destruction rune 85

			AddMapSprite(512, 512, 64, 64, 0, 0); // region gem 86

			AddMapSprite(320, 448, 64, 64, 0, 0); // spider web 87

			AddMapSprite(0, 704, 64, 64, 4, 150); // teleporter 88

			AddMapSprite(512, 0, 64, 64, 0, 0); // mana potion 89

			AddMapSprite(448, 448, 64, 64, 0, 0); // small health potion 90
			AddMapSprite(512, 448, 64, 64, 0, 0); // small mana potion 91

			AddMapSprite(448, 320, 64, 64, 0, 0); // potion of blink 92

			AddMapSprite(256, 704, 64, 64, 0, 0); // potion of endurance 93
			AddMapSprite(320, 704, 64, 64, 0, 0); // potion of energy 94
			AddMapSprite(384, 704, 64, 64, 0, 0); // potion of strength 95

			AddMapSprite(448, 704, 64, 64, 0, 0); // slingshot 96
			AddMapSprite(512, 704, 64, 64, 0, 0); // stone 97

			AddMapSprite(64, 768, 64, 64, 0, 0); // trap lever off 100
			AddMapSprite(128, 768, 64, 64, 0, 0); // trap lever on 101
			AddMapSprite(192, 768, 64, 64, 0, 0); // trap 102

			AddMapSprite(128, 0, 64, 64, 0, 0); // invisible 103

			AddMapSprite(576, 704, 64, 64, 0, 0); // summon skeleton rune 104
		}

		public static void AddCreatureSprite(int startX, int startY, int spriteWidth, int spriteHeight, int frameCount, float timePerFrame)
		{
			CreatureSpritesheet.Add(new TilesheetSprite(new Rectangle(startX, startY, spriteWidth, spriteHeight), frameCount, timePerFrame));
		}

		public static void LoadCreatureSpritesheet()
		{
			AddCreatureSprite(0, 0, 64, 64, 0, 0); // hero idle down 0
			AddCreatureSprite(128, 0, 64, 64, 0, 0); // hero idle up 1
			AddCreatureSprite(256, 0, 64, 64, 0, 0); // hero idle left 2
			AddCreatureSprite(384, 0, 64, 64, 0, 0); // hero idle right 3
			AddCreatureSprite(0, 64, 64, 64, 2, 150); // hero moving south 4
			AddCreatureSprite(128, 64, 64, 64, 2, 150); // hero moving north 5
			AddCreatureSprite(256, 64, 64, 64, 2, 150); // hero moving west 6
			AddCreatureSprite(384, 64, 64, 64, 2, 150); // hero moving east 7
			AddCreatureSprite(0, 128, 64, 64, 0, 0); // spider idle 8
			AddCreatureSprite(0, 192, 64, 64, 0, 0); // spirit idle 9
			AddCreatureSprite(0, 256, 64, 64, 0, 0); // traveller idle 10
			AddCreatureSprite(0, 320, 64, 64, 0, 0); // stone guardian idle 11
			AddCreatureSprite(0, 384, 64, 64, 0, 0); // rat idle 12
			AddCreatureSprite(0, 448, 64, 64, 0, 0); // skeleton idle 13
			AddCreatureSprite(0, 512, 64, 64, 0, 0); // shadow warrior idle 14
			AddCreatureSprite(0, 576, 64, 64, 0, 0); // seeker idle 15
			AddCreatureSprite(0, 640, 64, 64, 0, 0); // wolf idle 16
			AddCreatureSprite(0, 704, 64, 64, 0, 0); // zombie idle 17
			AddCreatureSprite(0, 768, 64, 64, 0, 0); // lich idle 18
		}
	}
}
