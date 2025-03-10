﻿using GameBackend;
using UnityEngine;

namespace GameFrontEnd.Effects.SkillEffects.TestSpecialSkill
{
    public class FlameVFX:SkillEffect
    {
        public float time{get;set;}
        
        private void Start()
        {
            timeIgnore=true;
            transform.localScale=new Vector3(0.1f,0.1f,0.1f);
        }

        protected override void update(float deltaTime)
        {
            timer += deltaTime;
            checkDestroy(time);
        }
    }
}