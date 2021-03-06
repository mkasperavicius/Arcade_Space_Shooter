﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("Set in Inspector")]
    public Vector2 rotMinMax = new Vector2(15, 90);
    public Vector2 driftMinMax = new Vector2(0.3f, 1f);
    public float lifeTime = 1.5f;
    public float fadeTime = 1.5f;



    [Header("Set Dynamically")]
    public static List<Color> allPossibleColors = null;

    public WeaponType type; //type of weapon
    public GameObject cube; //power up cube
    public TextMesh letter; //letter on cube
    public Vector3 rotPerSecond; //cube rotation per second
    public float birthTime; //time of birth
    public float duration; //how long weapon will last for
    public int direction;

    private Rigidbody _rigidbody;
    private BoundsCheck _boundsCheck;
    private Renderer _cubeRend;
    private int _randCurrentColourIndex; //if its a rand cube this is the index of its current color in the array
    private bool _randCube = false; //is it a rand cube


    void Awake()
    {
        cube = transform.Find("Cube").gameObject;
        letter = GetComponent<TextMesh>();
        _rigidbody = GetComponent<Rigidbody>();
        _boundsCheck = GetComponent<BoundsCheck>();
        _cubeRend = cube.GetComponent<Renderer>();
        

        Vector3 vel = Random.onUnitSphere;
        direction = Random.Range(0, 2);
        //Assigns a random x direction and velocity
        if(direction == 0)
        {
            direction = -2;
        }
        else
        {
            direction = 2;
        }

        vel.z = 0;
        vel.Normalize();

        //Assigns the x and y velocities depending on the result of all prior random events
        vel.x = direction*Random.Range(driftMinMax.x, driftMinMax.y);
        vel.y = -15f*Random.Range(driftMinMax.x, driftMinMax.y);
        _rigidbody.velocity = vel;

        transform.rotation = Quaternion.identity;

        rotPerSecond = new Vector3(Random.Range(rotMinMax.x, rotMinMax.y), Random.Range(rotMinMax.x, rotMinMax.y), Random.Range(rotMinMax.x, rotMinMax.y)); //rotation per second is constant throughout life
        birthTime = Time.time;

    }

    void Update()
    {

        cube.transform.rotation = Quaternion.Euler(rotPerSecond * Time.time);
        float timeColor = (Time.time - (birthTime + lifeTime)) / fadeTime;

        //destroys the powerup if it is on screen too long, or if it goes off screen
        if(timeColor >= 1 || !_boundsCheck.onScreen)
        {
            if (_randCube)
            {
                CancelInvoke("ColorChange"); //stops invoke of color change as object is destroyed 
            }
            Destroy(this.gameObject);
            return;
        }

        // deals with fading
        if(timeColor > 0)
        {
            Color c = _cubeRend.material.color;
            c.a = 1f - timeColor;
            _cubeRend.material.color = c;

            c = letter.color;
            c.a = 1f - (timeColor * 0.75f);
            letter.color = c;
        }        
    }

    //sets the powerup type depending on what WeaponDefinition / WeaponType is associated with it.
    public void SetType(WeaponType wt, bool isRandCube=false)
    {
        WeaponDefinition def = Main_MainScene.GetWeaponDefinition(wt);
        type = wt;
        duration = def.duration;
        if (isRandCube)
        {
            _randCurrentColourIndex = Random.Range(0, allPossibleColors.Count - 1);
            _cubeRend.material.color = allPossibleColors[_randCurrentColourIndex];
            letter.text = "?";
            _randCube = true;
            Invoke("ColorChange", 0.1f); //invokes function to change color
        }
        else
        {
            _cubeRend.material.color = def.color;
            letter.text = def.letter;
        }
        
    }

    //Function that is invoked repeatedly to cycle through all other powerup colors, if the cube is 'Random'
    private void ColorChange()
    {
        float timeChange = (Time.time - (birthTime + lifeTime)) / fadeTime;
        _randCurrentColourIndex = (_randCurrentColourIndex + 1) % allPossibleColors.Count; //loop through the array of colors
        Color c = allPossibleColors[_randCurrentColourIndex];
        c.a = 1f - timeChange; //fading color to keep look consistent as object dies
        _cubeRend.material.color = c;
        Invoke("ColorChange", 0.1f); //invoke the method again
    }

    //destroys the powerup when appropriate
    public void AbsorbedBy(GameObject target)
    {
        if (_randCube)
        {
            CancelInvoke("ColorChange");
        }
        Destroy(this.gameObject);
    }
}
