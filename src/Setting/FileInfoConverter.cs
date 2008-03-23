using System;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Net;

namespace Hazychill.Setting {
  internal class FileInfoConverter : TypeConverter {
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

      string pathString = value as string;
      return new FileInfo(pathString);
    }

    public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType) {
      if (!CanConvertTo(context, destinationType)) {
        throw new NotSupportedException();
      }

      if (destinationType == typeof(string) &&
          value is FileInfo) {
        return (value as FileInfo).FullName;
      }

      throw new NotSupportedException();
    }
  }
}
