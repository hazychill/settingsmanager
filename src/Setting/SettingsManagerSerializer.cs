using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;

namespace Hazychill.Setting {
  [Serializable]
  public class SettingsManagerSerializer {
    private SettingsItemInfo[] _smngInfo;

    public SettingsManagerSerializer(SettingsManager smng) {
      List<SettingsItemInfo> smngInfoList = new List<SettingsItemInfo>(smng);
//       foreach (SettingsItemInfo info in smng.EnumInfo()) {
//         smngInfoList.Add(info);
//       }

      _smngInfo = smngInfoList.ToArray();
    }

    public SettingsItemInfo[] InfoList {
      get { return _smngInfo; }
    }
  }
}
