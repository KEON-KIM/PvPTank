using UnityEngine;

namespace PvpTank
{
    public partial class Player : MonoBehaviour
    {
        public int m_PlayerNumber = 1;
        public float m_Speed = 12f;
        public float m_TurnSpeed = 180f;
        public AudioSource m_EffectAudio;
        public AudioClip m_EngineIdling;
        public AudioClip m_EngineDriving;  
        public AudioClip m_Explosion;
        
        public float m_MinPitch = 0.2f;


        private string m_MovementAxisName;
        private string m_TurnAxisName;
        private float m_MovementInputValue;
        private float m_TurnInputValue;
        private float m_OriginalPitch = 0.8f;
        private float m_OriginalVolume = 0.5f;

        /*private void Update()
        {
            // Store the player's input and make sure the audio for the engine is playing.
            *//*m_MovementInputValue = Input.GetAxis (m_MovementAxisName);
            m_TurnInputValue = Input.GetAxis (m_TurnAxisName);*//*
        }
    */
        public void EngineIdleAudio()
        {
            // ... change the clip to driving and play.
            if (m_EffectAudio.clip == m_EngineIdling) return;
            m_EffectAudio.clip = m_EngineIdling;
            m_EffectAudio.pitch = m_OriginalPitch; //Mathf.SmoothDamp(m_rigidBody.velocity.x) Mathf.Abs(m_Rigidbody.velocity.x) * Mathf.Abs(m_Rigidbody.velocity.z);
            m_EffectAudio.volume = m_OriginalVolume;
            m_EffectAudio.Play();

        }
        public void ExplosionAuidio()
        {
            m_gameObject.SetActive(false);
            if(m_EffectAudio.clip != m_Explosion)
            {
                m_EffectAudio.clip = m_Explosion;
                m_EffectAudio.pitch = m_OriginalPitch;
                m_EffectAudio.volume = m_OriginalVolume;
                m_EffectAudio.Play();

                return;
            }
        }
        public void EngineMovementAudio()
        {
            if(m_EffectAudio.clip != m_EngineDriving)
            {
                m_EffectAudio.clip = m_EngineDriving;
                m_EffectAudio.pitch = m_MinPitch;
                m_EffectAudio.volume = m_OriginalVolume;
                m_EffectAudio.Play();

                return;
            }
            // ... change the clip to driving and play.
            if (m_EffectAudio.pitch < 1.0f)
                m_EffectAudio.pitch = m_MinPitch + (Mathf.Abs(m_Rigidbody.velocity.x) * m_MinPitch * 0.25f);
            else
                m_EffectAudio.pitch = 1.0f;

        }
        /*private void EngineAudio()
        {
            // Play the correct audio clip based on whether or not the tank is moving and what audio is currently playing.
            // If there is no input (the tank is stationary)...
            if (Mathf.Abs (m_MovementInputValue) < 0.1f && Mathf.Abs (m_TurnInputValue) < 0.1f)
            {
                // ... and if the audio source is currently playing the driving clip...
                if (m_MovementAudio.clip == m_EngineDriving)
                {
                    // ... change the clip to idling and play it.
                    m_MovementAudio.clip = m_EngineIdling;
                    m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play ();
                }
            }
            else
            {
                // Otherwise if the tank is moving and if the idling clip is currently playing...
                if (m_MovementAudio.clip == m_EngineIdling)
                {
                    // ... change the clip to driving and play.
                    m_MovementAudio.clip = m_EngineDriving;
                    m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play();
                }
            }
        }*/


        /*private void FixedUpdate()
        {
            // Move and turn the tank.
            Move ();
            Turn ();
        }


        private void Move()
        {
            // Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
            Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;

            // Apply this movement to the rigidbody's position.
            m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
        }


        private void Turn()
        {
            // Determine the number of degrees to be turned based on the input, speed and time between frames.
            float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

            // Make this into a rotation in the y axis.
            Quaternion turnRotation = Quaternion.Euler (0f, turn, 0f);

            // Apply this rotation to the rigidbody's rotation.
            m_Rigidbody.MoveRotation (m_Rigidbody.rotation * turnRotation);
        }*/
    }
}