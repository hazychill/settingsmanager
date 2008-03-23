CSC20 = %windir%\Microsoft.NET\Framework\v2.0.50727\csc.exe
CSC35 = %windir%\Microsoft.NET\Framework\v3.5\csc.exe
CSC = $(CSC35)

DIR_SRC = ..\src
DIR_SRC_ATOM = $(DIR_SRC)\Atom

SRC_SMNG = \
	$(DIR_SRC)\ArrayConverter.cs \
	$(DIR_SRC)\DateTimeConverter.cs \
	$(DIR_SRC)\FileInfoConverter.cs \
	$(DIR_SRC)\FlexibleConverter.cs \
	$(DIR_SRC)\IPAddressConverter.cs \
	$(DIR_SRC)\ISettingsManager.cs \
	$(DIR_SRC)\SettingsItem.cs \
	$(DIR_SRC)\SettingsItemInfo.cs \
	$(DIR_SRC)\SettingsManager.cs \
	$(DIR_SRC)\SettingsManagerBase.cs \
	$(DIR_SRC)\SettingsManagerException.cs \
	$(DIR_SRC)\UriOriginalStringEqualityComparer.cs \

SRC_ATOM = \
	$(DIR_SRC_ATOM)\AtomBaseElement.cs \
	$(DIR_SRC_ATOM)\AtomCategory.cs \
	$(DIR_SRC_ATOM)\AtomContent.cs \
	$(DIR_SRC_ATOM)\AtomContentType.cs \
	$(DIR_SRC_ATOM)\AtomDocument.cs \
	$(DIR_SRC_ATOM)\AtomEntry.cs \
	$(DIR_SRC_ATOM)\AtomExtensionEventArgs.cs \
	$(DIR_SRC_ATOM)\AtomFeed.cs \
	$(DIR_SRC_ATOM)\AtomFormatException.cs \
	$(DIR_SRC_ATOM)\AtomGenerator.cs \
	$(DIR_SRC_ATOM)\AtomLink.cs \
	$(DIR_SRC_ATOM)\AtomNameTable.cs \
	$(DIR_SRC_ATOM)\AtomOutOfLineContent.cs \
	$(DIR_SRC_ATOM)\AtomPersonConstruct.cs \
	$(DIR_SRC_ATOM)\AtomSource.cs \
	$(DIR_SRC_ATOM)\AtomTextConstruct.cs \
	$(DIR_SRC_ATOM)\AtomUri.cs \
	$(DIR_SRC_ATOM)\AtomXmlReader.cs \
	$(DIR_SRC_ATOM)\DocumentTypeDetectedEventArgs.cs \
	$(DIR_SRC_ATOM)\Helper.cs \
	$(DIR_SRC_ATOM)\IAtomXmlReader.cs \

binary : $(SRC_SMNG) $(DIR_SRC)\SettingsManagerSerializer.cs $(DIR_SRC)\SettingsManager.IO.Binary.cs
	$(CSC) /target:library /out:SettingsManager.dll $**
binarydebug : $(SRC_SMNG) $(DIR_SRC)\SettingsManagerSerializer.cs $(DIR_SRC)\SettingsManager.IO.Binary.cs
	$(CSC) /debug /target:library /out:SettingsManager_debug.dll $**

soap : $(SRC_SMNG) $(DIR_SRC)\SettingsManagerSerializer.cs $(DIR_SRC)\SettingsManager.IO.Soap.cs
	$(CSC) /target:library /out:SettingsManager.dll $**
soapdebug : $(SRC_SMNG) $(DIR_SRC)\SettingsManagerSerializer.cs $(DIR_SRC)\SettingsManager.IO.Soap.cs
	$(CSC) /debug /target:library /out:SettingsManager_debug.dll $**

inilike : $(SRC_SMNG) $(DIR_SRC)\SettingsManager.IO.IniLike.cs
	$(CSC) /target:library /out:SettingsManager.dll $**
inilikedebug : $(SRC_SMNG) $(DIR_SRC)\SettingsManager.IO.IniLike.cs
	$(CSC) /debug /target:library /out:SettingsManager_debug.dll $**

atom : $(SRC_SMNG) $(SRC_ATOM) $(DIR_SRC)\SettingsManager.IO.Atom.cs
	$(CSC) /target:library /out:SettingsManager.dll $**
atomdebug : $(SRC_SMNG) $(SRC_ATOM) $(DIR_SRC)\SettingsManager.IO.Atom.cs
	$(CSC) /debug /target:library /out:SettingsManager_debug.dll $**

clean :
	del SettingsManager*.dll
	del SettingsManager*.pdb
