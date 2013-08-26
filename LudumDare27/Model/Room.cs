using System;
using Flai;

namespace WVVW.Model
{
    public static class Room
    {
        // 5:4 atm
        public const int Width = WvvwGlobals.ScreenWidth / Tile.Size; // 40
        public const int Height = WvvwGlobals.ScreenHeight / Tile.Size; // 32
        public static readonly Vector2i Size = new Vector2i(Room.Width, Room.Height);
    }
}
