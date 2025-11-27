using System;
using UnityEngine;
public class DestroyAfterPlayedFiveSec : MonoBehaviour
{
    private void Awake()
    {
        if (this.life <= 0f)
        {
            UnityEngine.Object.Destroy(base.gameObject);
        }
    }
    private void Update()
    {
        this.life -= Time.deltaTime;
        if (this.life <= 0f)
        {
            UnityEngine.Object.Destroy(base.gameObject);
        }
    }
   
    public float life = 5f;
}