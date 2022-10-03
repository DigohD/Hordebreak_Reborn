using FNZ.Shared.Model.Entity;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Shared.Model.World
{
    
    // public ushort[] TileIdCodes;
    // public byte[] TileCosts;
    // public byte[] TileDangerLevels;
    // public bool[] BlockingTiles;
    // public bool[] TileObjectBlockingList;
    // public bool[] TileSeeThroughList;
    // public int[] TilePositionsX;
    // public int[] TilePositionsY;
    // public float[] TileTemperatures;
    // public long[] TileRooms;
    
    // public FNEEntity[] SouthEdgeObjects;
    // public FNEEntity[] WestEdgeObjects;
    //
    // public FNEEntity[] TileObjects;
    //
    // public List<FNETransform> FloatPointObjects;
    
    public sealed class GameWorldTile
    {
        public ushort TileIdCode { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public bool Blocking { get; set; }
        public bool TileObjectBlocking { get; set; }
        public bool SeeThrough { get; set; }

        public byte TileCost { get; set; }

        public FNEEntity SouthEdgeObject { get; set; }
        public FNEEntity WestEdgeObject { get; set; }
        public FNEEntity TileObject { get; set; }
    } 
    
    public abstract class GameWorldInstance
    {
        public int Width;
        public int Height;

        public GameWorldTile[,] Tiles;

        public GameWorldInstance(int width, int height)
        {
            Width = width;
            Height = height;

            Tiles = new GameWorldTile[Width, Height];
        }

        public GameWorldTile GetTile(float2 position)
        {
            return Tiles[(int) position.x, (int) position.y];
        }

        public GameWorldTile GetTile(int x, int y) => Tiles[x, y];
        
        public void AddTileObject(FNEEntity tileObject)
        {
            var tile = GetTile(tileObject.Position);
            tile.TileObject = tileObject;
        }

        public void RemoveTileObject(FNEEntity tileObject)
        {
            var tile = GetTile(tileObject.Position);
            tile.TileObject = null;
        }

        public void AddEdgeObject(FNEEntity edgeObject)
        {
            var isWest = edgeObject.Position.x % 1 == 0;
            var isSouth = edgeObject.Position.y % 1 == 0;

            var tile = GetTile(edgeObject.Position);
            
            if (isWest)
            {
                tile.WestEdgeObject = edgeObject;
            }
            else if (isSouth)
            {
                tile.SouthEdgeObject = edgeObject;
            }
            else
            {
                Debug.LogError("EdgeObject is neither south or west. Should never happen.");
            }
        }

        public void RemoveEdgeObject(FNEEntity edgeObject)
        {
            var isWest = edgeObject.Position.x % 1 == 0;
            var isSouth = edgeObject.Position.y % 1 == 0;

            var tile = GetTile(edgeObject.Position);
            
            if (isWest)
            {
                tile.WestEdgeObject = null;
            }
            else if (isSouth)
            {
                tile.SouthEdgeObject = null;
            }
            else
            {
                Debug.LogError("EdgeObject is neither south or west. Should never happen.");
            }
        }
    }
}