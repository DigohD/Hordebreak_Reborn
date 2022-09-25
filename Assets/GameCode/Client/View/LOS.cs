using System;
using System.Collections.Generic;
using FNZ.Shared.Model.Entity;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View
{
    [Obsolete("Replaced by LineOfSightSystem.cs in Client/Systems")]
    public class LOS : MonoBehaviour
    {
        private FNEEntity player;

        public static bool[,] PrevSeeing = new bool[51, 51];
        public static bool[,] Seeing = new bool[51, 51];
        public static bool[,] Revealed = new bool[51, 51];
        private bool[,] WasVisited = new bool[51, 51];
        
        private List<GameObject> tiles = new List<GameObject>();

        private GameObject tile;
        
        void Start()
        {
            tile = Resources.Load<GameObject>("Plane");
            for (int i = 0; i < 51; i++)
            {
                for (int j = 0; j < 51; j++)
                {
                    tiles.Add(Instantiate(tile));
                }
            }
        }

        private bool enabled;
        bool anyChanged;
        
        private int2[] revealPrio = new int2[51 * 51];
        
        void Update()
        {
            if (GameClient.LocalPlayerEntity == null)
            {
                return;
            }
            else if (player == null)
            {
                player = GameClient.LocalPlayerEntity;
                prevPlayerTile = new int2((int) player.Position.x, (int) player.Position.y);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.O))
            {
                enabled = !enabled;
                if (!enabled)
                {
                    for (int i = 0; i < 51; i++)
                    {
                        for (int j = 0; j < 51; j++)
                        {
                            var t = tiles[i + j * 51];
                            t.SetActive(false);
                        }
                    }
                }else if (enabled)
                {
                    for (int i = 0; i < 51; i++)
                    {
                        for (int j = 0; j < 51; j++)
                        {
                            var t = tiles[i + j * 51];
                            t.SetActive(true);
                        }
                    }
                    
                    PrevSeeing = new bool[51, 51];
                }
            }

            if (enabled)
            {
                CalculateSeeing();
                UpdateFog();
            }
                
        }

        private List<int2> visited = new List<int2>();
        private List<int2> toVisit = new List<int2>();
        private List<int2> toVisitNext = new List<int2>();
        
        private Stack<int2> visitStack1 = new Stack<int2>(2500);
        private Stack<int2> visitStack2 = new Stack<int2>(2500);
        
        public struct ChangedTile
        {
            public int x;
            public int y;
            public float worldX;
            public float worldY;
            public bool seen;
        }
        
        private Stack<ChangedTile> changedStack = new Stack<ChangedTile>(2500);
        
        public static List<Entity> s_Entities = new List<Entity>();

        private int2 prevPlayerTile = new int2(0, 0);
        
        private void CalculateSeeing()
        {
            PrevSeeing = Seeing;
            Seeing = new bool[51, 51];
            WasVisited = new bool[51, 51];

            visited.Clear();

            var world = GameClient.World;

            int originX = (int) player.Position.x;
            int originY = (int) player.Position.y;

            WasVisited[25, 25] = true;

            int revealPrioCounter = 0;
            for (int i = 0; i < 51; i++)
            {
                for (int j = 0; j < 51; j++)
                {
                    revealPrio[revealPrioCounter] = new int2(i, j);
                    revealPrioCounter++;
                }
            }

            var tomp = revealPrio[0];
            revealPrio[0] = new int2(25, 25);
            revealPrio[25 + 25 * 51] = tomp;
            
            var iterations = 0;
            var seen = 0;

            visitStack1.Clear();
            visitStack2.Clear();

            Stack<int2> toVisit = visitStack1;
            toVisit.Push(new int2(originX, originY));

            Stack<int2> visitNext = visitStack2;
            
            revealPrioCounter = 0;
            while (toVisit.Count > 0)
            {
                while (toVisit.Count > 0)
                {
                    revealPrioCounter++;
                    
                    var tilePos = toVisit.Pop();

                    var seeingIndexX = tilePos.x + 25 - originX;
                    var seeingIndexY = tilePos.y + 25 - originY;

                    if (seeingIndexX < 1 || seeingIndexX > 49)
                        continue;

                    if (seeingIndexY < 1 || seeingIndexY > 49)
                        continue;

                    seen++;
                    
                    //tomp = revealPrio[revealPrioCounter];
                    revealPrio[revealPrioCounter] = new int2(seeingIndexX, seeingIndexY);
                    //revealPrio[seeingIndexX + seeingIndexY * 51] = tomp;
                    
                    Seeing[seeingIndexX, seeingIndexY] = iterations < 35;

                    visited.Add(tilePos);

                    var playerInt2 = new int2(originX, originY);
                    
                    var northDiff = new int2(tilePos.x, tilePos.y + 1) - playerInt2;

                    bool traverseNorth = false;
                    bool traverseEast = false;
                    if (northDiff.x + northDiff.y < 20)
                    {
                        traverseNorth = world.IsTileNorthEdgeSeeThrough(tilePos, iterations);
                        traverseEast = world.IsTileEastEdgeSeeThrough(tilePos, iterations);
                    }
                    
                    var southDiff = new int2(tilePos.x, tilePos.y - 1) - playerInt2;

                    bool traverseSouth = false;
                    bool traverseWest = false;
                    
                    if (southDiff.x + southDiff.y > -20)
                    {
                        if (tilePos.y - 1 >= 0)
                            traverseSouth = world.IsTileSouthEdgeSeeThrough(tilePos, iterations);
                        
                        if (tilePos.x - 1 >= 0)
                            traverseWest = world.IsTileWestEdgeSeeThrough(tilePos, iterations);
                    }
                    
                    if (traverseNorth && !WasVisited[seeingIndexX, seeingIndexY + 1])
                    {
                        WasVisited[seeingIndexX, seeingIndexY + 1] = true;
                        visitNext.Push(new int2(tilePos.x, tilePos.y + 1));
                    }

                    if (traverseEast && !WasVisited[seeingIndexX + 1, seeingIndexY])
                    {
                        WasVisited[seeingIndexX + 1, seeingIndexY] = true;
                        visitNext.Push(new int2(tilePos.x + 1, tilePos.y));
                    }

                    if (traverseSouth && !WasVisited[seeingIndexX, seeingIndexY - 1])
                    {
                        WasVisited[seeingIndexX, seeingIndexY - 1] = true;
                        visitNext.Push(new int2(tilePos.x, tilePos.y - 1));
                    }


                    if (traverseWest && !WasVisited[seeingIndexX - 1, seeingIndexY])
                    {
                        WasVisited[seeingIndexX - 1, seeingIndexY] = true;
                        visitNext.Push(new int2(tilePos.x - 1, tilePos.y));
                    }
                }

                iterations++;
                var tmp = toVisit;
                toVisit = visitNext;
                visitNext = tmp;
            }

            if (originX != prevPlayerTile.x || originY != prevPlayerTile.y)
            {
                int xDiff = originX - prevPlayerTile.x;
                int yDiff = originY - prevPlayerTile.y;

                prevPlayerTile = new int2(originX, originY);
                
                if (math.abs(xDiff) > 1 || math.abs(yDiff) > 1)
                {
                    for (int i = 0; i < 51; i++)
                    {
                        for (int j = 0; j < 51; j++)
                        {
                            Revealed[i, j] = Seeing[i, j];
                        }
                    }
                }
                else
                {
                    if (xDiff >= 0)
                    {
                        for (int i = 1; i < 50; i++)
                        {
                            if(yDiff >= 0)
                                for (int j = 1; j < 50; j++)
                                {
                                    Revealed[i - xDiff, j - yDiff] = Revealed[i, j];
                                    revealPrio[(i - xDiff) + (j - yDiff) * 51] = revealPrio[i + j * 51];
                                }
                            else if(yDiff < 0)
                                for (int j = 49; j > 1; j--)
                                {
                                    Revealed[i - xDiff, j + 1] = Revealed[i, j];
                                    revealPrio[(i - xDiff) + (j + 1) * 51] = revealPrio[i + j * 51];
                                }
                        }
                    }
                    else if (xDiff < 0)
                    {
                        for (int i = 49; i > 1; i--)
                        {
                            if(yDiff >= 0)
                                for (int j = 1; j < 50; j++)
                                {
                                    Revealed[i + 1, j - yDiff] = Revealed[i, j];
                                    revealPrio[(i + 1) + (j - yDiff) * 51] = revealPrio[i + j * 51];
                                }
                            else if(yDiff < 0)
                                for (int j = 49; j > 1; j--)
                                {
                                    Revealed[i + 1, j + 1] = Revealed[i, j];
                                    revealPrio[(i + 1) + (j + 1) * 51] = revealPrio[i + j * 51];
                                }
                        }
                    }
                }
            }
            
            for (int i = 0; i < 51; i++)
            {
                for (int j = 0; j < 51; j++)
                {
                    if (!Seeing[i, j])
                    {
                        Revealed[i, j] = false;
                    }

                    var t = tiles[i + j * 51];
                    t.SetActive(false);
                    t.transform.position = new Vector3(originX - 25 + i + 0.5f, 1.05f, originY - 25 + j + 0.5f);
                    if (!Revealed[i, j])
                    {
                        t.SetActive(true);
                    }
                }
            }

            Debug.LogWarning("seen: " + visited.Count);
        }

        void UpdateFog()
        {
            int find = 0;
            for (int i = 0; i < 51 * 51; i++)
            {
                var index = revealPrio[i];
                if (Seeing[index.x, index.y] != Revealed[index.x, index.y])
                {
                    Revealed[index.x, index.y] = Seeing[index.x, index.y];
                    find++;
                    if (find > 10)
                    {
                        return;
                    }
                }
            }
            
            for (int i = 0; i < 51; i++)
            {
                for (int j = 0; j < 51; j++)
                {
                    if (Seeing[i, j] != Revealed[i, j])
                    {
                        find++;
                        Revealed[i, j] = Seeing[i, j];     
                        if (find > 10)
                        {
                            return;
                        }
                    }
                }
            }
        }
        
    }
}