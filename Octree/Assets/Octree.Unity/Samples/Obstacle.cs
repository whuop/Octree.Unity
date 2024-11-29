using UnityEngine;

namespace Octree.Unity.Samples
{
    public class Obstacle : MonoBehaviour, ISpatialData3D
    {
        [SerializeField] 
        private Collider m_collider;
        [SerializeField] 
        private MeshRenderer m_meshRenderer;

        private Vector3? m_cachedPosition;
        private Bounds? m_cachedBounds;
        private float? m_cachedRadius;

        private bool m_isDirty;
        
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public Vector3 GetLocation()
        {
            
        }

        public Bounds GetBounds()
        {
        }

        public float GetRadius()
        {
        }
    }
}
