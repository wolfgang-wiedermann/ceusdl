Prototyp: CEUS Definition Language (ceusdl)
===========================================

Prototyp einer Sprache zur Definition von Relationalen OLAP-Systemen.

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