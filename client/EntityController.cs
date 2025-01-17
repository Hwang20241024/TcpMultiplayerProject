using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour
{
    private Vector3 targetPosition; // �����κ��� ���� ��ġ
    private float lerpSpeed = 10f;
    
    private float moveSpeed = 10f;

    private Vector3 velocity = Vector3.zero; // �ӵ� ����
    private float smoothTime = 0.2f; // ���� �ð�

    private Vector3 currentVelocity = Vector3.zero; // ���� �ӵ�
    private float maxSpeed = 10f; // �ִ� �ӵ�
    private float acceleration = 20f; // ���ӵ�
    private float deceleration = 30f; // ���ӵ�



    // Update is called once per frame
    void Update()
    {
        // �ε巴�� �̵�
        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);

        // �ӵ� ��� ���� (MoveTowards ���)
        //transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // ���� ���� (SmoothDamp ���)
        //transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // �ӵ� ��� ���� + ���� ����
        //float distance = Vector3.Distance(transform.position, targetPosition);

        //// ��ǥ ������ �ſ� ����� ��� �̵� �ߴ�
        //if (distance < 0.01f)
        //{
        //    currentVelocity = Vector3.zero; // �ӵ� �ʱ�ȭ
        //    Debug.Log("Reached target position.");
        //    return;
        //}

        //// ����/���� ���
        //float speed = Mathf.Min(maxSpeed, currentVelocity.magnitude + acceleration * Time.deltaTime);
        //if (distance < 1f) // Ÿ�� ��ó���� ����
        //{
        //    speed = Mathf.Max(0f, currentVelocity.magnitude - deceleration * Time.deltaTime);
        //}

        //// �ӵ��� ���� �ӵ� ���Ϳ� �ݿ�
        //currentVelocity = (targetPosition - transform.position).normalized * speed;

        //// �̵�
        //transform.position += currentVelocity * Time.deltaTime;
    }

    public void UpdateTargetPosition(float x, float y)
    {
        targetPosition = new Vector3(x, y, 0f); // �����κ��� ���� ��ġ�� ����
    }
}
