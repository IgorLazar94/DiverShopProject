using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFOV : MonoBehaviour, IDataPersistence
{
    public float currentViewRadius { get; set; }
    public float defaultViewRadius { get; private set; }
    [Range(0, 360)] public float viewAngle;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();
    public float MeshResolution;
    public MeshFilter viewMeshFilter;
    public int edgeResolveIterations;
    public float edgeDistanceThreshold;
    private Mesh viewMesh;
    private float viewRadiusUpgradeFactor;
    private int currentharpoonLevel;
    private int harpoonTrainingFactor;
    private int defaultharpoonLevel;

    private void OnEnable()
    {
        TrainingZone.OnFOVUpdateParameter += UpgradeViewRadius;
        TrainingZone.OnHarpoonUpdateParameter += UpdateHarpoonLevel;
    }

    private void OnDisable()
    {
        TrainingZone.OnFOVUpdateParameter -= UpgradeViewRadius;
        TrainingZone.OnHarpoonUpdateParameter -= UpdateHarpoonLevel;
    }

    private void Start()
    {
        ConnectGameSettings();
        if (DataPersistenceManager.isNewGame)
        {
            currentharpoonLevel = defaultharpoonLevel;
            defaultViewRadius = GameSettings.Instance.GetDefaultPlayerFOV();
        }
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
        StartCoroutine(FindTargetsWithDelay(0.25f));
    }

    private void ConnectGameSettings()
    {
        defaultharpoonLevel = GameSettings.Instance.GetDefaultHarpoonLevel();
        harpoonTrainingFactor = GameSettings.Instance.GetHarpoonFactor();
        viewRadiusUpgradeFactor = GameSettings.Instance.GetPlayerFOVFactor();
    }

    private void UpdateHarpoonLevel()
    {
        currentharpoonLevel += harpoonTrainingFactor;
        DataPersistenceManager.Instance.SaveGame();
    }

    private void UpgradeViewRadius()
    {
        defaultViewRadius += viewRadiusUpgradeFactor;
        DataPersistenceManager.Instance.SaveGame();
    }

    public void EnableView()
    {
        viewMeshFilter.gameObject.SetActive(true);
    }

    public void DisableView()
    {
        viewMeshFilter.gameObject.SetActive(false);
    }

    private IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTarget();
        }
    }

    private void LateUpdate()
    {
        DrawFieldOfView();
        CheckNonVisibleFishg();
    }

    private void CheckNonVisibleFishg()
    {
        foreach (Fish fish in visibleFishes.ToArray())
        {
            if (fish == null)
            {
                visibleFishes.Remove(fish);
                return;
            }

            if (!IsFishVisible(fish))
            {
                fish.StopRunFromPlayer();
                visibleFishes.Remove(fish);
            }
        }
    }

    private bool IsFishVisible(Fish fish)
    {
        Vector3 dirToFish = (fish.transform.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToFish) < viewAngle / 2)
        {
            float distanceToFish = Vector3.Distance(transform.position, fish.transform.position);
            if (!Physics.Raycast(transform.position, dirToFish, distanceToFish, obstacleMask))
            {
                return true;
            }
        }
        return false;
    }

    private List<Fish> visibleFishes = new List<Fish>();
    private void FindVisibleTarget()
    {
        visibleTargets.Clear();
        Collider[] targetsInFOV = Physics.OverlapSphere(transform.position, currentViewRadius, targetMask);

        for (int i = 0; i < targetsInFOV.Length; i++)
        {
            Transform target = targetsInFOV[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                    if (target.TryGetComponent(out Fish fish) && !fish.isRunFromPlayer)
                    {
                        if (CheckHarpoonFishLevel(fish))
                        {
                            visibleFishes.Add(fish);
                            fish.StartCoroutine(fish.StartRunFromPlayer(transform));
                        }
                        else
                        {
                            fish.ShowBlockSprite();
                        }
                    }
                }
            }
        }
    }

    private bool CheckHarpoonFishLevel(Fish fish)
    {
        if (fish.typeOfFish == TypeOfFish.FishA)
        {
            return true;
        }
        else if (fish.typeOfFish == TypeOfFish.FishB && currentharpoonLevel > 1)
        {
            return true;
        }
        else if (fish.typeOfFish == TypeOfFish.FishC && currentharpoonLevel > 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * MeshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i < stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);
            if (i > 0)
            {
                bool edgeDistanceThresholdExceted = Mathf.Abs(oldViewCast.distance - newViewCast.distance) > edgeDistanceThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDistanceThresholdExceted))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.PointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.PointA);
                    }
                    if (edge.PointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.PointB);
                    }
                }
            }
            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);
            bool edgeDistanceThresholdExceted = Mathf.Abs(minViewCast.distance - newViewCast.distance) > edgeDistanceThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDistanceThresholdExceted)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }
        return new EdgeInfo(minPoint, maxPoint);
    }

    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirForAngle(globalAngle, true);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, currentViewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * currentViewRadius, currentViewRadius, globalAngle);
        }
    }

    public Vector3 DirForAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public void LoadData(GameData gameData)
    {
        currentharpoonLevel = gameData.currentHarpoonParameter;
        defaultViewRadius = gameData.currentFOVRadiusParameter;
    }

    public void SaveData(ref GameData gameData)
    {
        gameData.currentHarpoonParameter = currentharpoonLevel;
        gameData.currentFOVRadiusParameter = defaultViewRadius;
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;
        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            distance = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 PointA;
        public Vector3 PointB;
        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            PointA = _pointA;
            PointB = _pointB;
        }
    }
}
