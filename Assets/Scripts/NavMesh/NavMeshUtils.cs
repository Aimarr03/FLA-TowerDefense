using System.Collections.Generic;

namespace UnityEngine.AI
{
    public static class NavMeshUtils
    {
        public static Vector3[] GetPath(Vector3 from, Vector3 to)
        {
            NavMeshPath path = new NavMeshPath();
            if(NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
            {
                //Debug.Log($"Found Path with status: {path.status}");
                return path.corners;
            }
            return new Vector3[0];
        }
        public static Vector3[] GetSmoothPath(Vector3 from, Vector3 to)
        {
            Vector3[] paths = GetPath(from, to);
            return SmoothPath(paths);
        }
        public static Vector3[] SmoothPath(Vector3[] corners, int iterations = 2)
        {
            List<Vector3> points = new List<Vector3>(corners);

            for (int i = 0; i < iterations; i++)
            {
                List<Vector3> newPoints = new List<Vector3>();
                newPoints.Add(points[0]);

                for (int j = 0; j < points.Count - 1; j++)
                {
                    Vector3 p0 = points[j];
                    Vector3 p1 = points[j + 1];

                    Vector3 q = Vector3.Lerp(p0, p1, 0.25f);
                    Vector3 r = Vector3.Lerp(p0, p1, 0.75f);

                    newPoints.Add(q);
                    newPoints.Add(r);
                }

                newPoints.Add(points[^1]);
                points = newPoints;
            }

            return points.ToArray();
        }
        public static bool IsOnNavMesh(Vector3 position, float maxDistance = 1f)
        {
            NavMeshHit hit;
            bool onNavMesh = NavMesh.SamplePosition(position, out hit, maxDistance, NavMesh.AllAreas);
            return onNavMesh;
        }
    }
}

