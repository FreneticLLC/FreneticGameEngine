using System;
using System.Collections.Generic;
using System.Text;

namespace FGEGraphics.UISystem;

/// <summary>How to display a numeric setting.</summary>
public enum SettingNumericDisplayType
{
    /// <summary>Display as a slider (with number input on the side). The slider's position should be linear.</summary>
    Slider,
    /// <summary>Display as a slider (with number input on the side). The slider's position should be logarithmic rather than linear.</summary>
    LogSlider,
    /// <summary>Display as a number text box (no slider).</summary>
    NumberBox
}

/// <summary>Attribute for numeric settings. Define the display type and range limits.</summary>
[AttributeUsage(AttributeTargets.Field)]
public class SettingNumeric(SettingNumericDisplayType displayType, double min, double max) : Attribute
{
    /// <summary>The type of display for this setting (slider vs simple number text box).</summary>
    public SettingNumericDisplayType DisplayType = displayType;

    /// <summary>The minimum value for this setting. The user may not set values below this.</summary>
    public double Min = min;

    /// <summary>The maximum value for this setting. The user may not set values above this.</summary>
    public double Max = max;

    /// <summary>The minimum value when rendering a slider.
    /// If this is more than <see cref="Min"/>, the user may manually type a number lower than the slider limit.
    /// If this is less than <see cref="Min"/>, the slider should visually extend to this value but prevent being dragged past the real min.</summary>
    public double ViewMin = min;

    /// <summary>The maximum value when rendering a slider.
    /// If this is less than <see cref="Max"/>, the user may manually type a number higher than the slider limit.
    /// If this is more than <see cref="Max"/>, the slider should visually extend to this value but prevent being dragged past the real max.</summary>
    public double ViewMax = max;

    /// <summary>The tick interval when rendering a slider.</summary>
    public double Interval;

    /// <summary>The number of fractional digits this setting should display. 
    /// For decimal settings, this defaults to <c>1</c>.</summary>
    public int Digits = -1;
}

/// <summary>Attribute to mark that a setting is 'advanced', it should not be visible in the UI unless the user requests to view advanced settings.</summary>
[AttributeUsage(AttributeTargets.Field)]
public class SettingAdvanced : Attribute
{
}

/// <summary>Attribute to mark that a setting is 'developer mode', it should not be visible in the UI unless the user has indicated they are a developer and need developer settings.</summary>
[AttributeUsage(AttributeTargets.Field)]
public class SettingDeveloperMode : Attribute
{
}

/// <summary>Attribute for a setting that should operate as a dropdown of preset options.</summary>
[AttributeUsage(AttributeTargets.Field)]
public class SettingDropdown : Attribute
{
    /// <summary>Abstract base impl of a class that gives the options list.</summary>
    public interface IImpl
    {
        /// <summary>Implement this with a method that gives the relevant options.</summary>
        public abstract object[] GetOptions { get; }
    }

    /// <summary>Type the implements a getter for the option list.</summary>
    public Type Impl;

    /// <summary>The options to display in the dropdown.</summary>
    public virtual object[] Options => (Activator.CreateInstance(Impl) as IImpl).GetOptions;
}


/// <summary>Implements <see cref="SettingDropdown"/> with a manual list of options..</summary>
[AttributeUsage(AttributeTargets.Field)]
public class ManualSettingsDropdown(params object[] options) : SettingDropdown
{
    /// <summary>The actual underlying options.</summary>
    public object[] ActualOptions = options;

    /// <inheritdoc/>
    public override object[] Options => ActualOptions;
}

public interface IStaticEnumerable<TSelf> where TSelf : IStaticEnumerable<TSelf>
{
    public static abstract IEnumerable<TSelf> Options { get; }
}

public static class StaticEnumerable
{
    public static IEnumerable<T> GetOptions<T>() where T : IStaticEnumerable<T> => T.Options;
}
