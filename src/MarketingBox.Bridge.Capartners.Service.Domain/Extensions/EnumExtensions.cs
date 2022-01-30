using System;
using System.ComponentModel;
using System.Reflection;

namespace MarketingBox.Bridge.SimpleTrading.Service.Domain.Extensions
{
    public static class EnumExtensions
    {
        public static T MapEnum<T>(this object value)
            where T : struct, IConvertible
        {
            if (value == null)
            {
                throw new ArgumentException($"Value was null while mapping {typeof(T)}");
            }

            var sourceType = value.GetType();
            if (!sourceType.IsEnum)
                throw new ArgumentException($"Source type is not enum, while mapping {typeof(T)}");
            if (!typeof(T).IsEnum)
                throw new ArgumentException($"Destination type is not enum, while mapping {typeof(T)}");
            return (T)Enum.Parse(typeof(T), value.ToString()!);
        }

        /// <summary>
        /// Приведение значения перечисления в удобочитаемый формат.
        /// </summary>
        /// <remarks>
        /// Для корректной работы необходимо использовать атрибут [Description("Name")] для каждого элемента перечисления.
        /// </remarks>
        /// <param name="enumElement">Элемент перечисления</param>
        /// <returns>Название элемента</returns>
        public static string GetDescription(Enum enumElement)
        {
            Type type = enumElement.GetType();

            MemberInfo[] memInfo = type.GetMember(enumElement.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }

            return enumElement.ToString();
        }
    }
}
