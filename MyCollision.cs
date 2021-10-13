using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MyCollision : MonoBehaviour
{
    [SerializeField] GameObject hitMarker;
    public Action<GameObject> OnHitCallback;

    Rigidbody rbody;
    private void Update()
    {
        if (rbody == null) rbody = GetComponent<Rigidbody>();
        if (rbody.velocity.magnitude < 100) return;

        ScanForward(transform.position, rbody.velocity * Time.deltaTime);
    }

    public void ScanForward(Vector3 position, Vector3 direction)
    {
        //Raycastによる衝突判定
        RaycastHit hit;
        if (!Physics.Raycast(position, direction, out hit, direction.magnitude)) return;
        Debug.DrawRay(transform.position, direction, Color.red, 10f);

        //ヒットマーカー
        if (hitMarker != null)
        {
            GameObject newHitMarker = Instantiate(hitMarker);
            newHitMarker.transform.position = hit.point;
            Destroy(newHitMarker, 10f);
        }

        //ヒット後の処理
        OnHitCallback(hit.collider.gameObject);
    }
}
