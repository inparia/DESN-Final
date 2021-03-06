﻿using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[System.Serializable]
public enum ImpulseSounds
{
    JUMP,
    HIT1,
    HIT2,
    HIT3,
    DIE
}

public class PlayerBehaviour : MonoBehaviour
{
    [Header("Controls")]
    public float horizontalForce;
    public float verticalForce;

    [Header("Platform Detection")]
    public bool isGrounded;
    public bool isJumping;
    public bool isCrouching;
    public bool isInWater;
    public Transform spawnPoint;
    public Transform lookAheadPoint;
    public Transform lookInFrontPoint;
    public LayerMask collisionGroundLayer;
    public LayerMask collisionWallLayer;
    public RampDirection rampDirection;
    public bool onRamp;
    public float rampForceFactor;

    [Header("Player Abilities")] 
    public int health;
    public int lives;
    public BarController healthBar;
    public Animator livesHUD;

    [Header("Dust Trail")]
    public ParticleSystem dustTrail;
    public Color dustTrailColour;

    [Header("Impulse Sounds")] 
    public AudioSource[] sounds;

    [Header("Screen Shake")] 
    public CinemachineVirtualCamera vcam1;
    public CinemachineBasicMultiChannelPerlin perlin;
    public float shakeIntensity;
    public float maxShakeTime;
    public float shakeTimer;
    public bool isCameraShaking;

    private Rigidbody2D m_rigidBody2D;
    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;
    private RaycastHit2D groundHit;

    public Text saveText;
    private bool dispText;
    private bool victory;
    private float timeRemaining = 5, anotherTimeRemaining = 5;
    // Start is called before the first frame update
    void Start()
    {
        health = 100;
        lives = 3;
        isCameraShaking = false;
        shakeTimer = maxShakeTime;

        m_rigidBody2D = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
        dustTrail = GetComponentInChildren<ParticleSystem>();

        sounds = GetComponents<AudioSource>();

        perlin = vcam1.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        dispText = false;
    }


    void Update()
    {
        if (Input.GetKeyDown("p"))
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0f;
                saveText.text = "Paused.\nPress 'P' to resume.";
                saveText.gameObject.SetActive(true);
            }
            else
            {
                Time.timeScale = 1f;
                saveText.gameObject.SetActive(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _LookInFront();
        _LookAhead();
        _Move();

        if (isCameraShaking)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0.0f) // timed out
            {
                perlin.m_AmplitudeGain = 0.0f;
                shakeTimer = maxShakeTime;
                isCameraShaking = false;
            }
        }

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            dispText = false;
            timeRemaining = 5;
        }
        if (dispText)
        {
            saveText.gameObject.SetActive(true);
        }
        else
        {
            saveText.gameObject.SetActive(false);
        }

        if (victory)
        {
            if (anotherTimeRemaining > 0)
            {
                anotherTimeRemaining -= Time.deltaTime;
            }
            else
            {
                
                SceneManager.LoadScene("Win");
                SoundManager.Instance.Play("BGM");
            }
        }
    }

    private void _LookInFront()
    {
        if (!isGrounded)
        {
            rampDirection = RampDirection.NONE;
            return;
        }

        var wallHit = Physics2D.Linecast(transform.position, lookInFrontPoint.position, collisionWallLayer);
        if (wallHit && isOnSlope())
        {
            rampDirection = RampDirection.UP;
        }
        else if (!wallHit && isOnSlope())
        {
            rampDirection = RampDirection.DOWN;
        }

        Debug.DrawLine(transform.position, lookInFrontPoint.position, Color.red);
    }

    private void _LookAhead()
    {
        groundHit = Physics2D.Linecast(transform.position, lookAheadPoint.position, collisionGroundLayer);

        isGrounded = (groundHit) ? true : false;

        Debug.DrawLine(transform.position, lookAheadPoint.position, Color.green);
    }

    private bool isOnSlope()
    {
        if (!isGrounded)
        {
            onRamp = false;
            return false;
        }

        if (groundHit.normal != Vector2.up)
        {
            onRamp = true;
            return true;
        }

        onRamp = false;
        return false;
    }

    void _Move()
    {
        if (isGrounded)
        {
            if (!isJumping && !isCrouching)
            {
                if (Input.GetKey("d"))
                {
                    // move right
                    m_rigidBody2D.AddForce(Vector2.right * horizontalForce * Time.deltaTime);
                    transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    if (onRamp && rampDirection == RampDirection.UP)
                    {
                        m_rigidBody2D.AddForce(Vector2.up * horizontalForce * rampForceFactor * Time.deltaTime);
                    }
                    else if (onRamp && rampDirection == RampDirection.DOWN)
                    {
                        m_rigidBody2D.AddForce(Vector2.down * horizontalForce * rampForceFactor * Time.deltaTime);
                    }

                    CreateDustTrail();

                    m_animator.SetInteger("AnimState", (int)PlayerAnimationType.RUN);
                }
                else if (Input.GetKey("a"))
                {
                    // move left
                    m_rigidBody2D.AddForce(Vector2.left * horizontalForce * Time.deltaTime);
                    transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                    if (onRamp && rampDirection == RampDirection.UP)
                    {
                        m_rigidBody2D.AddForce(Vector2.up * horizontalForce * rampForceFactor * Time.deltaTime);
                    }
                    else if (onRamp && rampDirection == RampDirection.DOWN)
                    {
                        m_rigidBody2D.AddForce(Vector2.down * horizontalForce * rampForceFactor * Time.deltaTime);
                    }

                    CreateDustTrail();

                    m_animator.SetInteger("AnimState", (int)PlayerAnimationType.RUN);
                }
                else
                {
                    m_animator.SetInteger("AnimState", (int)PlayerAnimationType.IDLE);
                }
            }

            if (Input.GetKey("w") && (!isJumping))
            {
                // jump
                m_rigidBody2D.AddForce(Vector2.up * verticalForce);
                m_animator.SetInteger("AnimState", (int) PlayerAnimationType.JUMP);
                isJumping = true;

                sounds[(int) ImpulseSounds.JUMP].Play();

                CreateDustTrail();
            }
            else
            {
                isJumping = false;
            }

            if (Input.GetKey("s") && (!isCrouching))
            {
                m_animator.SetInteger("AnimState", (int)PlayerAnimationType.CROUCH);
                isCrouching = true;
            }
            else
            {
                isCrouching = false;
            }

            
        }
        if (isInWater)
        {
            if (Input.GetKey("d"))
            {
                m_rigidBody2D.AddForce(Vector2.right * horizontalForce / 5.0f * Time.deltaTime);
                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
            else if (Input.GetKey("a"))
            {
                // move left
                m_rigidBody2D.AddForce(Vector2.left * horizontalForce / 5.0f * Time.deltaTime);
                transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
            }

            if (Input.GetKey("w"))
            {
                m_rigidBody2D.AddForce(Vector2.up * verticalForce / 40);
                m_animator.SetInteger("AnimState", (int)PlayerAnimationType.JUMP);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // respawn
        if (other.gameObject.CompareTag("DeathPlane"))
        {
            LoseLife();
        }

        if (other.gameObject.CompareTag("Bullet"))
        {
        }

        if (other.gameObject.CompareTag("Water"))
        {
            m_rigidBody2D.gravityScale = 0.5f;
            isInWater = true;
        }

        if(other.gameObject.CompareTag("Goal"))
        {
            if (!victory)
            {
                SoundManager.Instance.Play("Victory");
                SoundManager.Instance.Stop("BGM");
                victory = true;
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                foreach (GameObject enemy in enemies)
                    GameObject.Destroy(enemy);
            }
        }
        if (other.gameObject.CompareTag("SavePoint") && !other.gameObject.GetComponent<SavePoint>().savePointEnabled)
        {
            other.gameObject.GetComponent<SavePoint>().savePointEnabled = true;
            spawnPoint.position = other.gameObject.transform.position;
            saveText.text = "Save Point Activated.";
            dispText = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            m_rigidBody2D.gravityScale = 5.0f;
            isInWater = false;
        }

        if (collision.gameObject.CompareTag("Bullet"))
        {
            LoseLife();
        }
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (m_rigidBody2D.position.y > other.gameObject.GetComponent<Rigidbody2D>().position.y + 0.5f)
            {
                m_rigidBody2D.AddForce(Vector2.up * verticalForce / 1.5f);
                other.gameObject.SetActive(false);
                isJumping = true;
                sounds[(int)ImpulseSounds.JUMP].Play();
            }
            else
            {
                LoseLife();

            }
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        
    }

    public void LoseLife()
    {
        lives -= 1;

        sounds[(int) ImpulseSounds.DIE].Play();
        ShakeCamera();
        livesHUD.SetInteger("LivesState", lives);

        if (lives > 0)
        {
            health = 100;
            healthBar.SetValue(health);
            transform.position = spawnPoint.position;
        }
        else
        {
            SceneManager.LoadScene("End");
        }
        
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        healthBar.SetValue(health);

        PlayRandomHitSound();

        

        if (health <= 0)
        {
            LoseLife();
        }
    }

    private void CreateDustTrail()
    {
        dustTrail.GetComponent<Renderer>().material.SetColor("_Color", dustTrailColour);

        dustTrail.Play();
    }

    private void PlayRandomHitSound()
    {
        var randomHitSound = Random.Range(1, 3);
        sounds[randomHitSound].Play();
    }

    private void ShakeCamera()
    {
        perlin.m_AmplitudeGain = shakeIntensity;
        isCameraShaking = true;
    }
}
