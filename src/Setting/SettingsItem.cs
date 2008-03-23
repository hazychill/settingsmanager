using System;

namespace Hazychill.Setting {
  public class SettingsItem<T> {
    private Uri _id;
    private T   _value;

    public SettingsItem(Uri id, T value) {
      _id = id;
      _value = value;
    }

    public static implicit operator T(SettingsItem<T> item) {
      return item.Value;
    }

    public T Value {
      get { return _value; }
    }

    public Uri ID {
      get { return _id; }
    }

    public override string ToString() {
      if (_value != null) {
        return _value.ToString();
      }
      else {
        return "null";
      }
    }
  }
}
