using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageUp : Item
{
    public override void DestroyAfterTime(){
        //Invoke("DestroyObject", 4.0f);
    }

    public override void RunItem()
    {
        GameObject tankObject = GameObject.Find("Tank");
        TankShooting tankShooting = tankObject.GetComponent<TankShooting>();
        tankShooting.m_MaxDamage *= 1.5f;

        DestroyObject();
    }

    public void DestroyObject(){
        Destroy(gameObject);
    }

    public void OnCollisionEnter(Collision other) {
        if(other.gameObject.layer == LayerMask.NameToLayer("Players")){
            Debug.Log("Player");
            RunItem();
        }
    }
}
