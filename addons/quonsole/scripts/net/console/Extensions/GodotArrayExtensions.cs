
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Quonsole.Extensions;

public static class GodotArrayExtensions
{
    public static Array ToGodotArray<[MustBeVariant] T>(this IEnumerable<T> collection)
    {
        var array = new Array();

        if (collection != null)
        {
            foreach (var item in collection)
            {
                array.Add(Variant.From(item));
            }
        }

        return array;
    }

    public static IEnumerable<T> FromGodotArray<[MustBeVariant] T>(this Array array)
    {
        if (array == null)
        {
            return System.Array.Empty<T>();
        }

        return array.Select(item => item.As<T>()).ToArray();
    }
}