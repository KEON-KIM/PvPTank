using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUp : Item
{
    public override void DestroyAfterTime(){
        //Invoke("DestroyObject", 4.0f);
    }

    public override void RunItem()
    {
        /*GameObject tankObject = GameObject.Find("Tank");
        TankHealth tankHealth = tankObject.GetComponent<TankHealth>();
        tankHealth.m_CurrentHealth += 20f;
        if(tankHealth.m_CurrentHealth >= tankHealth.m_StartingHealth){
            tankHealth.m_CurrentHealth = tankHealth.m_StartingHealth;
        }
        tankHealth.SetHealthUI();
        DestroyObject();*/
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
