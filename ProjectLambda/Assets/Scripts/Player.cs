﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : CustomPhysicsObject
{
    public float move_speed = 7f;
    public float jump_force = 7f;

    public float grapple_speed = 10f;
    public float swing_force = 20f;

    public Grapple grapple;

    bool did_grapple_jump = false;

    protected override Vector2 setInputAcceleration()
    {
        Vector2 move = Vector2.zero;

        move.x = Input.GetAxis("Horizontal");
        move.y = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                Velocity = new Vector2(Velocity.x, jump_force);
            }
            else if (grapple.isGrappleConnected) {
                grapple.detachGrapple();
                releaseTether();
                if (Velocity.magnitude <= 1f)
                {
                    Velocity = new Vector2(Velocity.x, jump_force);
                }
                else
                {
                     //Velocity += move * jump_force;
                }
                did_grapple_jump = true;
            }
        }
        else if(Input.GetButtonUp("Jump")) {
            if (did_grapple_jump) {
                did_grapple_jump = false;
            }
            else if (Velocity.y > 0) {
                Velocity = new Vector2(Velocity.x, Velocity.y * 0.5f);
            }
        }
        return move * move_speed;
    }
    protected override void fixedUpdate()
    {
        if (grapple.isGrappleConnected)
        {
            setTetherPoint(grapple.GrapplePoint, grapple.MaxLength);
        }
        else {
            releaseTether();
        }
    }
}