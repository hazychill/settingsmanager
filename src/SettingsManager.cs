using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace Hazychill.Setting {
  public partial class SettingsManager : SettingsManagerBase, ISettingsManager, IEnumerable<SettingsItemInfo> {

    private IDictionary<Uri, object> idMap;
    private IDictionary<string, IDictionary<Type, IList<Uri>>> nameMap;

    public SettingsManager() {
      IEqualityComparer<Uri> comparer = new UriOriginalStringEqualityComparer();
      idMap = new Dictionary<Uri, object>(comparer);

      nameMap = new Dictionary<string, IDictionary<Type, IList<Uri>>>();

      Initialize();
    }

    // Name interfaces ///////////////////////////

    // Use method of base class
    // public SettingsItem<T> GetItem<T>(string name);

    // Use method of base class
    // public bool TryGetItem<T>(string name, out T);

    public override bool TryGetItem<T>(string name, out SettingsItem<T> item) {
      bool isFirst = true;
      item = default(SettingsItem<T>);
      foreach (SettingsItem<T> oneItem in GetItems<T>(name)) {
        if (isFirst) {
          item = oneItem;
          isFirst = false;
        }
        else {
          item = default(SettingsItem<T>);
          return false;
        }
      }

      if (isFirst) {
        item = default(SettingsItem<T>);
        return false;
      }
      
      return true;
    }

    // Use method of base class
    // bool TryGetItem<T>(string name, out T item);

    public override IEnumerable<SettingsItem<T>> GetItems<T>(string name) {
      foreach (Uri id in EnumID<T>(name)) {
        T value = (T)idMap[id];
        yield return new SettingsItem<T>(id, value);
      }
    }

    public override Uri SetItem<T>(string name, T item) {
      bool isFirst = true;
      Uri targetID = null;
      foreach (Uri id in EnumID<T>(name)) {
        if (isFirst) {
          targetID = id;
          isFirst = false;
        }
        else {
          throw new SettingsManagerException(string.Format("More than one items have the same name and type ({0}/{1})",
                                                           name, typeof(T)));
        }
      }

      if (!isFirst) {
        idMap[targetID] = item;
        return targetID;
      }
      else {
        throw new SettingsManagerException(string.Format("Can't find item that have the name and type. ({0}/{1})",
                                           name, typeof(T)));
      }
    }
 
   public override Uri AddItem<T>(string name, T item) {
      IDictionary<Type, IList<Uri>> typeMap;
      if (!nameMap.TryGetValue(name, out typeMap)) {
        typeMap = CreateTypeListMap();
        nameMap.Add(name, typeMap);
      }
      IList<Uri> idList;
      if (!typeMap.TryGetValue(typeof(T), out idList)) {
        idList = new List<Uri>();
        typeMap.Add(typeof(T), idList);
      }
      Uri newID = GenerateUniqueID();
      idList.Add(newID);

      idMap.Add(newID, item);

      return newID;
    }

    // Use method of base class
    // public Uri SetOrAddNewItem<T>(string name, T item) {

    public override int GetCount<T>(string name) {
      IDictionary<Type, IList<Uri>> typeMap;

      if (nameMap.TryGetValue(name, out typeMap)) {
        IList<Uri> idList;
        if (typeMap.TryGetValue(typeof(T), out idList)) {
          return idList.Count;
        }
      }

      return 0;
    }

    // Use method of base class
    // public virtual bool Exists<T>(string name);

    public override int RemoveAll<T>(string name) {
      IDictionary<Type, IList<Uri>> typeMap;
      int removedCount = 0;
      if (nameMap.TryGetValue(name, out typeMap)) {
        IList<Uri> idList;
        if (typeMap.TryGetValue(typeof(T), out idList)) {
          removedCount = idList.Count;
          foreach (Uri id in idList) {
            idMap.Remove(id);
          }
          idList.Clear();
        }
      }

      return removedCount;
    }

    public void SetArray<T>(string name, T[] array) {
      if (name == null) {
        throw new ArgumentNullException("name");
      }
      
      if (array == null) {
        throw new ArgumentNullException("array");
      }

      string orderName = string.Format("{0}/order", name);

      RemoveAll<T>(name);
      RemoveAll<Uri[]>(orderName);

      Uri[] itemOrder = new Uri[array.Length];
      for (int i = 0; i < array.Length; i++) {
        T item = array[i];
        Uri id = AddItem<T>(name, item);
        itemOrder[i] = id;
      }

      AddItem<Uri[]>(orderName, itemOrder);
    }

    public T[] GetArray<T>(string name) {
      if (name == null) {
        throw new ArgumentNullException("name");
      }

      string orderName = string.Format("{0}/order", name);

      Uri[] itemOrder = GetItem<Uri[]>(orderName);

      T[] array = new T[itemOrder.Length];
      for (int i = 0; i < itemOrder.Length; i++) {
        Uri id = itemOrder[i];
        array[i] = GetItem<T>(id);
      }

      return array;
    }

    // ID interfaces ////////////////////////////

    public override T GetItem<T>(Uri id) {
      if (id == null) {
        throw new ArgumentNullException("id");
      }
      
      if (!id.IsAbsoluteUri) {
        throw new ArgumentException("ID must be an absolute URI (absolute URI reference)", "id");
      }

      return (T)idMap[id];
    }

    public override bool TryGetItem<T>(Uri id, out T item) {
      if (id == null) {
        throw new ArgumentNullException("id");
      }
      
      if (!id.IsAbsoluteUri) {
        item = default(T);
        return false;
      }

      item = (T)idMap[id];
      return true;
    }

    public override void SetItem<T>(Uri id, T item) {
      if (id == null) {
        throw new ArgumentNullException("id");
      }
      
      if (!id.IsAbsoluteUri) {
        throw new ArgumentException("ID must be an absolute URI (absolute URI reference)", "id");
      }

      if (idMap.ContainsKey(id)) {
        idMap[id] =  item;
      }
      else {
        throw new KeyNotFoundException();
      }
    }

    public override bool Exists(Uri id) {
      return idMap.ContainsKey(id);
    }

    public override bool Remove(Uri id) {
      if (idMap.Remove(id)) {
        foreach (IDictionary<Type, IList<Uri>> typeMap in nameMap.Values) {
          foreach (IList<Uri> idList in typeMap.Values) {
            if (idList.Remove(id)) {
              return true;
            }
          }
        }

        throw new SettingsManagerException("While removing item, some contradiction have found.");
      }
      else {
        return false;
      }
    }

    public override void Clear() {
      idMap.Clear();
      nameMap.Clear();
    }


    private void Add(SettingsItemInfo info) {
      Uri id = info.ID;
      string name = info.Name;
      Type type = info.Type;
      object value = info.Value;

      idMap.Add(id, value);

      IDictionary<Type, IList<Uri>> typeMap;
      if (!nameMap.TryGetValue(name, out typeMap)) {
        typeMap = CreateTypeListMap();
        nameMap.Add(name, typeMap);
      }
      IList<Uri> idList;
      if (!typeMap.TryGetValue(type, out idList)) {
        idList = new List<Uri>();
        typeMap.Add(type, idList);
      }
      idList.Add(id);
    }




    public IEnumerator<SettingsItemInfo> GetEnumerator() {
      foreach (string name in nameMap.Keys) {
        IDictionary<Type, IList<Uri>> typeMap = nameMap[name];

        foreach (Type type in typeMap.Keys) {
          IList<Uri> idList = typeMap[type];

          foreach (Uri id in idList) {
            object value = idMap[id];
            yield return new SettingsItemInfo(id, name, type, value);
          }
        }
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }

    private IEnumerable<Uri> EnumID<T>(string name) {
      IDictionary<Type, IList<Uri>> typeMap;
      if (nameMap.TryGetValue(name, out typeMap)) {
        IList<Uri> idList;
        if (typeMap.TryGetValue(typeof(T), out idList)) {
          return idList;
        }
      }

      return new Uri[0];
    }



    private Uri GenerateUniqueID() {
      return new Uri(string.Format("urn:uuid:{0}", Guid.NewGuid()));
    }

    private IDictionary<Type, IList<Uri>> CreateTypeListMap() {
      return new Dictionary<Type, IList<Uri>>();
    }
  }
}
