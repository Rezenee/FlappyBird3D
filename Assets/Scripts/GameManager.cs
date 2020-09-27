using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{

    public GameObject bird;
    public GameObject ground;
    public GameObject pipe;

    public GameObject panelStart;
    public GameObject panelPlay;
    public GameObject panelGameOver;

    public Text scoreText;
    public Text gameOverScoreText;

    public AudioSource flap;
    public AudioSource deathSound;
    public AudioSource pointScored;

    private GameObject birdObject;
    public static int pipesSpawned = 20;
    private GameObject[] pipes = new GameObject[pipesSpawned];
    private GameObject ground1;
    private GameObject ground2;

    public bool gameOver = false;
    public float yGravity = .45f;
    public int pipeDistance;
    private int pipeCount;
    private int groundCount;
    private bool jumpKey;
    public int horizontalMovementSpeed;
    private float horizontalInput;
    private int _score;

    public static GameManager Instance { get; private set; }
    public enum State { MENU, INIT, PLAY, GAMEOVER }

    State _state;
    public int Score
    {
        get { return _score; }
        set { _score = value;
            scoreText.text = "SCORE: " + _score;
            gameOverScoreText.text = scoreText.text;
        }
    }
    public void PlayClicked()
    {
        SwitchState(State.INIT);
    }
    public void SwitchState(State newState, float delay = 0)
    {
        StartCoroutine(SwitchDelay(newState, delay));
    }
    IEnumerator SwitchDelay(State newState, float delay)
    {

        yield return new WaitForSeconds(delay);
        EndState();
        _state = newState;
        BeginState(newState);

    }
    void Start()
    {
        Instance = this;
        SwitchState(State.MENU);
    }
    void BeginState(State newState)
    {
        switch (newState)
        {
            case State.MENU:
                Cursor.visible = true;
                panelStart.SetActive(true);

                pipeCount = 2;
                groundCount = 0;
                break;
            case State.INIT:
                Cursor.visible = false;
                panelPlay.SetActive(true);
                Score = 0;
                birdObject = Instantiate(bird);
                print(pipes.Length);
                for(int i = 0; i < pipes.Length; i++)
                {
                    pipes[i] = Instantiate(pipe, new Vector3(pipeDistance * pipeCount, UnityEngine.Random.Range(2, 10), UnityEngine.Random.Range(-9, 9)), Quaternion.identity);
                    pipeCount++;
                }
                ground1 = Instantiate(ground, new Vector3(160 * groundCount + 80, 0, 68), Quaternion.identity);
                groundCount++;
                ground2 = Instantiate(ground, new Vector3(160 * groundCount + 80, 0, 68), Quaternion.identity);
                groundCount++;
                SwitchState(State.PLAY);
                break;
            case State.PLAY:
                break;
            case State.GAMEOVER:
                for(int i=0; i < pipes.Length; i++)
                {
                    Destroy(pipes[i]);
                }
                Destroy(ground1);
                Destroy(ground2);
                Destroy(birdObject);
                panelGameOver.SetActive(true);
                break;
        }
    }
    void EndState()
    {
        switch (_state)
        {
            case State.MENU:
                panelStart.SetActive(false);
                break;
            case State.INIT:
                break;
            case State.PLAY:
                break;
            case State.GAMEOVER:
                panelPlay.SetActive(false);
                panelGameOver.SetActive(false);
                break;
        }
    }
    void Update()
    {
        switch (_state)
        {
            case State.MENU:
                break;
            case State.INIT:
                break;
            case State.PLAY:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    jumpKey = true;
                }
                horizontalInput = Input.GetAxis("Horizontal");
                if (gameOver)
                {
                    deathSound.Play();
                    SwitchState(State.GAMEOVER);
                }
                Rigidbody _BirdRigidbody = birdObject.GetComponent<Rigidbody>();

                Rigidbody _Ground1RigidBody = ground1.GetComponent<Rigidbody>();
                Rigidbody _Ground2RigidBody = ground2.GetComponent<Rigidbody>();
                // At spawn _bird z is between -1 and 1;
                
                // && _BirdRigidbody.position.z -1 >_PipeRigidBody.position.z  && _BirdRigidbody.position.z + 1 < _PipeRigidBody.position.z
                for (int i=0; i < pipes.Length; i++)
                {
                    Rigidbody _PipeRigidBody = pipes[i].GetComponent<Rigidbody>();
                    print(_PipeRigidBody.position.z);
                    if (_BirdRigidbody.position.x > _PipeRigidBody.position.x)
                        if(_BirdRigidbody.position.z + 1 > _PipeRigidBody.position.z &&  _BirdRigidbody.position.z -1 < _PipeRigidBody.position.z)
                        {
                            _PipeRigidBody.MovePosition(new Vector3(pipeDistance * pipeCount, UnityEngine.Random.Range(2, 10), UnityEngine.Random.Range(-9, 9)));
                            pipeCount++;
                            Score++;
                            pointScored.Play();
                        }
                        else
                        {
                            gameOver = true;
                        }

                }
                if (_BirdRigidbody.position.x > _Ground1RigidBody.position.x + 78)
                {
                    float x = _Ground1RigidBody.position.x;
                    _Ground1RigidBody.MovePosition(new Vector3(x + 320, 0, 0));
                    groundCount++;
                }
                if (_BirdRigidbody.position.x > _Ground2RigidBody.position.x + 78)
                {
                    float x = _Ground2RigidBody.position.x;
                    _Ground2RigidBody.MovePosition(new Vector3(x + 320, 0, 68));
                    groundCount++;
                }
                break;
            case State.GAMEOVER:
                if (Input.anyKeyDown)
                {
                    SwitchState(State.MENU);
                }
                gameOver = false;
                break;
        }

    }

    private void FixedUpdate()
    {
        Rigidbody _rigidbody = birdObject.GetComponent<Rigidbody>();
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _rigidbody.velocity.y, -horizontalInput * horizontalMovementSpeed);
        if (jumpKey)
        {
            Vector3 vel1 = _rigidbody.velocity;
            vel1.y = 0;
            _rigidbody.velocity = vel1;
            _rigidbody.AddForce(Vector3.up * 12, ForceMode.VelocityChange);
            flap.Play();
            jumpKey = false;
        }
        Vector3 vel = _rigidbody.velocity;
        vel.x = 5;
        vel.y -= yGravity;
        _rigidbody.velocity = vel;
    }


}