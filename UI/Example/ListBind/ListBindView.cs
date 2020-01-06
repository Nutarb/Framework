﻿/*
using System.Collections;
using System.Collections.Generic;

using AD.UI.Core;
using AD.UI.Example;
using UnityEngine;
using UnityEngine.UI;

public class ListBindView : View
{
    
    public ItemView item;
    public Dropdown dropdown;
    public Button deleteBtn;
    public Button addBtn;
    public Button updateBtn;
    private ListBindViewModel viewModel;

    protected override void OnVmChange()
    {
        viewModel = VM as ListBindViewModel;
        BindFactory<ListBindView, ListBindViewModel> binding =
            new BindFactory<ListBindView, ListBindViewModel>(this, viewModel);
        dropdown.options = viewModel.DropdownData;
        binding.RevertBind(dropdown, viewModel.SelectedDropDownIndex).InitBind();
        binding.BindList(viewModel.Items, item).InitBind();
        binding.BindCommand(addBtn,viewModel.AddItem).InitBind();
        binding.BindCommand(deleteBtn, viewModel.DeleteSelectedItem).InitBind();
        binding.BindCommand(updateBtn, viewModel.UpdateItem).InitBind();

    }
}

public class ListBindViewModel : ViewModel
{
    public BindableList<ItemViewModel> Items { get; private set; }
    public List<Dropdown.OptionData> DropdownData { get; private set; }
    public BindableField<int> SelectedDropDownIndex { get; private set; }
    private int selectedDropDownIndex
    {
        get { return SelectedDropDownIndex; }
        set { ((IBindableField<int>) SelectedDropDownIndex).Value = value; }
    }
    private ItemViewModel selectedItem;
    
    public ListBindViewModel()
    {
        Items = new BindableList<ItemViewModel>();
        SelectedDropDownIndex = new BindableField<int>(0);
        DropdownData = new List<Dropdown.OptionData>()
        {
            new Dropdown.OptionData("回锅肉"),
            new Dropdown.OptionData("梅菜扣肉"),
            new Dropdown.OptionData("水煮肉片"),
            new Dropdown.OptionData("辣子鸡丁"),
            new Dropdown.OptionData("鱼香肉丝"),
        };
        Items.AddListUpdateListener((list) => OnUpdateItem());
    }

    public void DeleteSelectedItem()
    {
        if(selectedItem == null) return;
        Items.Remove(selectedItem);
        selectedItem = null;
    }

    public void AddItem()
    {
        var vm = CreateItem();
        AddItem(vm);
    }

    public void UpdateItem()
    {
        if(selectedItem == null) return;
        selectedItem.Path.Value = DropdownData[selectedDropDownIndex].text;
    }
    
    private void AddItem(ItemViewModel itemViewModel)
    {
        Items.Add(itemViewModel);
    }

    private ItemViewModel CreateItem()
    {
        ItemViewModel vm = new ItemViewModel
        {
            Last = new BindableField<bool>(false),
            Path = new BindableField<string>(DropdownData[selectedDropDownIndex].text),
        };
        vm.OnItemClick = () => OnItemClick(vm);
        return vm;
    }

    private void OnItemClick(ItemViewModel viewModel)
    {
        if (selectedItem != null)
            selectedItem.Selected.Value = false;
        selectedItem = viewModel;
        selectedItem.Selected.Value = true;
    }

    private void OnUpdateItem()
    {
        var lastVm = Items[Items.Count - 1];
        foreach (var itemViewModel in Items)
        {
            itemViewModel.Last.Value = itemViewModel == lastVm;
        }
    }
    
}
*/