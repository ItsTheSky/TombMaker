
using UnityEngine;

public abstract class HistoryAction
{
    public abstract void Undo(LogicScript logic);
    
    public abstract void Redo(LogicScript logic);
    
    // #########################################################
    
    public abstract class BlockAction : HistoryAction
    {
        
        public Vector2 position;
        public Block block;
        
    }
    
    public class BlockPlaceAction : BlockAction
    {
        
        public override void Undo(LogicScript logic)
        {
            logic.RemoveBlock(position);
        }

        public override void Redo(LogicScript logic)
        {
            logic.PlaceBlock(position, block.blockPrefab, false);
        }
        
    }
    
    public class BlockDeleteAction : BlockAction
    {
        public override void Undo(LogicScript logic)
        {
            logic.PlaceBlock(position, block.blockPrefab, false);
        }

        public override void Redo(LogicScript logic)
        {
            logic.RemoveBlock(position);
        }
        
    }
    
}