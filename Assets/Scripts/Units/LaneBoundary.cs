using UnityEngine;

namespace MobaPrototype
{
    /// <summary>
    /// Builds an invisible physical boundary along both sides of the lane so the
    /// player's CharacterController can't wander off the playable corridor, and
    /// dresses the boundary with a simple tree line so it reads as a natural
    /// edge instead of an invisible wall. Everything here is built at runtime
    /// (same approach as UIManager/HealthBarWorld) so there is nothing new to
    /// hand-edit in the scene file itself.
    /// </summary>
    public class LaneBoundary : MonoBehaviour
    {
        [Header("Playable corridor (world space)")]
        public float laneHalfWidth = 3.5f;
        public float laneStartZ = -23f;
        public float laneEndZ = 23f;
        public float wallHeight = 4f;

        [Header("Tree line dressing")]
        public float treeSpacing = 4f;
        public float treeOffsetFromWall = 1.3f;

        private void Awake()
        {
            BuildSideWall(-laneHalfWidth);
            BuildSideWall(laneHalfWidth);
            BuildEndWall(laneStartZ);
            BuildEndWall(laneEndZ);

            BuildSideTreeLine(-laneHalfWidth - treeOffsetFromWall);
            BuildSideTreeLine(laneHalfWidth + treeOffsetFromWall);
            BuildEndTreeLine(laneStartZ - treeOffsetFromWall);
            BuildEndTreeLine(laneEndZ + treeOffsetFromWall);
        }

        private void BuildSideWall(float x)
        {
            GameObject wall = new GameObject(x < 0 ? "BoundaryWall_Left" : "BoundaryWall_Right");
            wall.transform.SetParent(transform, false);

            float length = laneEndZ - laneStartZ;
            wall.transform.position = new Vector3(x, wallHeight * 0.5f, (laneStartZ + laneEndZ) * 0.5f);

            BoxCollider box = wall.AddComponent<BoxCollider>();
            box.size = new Vector3(0.5f, wallHeight, length);
        }

        private void BuildEndWall(float z)
        {
            GameObject wall = new GameObject(z < 0 ? "BoundaryWall_Start" : "BoundaryWall_End");
            wall.transform.SetParent(transform, false);

            float width = (laneHalfWidth + treeOffsetFromWall) * 2f;
            wall.transform.position = new Vector3(0f, wallHeight * 0.5f, z);

            BoxCollider box = wall.AddComponent<BoxCollider>();
            box.size = new Vector3(width, wallHeight, 0.5f);
        }

        private void BuildSideTreeLine(float x)
        {
            int count = Mathf.FloorToInt((laneEndZ - laneStartZ) / treeSpacing) + 1;
            for (int i = 0; i < count; i++)
            {
                float z = laneStartZ + i * treeSpacing;
                BuildTree(new Vector3(x, 0f, z), $"{(x < 0 ? "Left" : "Right")}_{i}");
            }
        }

        private void BuildEndTreeLine(float z)
        {
            float halfWidth = laneHalfWidth + treeOffsetFromWall;
            int count = Mathf.FloorToInt((halfWidth * 2f) / treeSpacing) + 1;
            for (int i = 0; i < count; i++)
            {
                float x = -halfWidth + i * treeSpacing;
                BuildTree(new Vector3(x, 0f, z), $"{(z < 0 ? "Start" : "End")}_{i}");
            }
        }

        private void BuildTree(Vector3 position, string label)
        {
            GameObject tree = new GameObject($"BoundaryTree_{label}");
            tree.transform.SetParent(transform, false);
            tree.transform.position = position;

            const float trunkHeight = 1.8f;

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0f, trunkHeight * 0.5f, 0f);
            trunk.transform.localScale = new Vector3(0.35f, trunkHeight * 0.5f, 0.35f);
            Destroy(trunk.GetComponent<Collider>());
            trunk.GetComponent<Renderer>().material.color = new Color(0.35f, 0.22f, 0.09f);

            GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaves.name = "Leaves";
            leaves.transform.SetParent(tree.transform, false);
            leaves.transform.localPosition = new Vector3(0f, trunkHeight + 0.7f, 0f);
            leaves.transform.localScale = Vector3.one * 1.2f;
            Destroy(leaves.GetComponent<Collider>());
            leaves.GetComponent<Renderer>().material.color = new Color(0.13f, 0.45f, 0.15f);
        }
    }
}
