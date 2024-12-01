using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Octree.Unity
{
    public class Octree
    {
        private NativeList<Node> m_nodes;
        private int m_nodesIndex = 0;
        
        private int m_maxDataPerNode;
        private int m_minimumNodeSize;
        
        public Octree(int maxDataPerNode, int minimumNodeSize, Bounds bounds)
        {
            m_maxDataPerNode = maxDataPerNode;
            m_minimumNodeSize = minimumNodeSize;

            m_nodes = new NativeList<Node>(Allocator.Persistent);
            AddNode(new Node(bounds));
        }

        public unsafe void AddData<T>(T spatialData) where T : struct, ISpatialData3D
        {
            IntPtr ptr = new IntPtr();
            Marshal.StructureToPtr(spatialData, ptr, false);
            UntypedInstanceData data = new UntypedInstanceData(ptr.ToPointer());
            // Add to root node, and handle from there
            AddData(0, data);
        }

        private void AddData(int nodeIndex, UntypedInstanceData spatialData)
        {
            var parentNode = m_nodes[nodeIndex];
            if (parentNode.ChildCount == 0)
            {
                // Is this the first time we're adding data to this node?
                if (!parentNode.Data.IsCreated)
                    parentNode.Data = new NativeHashSet<UntypedInstanceData>(0, Allocator.Persistent);
                
                //  Should we split, and are we able to split?
                if ((parentNode.Data.Count + 1) >= m_maxDataPerNode && CanSplit())
                {
                    SplitNode();
                    AddDataToChildren(spatialData);
                }
                else
                {
                    parentNode.Data.Add(spatialData);
                }

                return;
            }

            AddDataToChildren(spatialData);
            m_nodes[nodeIndex] = parentNode;
        }

        private void SplitNode()
        {
            
        }

        private void AddNode(Node node)
        {
            m_nodes[m_nodesIndex] = node;
            m_nodesIndex++;
        }
        
        public static bool IsBoxed<T>(T value)
        {
            return 
                (typeof(T).IsInterface || typeof(T) == typeof(object)) &&
                value != null &&
                value.GetType().IsValueType;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Node
    {
        public Bounds Bounds;
        public int ChildCount;
        public int Depth;
        public NativeHashSet<UntypedInstanceData> Data;

        public Node(Bounds bounds, int depth = 0)
        {
            Bounds = bounds;
            Depth = depth;
            ChildCount = 0;
            Data = default;
        }
    }
    
    public unsafe struct UntypedInstanceData : IEquatable<UntypedInstanceData>
    {
        [NativeDisableUnsafePtrRestriction]
        internal void* m_ptr;

        public UntypedInstanceData(void* ptr)
        {
            m_ptr = ptr;
        }

        public T Resolve<T>() where T: unmanaged
        {
            T* resolved = (T*)m_ptr;
            return *resolved;
        }

        public bool IsValid => m_ptr != null;

        [System.Diagnostics.Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void CheckNull()
        {
            if (m_ptr == null)
                throw new System.NullReferenceException("Unable to resolve UntypedInstanceData because its internal data was either zero-sized or not initialized");
        }

        public bool Equals(UntypedInstanceData other)
        {
            return m_ptr == other.m_ptr;
        }

        public override bool Equals(object obj)
        {
            return obj is UntypedInstanceData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return unchecked((int)(long)m_ptr);
        }
    }
}


