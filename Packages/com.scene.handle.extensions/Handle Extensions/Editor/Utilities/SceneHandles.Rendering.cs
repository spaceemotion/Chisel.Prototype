﻿using UnityEngine;

namespace UnitySceneExtensions
{
    public static class HandleRendering
    {
        static void InfiniteLine(float x, float y, float z, Axis axis)
        {
            const float kLineSize		= 1000;
            const int	kLineParts		= 10;
            const float kLineMultiply	= kLineSize / kLineParts;
            switch (axis)
            {
                case Axis.X:
                {
                    for (int i = 1; i < kLineParts; i++)
                    {
                        var n0 = (i - 1) * kLineMultiply;
                        var n1 = i * kLineMultiply;
                        SceneHandles.DrawDottedLine(new Vector3(x - n1, y, z), new Vector3(x - n0, y, z), 4.0f);
                        SceneHandles.DrawDottedLine(new Vector3(x + n0, y, z), new Vector3(x + n1, y, z), 4.0f);
                    }
                    break;
                }
                case Axis.Y:
                {
                    for (int i = 1; i < kLineParts; i++)
                    {
                        var n0 = (i - 1) * kLineMultiply;
                        var n1 = i * kLineMultiply;
                        SceneHandles.DrawDottedLine(new Vector3(x, y - n1, z), new Vector3(x, y - n0, z), 4.0f);
                        SceneHandles.DrawDottedLine(new Vector3(x, y + n0, z), new Vector3(x, y + n1, z), 4.0f);
                    }
                    break;
                }
                case Axis.Z:
                {
                    for (int i = 1; i < kLineParts; i++)
                    {
                        var n0 = (i - 1) * kLineMultiply;
                        var n1 = i * kLineMultiply;
                        SceneHandles.DrawDottedLine(new Vector3(x, y, z - n1), new Vector3(x, y, z - n0), 4.0f);
                        SceneHandles.DrawDottedLine(new Vector3(x, y, z + n0), new Vector3(x, y, z + n1), 4.0f);
                    }
                    break;
                }
            }
        }

        public static void RenderCrossXZ(Extents3D extents, float y, Vector3 handleOrigin, SnapResult3D snapResult)
        {
            if ((snapResult & SnapResult3D.MinX) != 0) InfiniteLine(extents.min.x, y, handleOrigin.z, Axis.Z);
            if ((snapResult & SnapResult3D.MaxX) != 0) InfiniteLine(extents.max.x, y, handleOrigin.z, Axis.Z);

            if ((snapResult & SnapResult3D.MinZ) != 0) InfiniteLine(handleOrigin.x, y, extents.min.z, Axis.X);
            if ((snapResult & SnapResult3D.MaxZ) != 0) InfiniteLine(handleOrigin.x, y, extents.max.z, Axis.X);
            
            if ((snapResult & SnapResult3D.PivotX) != 0) InfiniteLine(handleOrigin.x, y, handleOrigin.z, Axis.Z);
            if ((snapResult & SnapResult3D.PivotZ) != 0) InfiniteLine(handleOrigin.x, y, handleOrigin.z, Axis.X);
        }

        public static void RenderSquareXZ(Matrix4x4 transformation, Vector3 start, Vector3 end)
        {
            var right		= Vector3.right;
            var forward		= Vector3.forward;

            var delta		= (end - start);
            var width		= Vector3.Dot(right,   delta) * (Vector3)right;
            var length		= Vector3.Dot(forward, delta) * (Vector3)forward;

            var v0 = start;
            var v1 = start + width;
            var v2 = start + width + length;
            var v3 = start + length;

            using (new SceneHandles.DrawingScope(transformation))
            {
                SceneHandles.DrawDottedLine(v0, v1, 1.0f);
                SceneHandles.DrawDottedLine(v1, v2, 1.0f);
                SceneHandles.DrawDottedLine(v2, v3, 1.0f);
                SceneHandles.DrawDottedLine(v3, v0, 1.0f);
            }
        }

        public static void RenderSquareXZ(Extents3D extents, float y)
        {
            var v0 = new Vector3(extents.min.x, y, extents.min.z);
            var v1 = new Vector3(extents.min.x, y, extents.max.z);
            var v2 = new Vector3(extents.max.x, y, extents.max.z);
            var v3 = new Vector3(extents.max.x, y, extents.min.z);
            SceneHandles.DrawDottedLine(v0, v1, 1.0f);
            SceneHandles.DrawDottedLine(v1, v2, 1.0f);
            SceneHandles.DrawDottedLine(v2, v3, 1.0f);
            SceneHandles.DrawDottedLine(v3, v0, 1.0f);
        }

        public static void RenderSquareXZ(Matrix4x4 transformation, Bounds bounds)
        {
            using (new SceneHandles.DrawingScope(transformation))
            {
                var min = bounds.min;
                var max = bounds.max;
                float minX = min.x, minY = min.y, minZ = min.z;
                float maxX = max.x,               maxZ = max.z;
                var v0 = new Vector3(minX, minY, minZ);
                var v1 = new Vector3(minX, minY, maxZ);
                var v2 = new Vector3(maxX, minY, maxZ);
                var v3 = new Vector3(maxX, minY, minZ);
                SceneHandles.DrawDottedLine(v0, v1, 1.0f);
                SceneHandles.DrawDottedLine(v1, v2, 1.0f);
                SceneHandles.DrawDottedLine(v2, v3, 1.0f);
                SceneHandles.DrawDottedLine(v3, v0, 1.0f);
            }
        }

        public static void RenderBox(Extents3D extents)
        {
            RenderSquareXZ(extents, extents.min.y);
            if (extents.min.y != extents.max.y)
                RenderSquareXZ(extents, extents.max.y);
        }
        
        static readonly Vector3[] boxVertices = new Vector3[8]
        {
            new Vector3( -1, -1, -1), // 0
            new Vector3( -1, +1, -1), // 1
            new Vector3( +1, +1, -1), // 2
            new Vector3( +1, -1, -1), // 3

            new Vector3( -1, -1, +1), // 4  
            new Vector3( -1, +1, +1), // 5
            new Vector3( +1, +1, +1), // 6
            new Vector3( +1, -1, +1)  // 7
        };
        
        
        public static void RenderBox(Matrix4x4 transformation, Bounds bounds)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            using (new SceneHandles.DrawingScope(transformation * Matrix4x4.TRS(bounds.center, Quaternion.identity, bounds.extents)))
            {
                SceneHandles.DrawDottedLine(boxVertices[0], boxVertices[1], 1.0f);
                SceneHandles.DrawDottedLine(boxVertices[1], boxVertices[2], 1.0f);
                SceneHandles.DrawDottedLine(boxVertices[2], boxVertices[3], 1.0f);
                SceneHandles.DrawDottedLine(boxVertices[3], boxVertices[0], 1.0f);
                
                SceneHandles.DrawDottedLine(boxVertices[4], boxVertices[5], 1.0f);
                SceneHandles.DrawDottedLine(boxVertices[5], boxVertices[6], 1.0f);
                SceneHandles.DrawDottedLine(boxVertices[6], boxVertices[7], 1.0f);
                SceneHandles.DrawDottedLine(boxVertices[7], boxVertices[4], 1.0f);
                
                SceneHandles.DrawDottedLine(boxVertices[0], boxVertices[4], 1.0f);
                SceneHandles.DrawDottedLine(boxVertices[1], boxVertices[5], 1.0f);
                SceneHandles.DrawDottedLine(boxVertices[2], boxVertices[6], 1.0f);
                SceneHandles.DrawDottedLine(boxVertices[3], boxVertices[7], 1.0f);			
            }
        }
        
        
        public static void RenderShape(Matrix4x4 transformation, Curve2D shape, float height)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            
            if (shape == null ||
                shape.controlPoints == null ||
                shape.controlPoints.Length == 1)
                return;

            using (new SceneHandles.DrawingScope(transformation))
            {
                for (int j = shape.controlPoints.Length - 1, i = 0; i < shape.controlPoints.Length; j = i, i++)
                {
                    var controlPointJ = shape.controlPoints[j].position;
                    var controlPointI = shape.controlPoints[i].position;
                    var pointJ0 = new Vector3(controlPointJ.x, 0, controlPointJ.y);
                    var pointI0 = new Vector3(controlPointI.x, 0, controlPointI.y);
                    var pointJ1 = new Vector3(controlPointJ.x, height, controlPointJ.y);
                    var pointI1 = new Vector3(controlPointI.x, height, controlPointI.y);

                    SceneHandles.DrawDottedLine(pointJ0, pointI0, 1.0f);
                    SceneHandles.DrawDottedLine(pointJ1, pointI1, 1.0f);
                    SceneHandles.DrawDottedLine(pointI0, pointI1, 1.0f);
                }
            }
        }
        
        static Vector3[]	cylinderVertices	 = null;
        static int			prevCylinderSegments = 0;

        public static void RenderCylinder(Matrix4x4 transformation, Bounds bounds, int segments)
        {
            if (Event.current.type != EventType.Repaint ||
                segments < 3)
                return;
            
            if (cylinderVertices == null ||
                cylinderVertices.Length < segments * 2)
                cylinderVertices = new Vector3[segments * 2];

            RenderSquareXZ(transformation, bounds);

            if (prevCylinderSegments != segments)
            {
                prevCylinderSegments = segments;
                float angleOffset = ((segments&1) == 1) ? 0.0f : ((360.0f / segments) * 0.5f);

                var xVector = Vector3.right; 
                var zVector = Vector3.forward;
                for (int v = 0; v < segments; v++)
                {
                    var r = (((v * 360.0f) / (float)segments) + angleOffset) * Mathf.Deg2Rad;
                    var s = Mathf.Sin(r);
                    var c = Mathf.Cos(r);
                    var bottomVertex = (xVector * c) + (zVector * s);
                    var topVertex	 = bottomVertex;
                    bottomVertex.y -= 1.0f;
                    topVertex.y    += 1.0f;
                    cylinderVertices[v         ] = bottomVertex;
                    cylinderVertices[v+segments] = topVertex;
                }
            }

            using (new SceneHandles.DrawingScope(transformation * Matrix4x4.TRS(bounds.center, Quaternion.identity, bounds.extents)))
            {
                for (int n0 = segments-1, n1 = 0; n1 < segments; n0 = n1, n1++)
                {
                    SceneHandles.DrawDottedLine(cylinderVertices[           n0], cylinderVertices[           n1], 1.0f);
                    SceneHandles.DrawDottedLine(cylinderVertices[segments + n0], cylinderVertices[segments + n1], 1.0f);
                    SceneHandles.DrawDottedLine(cylinderVertices[           n1], cylinderVertices[segments + n1], 1.0f);
                }
            }
        }

        public static void RenderSnapping3D(Grid grid, Extents3D extents, SnapResult3D snapResult)
        {
            RenderSnapping3D(grid, extents, extents.min, snapResult, true);
        }

        public static void RenderSnapping3D(Grid grid, Extents3D extents, Vector3 pivotPosition, SnapResult3D snapResult, bool ignorePivot = false)
        {
            if (grid == null ||
                Event.current.type != EventType.Repaint)
                return;
            using (new SceneHandles.DrawingScope(grid.GridToWorldSpace))
            { 
                if (extents.min.x == extents.max.x && (snapResult & SnapResult3D.MinX) != 0) snapResult &= ~SnapResult3D.MaxX;
                if (extents.min.y == extents.max.y && (snapResult & SnapResult3D.MinY) != 0) snapResult &= ~SnapResult3D.MaxY;
                if (extents.min.z == extents.max.z && (snapResult & SnapResult3D.MinZ) != 0) snapResult &= ~SnapResult3D.MaxZ;
            
                if (ignorePivot)
                {
                    snapResult &= ~SnapResult3D.PivotX;
                    snapResult &= ~SnapResult3D.PivotY;
                    snapResult &= ~SnapResult3D.PivotZ;
                } else
                { 
                    if (extents.min.x == pivotPosition.x && (snapResult & SnapResult3D.MinX) != 0) snapResult &= ~SnapResult3D.PivotX;
                    if (extents.min.y == pivotPosition.y && (snapResult & SnapResult3D.MinY) != 0) snapResult &= ~SnapResult3D.PivotY;
                    if (extents.min.z == pivotPosition.z && (snapResult & SnapResult3D.MinZ) != 0) snapResult &= ~SnapResult3D.PivotZ;
            
                    if (extents.max.x == pivotPosition.x && (snapResult & SnapResult3D.MaxX) != 0) snapResult &= ~SnapResult3D.PivotX;
                    if (extents.max.y == pivotPosition.y && (snapResult & SnapResult3D.MaxY) != 0) snapResult &= ~SnapResult3D.PivotY;
                    if (extents.max.z == pivotPosition.z && (snapResult & SnapResult3D.MaxZ) != 0) snapResult &= ~SnapResult3D.PivotZ;
                }

                float y = 0;
                if		(extents.min.y > 0) { y = extents.min.y; }
                else if (extents.max.y < 0) { y = extents.max.y; }
                RenderCrossXZ(extents, y, pivotPosition, snapResult); 
                RenderBox(extents);
            
                var color = SceneHandles.color;
                color.a *= 0.5f;
                SceneHandles.color = color;
            
                if (y != 0)
                {
                    RenderSquareXZ(extents, 0);

                    if ((snapResult & SnapResult3D.MinX) != 0)
                    {
                        var center = new Vector3(extents.min.x, y, pivotPosition.z);
                        if ((snapResult & SnapResult3D.MinZ) != 0) SceneHandles.DrawDottedLine(new Vector3(center.x, 0, extents.min.z), new Vector3(center.x, center.y, extents.min.z), 4.0f);
                        if ((snapResult & SnapResult3D.MaxZ) != 0) SceneHandles.DrawDottedLine(new Vector3(center.x, 0, extents.max.z), new Vector3(center.x, center.y, extents.max.z), 4.0f);
                    }
                    if ((snapResult & SnapResult3D.MinZ) != 0)
                    {
                        var center = new Vector3(pivotPosition.x, y, extents.min.z);
                        if ((snapResult & SnapResult3D.MinX) != 0) SceneHandles.DrawDottedLine(new Vector3(extents.min.x, 0, center.z), new Vector3(extents.min.x, center.y, center.z), 4.0f);
                        if ((snapResult & SnapResult3D.MaxX) != 0) SceneHandles.DrawDottedLine(new Vector3(extents.max.x, 0, center.z), new Vector3(extents.max.x, center.y, center.z), 4.0f);
                    }

                    if ((snapResult & SnapResult3D.MaxX) != 0)
                    {
                        var center = new Vector3(extents.max.x, y, pivotPosition.z);
                        if ((snapResult & SnapResult3D.MinZ) != 0) SceneHandles.DrawDottedLine(new Vector3(center.x, 0, extents.min.z), new Vector3(center.x, center.y, extents.min.z), 4.0f);
                        if ((snapResult & SnapResult3D.MaxZ) != 0) SceneHandles.DrawDottedLine(new Vector3(center.x, 0, extents.max.z), new Vector3(center.x, center.y, extents.max.z), 4.0f);
                    }
                    if ((snapResult & SnapResult3D.MaxZ) != 0)
                    {
                        var center = new Vector3(pivotPosition.x, y, extents.max.z);
                        if ((snapResult & SnapResult3D.MinX) != 0) SceneHandles.DrawDottedLine(new Vector3(extents.min.x, 0, center.z), new Vector3(extents.min.x, center.y, center.z), 4.0f);
                        if ((snapResult & SnapResult3D.MaxX) != 0) SceneHandles.DrawDottedLine(new Vector3(extents.max.x, 0, center.z), new Vector3(extents.max.x, center.y, center.z), 4.0f);
                    }
                }
            }
        }
        


        public static void RenderSnapping1D(Vector3 min, Vector3 max, Vector3 pivot, Vector3 slideDirection, SnapResult1D snapResult, Axis axis)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            using (new SceneHandles.DrawingScope(Matrix4x4.identity))
            { 
                if (max   == min && (snapResult & SnapResult1D.Min) != 0) snapResult &= ~SnapResult1D.Max;
                if (pivot == min && (snapResult & SnapResult1D.Min) != 0) snapResult &= ~SnapResult1D.Pivot;
                if (pivot == max && (snapResult & SnapResult1D.Max) != 0) snapResult &= ~SnapResult1D.Pivot;
            
                var grid			= Grid.ActiveGrid;
                var dotX			= Mathf.Abs(Vector3.Dot(grid.Forward.normalized, slideDirection));
                var dotZ			= Mathf.Abs(Vector3.Dot(grid.Right.normalized,   slideDirection));
                var dotY			= Mathf.Abs(Vector3.Dot(grid.Up.normalized,      slideDirection));
                                     
                if ((dotY - dotX) < 0.00001f && (dotY - dotZ) < 0.00001f &&
                    ((1.0f - dotX) < 0.00001f || (1.0f - dotZ) < 0.00001f))
                {
                    var direction	= (dotX < dotZ) ? grid.Forward : grid.Right;
                    if ((snapResult & SnapResult1D.Pivot) != 0) SceneHandles.DrawDottedLine(pivot + (direction * -1000), pivot + (direction *  1000), 4.0f); 
                    if ((snapResult & SnapResult1D.Min  ) != 0) SceneHandles.DrawDottedLine(min + (direction * -1000), min + (direction *  1000), 4.0f); 
                    if ((snapResult & SnapResult1D.Max  ) != 0) SceneHandles.DrawDottedLine(max + (direction * -1000), max + (direction *  1000), 4.0f); 
                }
            }
        }
    }
}
