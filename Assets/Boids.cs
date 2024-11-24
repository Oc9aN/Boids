using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IBoidRule
{
    public Vector3 Movement(GameObject boids, List<GameObject> neighbors);
}

public class Boids : MonoBehaviour
{
    private BoidsManager manager;
    private Vector3 velocity;

    private List<IBoidRule> rules;

    public void Init(BoidsManager manager)
    {
        this.manager = manager;
        velocity = transform.forward * manager.movementSpeed;

        rules = new List<IBoidRule>
        {
            new WeightedRule(new CohesionMovement(), manager.cohesionForce),
            new WeightedRule(new AlignmentMovement(), manager.alignmentForce),
            new WeightedRule(new SeparationMovement(), manager.separationForce),
            new WeightedRule(new LimitMovement(manager.spawnRadius), manager.limitForce)
        };
    }

    void Update()
    {
        velocity += UpdateMovement();
        UpdatePosition(velocity);
    }

    Vector3 UpdateMovement()
    {
        List<GameObject> neighbors = manager.GetNeighbors(gameObject);

        Vector3 newVelocity = rules
                .Select(rule => rule.Movement(gameObject, neighbors))
                .Aggregate(Vector3.zero, (acc, v) => acc + v);

        if (newVelocity.magnitude > manager.movementSpeed)
            newVelocity = newVelocity.normalized * manager.movementSpeed;

        return newVelocity;
    }

    void UpdatePosition(Vector3 velocity)
    {
        velocity.Normalize();

        transform.position += velocity * manager.movementSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(velocity);
    }
}

public class WeightedRule : IBoidRule
{
    private IBoidRule rule;
    private float weight;

    public WeightedRule(IBoidRule rule, float weight)
    {
        this.rule = rule;
        this.weight = weight;
    }

    public Vector3 Movement(GameObject boids, List<GameObject> neighbors)
    {
        return rule.Movement(boids, neighbors) * weight;
    }
}

// 응집
public class CohesionMovement : IBoidRule
{
    public Vector3 Movement(GameObject boids, List<GameObject> neighbors)
    {
        Vector3 velocity = Vector3.zero;

        if (neighbors.Count > 0)
        {
            // 이웃 객체의 평균 위치를 구함
            foreach (var neighbor in neighbors)
            {
                velocity += neighbor.transform.position - boids.transform.position;
            }
            velocity /= neighbors.Count;
            velocity.Normalize();
        }
        return velocity;
    }
}

// 정렬
public class AlignmentMovement : IBoidRule
{
    public Vector3 Movement(GameObject boids, List<GameObject> neighbors)
    {
        Vector3 velocity = Vector3.zero;

        if (neighbors.Count > 0)
        {
            // 이웃의 진행 방향의 평균을 구함
            foreach (var neighbor in neighbors)
            {
                velocity += neighbor.transform.forward;
            }
            velocity /= neighbors.Count;
            velocity.Normalize();
        }
        return velocity;
    }
}

// 분리
public class SeparationMovement : IBoidRule
{
    public Vector3 Movement(GameObject boids, List<GameObject> neighbors)
    {
        Vector3 velocity = Vector3.zero;

        if (neighbors.Count > 0)
        {
            // 이웃이 나를 바라보는 방향의 평균을 구함
            foreach (var neighbor in neighbors)
            {
                velocity += (boids.transform.position - neighbor.transform.position);
            }
            velocity /= neighbors.Count;
            velocity.Normalize();
        }
        return velocity;
    }
}

// 이동 범위 제한
public class LimitMovement : IBoidRule
{
    float limitRadius;
    public LimitMovement(float limitRadius)
    {
        this.limitRadius = limitRadius;
    }
    public Vector3 Movement(GameObject boids, List<GameObject> neighbors)
    {
        float distance = boids.transform.position.magnitude;

        if (distance > limitRadius)
        {
            Vector3 returnForce = -boids.transform.position.normalized * (distance - limitRadius);
            return returnForce;
        }
        return Vector3.zero;
    }
}
