using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 12f;
    public float runMultiplier = 3.0f;
    public float gravity = -9.8f;
    public float jumpHeight = 20f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public Terrain ground;
    public GameObject player;

    // SFX for jumping
    public AudioSource[] SFX_jump;
    public AudioSource[] SFX_step;
    public float timeBetweenWalkSteps = 1;
    public float timeBetweenRunSteps = 0.5f;
    private float nextTimeToStep = 0f;
    bool isGrounded;
    bool isRunning;
    Vector3 velocity;
    Vector3 move;

    private int iJumpSFX;
    private bool jumpSFXDone = true;

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        // Make sure he doesn't fall through the ground
        float terrainY = ground.SampleHeight(player.transform.position);
        if (player.transform.position.y < terrainY)
        {
            controller.enabled = false; // https://forum.unity.com/threads/does-transform-position-work-on-a-charactercontroller.36149/
            player.transform.position = new Vector3(player.transform.position.x, terrainY + 8f, player.transform.position.z);
            controller.enabled = true;

        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float cumSpeed = speed; // Cumulative speed lol
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            cumSpeed = speed * runMultiplier;
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        move = transform.right * x + transform.forward * z;

        controller.Move(move * cumSpeed * Time.deltaTime);
        if (Input.GetButton("Jump") && isGrounded)
        {
            if (jumpSFXDone)
            {
                iJumpSFX = Random.Range(0, SFX_jump.Length);
                SFX_jump[iJumpSFX].Play();
                StartCoroutine(waitForJumpSFXDone());
            }
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
        handleSFX();
    }

    IEnumerator waitForJumpSFXDone()
    {
        jumpSFXDone = false;
        // while (SFX_jump[iJumpSFX].isPlaying)
        // {
        //     yield return null;
        // }
        yield return new WaitForSeconds(0.5f);
        jumpSFXDone = true;
    }

    void handleSFX()
    {
        if (isGrounded && move != Vector3.zero)
        {
            if (Time.time >= nextTimeToStep)
            {
                if (isRunning) nextTimeToStep = Time.time + timeBetweenRunSteps;
                else nextTimeToStep = Time.time + timeBetweenWalkSteps;
                SFX_step[Random.Range(0, SFX_step.Length)].Play();
            }
        }
    }
    void runSFX()
    {
        if (Time.time >= nextTimeToStep)
        {
            nextTimeToStep = Time.time + timeBetweenRunSteps;
            SFX_step[Random.Range(0, SFX_step.Length)].Play();
        }
    }
}
