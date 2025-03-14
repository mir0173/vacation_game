﻿using System;
using GameBackend.Events;
using GameBackend.Status;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameBackend
{
    public class Enemy : Entity
    {
        protected int targetingRange=100;
        protected GameObject target;
        protected bool direction; // 오른쪽이 true
        protected bool staggered = false;
        protected bool isAttack;

        protected bool knockbacked = false;
        protected float forceSum = 0;
        protected float forceStaggered = 0;
        protected float staggerRisist = 0.4f;
        protected float knockbackRisist = 2f;
        protected float moveSpeed = 0.3f;
        
        protected float staggerTimer = 0;
        protected float knockbackTimer = 0;

        

        public GameObject getNearestPlayer(Vector2 position)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            GameObject nearest = null;
            float minDistance = float.MaxValue;

            foreach (GameObject player in players)
            {
                float distance = Vector2.Distance(position, player.transform.position);
                if (distance < minDistance && distance < targetingRange)
                {
                    minDistance = distance;
                    nearest = player;
                }
            }
            return nearest;
        }

        private void checkForce(float deltaTime)
        {
            forceSum -= deltaTime/5;
            forceStaggered -= deltaTime;
            forceSum = Mathf.Max(forceSum, 0);
            forceStaggered = Mathf.Max(forceStaggered, 0);
            
            if (forceStaggered > staggerRisist)
            {
                forceStaggered -= staggerRisist;
                staggered = true;
                staggerTimer = 0;
            }
            if (forceSum > knockbackRisist)
            {
                forceSum = 0;
                knockbacked = true;
                knockbackTimer = 0;
            }
        }
        
        private bool atk = false;
        private float atkTimer = 0;
        
        protected override void update(float deltaTime)
        {
            base.update(deltaTime);
            checkForce(deltaTime);
            if (knockbacked) { if (knockback(deltaTime)) return; }
            else if (staggered) { if (stagger(deltaTime)) return; }
            
            if (!target)
            {
                GameObject nearest = getNearestPlayer(transform.position);
                target = nearest;
            }

            if (target.transform.position.x >= this.transform.position.x) direction = true;
            else direction = false;
            float distance = Math.Abs(target.transform.position.x - transform.position.x);
            if (!direction && transform.localScale.x == 1 && (distance >= 0.30f || isAttack == true))
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (direction && transform.localScale.x == -1 && (distance >= 0.30f || isAttack == true))
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            if (distance < 0.5f)
            {
                if (!atk && atkTimer == 0) 
                {
                    atk = true;
                    animator.SetTrigger("atk");
                    isAttack = true;
                }

                if (atk)
                {
                    atkTimer += deltaTime;
                    if (atkTimer >= 1)
                    {
                        atk = false;
                        atkTimer = 0;
                    }
                    return;
                }
            }

            if(isAttack && animator.GetCurrentAnimatorStateInfo(0).IsName("idle"))
            {
                isAttack = false;
            }
            
            Vector3 pos = this.transform.position;
            pos.x += (direction?moveSpeed:-moveSpeed)*deltaTime;
            this.transform.position = pos;
        }

        protected override void OnTriggerStay2D(Collider2D other)
        {

        }

        public override void dmgtake(DmgGiveEvent dmg)
        {
            base.dmgtake(dmg);
            this.forceSum += dmg.force;
            this.forceStaggered += dmg.force;
        }

        public override bool stagger(float deltaTime)
        {
            if (staggerTimer == 0)
            {
                //경직 시작
                //Debug.Log("stagger");
            }

            Vector3 pos = this.transform.position;
            pos.x += (direction?-1:1)*(Mathf.Max(1-staggerTimer, 0))*deltaTime*0.7f;
            this.transform.position = pos;
            if (staggerTimer >= 1.5)
            {
                staggered = false;
                return false;
            }
            
            staggerTimer += deltaTime;
            return true;
        }

        public override bool knockback(float deltaTime)
        {
            this.target = null; //넉백 -> 타겟 초기화
            if (knockbackTimer == 0)
            {
                //넉백 시작
                //Debug.Log("knockback");
            }

            Vector3 pos = this.transform.position;
            pos.x += (direction?-1:1)*(Mathf.Max(3-knockbackTimer, 0))*deltaTime*0.5f;
            this.transform.position = pos;
            if (knockbackTimer >= 4)
            {
                staggered = false;
                knockbacked = false;
                return false;
            }

            knockbackTimer+=deltaTime;
            return true;
        }
    }
}