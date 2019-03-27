﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//abstract class to serve as parent for all enemies
public abstract class Enemy_Parent : MonoBehaviour
{
    //class to hold game object and its original colour - allows for switching back to original
    private class ObjectColourPairs
    {

        private GameObject _gameObj;
        private Color _originalColour;
        private bool _containsRender = false; //not all gameObject will have a render
        

        public ObjectColourPairs(GameObject objIn)
        {
            _gameObj = objIn;
            if (_gameObj.GetComponent<Renderer>() != null) //determine if it has a render
            {
                _containsRender = true;
                _originalColour = _gameObj.GetComponent<Renderer>().material.color;
            }
        }

        public GameObject GetGameObject()
        {
            return _gameObj;
        }

        //safe return - needs to have a colour
        public Color GetColor()
        {
            if (_containsRender)
            {
                return _originalColour;
            }
            Debug.LogError("Trying to get colour with no renderer");
            return new Color();
        }

        public bool ContainsRenderer()
        {
            return _containsRender;
        }
    }




    [Header("Set in Inspector: Enemy")]

    private List<ObjectColourPairs> _allGameObjectsInObject;
    private float _colourChangeTime = 0;//holds time that object got damaged
    private float _onFireTime = 0;//holds time that object got set on fire
    private float _timeToRemainColourChange = 0.15f;
    private float _timeToRemainOnFire = 1.0f;

    protected BoundsCheck _bound;
    protected float _health = 0; //set in child class


    //property to get and set the position of the enemy objects
    public Vector3 position
    {
        get
        {
            return (transform.position);
        }
        set
        {
            transform.position = value;
        }
    }

    protected void Awake()
    {
        _bound = GetComponent<BoundsCheck>();
        _allGameObjectsInObject = new List<ObjectColourPairs>();
        getAllChildren(this.gameObject.transform);
    }

    public virtual void Update()
    {
        Move(); //calls move which is defined in the child class
        if (_bound != null && (_bound.offScreenDown || _bound.offScreenLeft || _bound.offScreenRight))
        {
            Main.scriptReference.DestroyEnemy(gameObject);
        }
        if (_onFireTime != 0)
        {
            if((Time.time - _onFireTime) > _timeToRemainOnFire)
            {

            }
        }
        else if (_colourChangeTime != 0 && (Time.time - _colourChangeTime) > _timeToRemainColourChange)
        {
            ChangeColour(true);
        }

    }

    //since this is different for all classes it will be implemented by them
    protected abstract void Move();

    //this function damages the enemy when they collide with a projectile.
    void OnParticleCollision(GameObject otherColl)
    {
        if (otherColl.tag == "ProjectileHero")
        {
            _health -= Main.GetWeaponDefinition(WeaponType.plasmaThrower).damage * Time.deltaTime;//damage is per second
            CheckHealth();
            //ChangeColour(false, 75, -75, -75);
        }
        
    }

    //function for collision with a projectile
    void OnCollisionEnter(Collision coll)
    {
        GameObject otherColl = coll.gameObject;
        if(otherColl.tag == "ProjectileHero")
        {
            ChangeColour(false, 50, -75, -75);
            if (_bound.onScreen)
            {
                Projectile p = otherColl.GetComponent<Projectile>();
                _health -= Main.GetWeaponDefinition(p.type).damage; //update health
                CheckHealth();
            }
            Destroy(otherColl);
        }
    }

    //determines if the enemy should die
    void CheckHealth()
    {
        if (_health <= 0)
        {
            UpdateScore(gameObject);
            Main.scriptReference.DestroyEnemy(gameObject);
        }
    }

    //updates the user score according to the type of enemy
    void UpdateScore(GameObject gA)
    {
        switch(gA.tag)
        {
            case "Enemy0":
                Score.scoreControllerReference.AddScore(5);
                break;
            case "Enemy1":
                Score.scoreControllerReference.AddScore(10);
                break;
            case "Enemy2":
                Score.scoreControllerReference.AddScore(15);
                break;
            default:
                break;
        }
    }

    //use to show the enemy taking damage or being on plasma fire
    void ChangeColour(bool reset, float changeInR=0, float changeInG=0, float changeInB=0)
    {
        foreach(ObjectColourPairs pair in _allGameObjectsInObject)
        {
            if (pair.ContainsRenderer())//checks to see if object can have colour changed
            {
                Color col = pair.GetColor();

                //changes colour if equal
                if (reset)
                {
                    //resets colour if not equal
                    pair.GetGameObject().GetComponent<Renderer>().material.color = pair.GetColor();
                    _colourChangeTime = 0;

                }
                else
                {

                    float rValue = col.r;
                    float gValue = col.g;
                    float bValue = col.b;

                    Color newCol = new Color();//note rgb is scaled to be between 0 and 1 in unity (instead of 0 - 255)

                    //following blocks changes rbg value keeping it within 0 and 1 through min and max
                    if (changeInR >= 0)
                    {
                        newCol.r = Mathf.Min(255, (rValue * 255 + changeInR)) / 255;
                    }
                    else
                    {
                        newCol.r = Mathf.Max(0, (rValue * 255 + changeInR)) / 255;
                    }

                    if (changeInG >= 0)
                    {
                        newCol.g = Mathf.Min(255, (gValue * 255 + changeInG)) / 255;
                    }
                    else
                    {
                        newCol.g = Mathf.Max(0, (gValue * 255 + changeInG)) / 255;
                    }

                    if (changeInB >= 0)
                    {
                        newCol.b = Mathf.Min(255, (bValue * 255 + changeInB)) / 255;
                    }
                    else
                    {
                        newCol.b = Mathf.Max(0, (bValue * 255 + changeInB)) / 255;
                    }


                    pair.GetGameObject().GetComponent<Renderer>().material.color = newCol;
                    _colourChangeTime = Time.time;
                }
            }
        }
    }

    //gets all of the children and the original object in a game object
    //gets children all the way down the tree (so all parts will change color there is many layers of game objects)
    void getAllChildren(Transform parent)
    {
        _allGameObjectsInObject.Add(new ObjectColourPairs(parent.gameObject));
        foreach (Transform child in parent)
        {
            if (child.childCount > 0)
            {
                getAllChildren(child);
            }
            else
            {
                _allGameObjectsInObject.Add(new ObjectColourPairs(child.gameObject));
            }
        }
    }
}
