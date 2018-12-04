﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShockwaveBehavior : MonoBehaviour
{
    public SphereCollider capsuleCollider;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "FloorCube")
        {
            Animator animator = other.transform.parent.GetComponent<Animator>();
            if (animator != null)
                animator.SetTrigger("Pulse");
            else
                Debug.LogError("Could not find animator on GateCube");
        }
        if (other.tag == "Enemy")
        {
            Enemy e = other.GetComponent<Enemy>();
            if (e != null)
                e.Die();
            else
                Debug.LogError("Could not find Enemy component on Enemy GO");
        }
    }

    public IEnumerator Shockwave(float radius)
    {
        float ogRadius = capsuleCollider.radius;
        while (capsuleCollider.radius < radius)
        {
            capsuleCollider.radius += Time.deltaTime * 20;
            yield return null;
        }

        capsuleCollider.radius = ogRadius;
        gameObject.SetActive(false);
    }

}
