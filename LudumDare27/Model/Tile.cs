using Flai;
using Microsoft.Xna.Framework;

namespace WVVW.Model
{
    public enum TileType : byte
    {
        Air,
        Solid,

        SpikeUp,
        SpikeRight,
        SpikeDown,
        SpikeLeft,

        PlayerSpawn, // MEH!
        PlayerGoal, // PLOH! These shouldn't be tiles :| Remove them when saving to be Air

        Clock5,
        Clock10,
        Clock15,

        SpringHorizontal,
        SpringVertical,

        Key,
        DoorLock,

        CannonUp,
        CannonRight,
        CannonDown,
        CannonLeft,

        Stage1,
        Stage2,
        Stage3,
        Stage4,
        Stage5,
        Stage6,
        Stage7,
    }

    public static class Tile
    {
        public const int Size = 16;

        public static int WorldToTileCoordinate(float x)
        {
            return x >= 0 ? (int)(x / Tile.Size) : (int)(FlaiMath.Floor(x / Tile.Size));
        }

        public static Vector2i WorldToTileCoordinate(Vector2 v)
        {
            return new Vector2i(
                Tile.WorldToTileCoordinate(v.X),
                Tile.WorldToTileCoordinate(v.Y));
        }

        public static bool IsKilling(TileType tile)
        {
            return Tile.IsSpike(tile); // || ...
        }

        public static bool IsSolid(TileType tile)
        {
            return tile == TileType.Solid || tile == TileType.DoorLock || Tile.IsCannon(tile);
        }

        public static RectangleF GetTileBounds(int x, int y)
        {
            return new RectangleF(x * Tile.Size, y * Tile.Size, Tile.Size, Tile.Size);
        }

        public static bool IsSpike(TileType tile)
        {
            return tile == TileType.SpikeUp || tile == TileType.SpikeRight || tile == TileType.SpikeDown || tile == TileType.SpikeLeft;
        }

        public static bool IsClock(TileType tile)
        {
            return tile == TileType.Clock5 || tile == TileType.Clock10 || tile == TileType.Clock15;
        }

        public static bool IsSpring(TileType tile)
        {
            return tile == TileType.SpringHorizontal || tile == TileType.SpringVertical;
        }

        public static bool IsCannon(TileType tile)
        {
            return tile == TileType.CannonUp || tile == TileType.CannonRight || tile == TileType.CannonDown || tile == TileType.CannonLeft;
        }

        public static bool IsStage(TileType tile)
        {
            return (int)tile >= (int)TileType.Stage1 && (int)tile <= (int)TileType.Stage7;
        }
    }
}
