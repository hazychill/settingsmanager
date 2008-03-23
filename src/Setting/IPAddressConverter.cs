using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;

namespace Hazychill.Setting {
  internal class IPAddressConverter : TypeConverter {
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

      string ipString = value as string;
      IPAddress ipAddress;
      if (IPAddress.TryParse(ipString, out ipAddress)) {
        return ipAddress;
      }

      throw new NotSupportedException();
    }

    public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType) {
      if (!CanConvertTo(context, destinationType)) {
        throw new NotSupportedException();
      }

      if (destinationType == typeof(string) &&
          value is IPAddress) {
        return (value as IPAddress).ToString();
      }

      throw new NotSupportedException();
    }
  }
}
