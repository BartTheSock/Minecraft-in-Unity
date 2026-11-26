using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour
{
    public bool isLinked = false;
    public ItemSlot itemSlot;
    public Image slotImage;
    public Image slotIcon;
    public Text slotAmount;
    public Text slotAmountShadow;

    World world;

    private void Awake()
    {
        world = GameObject.Find("World").GetComponent<World>();
    }

    public bool HasItem
    {
        get
        {
            if (itemSlot == null)
                return false;
            else
                return itemSlot.HasItem;
        }
    }

    public void Link(ItemSlot _itemSlot)
    {
        itemSlot = _itemSlot;
        isLinked = true;
        itemSlot.LinkUISlot(this);
        UpdateSlot();
    }

    public void UnLink()
    {
        itemSlot.unLinkUISlot();
        itemSlot = null;
        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if (itemSlot != null && itemSlot.HasItem)
        {
            slotIcon.sprite = world.blockTypes[itemSlot.stack.id].icon;
            slotIcon.enabled = true;
            if (itemSlot.isCreative || itemSlot.stack.amount == 1)
            {
                slotAmount.text = "";
                slotAmountShadow.text = slotAmount.text;
                slotAmount.enabled = false;
                slotAmountShadow.enabled = false;
            }
            else
            {
                slotAmount.text = itemSlot.stack.amount.ToString();
                slotAmountShadow.text = slotAmount.text;
                slotAmountShadow.enabled = true;
                slotAmount.enabled = true;
            }
        }
        else
            Clear();
    }

    public void Clear()
    {
        slotIcon.sprite = null;
        slotIcon.enabled = false;
        slotAmount.text = "";
        slotAmountShadow.text = slotAmount.text;
        slotAmount.enabled = false;
        slotAmountShadow.enabled = false;
    }

    private void OnDestroy()
    {
        if (isLinked)
            itemSlot.unLinkUISlot();
    }
}

public class ItemSlot
{
    public ItemStack stack = null;
    private UIItemSlot uiItemSlot = null;
    public bool isCreative;

    public ItemSlot(UIItemSlot _uiItemSlot)
    {
        stack = null;
        uiItemSlot = _uiItemSlot;
        uiItemSlot.Link(this);
    }

    public ItemSlot(UIItemSlot _uiItemSlot, ItemStack _stack)
    {
        stack = _stack;
        uiItemSlot = _uiItemSlot;
        uiItemSlot.Link(this);
    }

    public void LinkUISlot(UIItemSlot uiSlot)
    {
        uiItemSlot = uiSlot;
    }

    public void unLinkUISlot()
    {
        uiItemSlot = null;
    }

    public void EmptySlot()
    {
        stack = null;
        if (uiItemSlot != null)
            uiItemSlot.UpdateSlot();
    }

    public int Take(int amt)
    {
        if (amt > stack.amount)
        {
            int _amt = stack.amount;
            EmptySlot();
            return _amt;
        }
        else if (amt < stack.amount)
        {
            stack.amount -= amt;
            uiItemSlot.UpdateSlot();
            return amt;
        }
        else
        {
            EmptySlot();
            return amt;
        }
    }

    public ItemStack TakeAll()
    {
        ItemStack handOver = new ItemStack(stack.id, stack.amount);
        EmptySlot();
        return handOver;
    }

    public ItemStack TakeHalf()
    {
        ItemStack handOver = new ItemStack(stack.id, Mathf.FloorToInt(stack.amount / 2));
        Take(Mathf.CeilToInt(stack.amount / 2));
        uiItemSlot.UpdateSlot();
        return handOver;
    }

    public ItemStack TakeOne()
    {
        ItemStack handOver = new ItemStack(stack.id, 1);
        Take(1);
        uiItemSlot.UpdateSlot();
        return handOver;
    }

    public void InsertStack(ItemStack _stack)
    {
        stack = _stack;
        uiItemSlot.UpdateSlot();
    }

    public void InsertSingle(ItemStack _stack)
    {
        stack = new ItemStack(_stack.id, 1);
        uiItemSlot.UpdateSlot();
    }

    public void UpdateUISlot()
    {
        uiItemSlot.UpdateSlot();
    }

    public bool HasItem
    {
        get
        {
            if (stack != null)
                return true;
            else
                return false;
        }
    }
}