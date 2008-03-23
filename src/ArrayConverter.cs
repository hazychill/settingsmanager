using System;
using System.ComponentModel;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;


namespace Hazychill.Setting {
  internal class ArrayConverter : TypeConverter {
    private Type _elementType;
    private TypeConverter _elementTypeConverter;

    public ArrayConverter(Type elementType, TypeConverter elementTypeConverter) {
      if (elementType == null) {
        throw new ArgumentNullException("elementType");
      }
      if (elementTypeConverter == null) {
        throw new ArgumentNullException("elementTypeConverter");
      }

      _elementType = elementType;
      _elementTypeConverter = elementTypeConverter;
    }

    public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType) {
      if (sourceType == typeof(string)) {
        return _elementTypeConverter.CanConvertFrom(context, typeof(string));
      }
      else {
        return false;
      }
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
      if (destinationType == typeof(string)) {
        return _elementTypeConverter.CanConvertTo(context, typeof(string));
      }
      else {
        return false;
      }
    }

    public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value) {
      if (!CanConvertFrom(context, value.GetType())) {
        throw new NotSupportedException();
      }

      string sourceString = value as string;

      string[] stringArray = sourceString.Split(' ');
      Array array = Array.CreateInstance(_elementType, stringArray.Length);
      for (int i = 0; i < stringArray.Length; i++) {
        string str = Uri.UnescapeDataString(stringArray[i]);
        object obj = _elementTypeConverter.ConvertFromString(str);
        array.SetValue(obj, i);
      }

      return array;
    }

    public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType) {
      if (!CanConvertTo(context, destinationType)) {
        throw new NotSupportedException();
      }

      if (destinationType == typeof(string) &&
          value is Array) {
        List<object> elementList = new List<object>();
        foreach (object obj in (value as IEnumerable)) {
          elementList.Add(obj);
        }

        List<string> stringList = elementList.ConvertAll<string>(_elementTypeConverter.ConvertToString);
        List<string> escapedList = stringList.ConvertAll<string>(Uri.EscapeDataString);
        return string.Join(" ", escapedList.ToArray());
      }

      throw new NotSupportedException();
    }


  }
}
