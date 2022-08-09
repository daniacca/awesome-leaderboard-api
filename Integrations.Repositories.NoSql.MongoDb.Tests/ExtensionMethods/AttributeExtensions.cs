using System;
using System.ComponentModel;
using System.Linq;

namespace NoSql.MongoDb.Tests.ExtensionMethods;

public static class AttributeExtensions
{
    public static TValue GetAttributeValue<TAttribute, TValue>(this Type type, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
    {
        var att = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
        if (att != null)
            return valueSelector(att);
        return default;
    }

    public static string GetDescription<TEnum>(this TEnum value) where TEnum : Enum
    {
        var type = typeof(TEnum);
        var member = type.GetMember(value.ToString());
        var attributes = member[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
        return ((DescriptionAttribute)attributes[0]).Description;
    }
}
