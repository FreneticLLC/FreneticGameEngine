using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FreneticUtilities.FreneticDataSyntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        if (UINumberSetting.NumberTypes.Contains(memberType))
        {
            SettingNumeric settingInfo = member.GetCustomAttribute<SettingNumeric>() ?? new SettingNumeric(SettingNumericDisplayType.NumberBox, double.MinValue, double.MaxValue);
            return new UINumberSetting(memberType, settingInfo, styling, layout);
        }
        if (memberType.GetInterfaces().Any(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IStaticEnumerable<>)))
        {
            var helper = typeof(StaticEnumerable).GetMethod(nameof(StaticEnumerable.GetOptions)).MakeGenericMethod(memberType);
            IEnumerable options = (IEnumerable)helper.Invoke(null, null);
            return new UIDropdownSetting(memberType, options, container, styling, layout);
        }
        if (member.GetCustomAttribute<SettingDropdown>() is SettingDropdown dropdownSetting)
        {
            return new UIDropdownSetting(memberType, dropdownSetting.Options, container, styling, layout);
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

    public static HashSet<Type> NumberTypes = [typeof(float), typeof(double), typeof(decimal), .. IntegerTypes];

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

    public UIDropdownSetting(Type valueType, IEnumerable options, UIElement container, UIStyling styling, UILayout layout) : base(layout)
    {
        Dropdown = new(null, styling with { Padding = 5 }, layout.Container()) { Layer = container };
        foreach (object option in options)
        {
            Dropdown.AddLabelChoice(option.ToString(), styling, option);
        }
        Dropdown.OnChoiceSelect += (choice) => OnValueEmitted?.Invoke(choice.Tag);
        AddChild(Dropdown);
    }

    public override void AcceptValue(object value)
    {
        // TODO: make this more intuitive in fge
        // blud what even is this
        UIElement selection = Dropdown.Entries.Items.Find(element => element.Tag == value);
        if (selection is not null)
        {
            Dropdown.SelectChoice(selection);
        }
    }
}

public class UIDebugPanel : UIList
{
    public static UIStyling Styling = new()
    {
        Fill = Color4F.White,
        Stroke = Color4F.Black,
        StrokeWeight = 2,
        ShowBackground = true,
    };

    public UIElement Element;

    public UIDebugPanel(UIElement element, UIStyling styling, UILayout layout) : base(styling, layout)
    {
        Element = element;
        foreach ((string name, UIDebugMember debugMember) in element.ElementInternal.DebugMembers)
        {
            UIList entry = new(null, new UILayout()) { Spacing = 10, Vertical = false };
            entry.AddListItem(new UILabel(name, styling with { ShowBackground = false }, new UILayout()));
            if (UISetting.Create(debugMember.Info, debugMember.Type, this, styling, new UILayout().SetSize(300, 60)) is UISetting setting)
            {
                entry.AddListItem(setting);
                setting.AcceptValue(Element.GetDebugValue(name));
                setting.OnValueEmitted += value => Element.SetDebugValue(name, value);
            }
            AddListItem(entry);
        }
    }
}
