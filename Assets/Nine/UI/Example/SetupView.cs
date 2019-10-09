using Assets.Nine.UI.Core;
using Assets.Nine.UI.Example;
using Nine.UI.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using State = Nine.Enums.State;

namespace Nine.UI.Example
{
    public class SetupView : View
    {

        public Text nameMessageText;
        public Text mulBindText;
        public InputField atkInputField;
        public Toggle joinToggle;
        public Button joinInButton;
        public Image img;
        public ItemView item;

        public SetupViewModel viewModel;

        void Start()
        {
            viewModel = new SetupViewModel();
            BindFactory<SetupView, SetupViewModel> binding = new BindFactory<SetupView, SetupViewModel>(this, viewModel);
            ////nameMessa|geText show or hide by vm.visible
            //binding.Bind(nameMessageText, (vm) => vm.Visible).OneWay();
            ////nameMessageText.text show text by vm.Name
            //binding.Bind(nameMessageText, (vm) => vm.Name).OneWay();
            ////mulBindText.text show text by (vm.ATK , vm.Name) , and wrap by third para
            //binding.Bind(mulBindText, (vm) => vm.Name, (vm) => vm.ATK, (name, atk) => $"name = {name} atk = {atk.ToString()}").OneWay();
            ////button bind vm.OnButtonClick
            //binding.BindCommand(joinInButton, (vm) => vm.OnButtonClick).Wrap(callback =>
            //{
            //    return () =>
            //    {
            //        callback();
            //        print("Wrap ��ť");
            //    };
            //}).OneWay();
            ////image bind path, when path changed, img.sprite change to res.load(path)
            //binding.Bind(img, (vm) => vm.Path).OneWay();
            //binding.Bind(joinToggle, (vm) => vm.Visible).Wrap((value) =>
            //{
            //    Log.I($"��Ϊ{value}");
            //    return value;
            //}).Revert();
            //binding.Bind(atkInputField, (vm) => vm.Name).Wrap((value) =>
            //{
            //    Log.I($"��Ϊ{value}");
            //    return value;
            //}).TwoWay();
            //binding.BindCommand<InputField, string>(atkInputField, (vm) => vm.OnInputChanged).Wrap((valueChangedFunc) =>
            //{
            //    return (value) =>
            //    {
            //        valueChangedFunc(value);
            //        print("Wrap InputField");
            //    };
            //}).OneWay();
            binding.BindList(item,viewModel.Items).Init();
        }
    }


}