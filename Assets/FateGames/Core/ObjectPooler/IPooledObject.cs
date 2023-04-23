using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
using UnityEngine.Pool;
using System;

public interface IPooledObject
{
    public event Action Release;

    public abstract void OnObjectSpawn();
    public abstract void OnRelease();

}
