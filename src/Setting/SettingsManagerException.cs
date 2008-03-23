using System;

namespace Hazychill.Setting {
  public class SettingsManagerException : Exception {
    public SettingsManagerException() : base() {
      
    }

    public SettingsManagerException(string message) : base(message) {
      
    }

    public SettingsManagerException(string message, Exception innerException) : base(message, innerException) {
      
    }
  }
}
