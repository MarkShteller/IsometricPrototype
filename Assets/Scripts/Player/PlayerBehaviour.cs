﻿using EZCameraShake;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public float movementSpeed = 0.1f;

    private float healthPoints;
    public float totalHealthPoints;

    private float manaPoints;
    public float totalManaPoints;
    public float manaRegenAmount;

    public float holeTimeToRegen;

    public float pushManaCost;
    public float holeManaCost;
    public float dashManaCost;
    public int shockwaveCoreCost;
    public float dashDuration;
    public float dashSpeed;
    public float fallDamage = 1;
    public float takenDamageCooldown;

    public int coresCount;
    public int enemyDefeatedCount;

    public AudioSource soundEffectSource;

    private List<BasePowerupBehaviour> activePowerUps;

    private Transform checkpoint;
    [HideInInspector] public Vector3 spawnPosition;

    private RaycastHit hit;
    private Ray ray;

    private float lastTimeDamageTaken=0;
    private bool enableDash = true;
    private bool isDashing = false;

    private float timeOverPit = 0;

    [HideInInspector] public GameObject prevHoveredObject;
    [HideInInspector] public GameObject currHoveredObject;

    [HideInInspector] public bool enableControlls = true;

    //private float defaultRadius = 0.35f;
    private Vector3 defaultForcePushTriggerSize;
    public float pushRadius = 4f;

    public float pushForce;
    public BoxCollider forcePushTriggerCollider;
    public GameObject forcePushEffect;

    public LevelGenerator gridHolder;
    public Transform visualsHolder;

    public PlayerShockwaveBehavior shockwaveBehavior;
    public ForcePushFloorTrigger forcePushFloorTrigger;

    public Animator animator;

    public Color tileHighlightColor;

    public bool IsTestMode = false;

    void Start()
    {
        if(!IsTestMode)
            gridHolder = GameObject.FindGameObjectWithTag("LevelGenerator").GetComponent<LevelGenerator>();

        defaultForcePushTriggerSize = forcePushTriggerCollider.size;
        manaPoints = totalManaPoints;
        healthPoints = totalHealthPoints;
        coresCount = 0;
        enemyDefeatedCount = 0;
        activePowerUps = new List<BasePowerupBehaviour>();
    }

    void FixedUpdate()
    {
        if (enableControlls)
        {
            float xMove = Input.GetAxis("HorizontalMove");
            float zMove = Input.GetAxis("VerticalMove");

            if (xMove == 0 && zMove == 0)
            {
                xMove = Input.GetAxis("Horizontal");
                zMove = Input.GetAxis("Vertical");
            }
            transform.Translate(xMove * movementSpeed, 0, zMove * movementSpeed);

            /*
            float xLook = Input.GetAxis("HorizontalLook");
            float yLook = Input.GetAxis("VerticalLook");
            float angle = AngleFromJoystick(xLook, yLook);

            //Debug.LogFormat("x: {0} y: {1} angle: {2}",xLook, yLook, angle);

            //rotate only if there is an input
            if (xLook != 0 || yLook != 0)
                this.visualsHolder.rotation = Quaternion.AngleAxis(angle - 90, Vector3.up);
                */

            Vector3 playerRotation = Vector3.right * -Input.GetAxisRaw("HorizontalLook") + Vector3.forward * Input.GetAxisRaw("VerticalLook");

            if (Input.GetAxis("AltVLook") != 0 || Input.GetAxis("AltHLook") != 0)
                playerRotation = Vector3.right * -Input.GetAxis("AltHLook") + Vector3.forward * (Input.GetAxis("AltVLook"));

            if (playerRotation.sqrMagnitude > 0.0f)
            {
                this.visualsHolder.rotation = Quaternion.LookRotation(playerRotation, Vector3.up);
            }

            //Vector2 hoverAnimDir = GetMovementDirection(new Vector2(xMove, zMove), new Vector2(visualsHolder.forward.x, visualsHolder.forward.z));
            //animator.SetFloat("HoverZ", hoverAnimDir.x);

            if (ControllerInputDevice.GetDashButtonDown())
            {
                if (manaPoints >= dashManaCost && enableDash)
                {
                    manaPoints -= dashManaCost;

                    Vector3 dashDir = new Vector3(xMove, 0, zMove).normalized;
                    if (dashDir == Vector3.zero)
                        dashDir = this.visualsHolder.forward *-1;//this.visualsHolder.rotation.eulerAngles.normalized;
                    print("DASH! dir: "+dashDir);
                    AudioManager.Instance.PlayEffect(soundEffectSource, 3);

                    animator.SetTrigger("Dash");

                    Vector3 rotation = this.visualsHolder.forward;//this.visualsHolder.rotation.eulerAngles.normalized * -1;
                    print("dash vis dir: " + rotation);

                    Vector2 dashAnimDirection = GetMovementDirection(new Vector2(xMove, zMove), new Vector2(rotation.x, rotation.z));
                    animator.SetFloat("DashX", dashAnimDirection.x);
                    animator.SetFloat("DashY", dashAnimDirection.y);

                    print("dashAnimDirection: "+ dashAnimDirection);

                    StartCoroutine(DashCoroutine(dashDir, dashDuration));
                }
            }

            if (ControllerInputDevice.GetSpecialButtonDown())
            {
                if (coresCount >= shockwaveCoreCost)
                {
                    coresCount -= shockwaveCoreCost;
                    UIManager.Instance.SetCoreCount(coresCount);
                    print("SHOCKWAVE!");
                    CameraShaker.Instance.ShakeOnce(2f, 8f, 0.1f, 2.5f);
                    shockwaveBehavior.gameObject.SetActive(true);
                    StartCoroutine(shockwaveBehavior.Shockwave(10));
                }
            }
        }
        DetectPlayerPositionOnGrid();


        if (transform.position.y < -1f)
        {
            FellIntoAPit();
        }

       /* Vector2 mousepos = Input.mousePosition;
        Vector2 screenCenter = Camera.main.WorldToScreenPoint(this.transform.position);

        float angle = AngleBetweenTwoPoints(screenCenter, mousepos) + 180;

        this.visualsHolder.rotation = Quaternion.AngleAxis(angle - 90, Vector3.up);
        */
    }

    private IEnumerator DashCoroutine(Vector3 direction, float duration)
    {
        isDashing = true;
        float time = duration;
        while (time > 0 && enableDash)
        {
            time -= Time.deltaTime;
            enableControlls = false;
            transform.Translate(direction * movementSpeed * dashSpeed * Time.deltaTime);
            yield return null;
        }
        isDashing = false;
        enableControlls = true;
    }

    public void PreformPush()
    {
        forcePushTriggerCollider.size = new Vector3(forcePushTriggerCollider.size.x + 1, forcePushTriggerCollider.size.y, forcePushTriggerCollider.size.z + pushRadius);
        forcePushTriggerCollider.center = new Vector3(0, 0, -pushRadius / 2);
        StartCoroutine(ShowForcePushEffect(0.1f));
        StartCoroutine(forcePushFloorTrigger.PlayEffectCoroutine(0.1f));
    }

    void Update()
    {
        ManipulateFloor();
        RegenMana();

        forcePushTriggerCollider.center = Vector3.zero;
        forcePushTriggerCollider.size = defaultForcePushTriggerSize;

        if (enableControlls)
        {
            if (Input.GetMouseButtonDown(1) || ControllerInputDevice.GetRightTriggerDown())
            {
                if (manaPoints >= pushManaCost)
                {
                    manaPoints -= pushManaCost;

                    //the animation triggers PreformPush()
                    animator.SetTrigger("PushA");
                }
            }
        }

        BasePowerupBehaviour powerupToRemove = null;
        if (activePowerUps.Count > 0)
        {
            foreach (BasePowerupBehaviour powerUp in activePowerUps)
            {
                powerUp.effectTime -= Time.deltaTime;
                //print(powerUp.powerUpName + " time: " + powerUp.effectTime);
                if (powerUp.effectTime <= 0)
                {
                    powerupToRemove = powerUp;
                    break;
                }
            }
        }
        if (powerupToRemove != null)
        {
            print("removing powerup "+ powerupToRemove.powerUpName);
            activePowerUps.Remove(powerupToRemove);
            RemovePowerupBonus(powerupToRemove);
        }

    }

    private IEnumerator ShowForcePushEffect(float duration)
    {
        forcePushEffect.SetActive(true);
        yield return new WaitForSeconds(duration);
        forcePushEffect.SetActive(false);
    }

    public void AddPowerup(BasePowerupBehaviour powerUp)
    {
        Debug.Log("Picked up: "+powerUp.name);
        switch (powerUp.type)
        {
            case PowerUpType.Health:
                healthPoints += powerUp.bonus;
                if (healthPoints > totalHealthPoints)
                    healthPoints = totalHealthPoints;
                UIManager.Instance.SetHealth(healthPoints / totalHealthPoints);
                break;
            case PowerUpType.Core:
                coresCount += powerUp.count;
                UIManager.Instance.SetCoreCount(coresCount);
                break;
        }

        if (powerUp.type != PowerUpType.Health && powerUp.type != PowerUpType.Core)
        {
            if (activePowerUps.Count == 0 || !activePowerUps.Exists(p => p.powerUpName == powerUp.powerUpName))
            {
                activePowerUps.Add(powerUp);
                UIManager.Instance.AddPowerup(powerUp.powerUpData);
                switch (powerUp.type)
                {
                    case PowerUpType.PushForceBoost:
                        pushForce += powerUp.bonus;
                        break;
                    case PowerUpType.RegenBoost:
                        manaRegenAmount += powerUp.bonus;
                        break;
                    case PowerUpType.PushRangeBoost:
                        pushRadius += powerUp.bonus;
                        break;
                    case PowerUpType.MoveSpeedBoost:
                        movementSpeed += powerUp.bonus;
                        break;
                    case PowerUpType.DashBoost:
                        dashSpeed += powerUp.bonus;
                        break;
                }
            }
            else
            {
                BasePowerupBehaviour pu = activePowerUps.Find(p => p.powerUpName == powerUp.powerUpName);
                pu.effectTime += powerUp.effectTime;
                UIManager.Instance.UpdatePowerupTimer(powerUp.powerUpData, pu.effectTime);
            }
        }
    }

    private void RemovePowerupBonus(BasePowerupBehaviour powerUp)
    {
        switch (powerUp.type)
        {
            case PowerUpType.PushForceBoost:
                pushForce -= powerUp.bonus;
                break;
            case PowerUpType.RegenBoost:
                manaRegenAmount -= powerUp.bonus;
                break;
            case PowerUpType.PushRangeBoost:
                pushRadius -= powerUp.bonus;
                break;
            case PowerUpType.MoveSpeedBoost:
                movementSpeed -= powerUp.bonus;
                break;
            case PowerUpType.DashBoost:
                dashSpeed -= powerUp.bonus;
                break;
        }
        Destroy(powerUp.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            TakeDamage(enemy.damage);
        }
        if (collision.gameObject.CompareTag("GoalCube"))
        {
            //GameManager.Instance.NextLevel();
            GameManager.Instance.LevelFinished();
            gameObject.SetActive(false);
        }
        if (collision.gameObject.CompareTag("WallCube") || collision.gameObject.CompareTag("GateCube"))
        {
            enableDash = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("WallCube") || collision.gameObject.CompareTag("GateCube"))
        {
            enableDash = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CheckpointCube"))
        {
            print("# setting new checkpoint: "+other.name);
            SetCheckpoint(other.transform);
        }
        /*if (other.CompareTag("EnemyPulseTrigger"))
        {
            Enemy enemy = other.transform.parent.GetComponent<Enemy>();
            TakeDamage(enemy.damage);
            print("taken damage from pulse");
        }*/
    }


    public void TakeDamage(float damage)
    {
        if (Time.time - lastTimeDamageTaken > takenDamageCooldown)
        {
            lastTimeDamageTaken = Time.time;

            healthPoints -= damage;
            animator.SetTrigger("TakeDamage");

            GameManager.Instance.SetScoreMultiplier(1);
            GameManager.Instance.AddDamageCount(damage);
            UIManager.Instance.SetHealth(healthPoints / totalHealthPoints);
            if (healthPoints <= 0)
            {
                GameManager.Instance.GameOver();
            }

            GameManager.Instance.cameraRef.GlitchScreen();
        }
    }

    public void SetCheckpoint(Transform checkpointTransform)
    {
        this.checkpoint = checkpointTransform;
    }

    private void FellIntoAPit()
    {
        enableControlls = false;

        //Transform respawnPoint = prevHoveredObject.transform;
        //transform.position = new Vector3(respawnPoint.position.x, 10, respawnPoint.position.z);
        if(checkpoint != null)
            transform.position = new Vector3(checkpoint.position.x, 10, checkpoint.position.z);
        else
            transform.position = new Vector3(spawnPosition.x, 10, spawnPosition.z);
        TakeDamage(fallDamage);
        StartCoroutine(WaitToRecover());
    }

    private IEnumerator WaitToRecover()
    {
        //SphereCollider sc = GetComponent<SphereCollider>();
        yield return new WaitForSeconds(1);
        enableControlls = true;
    }

    private void RegenMana()
    {
        if (manaPoints < totalManaPoints)
        {
            manaPoints += Time.deltaTime * manaRegenAmount;
            UIManager.Instance.SetMana(manaPoints / totalManaPoints);
        }
    }

    private void ManipulateFloor()
    {
        if (prevHoveredObject != null)
            if(prevHoveredObject.tag != "WeakCube")
                prevHoveredObject.GetComponent<Renderer>().material.color = Color.white;
        if (currHoveredObject != null)
            if(currHoveredObject.tag != "WeakCube")
                currHoveredObject.GetComponent<Renderer>().material.color = tileHighlightColor;

        //make a hole
        if (Input.GetMouseButtonDown(0) || ControllerInputDevice.GetLeftTriggerDown())
        {
            if (manaPoints >= holeManaCost)
            {
                Transform tileTransform = currHoveredObject.transform;
                string name = tileTransform.parent.name;
                Debug.Log("pressed on grid cube: " + name);

                GridNode node = gridHolder.GetGridNode(name);
                if (node.GetTileType() != TileType.Occupied)
                {
                    //move the tile down
                    tileTransform.GetComponent<BaseTileBehaviour>().Drop();

                    gridHolder.SetGridNodeType(node, TileType.Pit, holeTimeToRegen);
                    manaPoints -= holeManaCost;
                }
                else
                {
                    print("Pressed on occupied tile! tile: " + name);
                }
            }
        }
    }

    private void DetectPlayerPositionOnGrid()
    {
        //Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            if (hit.transform.tag == "FloorCube" || hit.transform.tag == "WeakCube")
            {
                string name = hit.transform.parent.name;
                string[] posArr = name.Split(',');

                Point pPoint = GameManager.Instance.playerPointPosition;

                if (pPoint != null)
                {
                    GridNode gNode = gridHolder.GetGridNode(pPoint.x, pPoint.y);
                    if (gNode.GetTileType() == TileType.Occupied)
                        gNode.SetType(TileType.Normal);
                }

                GameManager.Instance.playerPointPosition = new Point(int.Parse(posArr[0]), int.Parse(posArr[1]));
                pPoint = GameManager.Instance.playerPointPosition;

                if (pPoint != null)
                {
                    GridNode gNode = gridHolder.GetGridNode(pPoint.x, pPoint.y);

                    if (gNode.GetTileType() == TileType.Weak)
                    {
                        gNode.GetGameNodeRef().GetComponentInChildren<WeakTileBehaviour>().StepOnTile(()=> gNode.SetType(TileType.Pit));
                    }

                    if (gNode.GetTileType() != TileType.Pit)
                    {
                        gridHolder.SetGridNodeType(pPoint.x, pPoint.y, TileType.Occupied);
                        timeOverPit = 0;
                    }

                    if (gNode.GetTileType() == TileType.Pit)
                    {
                        timeOverPit += Time.deltaTime;
                        if (!isDashing && timeOverPit >= 0.3f)
                            FellIntoAPit();
                    }
                }
            }
        }
    }

    private Vector2 GetMovementDirection(Vector2 moveDir, Vector2 lookDir)
    {
        float angleDiff = GetAngle(moveDir.x, moveDir.y) - GetAngle(lookDir.x, lookDir.y)-90;
        return new Vector2(Mathf.Cos(angleDiff), Mathf.Sin(angleDiff));
    }

    float GetAngle(float x, float y)
    {
        float angle = Mathf.Atan(y / x);
        if (x < 0)
            angle = angle + Mathf.PI;
        return angle;
    }

    float AngleFromJoystick(float x, float y)
    {
        if (x != 0.0f || y != 0.0f)
        {
            return Mathf.Atan2(y, x) * Mathf.Rad2Deg; // flip x and y for 90 deg result
        }
        return 0;
    }

    float AngleBetweenTwoPoints(Vector2 a, Vector2 b)
    {
        return -Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }
}
