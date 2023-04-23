using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
using System.Linq;
[CreateAssetMenu(menuName = "Game/Car Platform Runtime Set")]
public class CarPlatformRuntimeSet : RuntimeSet<CarPlatform>
{
    public override void Add(CarPlatform t)
    {
        base.Add(t);
        Items = Items.OrderBy((carPlatform) => carPlatform.Order).ToList();
    }

    public override void Remove(CarPlatform t)
    {
        base.Remove(t);
        Items = Items.OrderBy((carPlatform) => carPlatform.Order).ToList();
    }
}
