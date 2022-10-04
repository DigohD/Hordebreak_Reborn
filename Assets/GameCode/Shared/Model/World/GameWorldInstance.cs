using System;
using System.Collections.Generic;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EdgeObject;
using FNZ.Shared.Model.World.Tile;
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
    
    public abstract class GameWorldInstance
    {
        public Guid Id { get; set; }
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
            var x = (int) position.x;
            var y = (int) position.y;

            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;
            
            return Tiles[x, y];
        }

        public GameWorldTile GetTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;
            
            return Tiles[x, y];
        } 
        
        public bool IsAnySurroundingTileInRadiusNull(int2 origin, int radius)
        {
            for (var y = -radius; y <= radius; y++)
            {
                for (var x = -radius; x <= radius; x++)
                {
                    var worldPos = new int2(origin.x + x, origin.y + y);
                    
                    if (GetTile(worldPos) == null) return true;
                }
            }

            return false;
        }

        public List<FNEEntity> GetPlayersOnTile(int2 tilePos)
        {
            var tile = GetTile(tilePos.x, tilePos.y);
            return tile?.Players;
        }
        
        public List<FNEEntity> GetEnemiesOnTile(int2 tilePos)
        {
            var tile = GetTile(tilePos.x, tilePos.y);
            return tile?.Enemies;
        }
        
        
        public List<int2> GetSurroundingTilesInRadius(int2 origin, int radius)
        {
            List<int2> surrounding = new List<int2>();

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (GetTile(new int2(origin.x + x, origin.y + y)) != null)
                        surrounding.Add(new int2(origin.x + x, origin.y + y));
                }
            }

            return surrounding;
        }

        public FNEEntity GetTileObject(int x, int y)
        {
            var tile = GetTile(x, y);
            return tile?.TileObject;
        }

        public FNEEntity GetEdgeObject(int tileX, int tileY, GameWorld.EdgeObjectDirection dir)
        {
            var tile = GetTile(tileX, tileY);
            if (dir == GameWorld.EdgeObjectDirection.SOUTH)
                return tile.SouthEdgeObject;
            if (dir == GameWorld.EdgeObjectDirection.WEST)
                return tile.WestEdgeObject;
            return null;
        }
        
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
        
        public List<int2> GetTileStraightNeighbors(int x, int y)
		{
			return new List<int2>
			{
				new int2(x, y - 1), //South
				new int2(x - 1, y), //West
				new int2(x, y + 1), //North
				new int2(x + 1, y)  //East
			};
		}

		public List<int2> GetTileDiagonalNeighbors(int x, int y)
		{
			return new List<int2>
			{
				new int2(x - 1, y - 1), //SouthWest
				new int2(x - 1, y + 1), //NorthWest
				new int2(x + 1, y + 1), //NorthEast
				new int2(x + 1, y - 1)  //SouthEast
			};
		}
		
		public void RemoveEnemyFromTile(FNEEntity enemy)
		{
			RemoveEnemyFromTile(new int2((int)enemy.Position.x, (int)enemy.Position.y), enemy);
		}

		public void RemoveEnemyFromTile(int2 tilePos, FNEEntity enemy)
		{
			var cc = GetTile(tilePos.x, tilePos.y);

			if (cc == null)
			{
				Debug.LogWarning($"No tile  found when trying to remove enemy on tile at position: {tilePos}");
				return;
			}

			cc.RemoveEnemy(enemy);		
		}
		
		public void AddEnemyToTile(int2 tilePos, FNEEntity enemy)
		{
			var cc = GetTile(tilePos.x, tilePos.y);
			if (cc == null)
			{
				Debug.LogWarning($"No chunk found when trying to add enemy to tile on position: {tilePos}");
				return;
			}

			cc.AddEnemy(enemy);
		}

		public void AddEnemyToTile(FNEEntity enemy)
		{
			AddEnemyToTile(new int2((int)enemy.Position.x, (int)enemy.Position.y), enemy);
		}

		public bool IsTileWestEdgeOccupied(int2 tilePos)
		{
			return GetEdgeObject(tilePos.x, tilePos.y, GameWorld.EdgeObjectDirection.WEST) != null;
		}

		public bool IsTileEastEdgeOccupied(int2 tilePos)
		{
			return GetEdgeObject(tilePos.x + 1, tilePos.y, GameWorld.EdgeObjectDirection.WEST) != null;
		}

		public bool IsTileNorthEdgeOccupied(int2 tilePos)
		{
			return GetEdgeObject(tilePos.x, tilePos.y + 1, GameWorld.EdgeObjectDirection.SOUTH) != null;
		}

		public bool IsTileSouthEdgeOccupied(int2 tilePos)
		{
			return GetEdgeObject(tilePos.x, tilePos.y, GameWorld.EdgeObjectDirection.SOUTH) != null;
		}

		public bool IsTileWestEdgeSeeThrough(int2 tilePos, int range)
		{
			var eo = GetEdgeObject(tilePos.x, tilePos.y, GameWorld.EdgeObjectDirection.WEST);
			if (eo == null)
			{
				return true;
			}

			var eoComp = eo.GetComponent<EdgeObjectComponentShared>();
			
			if(eoComp.IsSeethrough && range < eo.Data.seeThroughRange)
			{
				return true;
			}

			return false;
		}
		
		public bool IsTileSouthEdgeSeeThrough(int2 tilePos, int range)
		{
			var eo = GetEdgeObject(tilePos.x, tilePos.y, GameWorld.EdgeObjectDirection.SOUTH);
			if (eo == null)
			{
				return true;
			}

			var eoComp = eo.GetComponent<EdgeObjectComponentShared>();
			
			if(eoComp.IsSeethrough && range < eo.Data.seeThroughRange)
			{
				return true;
			}
			
			return false;
		}
		
		public bool IsTileEastEdgeSeeThrough(int2 tilePos, int range)
		{
			var eo = GetEdgeObject(tilePos.x + 1, tilePos.y, GameWorld.EdgeObjectDirection.WEST);
			if (eo == null)
			{
				return true;
			}

			var eoComp = eo.GetComponent<EdgeObjectComponentShared>();
			
			if(eoComp.IsSeethrough && range <= eo.Data.seeThroughRange)
			{
				return true;
			}
			
			return false;
		}
		
		public bool IsTileNorthEdgeSeeThrough(int2 tilePos, int range)
		{
			var eo = GetEdgeObject(tilePos.x, tilePos.y + 1, GameWorld.EdgeObjectDirection.SOUTH);
			if (eo == null)
			{
				return true;
			}

			var eoComp = eo.GetComponent<EdgeObjectComponentShared>();
			
			if(eoComp.IsSeethrough && range <= eo.Data.seeThroughRange)
			{
				return true;
			}
			
			return false;
		}
    }
}