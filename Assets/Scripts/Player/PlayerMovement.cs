﻿using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class PlayerMovement : MonoBehaviour {
    // external references
    public GameObject playerCamera;
    public GameObject playerGun;
    public GameObject body;
    public InteractionsScript vision;
    public CharacterController controller;

    // jump variables
    public Transform groundCheck;
    bool isGrounded;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    // movement variables
    Vector3 velocity;
    float DIAGONAL_SPEED = (Mathf.Sqrt(2) / 2.0f);
    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    // ghost specific variables
    bool isGhost = false;
    public List<PointInTime> ghostPath = new List<PointInTime>();
    public int currentGhostPoint = 0;
    public bool hasClickedE = false;
    public bool hasClickedLeftClick = false;

    // state variables
    public enum PlayerState { PLAY, PREYED, FACING_DEATH, DEAD };
    private PlayerState state;

    // control variables
    bool canFireGunInCurrentLevelLeft = true;
    bool canFireGunInCurrentLevelRight = true;

    // sound variables
    public StudioEventEmitter soundEvent;
    public StudioEventEmitter timeTravelEvent;
    public StudioEventEmitter noClonesEvent;
    public StudioEventEmitter jumpEvent;
    public StudioEventEmitter fallEvent;
    public float soundDelay = 0.5f;
    float soundTimer;
    bool hasJumped = false;

    // death animation variables
    Quaternion deathAngle;
    float deathTime = 0;
    float deathRange = 0;
    public float deathRotationSpeed = 1.0f;
    public PlayerLook playerLook;

    private void Awake() {
        canFireGunInCurrentLevelLeft = SceneController.Instance.GetCanFireGunInCurrentLevel();
        canFireGunInCurrentLevelRight = SceneController.Instance.GetCanFireGunInCurrentLevel();

        if (!canFireGunInCurrentLevelLeft) {
            TogglePlayerGun(false);
        }
    }

    void Start() {
        state = PlayerState.PLAY;

        deathTime = 1 / deathRotationSpeed;
    }

    // Update is called once per frame
    void Update() {
        if (!SceneController.Instance.CanStartRun()) return;

        if (!isGhost) {
            // horizontal movement
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            if (x != 0 && z != 0) {
                x *= DIAGONAL_SPEED;
                z *= DIAGONAL_SPEED;
            }

            Vector3 move = transform.right * x + transform.forward * z;
            Vector3 stepSpeed = move * speed;

            if (state != PlayerState.PLAY) {
                if (state == PlayerState.PREYED) {
                    FaceDeath();
                }

                return;
            };

            // jumping/vertical movement
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            if (isGrounded && velocity.y < 0) {
                if (hasJumped) {
                    hasJumped = false;
                    fallEvent.Play();
                }
                velocity.y = -2f;
            }

            if (Input.GetButtonDown("Jump") && isGrounded) {
                jumpEvent.Play();
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                hasJumped = true;
            }
            velocity.y += gravity * Time.deltaTime;

            FootStepSound(stepSpeed.magnitude, velocity.y, Time.deltaTime);

            // move controller
            if (controller.enabled) {
                controller.Move(stepSpeed * Time.deltaTime);
                controller.Move(velocity * Time.deltaTime);
            }

            if (Input.GetKeyDown(KeyCode.E)) {
                hasClickedE = true;
                Debug.Log("PLAYER CLICKED E");
                Interact();
            }

            // ANY NEW INPUT THAT THE GHOSTS HAVE TO REPLICATE MUST BE ADDED HERE

            // if player has fired gun, ignore rest of inputs
            if (SceneController.Instance.GetIsReseting()) return;

            // reseting level actions
            if (canFireGunInCurrentLevelLeft && Input.GetButtonDown("Fire1")) {
                if (Input.GetKeyDown(KeyCode.LeftControl)) return;
                if (SceneController.Instance.GetIsPaused()) return;
                if (!SceneController.Instance.CanCreateClones()) {
                    noClonesEvent.Play();
                    return;
                }

                hasClickedLeftClick = true;
                Debug.Log("PLAYER LEFT CLICK!");
                if (playerGun != null) {
                    playerGun.GetComponent<Gun>().Shoot();
                }
                Invoke("TimeTravelSound", 0.1f);
                SceneController.Instance.ResetWithSave();
            }
            if (canFireGunInCurrentLevelRight && Input.GetButtonDown("Fire2")) {
                if (Input.GetKeyDown(KeyCode.LeftAlt)) return;
                if (SceneController.Instance.GetIsPaused()) return;

                Invoke("TimeTravelSound", 0.1f);
                SceneController.Instance.ResetWithoutSave();
            }
            if (canFireGunInCurrentLevelRight && Input.GetKeyDown(KeyCode.R)) {
                if (SceneController.Instance.GetIsPaused()) return;

                TimeTravelSound();
                SceneController.Instance.ResetAndDeletePrevious();
            }
            if (canFireGunInCurrentLevelRight && Input.GetKeyDown(KeyCode.L)) {
                if (SceneController.Instance.GetIsPaused()) return;

                TimeTravelSound();
                SceneController.Instance.ResetHard();
            }
        }
    }

    void FixedUpdate() {
        // do nothing if the run can't start yet
        if (!SceneController.Instance.CanStartRun()) return;

        if (isGhost) {
            if (currentGhostPoint < ghostPath.Count) {
                PointInTime currentPointInTime = ghostPath[currentGhostPoint];
                this.transform.position = currentPointInTime.position;
                this.transform.rotation = currentPointInTime.rotation;
                playerCamera.transform.rotation = currentPointInTime.cameraAngle;
                currentGhostPoint++;
                if (currentPointInTime.clickE) {
                    // Debug.Log("GHOST PRESSED E!");
                    Interact();
                }
                if (currentPointInTime.clickLeftClick) {
                    // Debug.Log("GHOST LEFT CLICK!");
                    playerGun.GetComponent<Gun>().Shoot();
                }
                // if last recorded position
                if (currentGhostPoint == ghostPath.Count - 1) {
                    // Debug.Log("LAST POSITION!");
                    this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
                }
            }
        }
    }

    public bool GetIsGhost() {
        return isGhost;
    }

    public void SetAsGhost(List<PointInTime> path) {
        isGhost = true;
        ghostPath = path;
    }

    public void TogglePlayerGun(bool activeValue) {
        canFireGunInCurrentLevelLeft = activeValue;
        playerGun.SetActive(activeValue);
    }

    public PlayerState GetState() {
        return this.state;
    }

    public void SetState(PlayerState newState) {
        this.state = newState;
    }

    public void Preyed(Transform monster) {
        if (!isGhost) {
            if (state != PlayerState.PLAY) return;
            controller.enabled = false;
            playerLook.enabled = false;
            this.state = PlayerState.PREYED;

            Vector3 deathView = monster.position;
            Vector3 direction = (deathView - transform.position).normalized;
            direction.y = 0.2f;
            deathAngle = Quaternion.LookRotation(direction);
        } else {
            Invoke("DisintegrateGhost", 1.0f);
        }
    }

    public void Die() {
        this.state = PlayerState.DEAD;
        Debug.Log("Player is Dead");
        SceneController.Instance.PlayerDied();
    }

    public void FaceDeath() {
        deathRange += Time.deltaTime / deathTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, deathAngle, deathRange);

        if (Quaternion.Angle(transform.rotation, deathAngle) == 0) {
            transform.rotation = deathAngle;
            state = PlayerState.FACING_DEATH;
            Invoke("Die", 1.0f);
        }
    }

    void Interact() {
        GameObject obj = vision.GetInteractiveObject();

        if (obj != null) {
            obj.GetComponent<Interactable>().Interact();
        }
    }

    void FootStepSound(float stepSpeed, float velocityY, float deltaTime) {
        if (!controller.enabled || velocityY > 0 || stepSpeed == 0) {
            soundTimer = 0;
            return;
        }

        soundTimer += deltaTime;

        if (soundTimer >= soundDelay) {
            soundEvent.Play();
            soundTimer = 0;
        }
    }

    void TimeTravelSound() {
        timeTravelEvent.Play();
    }

    void DisintegrateGhost() {
        Destroy(this.gameObject);
    }
}
