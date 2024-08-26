using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int health;

    public float timer;
    public float timeToGetCoin;
    public float timeScale = 1;

    public float topFitness;
    public bool hasFoundWinner;

    public GameObject[] objects;
    public PlayerScript[] players;

    // Start is called before the first frame update
    void Start()
    {
        objects = FindObjectsOfType<GameObject>();
        players = FindObjectsOfType<PlayerScript>();
        health = 1;
        timer = timeToGetCoin;
        GetTopPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        Time.timeScale = timeScale;

        if(timer <= 0)
        {
            health = 0;
        }

        if(health <= 0)
        {
            ResetLevel();
        }
    }

    public void GetTopPlayer()
    {
        topFitness = 0;
        hasFoundWinner = false;

        foreach (PlayerScript p in players)
        {
            if ((p.fitness > topFitness) || (p.fitness == topFitness && !hasFoundWinner))
            {
                topFitness = p.fitness;
                p._isWinning = true;
                hasFoundWinner = true;
            }
            else if ((p.fitness < topFitness) || (p.fitness == topFitness && hasFoundWinner))
            {
                p._isWinning = false;
            }
        }
    }



    private void ResetLevel()
    {
        objects = FindObjectsOfType<GameObject>();
        players = FindObjectsOfType<PlayerScript>();

        hasFoundWinner = false;

        foreach (GameObject obj in objects)
        {
            if (obj.gameObject.TryGetComponent(out IEntity entity))
            {
                entity.Reset();
            }
        }

        players = FindObjectsOfType<PlayerScript>();
        objects = FindObjectsOfType<GameObject>();

        timer = timeToGetCoin;
        health = 1;
        GetTopPlayer();
    }
}
