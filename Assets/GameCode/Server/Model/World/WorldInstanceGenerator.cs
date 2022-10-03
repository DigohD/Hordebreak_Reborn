using FNZ.Server.Model.World;
using FNZ.Server.Utils;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Utils;
using UnityEngine;

namespace GameCode.Server.Model.World
{
    public class WorldInstanceGenerator
    {
        private readonly WorldGenData m_WorldGenData;

        //private bool[] m_TileObjectGenerationMap;
        
        public WorldInstanceGenerator(WorldGenData data)
        {
            m_WorldGenData = data;

            // m_TileObjectGenerationMap = new bool[256*256];
            //
            // for (int i = 0; i < 256*256; i++)
            // {
            //     m_TileObjectGenerationMap[i] = false;
            // }
        }
        
        public ServerWorldInstance GenerateWorldInstance(int width, int height, int seedX, int seedY)
        {
            IdTranslator.Instance.GenerateMissingIds();

            var instance = new ServerWorldInstance(width, height, seedX, seedY);
            
            GenerateWorldInstanceTileMap(instance);

            return instance;
        }

        public void GenerateWorldInstanceTileMap(ServerWorldInstance worldInstance)
        {
            float[] heightMap = GenerateInstanceHeightMap(worldInstance.Width, worldInstance.Height, worldInstance.SeedX, worldInstance.SeedY);

            for (int y = 0; y < worldInstance.Height; y++)
            {
                for (int x = 0; x < worldInstance.Width; x++)
                {
                    float height = Mathf.Clamp(heightMap[x + y * worldInstance.Width], -1.0f, 1.0f);

                    foreach (var biomeTileData in m_WorldGenData.tilesInBiome)
                    {
                        TileData tileData = DataBank.Instance.GetData<TileData>(biomeTileData.tileRef);
                        //byte tileId = tileData.textureSheetIndex;

                        if (biomeTileData.height >= height)
                        {
                            var tile = worldInstance.GetTile(x, y);
                            tile.TileIdCode = IdTranslator.Instance.GetIdCode<TileData>(tileData.Id);

                            tile.Blocking = tileData.isBlocking;

                            tile.PosX = x;
                            tile.PosY = y;

                            // if (FNERandom.GetRandomIntInRange(0, 30) == 0)
                            //     chunk.TileDangerLevels[index] = (byte)FNERandom.GetRandomIntInRange(0, 2);
							
                            break;
                        }
                    }
                }
            }
        }
        
        private float[] GenerateInstanceHeightMap(int width, int height, int seedX, int seedY)
        {
            float[] heightMap = new float[width * height];
            float layerFrequency = m_WorldGenData.layerFrequency;
            float layerWeight = m_WorldGenData.layerWeight;
			
            for (int octave = 0; octave < m_WorldGenData.octaves; octave++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float inputX =  width + x + seedX;
                        float inputY = height + y + seedY;
                        float noise = layerWeight * SimplexNoise.Noise(inputX * layerFrequency, inputY * layerFrequency);
                        heightMap[x + y * width] += noise;
                    }
                }

                layerFrequency *= 2.0f;
                layerWeight *= m_WorldGenData.roughness;
            }

            return heightMap;
        }
    }
}