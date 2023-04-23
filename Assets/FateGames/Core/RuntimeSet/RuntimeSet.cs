using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FateGames.Core
{
    public abstract class RuntimeSet<T> : ScriptableObject
    {
        public List<T> Items = new();

        public virtual void Add(T t)
        {
            if (!Items.Contains(t)) Items.Add(t);
        }

        public virtual void Remove(T t)
        {
            if (Items.Contains(t)) Items.Remove(t);
        }
    }
}
