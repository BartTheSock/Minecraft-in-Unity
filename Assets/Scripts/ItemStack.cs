using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack
{
    public ushort id;
    public int amount;

    public ItemStack(ushort _id, int _amount)
    {
        id = _id;
        amount = _amount;
    }
}