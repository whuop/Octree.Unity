#define OCTREE_PROFILE
/*
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace Octree.Unity
{
    public class Octree_Old : MonoBehaviour
    {
        class Node
        {
            private Bounds m_nodeBounds;
            private Node[] m_children;
            private int m_depth;

            private HashSet<ISpatialData3D> m_data;

            public Node(Bounds inBounds, int inDepth = 0)
            {
                m_nodeBounds = inBounds;
                m_depth = inDepth;
            }

            public void AddData(Octree_Old owner, ISpatialData3D spatialData)
            {
                if (m_children == null)
                {
                    //  Is this the first we're adding data to this node?
                    if (m_data == null)
                        m_data = new();
                    
                    //  Should we split AND are we able to split?
                    if ((m_data.Count + 1) >= owner.PreferredMaxDataPerNode && CanSplit(owner))
                    {
                        SplitNode(owner);
                        AddDataToChildren(owner, spatialData);
                    }
                    else
                    {
                        m_data.Add(spatialData);
                    }
                    return;
                }

                AddDataToChildren(owner, spatialData);
            }

            private void SplitNode(Octree_Old owner)
            {
                //  Extents is half size of nodebounds, therefore perfect fit for subdivision.
                Vector3 childSize = m_nodeBounds.extents;
                Vector3 offset = childSize / 2f;
                int newDepth = m_depth + 1;
#if OCTREE_PROFILE
                OctreeProfiler.NodeCount.Value += 8;
                OctreeProfiler.MaxDepth.Value = Mathf.Max(newDepth, OctreeProfiler.MaxDepth.Value);
#endif

                // 8 children, the Oc in Octree.
                m_children = new Node[8]
                {
                    new Node(new Bounds(m_nodeBounds.center + new Vector3(-offset.x, -offset.y,  offset.z), childSize), newDepth),
                    new Node(new Bounds(m_nodeBounds.center + new Vector3( offset.x, -offset.y,  offset.z), childSize), newDepth),
                    new Node(new Bounds(m_nodeBounds.center + new Vector3(-offset.x, -offset.y, -offset.z), childSize), newDepth),
                    new Node(new Bounds(m_nodeBounds.center + new Vector3( offset.x, -offset.y, -offset.z), childSize), newDepth),
                    new Node(new Bounds(m_nodeBounds.center + new Vector3(-offset.x,  offset.y,  offset.z), childSize), newDepth),
                    new Node(new Bounds(m_nodeBounds.center + new Vector3( offset.x,  offset.y,  offset.z), childSize), newDepth),
                    new Node(new Bounds(m_nodeBounds.center + new Vector3(-offset.x,  offset.y, -offset.z), childSize), newDepth),
                    new Node(new Bounds(m_nodeBounds.center + new Vector3( offset.x,  offset.y, -offset.z), childSize), newDepth),
                };

                foreach (var data3D in m_data)
                {
                    AddDataToChildren(owner, data3D);
                }

                m_data = null;
            }

            void AddDataToChildren(Octree_Old owner, ISpatialData3D spatialData)
            {
                foreach (var child in m_children)
                {
                    if (child.Overlaps(spatialData.GetBounds()))
                        child.AddData(owner, spatialData);
                }
            }

            private bool Overlaps(Bounds other)
            {
                return m_nodeBounds.Intersects(other);
            }

            private bool CanSplit(Octree_Old owner)
            {
                return m_nodeBounds.size.x >= owner.MinimumNodeSize &&
                       m_nodeBounds.size.y >= owner.MinimumNodeSize &&
                       m_nodeBounds.size.z >= owner.MinimumNodeSize;
            }

            public void FindDataInBox(Bounds searchBounds, HashSet<ISpatialData3D> foundData, bool exactBounds)
            {
                if (m_children == null)
                {
                    if (m_data == null || m_data.Count == 0)
                        return;

                    //  Optimized check for a root node with no children
                    if (m_depth == 0 && exactBounds)
                    {
                        foreach (var spatialData in m_data)
                        {
                            if (searchBounds.Intersects(spatialData.GetBounds()))
                                foundData.Add(spatialData);
                        }
                    }
                    
                    foundData.UnionWith(m_data);
                    return;
                }

                foreach (var child in m_children)
                {
                    if (child.Overlaps(searchBounds))
                        child.FindDataInBox(searchBounds, foundData, exactBounds);
                }

                //  If we're on the root node filter out spatial data not within search bounds.
                if (m_depth == 0 && exactBounds)
                {
                    foundData.RemoveWhere(spatialData => !searchBounds.Intersects(spatialData.GetBounds()));
                }
            }
            
            public void FindDataInRange(Vector3 searchLocation, float searchRange, HashSet<ISpatialData3D> foundData, bool exactBounds)
            {
                if (m_depth != 0)
                {
                    throw new System.InvalidOperationException(
                        "FindDataInRange cannot be run on anything other than the root node!");
                }

                Bounds searchBounds = new Bounds(searchLocation, searchRange * Vector3.one * 2f);
                
                FindDataInBox(searchBounds, foundData, exactBounds);

                foundData.RemoveWhere(spatialData =>
                {
                    float testRange = searchRange + spatialData.GetRadius();
                    return (searchLocation - spatialData.GetLocation()).sqrMagnitude > (testRange * testRange);
                });
            }
        }

        [field: SerializeField] 
        public int PreferredMaxDataPerNode { get; private set; } = 50;
        [field: SerializeField] 
        public int MinimumNodeSize { get; private set; } = 5;
        
        private Node m_rootNode;
        
        public void PrepareTree(Bounds inBounds)
        {
            m_rootNode = new Node(inBounds);
#if OCTREE_PROFILE
            OctreeProfiler.MaxDepth.Value = 0;
            OctreeProfiler.NodeCount.Value = 1;
#endif
        }

        public void AddData(ISpatialData3D data)
        {
            m_rootNode.AddData(this, data);
        }

        public void AddData(List<ISpatialData3D> data)
        {
            foreach (var data3D in data)
            {
                AddData(data3D);
            }
        }

        public HashSet<ISpatialData3D> FindDataInRange(Vector3 searchLocation, float searchRange, bool exactBounds = true)
        {
#if OCTREE_PROFILE
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
#endif
            
            HashSet<ISpatialData3D> foundData = new();
            m_rootNode.FindDataInRange(searchLocation, searchRange, foundData, exactBounds);
            
#if OCTREE_PROFILE
            stopwatch.Stop();
            OctreeProfiler.LastQueryMS.Sample(stopwatch.ElapsedMilliseconds);
            OctreeProfiler.LastQueryResultCount.Sample(foundData.Count);
#endif
            
            
            return foundData;
        }
    }
    
#if OCTREE_PROFILE
    /*class OctreeProfiler
    {
        public static readonly ProfilerCategory Category = new ProfilerCategory("Octree");
        public static readonly ProfilerCounterValue<int> NodeCount =
            new ProfilerCounterValue<int>(Category, "Node Count", ProfilerMarkerDataUnit.Count);
        public static readonly ProfilerCounterValue<int> MaxDepth =
            new ProfilerCounterValue<int>(Category, "Max Depth", ProfilerMarkerDataUnit.Count);
        public static readonly ProfilerCounter<float> LastQueryMS =
            new ProfilerCounter<float>(Category, "Last Query MS", ProfilerMarkerDataUnit.Undefined);
        public static readonly ProfilerCounter<float> LastQueryResultCount =
            new ProfilerCounter<float>(Category, "Last Query Result", ProfilerMarkerDataUnit.Count);
    }*/
/*#endif
}*/
