Prototyp: CEUS Definition Language (ceusdl)
===========================================

Bei dem hier vorliegenden Code handelt es sich um den __Prototypen einer Sprache zur Definition und Generierung von Data-Warehouse-Systemen__.
Das Ziel der Sprache ist es, alle für die systematische Archivierung und Aufbereitung der Daten erforderlichen Artefakte auf der Basis
eines Best-Practice-Ansatzes zu generieren. Die Zieltechnologien des Prototypen sind Microsoft SQL Server und Microsoft .Net Core. Folglich wird
das generierte Modell als ein, auf relationalen Datenbanken basierendes Data-Warehouse-System generiert.  

__Bitte beachten Sie, dass es inzwischen eine noch nicht öffentliche Implementierung von ceusdl gibt, die sich noch in Entwicklung befindet 
und verschiedene im Prototyp noch nicht berücksichtigte Features, wie z. B. die Historisierung von Attributwerten in DimTables etc. umsetzt.__

Funktionsumfang:
----------------

* Generierung des Datenbankmodells in vier logischen Schichten
* Generierung von SQL-Code zum Löschen des Datenmodells
* Generierung der erforderlichen Constraints (Foreign Key und Unique Key)
* Generierung des vollständigen Codes für den Ladevorgang (ETL)
* Generierung einer grafischen Visualisierung der Beziehungen innerhalb des Modells

Beispiel:
---------

```
config {
     prefix="AP";
     il_database="XY_InterfaceLayer";
     bl_database="XY_AP_BaseLayer";
     bt_database="XY_AP_BaseLayer";
     al_database="XY_AP_Warehouse";     
     etl_db_server="ETL-Datenbankserver";
}

interface Semester : DefTable {
    base KNZ:varchar(len="50", primary_key="true");
    base KURZBEZ:varchar(len="100");
    base LANGBEZ:varchar(len="500");
}

interface FachSemester : DefTable {
    base KNZ:varchar(len="50", primary_key="true");
    base KURZBEZ:varchar(len="100");
    base LANGBEZ:varchar(len="500");
}

interface Studiengang : DimTable(mandant=true) {
    base KNZ:varchar(len="50", primary_key="true");
    base KURZBEZ:varchar(len="100");
    base LANGBEZ:varchar(len="500");   
}

interface Student : FactTable(mandant=true, history=Semester.KNZ) {
    base Matrikelnummer:varchar(len="10", primary_key="true");
    ref  Semester.KNZ;
    ref  FachSemester.KNZ;
    ref  Studiengang.KNZ;
    fact Anzahl_F:int;       // immer 1
    fact CreditPoints_F:int; // Anzahl der bis zum angegebenen Semester erworbenen Creditpoints
}
```

## Installation

Die (testweise) Nutzung von ceusdl setzt die folgenden Werkzeuge voraus und ist auf Windows, MacOS und Linux (getestet mit Ubuntu 16.04) möglich:

* git (siehe https://git-scm.com/downloads)
* .net core SDK (https://www.microsoft.com/net/download/core)
* Visual Studio Code (https://code.visualstudio.com/download)

Bitte beachten Sie, dass der Prototyp in keiner Weise hinsichtlich einer komfortablen Benutzung optimiert ist. Das ist Aufgabe der bereits angesprochenen produktiven Implementierung.

Um den Prototypen zu testen installieren Sie die genannten Tools und führen Sie anschließend
die folgenden Aufrufe in einem Consolenfenster aus.

```
mkdir ceusdl
cd ceusdl
git init
git remote add origin https://github.com/wolfgang-wiedermann/ceusdl.git
git pull origin master
dotnet restore
dotnet build

# Mit dotnet run können Sie nun die Übersetzung der Datei bewerber.ceusdl anstoßen
dotnet run

# Mit code . können Sie das Projekt in Visual Studio Code öffnen
# (Achtung, beim ersten Mal kann es etwas dauern, bis die .Net Core-Integration
#  vollständig nachgeladen ist)
code .
```