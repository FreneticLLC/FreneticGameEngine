using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FGEGraphics.UISystem;

public interface IStylingAcceptor<T> where T: StylingComponent
{
    public void AcceptStyling(T styling);
}

public abstract record StylingComponent
{
    public UIStyling AsStyling() => new() { Components = [this] };

    public static implicit operator UIStyling(StylingComponent component) => component.AsStyling();
}

public class StylingAcceptors
{
    public delegate void Applicator(UIElement element, StylingComponent component);

    public static Dictionary<Type, Applicator> Applicators = [];

    public static Dictionary<Type, Dictionary<Type, Applicator>> ApplicatorsByElementType = [];

    public static Applicator CompileApplicator(Type iface, Type componentType)
    {
        MethodInfo acceptMethod = iface.GetMethod(nameof(IStylingAcceptor<>.AcceptStyling));
        ParameterExpression elementParam = Expression.Parameter(typeof(UIElement));
        ParameterExpression componentParam = Expression.Parameter(typeof(StylingComponent));
        MethodCallExpression body = Expression.Call(Expression.Convert(elementParam, iface), acceptMethod, Expression.Convert(componentParam, componentType));
        return Expression.Lambda<Applicator>(body, elementParam, componentParam).Compile();
    }

    public static Dictionary<Type, Applicator> GetOrCreateApplicators(Type elementType)
    {
        if (ApplicatorsByElementType.TryGetValue(elementType, out Dictionary<Type, Applicator> found))
        {
            return found;
        }
        Dictionary<Type, Applicator> applicatorsForElement = [];
        foreach (Type iface in elementType.GetInterfaces())
        {
            if (!iface.IsGenericType || iface.GetGenericTypeDefinition() != typeof(IStylingAcceptor<>))
            {
                continue;
            }
            Type componentType = iface.GetGenericArguments()[0];
            if (!Applicators.ContainsKey(componentType))
            {
                Applicators[componentType] = CompileApplicator(iface, componentType);
            }
            applicatorsForElement[componentType] = Applicators[componentType];
        }
        ApplicatorsByElementType[elementType] = applicatorsForElement;
        return applicatorsForElement;
    }
}
