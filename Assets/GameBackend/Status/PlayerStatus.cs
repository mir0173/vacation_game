﻿using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace GameBackend.Status
{
    public class PlayerStatus
    {
        private int baseHp { get; }
        public int addHp { get; set; }
        public float increaseHp { get; set; }

        public int maxHp
        {
            get { return (int)(baseHp * (increaseHp / 100 + 1) + addHp); }
        }

        public int nowHp { get; set; }
        public int shieldHp { get; set; }

        private int baseAtk { get; }
        public int addAtk { get; set; }
        public float increaseAtk { get; set; }

        public int atk
        {
            get { return (int)(baseAtk * (increaseAtk / 100 + 1) + addAtk); }
        }

        private int baseDef { get; }
        public int addDef { get; set; }
        public float increaseDef { get; set; }

        public int def
        {
            get { return (int)(baseDef * (increaseDef / 100 + 1) + addDef); }
        }

        public float crit { get; set; }
        public float critDmg { get; set; }
        public float[] dmgUp { get; set; }
        public float movePower { get; set; }
        public float energyRecharge { get; set; }

        public PlayerStatus(int baseHp, int baseAtk, int baseDef)
        {
            this.baseHp = baseHp;
            this.addHp = 0;
            this.increaseHp = 0;
            this.nowHp = this.maxHp;
            this.shieldHp = 0;
            this.baseAtk = baseAtk;
            this.addAtk = 0;
            this.increaseAtk = 0;
            this.baseDef = baseDef;
            this.addDef = 0;
            this.increaseDef = 0;
            this.crit = 5;
            this.critDmg = 50;
            this.dmgUp = new float[Tag.atkTagCount];
            
            this.movePower = 1.6f;
            this.energyRecharge = 100f;
        }

        public PlayerStatus(PlayerStatus copy)
        {
            this.baseHp = copy.baseHp;
            this.addHp = copy.addHp;
            this.increaseHp = copy.increaseHp;
            this.nowHp = copy.nowHp;
            this.shieldHp = copy.shieldHp;
            this.baseAtk = copy.baseAtk;
            this.addAtk = copy.addAtk;
            this.increaseAtk = copy.increaseAtk;
            this.baseDef = copy.baseDef;
            this.addDef = copy.addDef;
            this.increaseDef = copy.increaseDef;
            this.crit = copy.crit;
            this.critDmg = copy.critDmg;
            this.dmgUp = new float[Tag.atkTagCount];
            
            this.movePower=copy.movePower;
            this.energyRecharge = copy.energyRecharge;
            Array.Copy(copy.dmgUp, this.dmgUp, Tag.atkTagCount);
        }

        public int calculateTrueDamage(List<AtkTags> atkTags, float atkCoef=0, float hpCoef=0, float defCoef=0)
        {
            int dmg = (int)((atkCoef * atk + defCoef * def + hpCoef * maxHp)/100);
            if (Random.value < crit / 100)
            {
                dmg = (int)(dmg * (1 + critDmg / 100));
                atkTags.Add(AtkTags.criticalHit);
            }
            else if (atkTags.Contains(AtkTags.criticalHit))
            {
                atkTags.Remove(AtkTags.criticalHit);
            }
            
            foreach (AtkTags atkTag in atkTags)
            {
                dmg = (int)((dmgUp[(int)atkTag] / 100 + 1) * dmg);
            }
            return dmg;
        }
    }

    public interface IBuffStatus
    {
        public void buffStatus(PlayerStatus status);
    }
}