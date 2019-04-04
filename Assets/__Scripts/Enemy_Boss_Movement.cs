﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Boss_Movement : Enemy_Parent
{
    public float speed;
    public delegate void fireWeapons(); //creates delegate type
    public fireWeapons fireWeaponsDelegate; //creates variable of type fireWeapons

    void Start()
    {
        _health = 200; // sets health from parent class to the appropriate value, depending on the enemy type.
    }

    //handles movement of the enemy
    protected override void Move()
    {
        Vector3 temporaryPosition = position;
        temporaryPosition.y -= 2f * Time.deltaTime * _speedFactor;
        position = temporaryPosition;
    }

    void Update()
    {
        if (fireWeaponsDelegate != null)
        {
            fireWeaponsDelegate();
        }
        base.Update();
    }
}