using System;
using System.Collections.Generic;

namespace Hazychill.Setting {
  public class UriOriginalStringEqualityComparer : IEqualityComparer<Uri> {
    public bool Equals(Uri x, Uri y) {
      string xStr = x.OriginalString;
      string yStr = y.OriginalString;
      return EqualityComparer<string>.Default.Equals(xStr, yStr);
    }

    public int GetHashCode(Uri obj) {
      string objStr = obj.OriginalString;
      return EqualityComparer<string>.Default.GetHashCode(objStr);
    }
  }
}
