//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGEGraphics.ClientSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static FGECore.EntitySystem.EntityPhysicsCharacterProperty;
using static FGEGraphics.UISystem.UIElement;

namespace FGEGraphics.UISystem;

/// <summary>
/// Indicates that a property or field should be displayed in debug mode (when <see cref="ViewUI2D.IsDebug"/> is <c>true</c>).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class UIDebugAttribute : Attribute
{
}

/// <summary>Data for a class member marked with <see cref="UIDebugAttribute"/>.</summary>
/// <param name="Info">The <see cref="MemberInfo"/> of the member.</param>
/// <param name="Type">The type of the member.</param>
/// <param name="Getter">A delegate returning the current value of the member from an instance. This is <c>null</c> if the member is write-only.</param>
/// <param name="Setter">A delegate to set the value of the member on an instance. This is <c>null</c> if the member is read-only.</param>
public record UIDebugMember(MemberInfo Info, Type Type, Func<object, object> Getter, Action<object, object> Setter)
{
    /// <summary>A map of element types to caches of <see cref="UIDebugMember"/>s keyed by name.</summary>
    public static Dictionary<Type, Dictionary<string, UIDebugMember>> CacheByElementType = [];

    /// <summary>Compiles a lambda to get the value of the <paramref name="typedMember"/> from the <paramref name="instance"/>.</summary>
    public static Func<object, object> CreateGetter(ParameterExpression instance, MemberExpression typedMember)
    {
        var member = Expression.Convert(typedMember, typeof(object));
        return Expression.Lambda<Func<object, object>>(member, instance).Compile();
    }

    /// <summary>Compiles a lambda to set the value of <paramref name="typedMember"/> on the <paramref name="instance"/> given <paramref name="memberType"/>.</summary>
    public static Action<object, object> CreateSetter(ParameterExpression instance, MemberExpression typedMember, Type memberType)
    {
        var value = Expression.Parameter(typeof(object), "value");
        var typedValue = Expression.Convert(value, memberType);
        var assignment = Expression.Assign(typedMember, typedValue);
        return Expression.Lambda<Action<object, object>>(assignment, instance, value).Compile();
    }

    /// <summary>Creates a <see cref="UIDebugMember"/>.</summary>
    /// <param name="elementType">The specific type of <see cref="UIElement"/>.</param>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> of the member marked with <see cref="UIDebugAttribute"/>.</param>
    public static UIDebugMember Create(Type elementType, MemberInfo memberInfo)
    {
        var instance = Expression.Parameter(typeof(object), "element");
        var typedInstance = Expression.Convert(instance, elementType);
        (var typedMember, var memberType, bool canRead, bool canWrite) = memberInfo switch
        {
            PropertyInfo property => (Expression.Property(typedInstance, property), property.PropertyType, property.CanRead, property.CanWrite),
            FieldInfo field => (Expression.Field(typedInstance, field), field.FieldType, true, true),
            _ => throw new ArgumentException()
        };
        Func<object, object> getter = canRead ? CreateGetter(instance, typedMember) : null;
        Action<object, object> setter = canWrite ? CreateSetter(instance, typedMember, memberType) : null;
        return new(memberInfo, memberType, getter, setter);
    }

    /// <summary>Creates or caches <see cref="UIDebugMember"/>s for a specific <paramref name="elementType"/>.</summary>
    public static Dictionary<string, UIDebugMember> GetOrCreateDebugMembers(Type elementType)
    {
        if (CacheByElementType.TryGetValue(elementType, out Dictionary<string, UIDebugMember> found))
        {
            return found;
        }
        Dictionary<string, UIDebugMember> members = [];
        foreach (MemberInfo memberInfo in elementType.GetMembers())
        {
            if (memberInfo.IsDefined(typeof(UIDebugAttribute), true))
            {
                members[memberInfo.Name] = Create(elementType, memberInfo);
            }
        }
        CacheByElementType[elementType] = members;
        return members;
    }
}
