
using System;
using GlmSharp;
using panpan.Scene;
using panpan.Util;
using SDL3;

namespace panpan.Collision
{
    public class CollisionManager: Component
    {
        public enum ManagerType
        {
            SPACIAL_HASH,
            QUADTREE,
            INVALID
        }
        private readonly SpatialHash spatialHash;

        private ManagerType type;
        private vec2 worldSize;
        private int cellSize;

        public bool ShowColliderDebug;

        public CollisionManager(ManagerType type, int cellSize, vec2 worldSize)
        {
            this.type = type;
            this.cellSize = cellSize;
            this.worldSize = worldSize;
            if (type != ManagerType.SPACIAL_HASH)
            {
                throw new NotSupportedException($"{nameof(CollisionManager)} currently only supports {ManagerType.SPACIAL_HASH}.");
            }

            spatialHash = new SpatialHash(cellSize, worldSize);
        }

        public void AddCollider(Collider col)
        {
            spatialHash.AddCollider(col, col.Parent.GetType());
        }

        public bool IntersectsWith(Collider self, Type other, vec2? pos = null)
        {
            return spatialHash.IntersectsWith(self, other, pos);
        }

        public bool IntersectsPosition(vec2 pos, Type other)
        {
            return spatialHash.IntersectsPosition(pos, other);
        }

        //--------------- Spatial Hash
        //----------------------------
        private class SpatialHash
        {
            // Maps cell coordinates (x, y) to sets of colliders in that cell
            private Dictionary<(int, int), HashSet<Collider>> cells;
            // Maps collider to the cells it's currently in (for efficient removal/updates)
            private Dictionary<Collider, HashSet<(int, int)>> colliderCells;
            // Maps type to colliders of that type (for type-filtered queries)
            private Dictionary<Type, HashSet<Collider>> colliderTypes;
            private int cellSize;
            private vec2 worldSize;

            public SpatialHash(int cellSize, vec2 worldSize)
            {
                this.cellSize = cellSize;
                this.worldSize = worldSize;
                cells = new Dictionary<(int, int), HashSet<Collider>>();
                colliderCells = new Dictionary<Collider, HashSet<(int, int)>>();
                colliderTypes = new Dictionary<Type, HashSet<Collider>>();
            }

            public void AddCollider(Collider col, Type parentType)
            {
                col.Update();
                
                RemoveCollider(col);
                
                Rect bounds = GetColliderBounds(col);
                
                // Calculate which cells this collider overlaps
                int minCellX = (int)Math.Floor(bounds.X / (float)cellSize);
                int maxCellX = (int)Math.Floor((bounds.X + bounds.Width) / (float)cellSize);
                int minCellY = (int)Math.Floor(bounds.Y / (float)cellSize);
                int maxCellY = (int)Math.Floor((bounds.Y + bounds.Height) / (float)cellSize);
                
                HashSet<(int, int)> colliderCellSet = new HashSet<(int, int)>();
                
                // Add collider to all overlapping cells
                for (int x = minCellX; x <= maxCellX; x++)
                {
                    for (int y = minCellY; y <= maxCellY; y++)
                    {
                        var cellKey = (x, y);
                        
                        if (!cells.ContainsKey(cellKey))
                        {
                            cells[cellKey] = new HashSet<Collider>();
                        }
                        
                        cells[cellKey].Add(col);
                        colliderCellSet.Add(cellKey);
                    }
                }
                
                colliderCells[col] = colliderCellSet;
                
                // Track by type
                if (!colliderTypes.ContainsKey(parentType))
                {
                    colliderTypes[parentType] = new HashSet<Collider>();
                }
                colliderTypes[parentType].Add(col);
            }

            public void RemoveCollider(Collider col)
            {
                if (!colliderCells.TryGetValue(col, out var cellSet))
                {
                    return;
                }
                
                // Remove from all cells
                foreach (var cellKey in cellSet)
                {
                    if (cells.TryGetValue(cellKey, out var cellColliders))
                    {
                        cellColliders.Remove(col);
                        if (cellColliders.Count == 0)
                        {
                            cells.Remove(cellKey);
                        }
                    }
                }
                
                colliderCells.Remove(col);
                
                // Remove from type tracking
                foreach (var typeSet in colliderTypes.Values)
                {
                    typeSet.Remove(col);
                }
            }

            public bool IntersectsWith(Collider self, Type other, vec2? pos = null)
            {
                // Get the bounds of the query collider
                Rect queryBounds = GetColliderBounds(self, pos);
                
                // Calculate which cells to check
                int minCellX = (int)Math.Floor(queryBounds.X / (float)cellSize);
                int maxCellX = (int)Math.Floor((queryBounds.X + queryBounds.Width) / (float)cellSize);
                int minCellY = (int)Math.Floor(queryBounds.Y / (float)cellSize);
                int maxCellY = (int)Math.Floor((queryBounds.Y + queryBounds.Height) / (float)cellSize);
                
                // Track which colliders we've already checked (to avoid duplicates)
                HashSet<Collider> checkedColliders = new HashSet<Collider>();
                
                // Check only relevant cells
                for (int x = minCellX; x <= maxCellX; x++)
                {
                    for (int y = minCellY; y <= maxCellY; y++)
                    {
                        var cellKey = (x, y);
                        if (!cells.TryGetValue(cellKey, out var cellColliders))
                        {
                            continue;
                        }
                        
                        foreach (var otherCol in cellColliders)
                        {
                            // Check if the collider's parent type is the same as or derived from 'other'
                            Type parentType = otherCol.Parent.GetType();
                            if (other.IsAssignableFrom(parentType) && !checkedColliders.Contains(otherCol))
                            {
                                checkedColliders.Add(otherCol);
                                if (self.Intersects(otherCol, pos))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                
                return false;
            }

            public bool IntersectsPosition(vec2 pos, Type other)
            {
                // Calculate which cell the position is in
                int cellX = (int)Math.Floor(pos.x / cellSize);
                int cellY = (int)Math.Floor(pos.y / cellSize);
                var cellKey = (cellX, cellY);
                
                // Check only colliders in that cell
                if (!cells.TryGetValue(cellKey, out var cellColliders))
                {
                    return false;
                }
                
                foreach (var col in cellColliders)
                {
                    // Check if the collider's parent type is the same as or derived from 'other'
                    Type parentType = col.Parent.GetType();
                    if (other.IsAssignableFrom(parentType) && col.IntersectsPosition(pos))
                    {
                        return true;
                    }
                }
                
                return false;
            }

            private Rect GetColliderBounds(Collider col, vec2? pos = null)
            {
                if (col is BoxCollider boxCollider)
                {
                    if (pos != null)
                    {
                        // Calculate bounds at the query position
                        var bounds = boxCollider.bounds;
                        var parentPos = col.Parent.Position;
                        
                        int currentBaseX = (int)MathF.Floor(parentPos.x + 0.5f);
                        int currentBaseY = (int)MathF.Floor(parentPos.y + 0.5f);
                        int offsetX = bounds.X - currentBaseX;
                        int offsetY = bounds.Y - currentBaseY;
                        
                        return new Rect(
                            (int)MathF.Floor(pos.Value.x + offsetX + 0.5f),
                            (int)MathF.Floor(pos.Value.y + offsetY + 0.5f),
                            bounds.Width + 1,
                            bounds.Height + 1
                        );
                    }
                    return boxCollider.bounds;
                }
                
                // Fallback: create a small bounds around center point
                vec2 center = col.CenterPoint();
                return new Rect((int)center.x - 1, (int)center.y - 1, 2, 2);
            }
        }
    }
}