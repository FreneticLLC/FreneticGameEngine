using FGECore.CoreSystems;
using FreneticUtilities.FreneticDataSyntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FGEGraphics.UISystem;

public abstract class UISetting(UILayout layout) : UIElement(null, layout)
{
    public abstract void AcceptValue(object value);

    public Action<object> OnValueEmitted;

    public static UISetting Create(MemberInfo member, Type memberType, UIElement container, UIStyling styling, UILayout layout)
    {
        if (memberType == typeof(bool))
        {
            return new UIBoolSetting(styling, layout);
        }
        if (member.GetCustomAttribute<SettingNumeric>() is SettingNumeric numericSetting)
        {
            return new UINumberSetting(memberType, numericSetting, styling, layout);
        }
        if (member.GetCustomAttribute<SettingDropdown>() is SettingDropdown dropdownSetting)
        {
            return new UIDropdownSetting(memberType, dropdownSetting, container, styling, layout);
        }
        return null;
    }
}

public class UIBoolSetting : UISetting
{
    public UISelectionGroup Selections;

    public UIButton Yes, No;

    public UIBoolSetting(UIStyling styling, UILayout layout) : base(layout)
    {
        Selections = new(new UILayout()) { MinSelections = 1, MaxSelections = 1, IsCyclic = true };
        Yes = new("Yes", styling, layout.Copy().SetOrigin().SetWidth(() => layout.Width / 2));
        No = new("No", styling, Yes.Layout.Copy().SetX(() => Yes.Width));
        Selections.AddElement(Yes, addChild: false);
        Selections.AddElement(No, addChild: false);
        AddChild(Yes);
        AddChild(No);
        Selections.OnSelectElement += (off, on) => OnValueEmitted?.Invoke(on == Yes);
    }

    public override void AcceptValue(object value)
    {
        Selections.SelectElement((bool)value ? Yes : No);
    }
}

public class UINumberSetting : UISetting
{
    /// <summary>Set of core C# data types for basic integer value types.</summary>
    public static HashSet<Type> IntegerTypes = [typeof(int), typeof(long), typeof(short), typeof(byte), typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte)];

    public UINumberInputLabel InputLabel;

    public UINumberSlider Slider;

    public UINumberSetting(Type numberType, SettingNumeric numeric, UIStyling styling, UILayout layout) : base(layout)
    {
        bool integer = IntegerTypes.Contains(numberType);
        string labelFormat = integer ? "0" : (numeric.Digits >= 1 ? "0." + new string('0', numeric.Digits) : "0.0");
        // TODO: box with 2 padding
        InputLabel = new(integer, styling, styling, styling, layout.Container(), format: labelFormat) { Min = numeric.Min, Max = numeric.Max };
        if (numeric.DisplayType == SettingNumericDisplayType.NumberBox)
        {
            InputLabel.OnTextSubmit += _ => OnValueEmitted?.Invoke(Convert.ChangeType(InputLabel.Value, numberType));
            AddChild(InputLabel);
        }
        else if (numeric.DisplayType == SettingNumericDisplayType.Slider)
        {
            UILayout container = layout.Container();
            // todo: optimize
            InputLabel.Layout.SetWidth(() => (int)(container.Width * 0.3));
            Slider = new(numeric.ViewMin, numeric.ViewMax, 0.0, numeric.Interval, integer, styling, layout.Container().SetWidth(() => (int)(container.Width * 0.4))) { Digits = numeric.Digits };
            Slider.OnValueEdit += value => OnValueEmitted?.Invoke(Convert.ChangeType(value, numberType));
            UIList list = UINumberSlider.WithLabel(Slider, InputLabel, 10, new UILayout(), trackLabelEdits: true);
            AddChild(list);
        }
        // TODO: implement LogSlider
    }

    public override void AcceptValue(object value)
    {
        double number = Convert.ToDouble(value);
        InputLabel.Value = number;
        Slider?.Value = number;
    }
}

public class UIDropdownSetting : UISetting
{
    public UIDropdown Dropdown;

    public UIDropdownSetting(Type valueType, SettingDropdown dropdownData, UIElement container, UIStyling styling, UILayout layout) : base(layout)
    {
        Dropdown = new(null, styling with { Padding = 5 }, layout.Container()) { Layer = container };
        foreach (string option in dropdownData.Options)
        {
            object value = option;
            if (valueType == typeof(int)) { value = int.Parse(option); }
            else if (valueType == typeof(long)) { value = long.Parse(option); }
            else if (valueType == typeof(float)) { value = float.Parse(option); }
            else if (valueType == typeof(double)) { value = double.Parse(option); }
            Dropdown.AddLabelChoice(option, styling, value);
        }
        Dropdown.OnChoiceSelect += (choice) => OnValueEmitted?.Invoke(choice.Tag);
        AddChild(Dropdown);
    }

    public override void AcceptValue(object value)
    {
        // TODO: make this more intuitive in fge
        // blud what even is this
        UIElement selection = Dropdown.Entries.Items.Find(element => $"{element.Tag}" == $"{value}");
        if (selection is not null)
        {
            Dropdown.SelectChoice(selection);
        }
    }
}

