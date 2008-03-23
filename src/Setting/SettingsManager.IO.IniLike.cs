using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Reflection;

namespace Hazychill.Setting {
  public partial class SettingsManager : ISettingsManager {

    private IDictionary<Type, TypeConverter> _converterMap;
    private IDictionary<string, Type> _typeMap = null;

    protected virtual void Initialize() {
      _converterMap = new Dictionary<Type, TypeConverter>();
      _converterMap.Add(typeof(DateTime), new DateTimeConverter());
      _converterMap.Add(typeof(IPAddress), new IPAddressConverter());
      _converterMap.Add(typeof(FileInfo), new FileInfoConverter());

      _typeMap = new Dictionary<string, Type>();
    }

    public virtual void Save(TextWriter writer) {
      foreach (SettingsItemInfo info in this) {
        Uri id = info.ID;
        string name = info.Name;
        Type type = info.Type;
        object value = info.Value;

        string idString = Escape(id.OriginalString, ',');
        string nameString = Escape(name, '=');
        string typeString = Escape(type.AssemblyQualifiedName, ',');
        string valueString = Escape(ConvertToString(value), ',');

        
        writer.WriteLine("\"{0}\"=\"{1}\",\"{2}\",\"{3}\"",
                         nameString,
                         valueString,
                         typeString,
                         idString);
      }

      writer.Flush();
    }

    public virtual void Save(Stream output) {
      TextWriter writer = new StreamWriter(output, new UTF8Encoding());
      Save(writer);
      output.Flush();
    }

    public virtual void Save(string fileName) {
      using (Stream output = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read)) {
        Save(output);
      }
    }

    private string Escape(string text, params char[] separators) {
      text = text.Replace("\\", "\\\\");
      text = text.Replace("\"", "\\\"");
      text = text.Replace("\r", "\\r");
      text = text.Replace("\n", "\\n");

      return text;
    }

    public string Unescape(string text) {
      text = text.Replace("\\n", "\n");
      text = text.Replace("\\r", "\r");
      text = text.Replace("\\\"", "\"");
      text = text.Replace("\\\\", "\\");

      return text;
    }

    private string ConvertToString(object obj) {
      Type type = obj.GetType();
      TypeConverter converter = GetTypeConverter(type);
      return converter.ConvertToString(obj);
    }

    private object ConvertFromString(string text, Type type) {
      TypeConverter converter = GetTypeConverter(type);
      return converter.ConvertFromString(text);
    }

    private TypeConverter GetTypeConverter(Type type) {
      TypeConverter converter;

      if (!_converterMap.TryGetValue(type, out converter)) {
        if (type.IsArray) {
          Type elementType = type.GetElementType();
          converter = GetArrayConverter(elementType);
        }
        else {
          converter = TypeDescriptor.GetConverter(type);
        }
        _converterMap.Add(type, converter);
      }

      return converter;
    }

    private TypeConverter GetArrayConverter(Type elementType) {
      TypeConverter elementTypeConverter = GetTypeConverter(elementType);
      TypeConverter converter = new ArrayConverter(elementType, elementTypeConverter);
      return converter;
    }

    public void Load(TextReader reader) {
      Clear();

      string line;

      while ((line = reader.ReadLine()) != null) {
        if (IsSettingLine(line)) {
          int offset = 0;
          string nameString = ParseName(line, ref offset);
          string valueString = ParseValue(line, ref offset);
          string typeString = ParseType(line, ref offset);
          string idString = ParseID(line, ref offset);

          nameString = (nameString!=null) ? (Unescape(nameString)) : (null);
          valueString = (valueString!=null) ? (Unescape(valueString)) : (null);
          typeString = (typeString!=null) ? (Unescape(typeString)) : (null);
          idString = (idString!=null) ? (Unescape(idString)) : (null);

          string name = nameString;

          Type type;
          if (string.IsNullOrEmpty(typeString)) {
            type = typeof(string);
          }
          else {
            type = FindType(typeString);
            if (type == null) {
              throw new FormatException();
            }
          }
        
          object value = ConvertFromString(valueString, type);

          Uri id;
          if (idString != null) {
            id = new Uri(idString, UriKind.Absolute);
          }
          else {
            id = GenerateUniqueID();
          }

          SettingsItemInfo info = new SettingsItemInfo(id, name, type, value);
          Add(info);
        }
      }
    }

    public void Load(Stream input) {
      TextReader reader = new StreamReader(input, new UTF8Encoding());
      Load(reader);
    }

    public void Load(string fileName) {
      using (Stream input = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
        Load(input);
      }
    }

    private bool IsSettingLine(string text) {
      if (Regex.IsMatch(text, "^[\\s]*#")) {
        return false;
      }
      else {
        return Regex.IsMatch(text, "[^\\s]");
      }
    }

    private string ParseName(string text, ref int offset) {
      if (text == null) {
        throw new ArgumentNullException("text");
      }

      if (offset < 0 || text.Length < offset) {
        throw new ArgumentOutOfRangeException("offset");
      }

      int startIndex;
      int length;

      startIndex = offset;
      
      while (text[startIndex] == ' ' || text[startIndex] == '\t') {
        startIndex++;
        if (startIndex == text.Length) {
          throw new FormatException("Name cannot be empty string.");
        }
      }

      if (text[startIndex] == '\"') {
        startIndex++;
        int indexOfClosingDQ = startIndex;
        bool escaped = false;

        do {
          if (indexOfClosingDQ >= text.Length) {
            throw new FormatException("Can't find closing double quotation (\").");
          }

          char c = text[indexOfClosingDQ];

          if (c == '\"' && !escaped) {
            break;
          }
          else if (c == '\\' && !escaped) {
            escaped = true;
          }
          else {
            escaped = false;
          }

          indexOfClosingDQ++;
        } while (true);

        length = indexOfClosingDQ - startIndex;

        offset = text.IndexOf('=', indexOfClosingDQ);

        if (offset == -1) {
          throw new FormatException("Can't find equals (=).");
        }
        
        offset++;
      }
      else {
        int indexOfEquals = text.IndexOf('=', startIndex);

        if (indexOfEquals == -1) {
          throw new FormatException("Can't find equals (=).");
        }

        int indexOfLastChar = indexOfEquals;

        do {
          indexOfLastChar--;
          char c = text[indexOfLastChar];
          if (c != ' ' && c != '\t') {
            break;
          }

          if (indexOfLastChar <= startIndex) {
            // this leads length to be 0
            // so that empty string will be returned.
            indexOfLastChar = startIndex - 1;
            break;
          }

        } while(true);

        length = indexOfLastChar - startIndex + 1;
        offset = indexOfEquals + 1;
      }

      return text.Substring(startIndex, length);
    }

    private string ParseValue(string text, ref int offset) {
      if (text == null) {
        throw new ArgumentNullException("text");
      }

      if (offset < 0 || text.Length < offset) {
        throw new ArgumentOutOfRangeException("offset");
      }

      int startIndex;
      int length;

      startIndex = offset;
      
      while (text[startIndex] == ' ' || text[startIndex] == '\t') {
        startIndex++;
        if (startIndex == text.Length) {
          offset = text.Length;
          return string.Empty;
        }
        else if (text[startIndex] == ',') {
          offset = startIndex + 1;
          return string.Empty;
        }
      }

      if (text[startIndex] == '\"') {
        startIndex++;
        int indexOfClosingDQ = startIndex;
        bool escaped = false;

        do {
          if (indexOfClosingDQ >= text.Length) {
            throw new FormatException("Can't find closing double quotation (\").");
          }

          char c = text[indexOfClosingDQ];

          if (c == '\"' && !escaped) {
            break;
          }
          else if (c == '\\' && !escaped) {
            escaped = true;
          }
          else {
            escaped = false;
          }

          indexOfClosingDQ++;
        } while (true);

        length = indexOfClosingDQ - startIndex;

        offset = text.IndexOf(',', indexOfClosingDQ);

        if (offset == -1) {
          offset = text.Length;
        }
        else {
          offset++;
        }
      }
      else {
        int indexOfComma = text.IndexOf(',', startIndex);
        if (indexOfComma == -1) {
          indexOfComma = text.Length;
          offset = indexOfComma;
        }
        else {
          offset = indexOfComma + 1;
        }
        

        int indexOfLastChar = indexOfComma;

        do {
          indexOfLastChar--;
          char c = text[indexOfLastChar];
          if (c != ' ' && c != '\t') {
            break;
          }

          if (indexOfLastChar <= startIndex) {
            // this leads length to be 0
            // so that empty string will be returned.
            indexOfLastChar = startIndex - 1;
            break;
          }

        } while(true);

        length = indexOfLastChar - startIndex + 1;
      }

      return text.Substring(startIndex, length);
    }

    private string ParseType(string text, ref int offset) {

      if (text == null) {
        throw new ArgumentNullException("text");
      }

      if (offset < 0) {
        throw new ArgumentOutOfRangeException("offset");
      }
      else if (offset >= text.Length) {
        offset = text.Length;
        return null;
      }
      
      int startIndex;
      int length;

      startIndex = offset;
      
      while (text[startIndex] == ' ' || text[startIndex] == '\t') {
        startIndex++;
        if (startIndex == text.Length) {
          offset = text.Length;
          return string.Empty;
        }
        else if (text[startIndex] == ',') {
          offset = startIndex + 1;
          return string.Empty;
        }
      }

      if (text[startIndex] == '\"') {
        startIndex++;
        int indexOfClosingDQ = startIndex;
        bool escaped = false;

        do {
          if (indexOfClosingDQ >= text.Length) {
            throw new FormatException("Can't find closing double quotation (\").");
          }

          char c = text[indexOfClosingDQ];

          if (c == '\"' && !escaped) {
            break;
          }
          else if (c == '\\' && !escaped) {
            escaped = true;
          }
          else {
            escaped = false;
          }

          indexOfClosingDQ++;
        } while (true);

        length = indexOfClosingDQ - startIndex;

        offset = text.IndexOf(',', indexOfClosingDQ);

        if (offset == -1) {
          offset = text.Length;
        }
        else {
          offset++;
        }
      }
      else {
        int indexOfComma = text.IndexOf(',', startIndex);
        if (indexOfComma == -1) {
          indexOfComma = text.Length;
          offset = indexOfComma;
        }
        else {
          offset = indexOfComma + 1;
        }
        

        int indexOfLastChar = indexOfComma;

        do {
          indexOfLastChar--;
          char c = text[indexOfLastChar];
          if (c != ' ' && c != '\t') {
            break;
          }

          if (indexOfLastChar <= startIndex) {
            // this leads length to be 0
            // so that empty string will be returned.
            indexOfLastChar = startIndex - 1;
            break;
          }

        } while(true);

        length = indexOfLastChar - startIndex + 1;
      }

      return text.Substring(startIndex, length);
    }

    private string ParseID(string text, ref int offset) {

      if (text == null) {
        throw new ArgumentNullException("text");
      }

      if (offset < 0) {
        throw new ArgumentOutOfRangeException("offset");
      }
      else if (offset >= text.Length) {
        offset = text.Length;
        return null;
      }
      
      int startIndex;
      int length;

      startIndex = offset;
      
      while (text[startIndex] == ' ' || text[startIndex] == '\t') {
        startIndex++;
        if (startIndex == text.Length) {
          offset = text.Length;
          return string.Empty;
        }
        else if (text[startIndex] == ',') {
          offset = startIndex + 1;
          return string.Empty;
        }
      }

      if (text[startIndex] == '\"') {
        startIndex++;
        int indexOfClosingDQ = startIndex;
        bool escaped = false;

        do {
          if (indexOfClosingDQ >= text.Length) {
            throw new FormatException("Can't find closing double quotation (\").");
          }

          char c = text[indexOfClosingDQ];

          if (c == '\"' && !escaped) {
            break;
          }
          else if (c == '\\' && !escaped) {
            escaped = true;
          }
          else {
            escaped = false;
          }

          indexOfClosingDQ++;
        } while (true);

        length = indexOfClosingDQ - startIndex;

        offset = text.IndexOf(',', indexOfClosingDQ);

        if (offset == -1) {
          offset = text.Length;
        }
        else {
          offset++;
        }
      }
      else {
        int indexOfComma = text.IndexOf(',', startIndex);
        if (indexOfComma == -1) {
          indexOfComma = text.Length;
          offset = indexOfComma;
        }
        else {
          offset = indexOfComma + 1;
        }
        

        int indexOfLastChar = indexOfComma;

        do {
          indexOfLastChar--;
          char c = text[indexOfLastChar];
          if (c != ' ' && c != '\t') {
            break;
          }

          if (indexOfLastChar <= startIndex) {
            // this leads length to be 0
            // so that empty string will be returned.
            indexOfLastChar = startIndex - 1;
            break;
          }

        } while(true);

        length = indexOfLastChar - startIndex + 1;
      }

      return text.Substring(startIndex, length);
    }

    private Type FindType(string typeName) {
      Type type;
      if (_typeMap.TryGetValue(typeName, out type)) {
        return type;
      }
    
      type = Type.GetType(typeName);
      if (type != null) {
        _typeMap.Add(typeName, type);
        return type;
      }

      Assembly myAsm = Assembly.GetEntryAssembly();
      type = Array.Find(myAsm.GetTypes(), delegate(Type t) {
        return string.Equals(typeName, t.FullName, StringComparison.Ordinal);
      });
      if (type != null) {
        return type;
      }

      AssemblyName[] refedAsms = myAsm.GetReferencedAssemblies();

      foreach (AssemblyName asmName in refedAsms) {
        Assembly asm = Assembly.Load(asmName);
        Type[] types = asm.GetExportedTypes();
        type = Array.Find(types, delegate(Type t) {
          return string.Equals(typeName, t.FullName, StringComparison.Ordinal);
        });
        if (type != null) {
          _typeMap.Add(typeName, type);
          return type;
        }
      }

      return null;
    }

    public IDictionary<Type, TypeConverter> ConverterMap {
      get { return _converterMap; }
    }
  }
}
