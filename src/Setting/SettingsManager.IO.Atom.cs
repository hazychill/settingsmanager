using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Xml;
using System.Net;
using System.IO;
using System.Text;
using WebFeed.Atom10;

namespace Hazychill.Setting {
  public partial class SettingsManager : ISettingsManager {

    private IDictionary<Type, TypeConverter> _converterMap;
    private AtomXmlReader _settingsReader;

    protected virtual void Initialize() {
      _converterMap = new Dictionary<Type, TypeConverter>();
      _converterMap.Add(typeof(DateTime), new DateTimeConverter());
      _converterMap.Add(typeof(IPAddress), new IPAddressConverter());
      _converterMap.Add(typeof(FileInfo), new FileInfoConverter());

      _settingsReader = new SettingsAtomReader();
    }


    public virtual void Save(XmlWriter writer) {
      Uri feedID = GenerateUniqueID();
      AtomTextConstruct feedTitle = new AtomTextConstruct(string.Empty);
      DateTime feedUpdated = DateTime.Now;
      AtomFeed feed = new SettingsFeed(feedID, feedTitle, feedUpdated);

      AtomPersonConstruct feedAuthor = new AtomPersonConstruct(string.Empty);
      feed.AddAuthor(feedAuthor);

      foreach (SettingsItemInfo info in this) {
        Uri entryID = info.ID;
        AtomTextConstruct entryTitle = new AtomTextConstruct(info.Name);
        DateTime entryUpdated = feedUpdated;

        SettingsEntry entry = new SettingsEntry(entryID, entryTitle, entryUpdated);

        string valueString = ConvertToString(info.Value);
        AtomContent content = new AtomContent(valueString);
        entry.Content = content;
        entry.Type = info.Type;

        feed.Entries.Add(entry);
      }

      feed.WriteDocument(writer);
    }

    public virtual void Save(TextWriter tw) {
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Indent = true;
      settings.CloseOutput = false;
      using (XmlWriter writer = XmlWriter.Create(tw, settings)) {
        Save(writer);
      }
    }

    public virtual void Save(Stream output) {
      TextWriter tw = new StreamWriter(output, new UTF8Encoding());
      Save(tw);
    }

    public virtual void Save(string fileName) {
      using (Stream output = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read)) {
        Save(output);
      }
    }

    public virtual void Load(XmlReader reader) {
      Clear();

      AtomFeed feed = _settingsReader.ReadFeed(reader);

      foreach (SettingsEntry entry in feed.Entries) {
        string name = entry.Title.Text;

        string valueString = entry.Content.Text;
        Type type = entry.Type;
        if (type == null) {
          type = typeof(string);
        }

        object value = ConvertFromString(valueString, type);

        Uri id = entry.ID;

        SettingsItemInfo info = new SettingsItemInfo(id, name, type, value);
        Add(info);
      }
    }

    public virtual void Load(TextReader tr) {
      XmlReaderSettings settings = new XmlReaderSettings();
      settings.CloseInput = false;
      using (XmlReader reader = XmlReader.Create(tr, settings)) {
        Load(reader);
      }
    }

    public virtual void Load(Stream input) {
      TextReader tr = new StreamReader(input, new UTF8Encoding());
      Load(tr);
    }

    public virtual void Load(string fileName) {
      using (Stream input = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
        Load(input);
      }
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

    public IDictionary<Type, TypeConverter> ConverterMap {
      get { return _converterMap; }
    }
  }

  internal class SettingsFeed : AtomFeed {
    public SettingsFeed(Uri id, AtomTextConstruct title, DateTime updated) : base(id, title, updated) { }

    internal static string NS {
      get { return "urn:uuid:3706f42a-0b0b-4c3d-2af4-06370b0b3d4c"; }
    }

    internal static string Prefix {
      get { return "smng"; }
    }

    protected override void WriteAttributes(XmlWriter writer) {
      base.WriteAttributes(writer);
      writer.WriteAttributeString("xmlns", Prefix, "http://www.w3.org/2000/xmlns/", NS);
    }
  }

  internal class SettingsEntry : AtomEntry {
    Type _type;

    public SettingsEntry(Uri id, AtomTextConstruct title, DateTime updated) : base(id, title, updated) {
      _type = null;
    }

    public Type Type {
      get { return _type; }
      set { _type = value; }
    }

    protected override void WriteAttributes(XmlWriter writer) {
      base.WriteAttributes(writer);
      if (_type != null) {
        writer.WriteAttributeString(SettingsFeed.Prefix, "type", SettingsFeed.NS, _type.AssemblyQualifiedName);
      }
    }
  }

  internal class SettingsAtomReader : AtomXmlReader {
    public SettingsAtomReader() : base() {
      string ns = SettingsFeed.NS;
      ReadExtension += delegate(object sender, AtomExtensionEventArgs e) {
        XmlNode node = e.ExtensionNode;
        if (e.ReadingElement is SettingsEntry      &&
            node.NodeType == XmlNodeType.Attribute &&
            node.NamespaceURI == ns                &&
            node.LocalName == "type") {
          SettingsEntry entry = e.ReadingElement as SettingsEntry;
          entry.Type = Type.GetType(node.Value);
        }
      };
    }

    private static readonly Uri _dummyUri = new Uri("http://dummy/");
    private static readonly AtomTextConstruct _dummyTitle = new AtomTextConstruct("dummy", AtomContentType.Text);
    private static readonly DateTime _dummyUpdated = DateTime.MinValue;

    protected override AtomFeed CreateFeedTemplate() {
      return new SettingsFeed(_dummyUri, _dummyTitle, _dummyUpdated);
    }
    protected override AtomEntry CreateEntryTemplate() {
      return new SettingsEntry(_dummyUri, _dummyTitle, _dummyUpdated);
    }
  }
}
