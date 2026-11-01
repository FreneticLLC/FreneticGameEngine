using BepuPhysics.Trees;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.UISystem.Elements;
using FreneticUtilities.FreneticDataSyntax;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FGEGraphics.UISystem;

public abstract class UISetting(UIStyling styling, UILayout layout) : UIElement(styling, layout)
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
        if (memberType == typeof(Vector2i))
        {
            return new UIVector2iSetting(styling, layout);
        }
        return null;
    }
}

public class UIBoolSetting : UISetting, IStylingAcceptor<UIBoolSetting.StylingOptions>
{
    public record StylingOptions : StylingComponent
    {
        public UIStyleValue<UIStyling> ButtonStyling;
    }

    public UISelectionGroup Selections;

    public UIButton Yes, No;

    public UIBoolSetting(UIStyling styling, UILayout layout) : base(styling, layout)
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

    public void AcceptStyling(StylingOptions styling)
    {
        Yes.Styling = styling.ButtonStyling.Get(this);
        No.Styling = styling.ButtonStyling.Get(this);
    }
}

public class UINumberSetting : UISetting
{
    /// <summary>Set of core C# data types for basic integer value types.</summary>
    public static HashSet<Type> IntegerTypes = [typeof(int), typeof(long), typeof(short), typeof(byte), typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte)];

    public static HashSet<Type> NumberTypes = [typeof(float), typeof(double), typeof(decimal), .. IntegerTypes];

    public UINumberInputLabel InputLabel;

    public UINumberSlider Slider;

    public UINumberSetting(Type numberType, SettingNumeric numeric, UIStyling styling, UILayout layout) : base(styling, layout)
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

    public UIDropdownSetting(Type valueType, IEnumerable options, UIElement container, UIStyling styling, UILayout layout) : base(styling, layout)
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

public class UIVector2iSetting : UISetting
{
    public UINumberSetting SettingX;

    public UINumberSetting SettingY;

    public UIVector2iSetting(UIStyling styling, UILayout layout) : base(styling, layout)
    {
        AddChild(SettingX = new(typeof(int), new SettingNumeric(SettingNumericDisplayType.NumberBox, int.MinValue, int.MaxValue), styling, layout.Copy().SetOrigin().SetWidth(() => layout.Width / 2)));
        AddChild(SettingY = new(typeof(int), new SettingNumeric(SettingNumericDisplayType.NumberBox, int.MinValue, int.MaxValue), styling, SettingX.Layout.Copy().SetX(() => SettingX.Width)));
        SettingY.OnValueEmitted += SettingX.OnValueEmitted += _ =>
        {
            OnValueEmitted?.Invoke(new Vector2i(
                (int)SettingX.InputLabel.Value,
                (int)SettingY.InputLabel.Value
            ));
        };
    }

    public override void AcceptValue(object value)
    {
        Vector2i vector = (Vector2i)value;
        SettingX.AcceptValue(vector.X);
        SettingY.AcceptValue(vector.Y);
    }
}

public class UIElementDebugPanel : UIList, IStylingAcceptor<UIElementDebugPanel.StylingOptions>
{
    public record StylingOptions : StylingComponent
    {
        public UIStyleValue<UIStyling> LabelStyling;
    }

    public UIElement Element;

    public List<UILabel> Labels = [];

    public UIElementDebugPanel(UIElement element, UIStyling styling, UILayout layout) : base(styling, layout)
    {
        Element = element;
        foreach ((string name, UIDebugMember debugMember) in element.ElementInternal.DebugMembers)
        {
            UIList entry = new(null, new UILayout()) { Spacing = 10, Vertical = false };
            UILabel label = new(name, null, new UILayout());
            Labels.Add(label);
            entry.AddListItem(label);
            if (UISetting.Create(debugMember.Info, debugMember.Type, this, styling, new UILayout().SetSize(100, 30)) is UISetting setting)
            {
                entry.AddListItem(setting);
                setting.AcceptValue(Element.GetDebugValue(name));
                setting.OnValueEmitted += value => Element.SetDebugValue(name, value);
            }
            AddListItem(entry);
        }
    }

    public void AcceptStyling(StylingOptions options)
    {
        foreach (UILabel label in Labels)
        {
            label.Styling = options.LabelStyling.Get(this);
        }
    }
}

public class UIElementTree : UIList2, IStylingAcceptor<UIElementTree.StylingOptions>
{
    public record StylingOptions : StylingComponent
    {
        public UIStyleValue<UIStyling> LabelStyling;
    }

    public UIDebugPanel Panel;
    public UIElementTree ParentTree;
    public UIElement Element;
    public UILabel Label;

    public OrderedDictionary<UIElement, UIElementTree> ChildTrees = [];

    public UIElementTree(UIDebugPanel panel, UIElementTree parent, UIElement element, UIStyling styling, UILayout layout) : base(null, layout)
    {
        Styling = styling with { ShowBackground = false };
        Panel = panel;
        ParentTree = parent;
        Element = element;
        Label = new(element.Name, null);
        AddChild(Label);
        foreach (UIElement child in Element.Children)
        {
            UIElementTree tree = new(panel, this, child, styling, new UILayout()) { PaddingLeft = 10 };
            ChildTrees.Add(child, tree);
            AddChild(tree);
        }
        Label.OnClick += () =>
        {
            if (View.Client.Window.KeyboardState.IsKeyDown(Keys.LeftControl))
            {
                Panel.OpenElementView(Element);
                return;
            }
            foreach ((_, UIElementTree tree) in ChildTrees)
            {
                ToggleChild(tree);
            }
        };
    }

    public void AcceptStyling(StylingOptions options)
    {
        Label.Styling = options.LabelStyling.Get(this);
    }
}

public class UIDebugPanel : UIList2
{
    public UIElement MainArea;

    public UIElementDebugPanel ElementView;

    public UIDebugPanel(UIElement root, UIStyling styling, UILayout layout) : base(null, new UILayout())
    {
        Vertical = false;
        Spacing = 10;
        AddChild(MainArea = new(styling, layout));
        UIScrollGroup scrollGroup = new(MainArea.Layout.Container());
        UIElementTree tree = new(this, null, root, styling, new UILayout());
        scrollGroup.AddScrollableChild(tree);
        scrollGroup.XAxis.AddScrollBar(styling, 10);
        scrollGroup.YAxis.AddScrollBar(styling, 10);
        tree.OnSizeChange += (_, size) =>
        {
            scrollGroup.XAxis.MaxValue = size.X;
            scrollGroup.YAxis.MaxValue = size.Y;
        };
        MainArea.AddChild(scrollGroup);
    }

    public void OpenElementView(UIElement element)
    {
        if (HasChild(ElementView))
        {
            RemoveChild(ElementView);
        }
        ElementView = new(element, MainArea.Styling, new UILayout().SetSize(500, 300));
        AddChild(ElementView);
    }
}
