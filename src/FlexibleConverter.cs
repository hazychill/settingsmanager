using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Hazychill.Setting {
  public class FlexibleConverter<T> : TypeConverter {
    Converter<T, string> _toString;
    Converter<string, T> _fromString;
      
    public FlexibleConverter(Converter<T, string> toString, Converter<string, T> fromString) {
      _toString = toString;
      _fromString = fromString;
    }

    public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType) {
      return sourceType == typeof(string);
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
      return destinationType == typeof(string);
    }

    public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value) {
      if (!CanConvertFrom(context, value.GetType())) {
        throw new NotSupportedException();
      }

      string str = value as string;
      return _fromString(str);
    }

    public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType) {
      if (!CanConvertTo(context, destinationType)) {
        throw new NotSupportedException();
      }

      if (destinationType == typeof(string) &&
          value is T) {
        return _toString((T)value);
      }
      else {
        throw new NotSupportedException();
      }
    }
  }
}
