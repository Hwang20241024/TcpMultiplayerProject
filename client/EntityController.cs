using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour
{
    private Vector3 targetPosition; // 서버로부터 받은 위치
    private float lerpSpeed = 10f;
    
    private float moveSpeed = 10f;

    private Vector3 velocity = Vector3.zero; // 속도 벡터
    private float smoothTime = 0.2f; // 감속 시간

    private Vector3 currentVelocity = Vector3.zero; // 현재 속도
    private float maxSpeed = 10f; // 최대 속도
    private float acceleration = 20f; // 가속도
    private float deceleration = 30f; // 감속도



    // Update is called once per frame
    void Update()
    {
        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);

        // 속도 기반 보간 (MoveTowards 사용)
        //transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // 감속 보간 (SmoothDamp 사용)
        //transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // 속도 기반 보간 + 가속 적용
        //float distance = Vector3.Distance(transform.position, targetPosition);

        //// 목표 지점에 매우 가까운 경우 이동 중단
        //if (distance < 0.01f)
        //{
        //    currentVelocity = Vector3.zero; // 속도 초기화
        //    Debug.Log("Reached target position.");
        //    return;
        //}

        //// 가속/감속 계산
        //float speed = Mathf.Min(maxSpeed, currentVelocity.magnitude + acceleration * Time.deltaTime);
        //if (distance < 1f) // 타겟 근처에서 감속
        //{
        //    speed = Mathf.Max(0f, currentVelocity.magnitude - deceleration * Time.deltaTime);
        //}

        //// 속도를 현재 속도 벡터에 반영
        //currentVelocity = (targetPosition - transform.position).normalized * speed;

        //// 이동
        //transform.position += currentVelocity * Time.deltaTime;
    }

    public void UpdateTargetPosition(float x, float y)
    {
        targetPosition = new Vector3(x, y, 0f); // 서버로부터 받은 위치를 설정
    }
}
