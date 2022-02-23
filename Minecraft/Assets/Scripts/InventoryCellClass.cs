using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class InventoryCellClass : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public delegate void OnCellTransport(bool QuickSlot);
    public delegate void OnCellChanged();
    public event OnCellChanged OnChanged;
    public event OnCellTransport OnClick;
    public ItemStack Stack;
    public TextMeshProUGUI ItemCountDisplay;
    public Image ItemDisplay;
    [SerializeField] private bool isTransport = false;
    [SerializeField] private InventoryCellClass TransportCell;
    [SerializeField] private HotbarNavigator HotBarManager;

    public bool OneWay = false;

    [HideInInspector] public bool Accessed;

    private bool MouseOver = false;

    void Start()
    {
        UpdateCell();
    }

    public void UpdateCell()
    {
        OnChanged?.Invoke();

        if (Stack == null)
        {
            ItemDisplay.sprite = null;
            ItemDisplay.color = new Color(0, 0, 0, 0);
            ItemCountDisplay.SetText(string.Empty);
            return;
        }

        if (Stack.Count <= 0)
        {
            Stack = null;
            UpdateCell();
            return;
        }

        ItemDisplay.sprite = ItemManager.Instance.ItemImages[Stack.CurrentStackedItem.GetID()];
        ItemDisplay.color = new Color(255, 255, 255, 255);
        ItemCountDisplay.SetText(Stack.Count > 1 ? Stack.Count.ToString() : string.Empty);
    }

    void Update()
    {
        if(isTransport)
        {
            transform.position = new Vector3(Input.mousePosition.x + .1f, Input.mousePosition.y + .1f, 0);
        }

        if(MouseOver)
        {
            QuickSlot();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked Cell");

        if(eventData.button == PointerEventData.InputButton.Left)
        {
            if(TransportCell.Stack != null && OneWay == false)
            {
                if(Stack != null)
                {
                    if (TransportCell.Stack.CurrentStackedItem.GetID() == Stack.CurrentStackedItem.GetID())
                    {
                        if(TransportCell.Stack.CurrentStackedItem.GetProperties().Stackable && Stack.CurrentStackedItem.GetProperties().Stackable)
                        {
                            int CellStackCount = Stack.Count;
                            int TransportCellCount = TransportCell.Stack.Count;

                            Stack.AddToStack(TransportCellCount, out int Overflow);
                            TransportCell.Stack.Count -= 64 - CellStackCount;
                        }
                    }
                }
                else
                {
                    Stack = TransportCell.Stack;
                    TransportCell.Stack = null;
                }

            }
            else
            {
                if(Stack != null)
                {
                    if(TransportCell.Stack == null)
                    {
                        TransportCell.Stack = Stack;
                        Stack = null;
                    }
                    else
                    {
                        if (TransportCell.Stack.CurrentStackedItem.GetID() == Stack.CurrentStackedItem.GetID())
                        {
                            if (TransportCell.Stack.CurrentStackedItem.GetProperties().Stackable && Stack.CurrentStackedItem.GetProperties().Stackable)
                            {
                                int CellStackCount = Stack.Count;
                                int TransportCellCount = TransportCell.Stack.Count;

                                TransportCell.Stack.AddToStack(CellStackCount, out int Overflow);
                                Stack.Count -= 64 - TransportCellCount;
                            }
                        }
                    }
                }
            }


        }
        else if(eventData.button == PointerEventData.InputButton.Right)
        {
            if(Stack != null && TransportCell.Stack != null)
            {
                if(Stack.CurrentStackedItem.GetID() == TransportCell.Stack.CurrentStackedItem.GetID())
                {
                    if(Stack.Count < 64)
                    {
                        Stack.Count++;
                        TransportCell.Stack.Count--;
                    }
                }
            }
            else if(Stack == null && TransportCell.Stack != null)
            {
                Stack = new ItemStack(TransportCell.Stack.CurrentStackedItem, 1);
                TransportCell.Stack.Count--;
            }
        }
        
        TransportCell.UpdateCell();
        OnClick?.Invoke(false);
        UpdateCell();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        MouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MouseOver = false;
    }

    private void QuickSlot()
    {

        int TargetSlot = -1;

        // pulled a yandere dev, im sorry
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            TargetSlot = 9;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TargetSlot = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TargetSlot = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TargetSlot = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TargetSlot = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TargetSlot = 4;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            TargetSlot = 5;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            TargetSlot = 6;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            TargetSlot = 7;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            TargetSlot = 8;
        }


        if (TargetSlot == -1) return;

        ItemStack targetstack = HotBarManager.HotBarCellRoots[TargetSlot].Stack;
        if (HotBarManager.HotBarCellRoots[TargetSlot].Stack != null && Stack != null)
        {
            if (HotBarManager.HotBarCellRoots[TargetSlot].Stack.CurrentStackedItem.GetID() == Stack.CurrentStackedItem.GetID())
            {
                if(HotBarManager.HotBarCellRoots[TargetSlot].Stack.Count < 64)
                {
                    int TransportCellCount = HotBarManager.HotBarCellRoots[TargetSlot].Stack.Count;

                    HotBarManager.HotBarCellRoots[TargetSlot].Stack.AddToStack(TransportCellCount, out int Overflow);
                    Stack.Count -= 64 - TransportCellCount;
                }

            }
            else
            {
                HotBarManager.HotBarCellRoots[TargetSlot].Stack = new ItemStack(Stack.CurrentStackedItem, Stack.Count);
                Stack = targetstack == null ? null : new ItemStack(targetstack.CurrentStackedItem, targetstack.Count);
            }
        }
        else
        {
            HotBarManager.HotBarCellRoots[TargetSlot].Stack = new ItemStack(Stack.CurrentStackedItem, Stack.Count);
            Stack = targetstack == null ? null : new ItemStack(targetstack.CurrentStackedItem, targetstack.Count);
        }

        HotBarManager.HotBarCellRoots[TargetSlot].UpdateCell();
        OnClick?.Invoke(true);
        UpdateCell();
    }
}
