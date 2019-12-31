# Ericsson 3G CM/PM XML Parser

Converts Ericsson 3G CM/PM XML data to CSV

## Usage

### Convert CM data

E3gXmlParser.exe /cm e3gCm_input.xml

### Convert PM data

E3gXmlParser.exe /pm e3gPm_input.xml

### Console Output Sample for CM data

Done in 1.3902016 sec

Table: Meta RowCount: 1
FileFormatVersion;Vendor
32.615 V4.5;Ericsson

Table: UtranCell RowCount: 85
RncName;UtranCell;localCellId;uarfcnUl;uarfcnDl
RNC001;UCell1005B12;10161;2314;2539
RNC001;UCell1006A22;10167;2687;2912

Table: GsmRelation RowCount: 1219
RncName;UtranCell;qOffset1sn;selectionPriority
RNC001;UCell1005B12;7;8
RNC001;UCell1005B12;7;1

Table: UtranRelation RowCount: 3750
RncName;UtranCell;qOffset1sn;selectionPriority
RNC001;UCell1005B12;0;7
RNC001;UCell1005B12;0;5

### CSV Output Sample for CM data

E3gCm-GsmRelation.csv 
E3gCm-Meta.csv 
E3gCm-UtranCell.csv 
E3gCm-UtranRelation.csv 

## App Configuration Sample

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="consoleOutputRowCount" value="2" />
    <add key="pm_input" value="e3gPm_input.xml" />
    <add key="pm_columns" value="UtranCell;UtranRelation;GsmRelation;" />
    <add key="pm_counters" value="pmNoRabEstablishSuccessSpeech;pmNoSystemRabReleaseSpeech;pmNoSpeechDchDiscAbnorm;" />
    <add key="cm_input" value="e3gCm_input.xml" />
    <add key="cm_columns" value="UtranCell;UtranRelation;GsmRelation;" />
    <add key="cm_parameters" value="localCellId;uarfcnUl;uarfcnDl;selectionPriority;qOffset1sn" />
  </appSettings>
</configuration>
```