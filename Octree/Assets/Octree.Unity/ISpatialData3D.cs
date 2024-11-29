using UnityEngine;

namespace Octree.Unity
{
    public interface ISpatialData3D
    {
        Vector3 GetLocation();
        Bounds GetBounds();
        float GetRadius();
    }
}
