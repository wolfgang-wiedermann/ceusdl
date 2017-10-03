Prototyp: CEUS Definition Language (ceusdl)
===========================================

Bei dem hier vorliegenden Code handelt es sich um den __Prototypen einer Sprache zur Definition und Generierung von Data-Warehouse-Systemen__.
Das Ziel der Sprache ist es, alle für die systematische Archivierung und Aufbereitung der Daten erforderlichen Artefakte auf der Basis
eines Best-Practice-Ansatzes zu generieren. Die Zieltechnologien des Prototypen sind Microsoft SQL Server und Microsoft .Net Core. Folglich wird
das generierte Modell als ein, auf relationalen Datenbanken basierendes Data-Warehouse-System generiert.  

__Bitte beachten Sie, dass es inzwischen eine noch nicht öffentliche Implementierung von ceusdl gibt, die sich noch in Entwicklung befindet 
und verschiedene im Prototyp noch nicht berücksichtigte Features, wie z. B. die Historisierung von Attributwerten in DimTables, die
Zerlegung in mehrere Code-Dateien, das datenverlustfreie Neugenerieren nach Code-Änderungen etc. umsetzt.__

Funktionsumfang:
----------------

* Generierung des Datenbankmodells in vier logischen Schichten
* Generierung von SQL-Code zum Löschen des Datenmodells
* Generierung der erforderlichen Constraints (Foreign Key und Unique Key)
* Generierung des vollständigen Codes für den Ladevorgang (ETL)
* Generierung einer grafischen Visualisierung der Beziehungen innerhalb des Modells

Beispiel:
---------

Hier finden Sie ein einfaches Syntax-Beispiel der in ceusdl. Es beschreibt ein Data-Warehouse-System, in dem 
Studenten klassifiziert nach Studiengang, Fachsemester und Semester verwaltet werden. Das Semester 
dient hierbei als Historien-Attribut. Das bedeutet, dass Studenten-Verlaufssätze pro Semester verwaltet werden,
also ein und derselbe Student in unterschiedlichen Semestern z. B. unterschiedlichen Studiengängen
zugeordnet sein kann.

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
* .net core 2.0 SDK (https://www.microsoft.com/net/download/core)
* Visual Studio Code (https://code.visualstudio.com/download)

Bitte beachten Sie, dass der Prototyp in keiner Weise hinsichtlich einer komfortablen Benutzung optimiert ist. Das ist Aufgabe der bereits angesprochenen produktiven Implementierung.

Um den Prototypen zu testen installieren Sie die genannten Tools und führen Sie anschließend
die folgenden Aufrufe in einem Consolenfenster aus.

```bash
mkdir ceusdl
cd ceusdl
git init
git remote add origin https://github.com/wolfgang-wiedermann/ceusdl.git
git pull origin master
dotnet restore
dotnet build

# Mit dotnet run können Sie nun die Übersetzung der Datei bewerber.ceusdl anstoßen
# danach muss in den Ordnern GeneratedCode, GeneratedSQL ... das Ergebnis der Generierung
# zu finden sein.
dotnet run

# Mit code . können Sie das Projekt in Visual Studio Code öffnen
# (Achtung, beim ersten Mal kann es etwas dauern, bis die .Net Core-Integration
#  vollständig nachgeladen ist)
code .
```

## Status

Der hier vorliegende Prototyp wurde bereits erfolgreich für die Entwicklung des Systems CEUS AP (Applicants/Studienbewerber) eingesetzt, das sich seit
Mai 2017 mit 4 Pilothochschulen im Einsatz befindet. Die Datei bewerber.ceusdl (siehe https://github.com/wolfgang-wiedermann/ceusdl/blob/master/dsl/bewerber.ceusdl) beinhaltet den vollständigen ceusdl-Code dieses Systems und kann somit als
praxiserprobtes Beispiel für die Tauglichkeit des Konzepts hinter ceusdl angesehen werden.

## Zielsetzung

Das langfristige Ziel hinter der Entwicklung dieser Sprache ist