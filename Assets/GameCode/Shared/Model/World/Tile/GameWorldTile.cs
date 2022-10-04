using System.Collections.Generic;
using FNZ.Shared.Model.Entity;

namespace FNZ.Shared.Model.World.Tile
{
    public sealed class GameWorldTile
    {
        public ushort TileIdCode { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public bool Blocking { get; set; }
        public bool TileObjectBlocking { get; set; }
        public bool SeeThrough { get; set; }

        public byte TileCost { get; set; }

        public long RoomId { get; set; }

        public FNEEntity SouthEdgeObject { get; set; }
        public FNEEntity WestEdgeObject { get; set; }
        public FNEEntity TileObject { get; set; }

        public List<FNEEntity> Enemies { get; set; } = new List<FNEEntity>();
        public List<FNEEntity> Players { get; set; } = new List<FNEEntity>();
        
        public void AddEnemy(FNEEntity enemy)
        {
            if (!Enemies.Contains(enemy))
                Enemies.Add(enemy);
        }

        public void RemoveEnemy(FNEEntity enemy)
        {
            Enemies.Remove(enemy);
        }

        public List<FNEEntity> GetEnemies()
        {
            return Enemies;
        }
    } 
}