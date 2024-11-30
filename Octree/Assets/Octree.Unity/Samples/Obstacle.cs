using System;
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

        private Color m_oldColor;

        private bool IsCachedDataDirty
        {
            get
            {
                if (m_cachedPosition == null || m_cachedBounds == null || m_cachedRadius == null)
                    return true;
                return !Mathf.Approximately((transform.position - m_cachedPosition.Value).sqrMagnitude, 0.0f);
            }
        }

        private void Awake()
        {
            m_oldColor = m_meshRenderer.material.color = Color.red;
        }

        public void AddHighlight()
        {
            m_meshRenderer.material.color = Color.yellow;
        }

        public void RemoveHighlight()
        {
            m_meshRenderer.material.color = m_oldColor;
        }

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
            if (IsCachedDataDirty)
                UpdateCachedData();
            return m_cachedPosition.Value;
        }

        public Bounds GetBounds()
        {
            if (IsCachedDataDirty)
                UpdateCachedData();
            return m_cachedBounds.Value;
        }

        public float GetRadius()
        {
            if (IsCachedDataDirty)
                UpdateCachedData();
            return m_cachedRadius.Value;
        }

        private void UpdateCachedData()
        {
            m_cachedPosition = transform.position;
            m_cachedBounds = m_collider.bounds;
            m_cachedRadius = m_collider.bounds.extents.magnitude;
        }
    }
}
