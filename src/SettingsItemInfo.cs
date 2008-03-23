using System;

namespace Hazychill.Setting {
  [Serializable]
  public class SettingsItemInfo {
    private Uri _id;
    private string _name;
    private Type _type;
    private object _value;

    public SettingsItemInfo(Uri id, string name, Type type, object value) {
      _id = id;
      _name = name;
      _type = type;
      _value = value;
    }

    public Uri ID {
      get { return _id; }
    }

    public string Name {
      get { return _name; }
    }

    public Type Type {
      get { return _type; }
    }

    public object Value {
      get { return _value; }
    }
  }
}
