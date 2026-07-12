using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FGEGraphics.UISystem;

public interface IStylingAcceptor<T>
{
    public void AcceptStyling(T styling);
}

public class StylingAcceptors
{
    public delegate void Applicator(UIElement element, object component);

    public static Dictionary<Type, Applicator> Applicators = [];

    public static Applicator CompileApplicator(Type iface, Type componentType)
    {
        MethodInfo acceptMethod = iface.GetMethod(nameof(IStylingAcceptor<>.AcceptStyling));
        ParameterExpression elementParam = Expression.Parameter(typeof(UIElement));
        ParameterExpression componentParam = Expression.Parameter(typeof(object));
        MethodCallExpression body = Expression.Call(Expression.Convert(elementParam, iface), acceptMethod, Expression.Convert(componentParam, componentType));
        return Expression.Lambda<Applicator>(body, elementParam, componentParam).Compile();
    }

    public static void RegisterApplicators(Type elementType)
    {
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
        }
    }
}
