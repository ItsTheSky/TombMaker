
// This class is used to calculate all the possibilites (and passed through blocks)
// of move of a player (keeping in mind once he start moving in a direction, he can't stop until he hits a wall)
// Do not count deadly paths (paths that will kill the player aka hurting spikes)

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinder
{
       private static readonly int MaxDepth = 25;
         
       public static List<Vector2Int> PassedThroughBlocks(LogicScript logic, Vector2Int pos)
       {
           var visitedBlocks = new List<Vector2Int>();
     
           for (int dir = 1; dir <= 4; dir++)
           {
               var newPos = pos;
               var direction = PlayerScript.DirectionToVector(dir);
               var tempVisitedBlocks = new HashSet<Vector2Int>();
                 
               for(int occurrences = 0; occurrences < MaxDepth; occurrences++)
               {
                   Debug.Log($"occurrences: {occurrences} | dir: {dir} | newPos: {newPos} | direction: {direction}");
                   newPos += direction;
                   if (tempVisitedBlocks.Contains(newPos) || logic.GetBlock(newPos) == null)
                   {
                       break;
                   }
     
                   tempVisitedBlocks.Add(newPos);
     
                   var collidedBlock = logic.GetBlock(newPos)?.GetBlock();
                   if (collidedBlock != null && !collidedBlock.IsDeadly())
                   {
                       var passedThroughBlocks = PassedThroughBlocks(logic, newPos);
                       visitedBlocks.AddRange(passedThroughBlocks);
                   }
               }
             
               visitedBlocks.AddRange(tempVisitedBlocks);
           }
     
           return visitedBlocks.ToList();
       }

    public static List<Vector2Int> CalculatePossibilities(LogicScript logic)
    {
        var spawnLoc = logic.GetSpawnLocation();
        return PassedThroughBlocks(logic, new Vector2Int((int) spawnLoc.x, (int) spawnLoc.y));
    }
    
}