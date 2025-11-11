using Godot;
using System;
using System.ComponentModel;
using System.Reflection;

namespace Godot.Steamworks.Net.Utils;

public static class GodotSteamworksEditorExtensions
{
    public static string GetDescription(this Enum value)
    {
        FieldInfo? fi = value.GetType().GetField(value.ToString());
        if (fi == null)
            return value.ToString();

        DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (attributes != null && attributes.Length > 0)
            return attributes[0].Description;
        else
            return value.ToString();
    }
        public static TAttribute? GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
    {
        var type = value.GetType();
        var memberInfo = type.GetMember(value.ToString());
        if (memberInfo.Length > 0)
        {
            var attrs = memberInfo[0].GetCustomAttributes(typeof(TAttribute), false);
            if (attrs.Length > 0)
                return (TAttribute)attrs[0];
        }
        return null;
    }
}