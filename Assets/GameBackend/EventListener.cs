﻿using System.Collections.Generic;
using UnityEngine;

namespace GameBackend
{
    [System.Serializable]
    public abstract class EventArgs
    {
        public string name { get; protected set; }
        public abstract void trigger();
    }
    
    public interface IEntityEventListener
    {
        public void eventActive<T>(T eventArgs) where T : EventArgs;
        public void registrarTarget(Entity target);
        public void removeSelf();
        public void update(float deltaTime);
    }

    
    
    
}