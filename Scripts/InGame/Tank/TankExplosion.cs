using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PvpTank
{
    public class TankExplosion : MonoBehaviour
    {
        private ParticleSystem particle;
        // Start is called before the first frame update
        void Start()
        {
            particle = this.transform.GetComponent<ParticleSystem>();
            ParticleEvent(); 
        }

        // Update is called once per frame
        
        private void ParticleEvent()
        {
            ParticleSystem.MainModule mainModule = particle.main;

            particle.Play();
            Destroy(this.gameObject, mainModule.duration); 
        }
    }
}
