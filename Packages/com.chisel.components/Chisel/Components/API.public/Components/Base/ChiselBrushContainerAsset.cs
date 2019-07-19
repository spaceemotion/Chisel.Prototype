﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Chisel.Core;

namespace Chisel.Components
{
    // This class holds the brushMeshes that have been generated by the generators, 
    // and holds the instances (not serialized) on the CSG side for these brushMeshes.
    // This class could be shared between multiple generators that are guaranteed to 
    // produce the same output. (if for instance, they're copies from each other)
    [Serializable]
    public sealed class ChiselBrushContainerAsset : ScriptableObject
    {
        internal void OnEnable()	{ ChiselBrushContainerAssetManager.Register(this); }
        internal void OnDisable()	{ ChiselBrushContainerAssetManager.Unregister(this); }
        internal void OnValidate()	{ ChiselBrushContainerAssetManager.NotifyContentsModified(this); }

        // returns false if it was already dirty
        public new bool SetDirty()	{ return ChiselBrushContainerAssetManager.SetDirty(this); }
        public bool Dirty			{ get { return ChiselBrushContainerAssetManager.IsDirty(this); } }

        [SerializeField] private ChiselBrushContainer brushContainer;
        [NonSerialized] private BrushMeshInstance[] instances;
        
        public bool					Empty			{ get { return brushContainer.Empty; } }
        public int					SubMeshCount	{ get { return brushContainer.Count; } }
        public BrushMesh[]	        BrushMeshes		{ get { return brushContainer.brushMeshes; } }
        public CSGOperationType[]	Operations		{ get { return brushContainer.operations; } }
        public BrushMeshInstance[]	Instances		{ get { if (HasInstances) return instances; return null; } }
        
        public void Generate(IChiselGenerator generator)
        {
            if (!generator.Generate(ref brushContainer))
                brushContainer.Reset();
            CalculatePlanes();
            SetDirty();
            ChiselBrushContainerAssetManager.NotifyContentsModified(this);
        }

        public bool SetSubMeshes(BrushMesh[] brushMeshes)
        {
            if (brushMeshes == null)
            {
                Clear();
                return false;
            }
            this.brushContainer.brushMeshes = brushMeshes;
            this.brushContainer.operations = new CSGOperationType[brushMeshes.Length]; // default is Additive
            OnValidate();
            return true;
        }

        public void Clear() { brushContainer.Reset(); OnValidate(); }
        
        internal bool HasInstances { get { return instances != null && instances.Length > 0 && instances[0].Valid; } }

        internal void CreateInstances()
        {
            DestroyInstances();
            if (Empty) return;

            if (instances == null ||
                instances.Length != brushContainer.brushMeshes.Length)
                instances = new BrushMeshInstance[brushContainer.brushMeshes.Length];

            var userID = GetInstanceID();
            for (int i = 0; i < instances.Length; i++)
            {
                ref var brushMesh = ref brushContainer.brushMeshes[i];
                if (!brushMesh.Validate(logErrors: true))
                    brushMesh.Clear();
                instances[i] = BrushMeshInstance.Create(brushMesh, userID: userID);
            }
        }

        internal void UpdateInstances()
        {
            if (instances == null) return;						
            if (Empty) { DestroyInstances(); return; }
            if (instances.Length != brushContainer.brushMeshes.Length) { CreateInstances(); return; }

            for (int i = 0; i < instances.Length; i++)
            {
                ref var brushMesh = ref brushContainer.brushMeshes[i];
                if (!brushMesh.Validate(logErrors: true))
                    brushMesh.Clear();
                instances[i].Set(brushMesh);
            }
        }

        internal void DestroyInstances()
        {
            if (instances != null)
            {
                for (int i = 0; i < instances.Length; i++)
                    if (instances[i].Valid)
                        instances[i].Destroy();
            }
            instances = null;
        }

        public void	CalculatePlanes()
        {
            if (brushContainer.brushMeshes == null)
                return;

            for (int i = 0; i < brushContainer.brushMeshes.Length; i++)
            {
                if (brushContainer.brushMeshes[i] == null)
                    throw new NullReferenceException("SubMeshes[" + i + "] is null");
                ref var brushMesh = ref brushContainer.brushMeshes[i];
                brushMesh.CalculatePlanes();
                brushMesh.UpdateHalfEdgePolygonIndices();
            }
        }


        static readonly Vector3 positiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        static readonly Vector3 negativeInfinityVector = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        public Bounds CalculateBounds(Matrix4x4 transformation)
        {
            if (brushContainer.brushMeshes == null)
                return new Bounds();

            var min = positiveInfinityVector;
            var max = negativeInfinityVector;
            
            for (int m = 0; m < brushContainer.brushMeshes.Length; m++)
            {
                var vertices = brushContainer.brushMeshes[m].vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    var point = transformation.MultiplyPoint(vertices[i]);

                    min.x = Mathf.Min(min.x, point.x);
                    min.y = Mathf.Min(min.y, point.y);
                    min.z = Mathf.Min(min.z, point.z);

                    max.x = Mathf.Max(max.x, point.x);
                    max.y = Mathf.Max(max.y, point.y);
                    max.z = Mathf.Max(max.z, point.z);
                }
            }
            return new Bounds { min = min, max = max };
        }

        public void Cut(Plane cutPlane, in ChiselSurface chiselSurface)
        {
            if (brushContainer.brushMeshes == null)
                return;

            for (int i = brushContainer.brushMeshes.Length - 1; i >= 0; i--)
            {
                if (!brushContainer.brushMeshes[i].Cut(cutPlane, in chiselSurface))
                {
                    if (brushContainer.brushMeshes.Length > 1)
                    {
                        var newBrushMeshes = new List<BrushMesh>(brushContainer.brushMeshes);
                        var newOperations = new List<CSGOperationType>(brushContainer.operations);
                        newBrushMeshes.RemoveAt(i);
                        newOperations.RemoveAt(i);
                        brushContainer.brushMeshes = newBrushMeshes.ToArray();
                        brushContainer.operations = newOperations.ToArray();
                    } else
                    {
                        brushContainer.brushMeshes = null;
                        brushContainer.operations = null;
                    }
                    continue;
                }
            }
            if (brushContainer.brushMeshes == null ||
                brushContainer.brushMeshes.Length == 0)
                Clear();
        }
    }
}