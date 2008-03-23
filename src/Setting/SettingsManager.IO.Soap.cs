using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;

namespace Hazychill.Setting {
  public partial class SettingsManager : ISettingsManager {

    private void Initialize() { }

    public virtual void Save(Stream output) {
      SettingsManagerSerializer sms = new SettingsManagerSerializer(this);
      IFormatter formatter = GetFormatter();
      formatter.Serialize(output, sms);
    }

    public virtual void Save(string fileName) {
      using (Stream output = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read)) {
        Save(output);
      }
    }

    public virtual void Load(Stream input) {
      Clear();
      
      IFormatter formatter = GetFormatter();
      SettingsManagerSerializer sms = formatter.Deserialize(input) as SettingsManagerSerializer;
      if (sms != null) {
        Array.ForEach(sms.InfoList, Add);
      }
    }
    
    public void Load(string fileName) {
      using (Stream input = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
        Load(input);
      }
    }

    protected virtual IFormatter GetFormatter() {
      return new SoapFormatter();
    }
  }
}
