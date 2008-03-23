using System;
using System.Collections.Generic;

namespace Hazychill.Setting {
  public interface ISettingsManager {

    // Name interfaces ///////////////////////////

    SettingsItem<T> GetItem<T>(string name);

    bool TryGetItem<T>(string name, out SettingsItem<T> item);

    bool TryGetItem<T>(string name, out T item);

    IEnumerable<SettingsItem<T>> GetItems<T>(string name);

    Uri SetItem<T>(string name, T item);

    Uri AddItem<T>(string name, T item);

    Uri SetOrAddNewItem<T>(string name, T item);

    int GetCount<T>(string name);

    bool Exists<T>(string name);

    int RemoveAll<T>(string name);


    // ID interfaces ////////////////////////////

    T GetItem<T>(Uri id);

    bool TryGetItem<T>(Uri id, out T item);

    void SetItem<T>(Uri id, T item);

    bool Exists(Uri id);

    bool Remove(Uri id);


    // Other////////////////////////////////////

    void Clear();
  }
}
