using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public abstract void DestroyAfterTime();
    public abstract void RunItem();
    // Start is called before the first frame update
    void Start()
    {
        DestroyAfterTime();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter(Collision other) {
        if(other.gameObject.layer == LayerMask.NameToLayer("Players")){
            RunItem();
        }
    }
}
