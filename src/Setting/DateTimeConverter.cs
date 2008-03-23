using System;
using System.ComponentModel;
using System.Globalization;

namespace Hazychill.Setting {
  internal class DateTimeConverter : TypeConverter {
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

      string dateTimeString = value as string;
      return DateTime.Parse(dateTimeString);
    }

    public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType) {
      if (!CanConvertTo(context, destinationType)) {
        throw new NotSupportedException();
      }
      
      if (destinationType == typeof(string) &&
          value is DateTime) {
        return ((DateTime)value).ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz");
      }

      throw new NotSupportedException();
    }
  }
}
