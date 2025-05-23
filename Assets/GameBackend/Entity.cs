using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using GameBackend.Events;
using GameBackend.Status;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace GameBackend
{
    public abstract class Entity : MonoBehaviour
    {
        protected readonly InvokeManager invokeManager=new();
        
        private float _speed = 1;
        public float speed
        {
            get => _speed;
            set
            {
                _speed = value;
                animator.speed = _speed*TimeManager.timeRate;
            }
        }

        public bool dead { get; set; } = false;
        private bool rightBefore = false;

        public PlayerStatus statusData=new(1, 0, 0);

        public PlayerStatus status
        {
            get
            {
                var copy=new PlayerStatus(statusData);
                foreach (var buffs in buffStatus)
                {
                    buffs.buffStatus(copy);
                }
                return copy;
            }
            protected set => statusData = value;
        }

        public List<IEntityEventListener> eventListener { get; set; } = new();
        public List<IBuffStatus> buffStatus { get; set; } = new();

        public Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            TimeManager.registrarEntity(this);
        }

        public void addListener(IEntityEventListener listener)
        {
            this.eventListener.Add(listener);
        }

        public void addBuff(IBuffStatus buffStatus)
        {
            this.buffStatus.Add(buffStatus);
        }

        public void removeListener(IEntityEventListener listener)
        {
            this.eventListener.Remove(listener);
        }

        public void removeBuff(IBuffStatus buffStatus)
        {
            this.buffStatus.Remove(buffStatus);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
        }

        protected virtual void OnTriggerStay2D(Collider2D other)
        {
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            
        }
        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
        }

        public virtual void eventActive(EventArgs e)
        {
            foreach (var listener in eventListener)
            {
                listener.eventActive(e);
            }
        }

        protected virtual void update(float deltaTime)
        {
            invokeManager.update(deltaTime);
            this.eventListener.ToList().ForEach(listener => listener.update(deltaTime));
        }

        protected void destroy()
        {
            TimeManager.removerEntity(this);
            Destroy(this.gameObject);
        }

        public virtual void Update()
        {
            if (dead)
            {
                {
                    if (this.animator.GetCurrentAnimatorStateInfo(0).IsName("dead") && this.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                    {
                        destroy();
                    }
                }
                if (!rightBefore) return;
                else rightBefore = false;
            }

            update(TimeManager.deltaTime * speed);
        }

        public virtual void dmgtake(DmgGiveEvent dmg)
        {
            int realDmg = this.status.calculateTakeDamage(dmg.trueDmg, dmg.atkTags);

            if (status.shieldHp >= realDmg)
            {
                statusData.shieldHp -= realDmg;
            }
            else
            {
                this.statusData.shieldHp = 0;
                new DmgTakeEvent(realDmg-status.shieldHp, dmg.attacker, dmg.target, dmg.atkTags).trigger();
                statusData.nowHp -= realDmg;
                if (statusData.nowHp <= 0) die();
            }
        }

        public virtual bool stagger(float deltaTime)
        {
            return true;
        }

        public virtual bool knockback(float deltaTime)
        {
            return true;
        }


        public virtual void die()
        {
            if (dead) return;
            this.dead = true;
            this.rightBefore = true;
            this.animator.SetTrigger("dead");
        }

    }

    public class EmptyEntity : Entity
    {
        public static EmptyEntity Instance { get; private set; }

        protected override void update(float deltaTime)
        {
            base.update(deltaTime);
        }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this; // Singleton 초기화
            }
            else
            {
                Destroy(gameObject); // 중복된 매니저 제거
            }
        }
    }
}