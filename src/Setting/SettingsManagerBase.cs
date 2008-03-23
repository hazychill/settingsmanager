using System;
using System.Collections.Generic;

namespace Hazychill.Setting {
  public abstract class SettingsManagerBase : ISettingsManager {

    public virtual SettingsItem<T> GetItem<T>(string name) {
      bool isFirst = true;
      SettingsItem<T> returnItem = null;
      foreach (SettingsItem<T> item in GetItems<T>(name)) {
        if (isFirst) {
          returnItem = item;
          isFirst = false;
        }
        else {
          throw new SettingsManagerException(string.Format("More than one items have the same name and type ({0}/{1})",
                                                           name, typeof(T)));
        }
      }

      if (isFirst) {
        throw new SettingsManagerException(string.Format("Can't find item that have the name and type. ({0}/{1})",
                                           name, typeof(T)));
      }
      
      return returnItem;
    }

    public abstract bool TryGetItem<T>(string name, out SettingsItem<T> item);

    public virtual bool TryGetItem<T>(string name, out T item) {
      SettingsItem<T> dummyItem;
      if (TryGetItem<T>(name, out dummyItem)) {
        item = dummyItem.Value;
        return true;
      }
      else {
        item = default(T);
        return false;
      }
    }

    public abstract IEnumerable<SettingsItem<T>> GetItems<T>(string name);

    public abstract Uri SetItem<T>(string name, T item);

    public abstract Uri AddItem<T>(string name, T item);

    public virtual Uri SetOrAddNewItem<T>(string name, T item) {
      int itemCount = GetCount<T>(name);
      Uri id;
      
      if (itemCount == 0) {
        id = AddItem<T>(name, item);
      }
      else if (itemCount == 1) {
        id = SetItem<T>(name, item);
      }
      else {
        throw new SettingsManagerException(string.Format("More than one items have the same name and type ({0}/{1})",
                                                         name, typeof(T)));
      }

      return id;
    }

    public abstract int GetCount<T>(string name);

    public virtual bool Exists<T>(string name) {
      return GetCount<T>(name) >= 1;
    }

    public abstract int RemoveAll<T>(string name);

    public abstract T GetItem<T>(Uri id);

    public abstract bool TryGetItem<T>(Uri id, out T item);

    public abstract void SetItem<T>(Uri id, T item);

    public abstract bool Exists(Uri id);

    public abstract bool Remove(Uri id);

    public abstract void Clear();
  }
}
