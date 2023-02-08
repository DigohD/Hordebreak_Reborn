using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Server.Model.World;
using FNZ.Shared.Utils;
using Unity.Mathematics;

namespace FNZ.Server.Utils
{
    public enum FlowFieldType : byte
    {
        General = 0,
        Sound = 1,
        Sight = 2
    }

    public struct FlowFieldGenData
    {
        public float2 SourcePosition;
        public int Range;
        public FlowFieldType FlowFieldType;
    }
    
    public static class FlowFieldUtility
    {
        public static FNEFlowField GenerateFlowField(ServerWorld world, float2 sourcePosition, int range)
        {
            return range <= 0 ? null : new FNEFlowField(world, sourcePosition, range);
        }

        public static void QueueFlowFieldForSpawn( 
            ServerWorld world,
            float2 sourcePosition, 
            int range, 
            FlowFieldType flowFieldType)
        {
            if (range <= 0) return;
            world.QueueFlowField(new FlowFieldGenData
            {
                SourcePosition = sourcePosition,
                Range = range,
                FlowFieldType = flowFieldType
            });
        }

        public static FNEFlowField GenerateFlowFieldAndUpdateEnemies(
            ServerWorld world,
            float2 sourcePosition, 
            int range, 
            FlowFieldType flowFieldType)
        {
            if (range <= 0) return null;
            
            var flowField = new FNEFlowField(world, sourcePosition, range);

            for (var y = flowField.worldStartY; y < (flowField.gridSizeY + flowField.worldStartY); y++)
            {
                for (var x = flowField.worldStartX;
                    x < (flowField.gridSizeX + flowField.worldStartX);
                    x++)
                {
                    var enemiesOnTile = world.GetTileEnemies(new int2(x, y));
                    if (enemiesOnTile == null) continue;

                    foreach (var e in enemiesOnTile)
                    {
                        var ffComp = e.GetComponent<FlowFieldComponentServer>();
                        if (ffComp == null) continue;

                        switch (flowFieldType)
                        {
                            case FlowFieldType.Sound:
                            {
                                e.GetComponent<NPCPlayerAwareComponentServer>()
                                    .SoundAlert(FNERandom.GetRandomIntInRange(300, 600));
                                ffComp.soundFlowField = flowField;
                            } break;

                            case FlowFieldType.Sight:
                            {
                                e.GetComponent<NPCPlayerAwareComponentServer>()
                                    .SeenAlert(FNERandom.GetRandomIntInRange(3, 6));
                                ffComp.sightFlowField = flowField;
                            } break;
                            
                            case FlowFieldType.General:
                            {
                                ffComp.generalFlowField = flowField;
                            } break;
                        }
                    }
                }
            }

            return flowField;
        }
    }
}