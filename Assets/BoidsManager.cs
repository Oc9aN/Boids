using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidsManager : MonoBehaviour
{
    public List<GameObject> boids = new List<GameObject>();
    public GameObject prefab;

    public int number;

    [Header("생성 반경")]
    public float spawnRadius;
    [Header("이탈 복귀 가중치")]
    public float limitForce;
    [Header("이동 속도")]
    public float movementSpeed;
    [Header("이웃으로 체크되는 거리")]
    public float neighborDistance;
    [Header("최대 이웃 수")]
    public float maxNeighbers;
    [Header("응집 가중치")]
    public float cohesionForce;
    [Header("정렬 가중치")]
    public float alignmentForce;
    [Header("분리 가중치")]
    public float separationForce;

    private void Awake()
    {
        for (int i = 0; i < number; i++)
        {
            GameObject boid = Instantiate(prefab, this.transform.position + Random.insideUnitSphere * spawnRadius, Random.rotation);
            boid.GetComponent<Boids>().Init(this);
            boids.Add(boid);
        }
    }

    public List<GameObject> GetNeighbors(GameObject boid)
    {
        List<GameObject> neighbors = new List<GameObject>();
        LayerMask layerMask = LayerMask.GetMask("Boids");
        Collider[] hits = Physics.OverlapSphere(boid.transform.position, neighborDistance, layerMask);

        foreach (var hit in hits)
        {
            if (hit.gameObject == boid) continue;       // 자기 자신은 스킵
            if (neighbors.Count >= maxNeighbers) break; // 이웃 수 체크
            neighbors.Add(hit.gameObject);
            Debug.DrawLine(boid.transform.position, hit.transform.position, Color.green);
        }
        return neighbors;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(Vector3.zero, spawnRadius);
    }
}
