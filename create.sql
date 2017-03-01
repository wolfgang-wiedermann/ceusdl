Project ceusdsl (.NETCoreApp,Version=v1.0) was previously compiled. Skipping compilation.
-- Interface Layer

create table IL_Raum (
    Raum_KNZ varchar(50), 
    Raum_KURZBEZ varchar(100), 
    Raum_LANGBEZ varchar(500), 
    Raum_FLAECHE decimal(12,2), 
    Gebaeude_KNZ varchar(50), 
    Raumnutzungsart_KNZ varchar(50), 
    Nachbargebaeude_Gebaeude_KNZ varchar(50)
);

create table IL_Gebaeude (
    Gebaeude_KNZ varchar(50), 
    Gebaeude_KURZBEZ varchar(100), 
    Gebaeude_LANGBEZ varchar(500)
);

create table IL_Raumnutzungsart (
    Raumnutzungsart_KNZ varchar(50), 
    Raumnutzungsart_KURZBEZ varchar(100), 
    Raumnutzungsart_LANGBEZ varchar(500)
);

create table IL_Inventar (
    Inventar_KNZ varchar(50), 
    Raum_KNZ varchar(50), 
    Inventar_LANGBEZ varchar(500), 
    Inventar_Nutzungsdauer int, 
    Inventar_Anschaffungsdatum varchar(20)
);


--
-- BaseLayer
--

create table BL_Raum (
    Raum_ID int primary key auto_increment,
    Raum_KNZ varchar(50),
    Raum_KURZBEZ varchar(100),
    Raum_LANGBEZ varchar(500),
    Raum_FLAECHE decimal(12, 2),
    Raum_Gebaeude_KNZ varchar(50),
    Raum_Raumnutzungsart_KNZ varchar(50),
    Nachbargebaeude_Gebaeude_KNZ varchar(50)
);

create table BL_Gebaeude (
    Gebaeude_ID int primary key auto_increment,
    Gebaeude_KNZ varchar(50),
    Gebaeude_KURZBEZ varchar(100),
    Gebaeude_LANGBEZ varchar(500)
);

create table BL_Raumnutzungsart (
    Raumnutzungsart_ID int primary key auto_increment,
    Raumnutzungsart_KNZ varchar(50),
    Raumnutzungsart_KURZBEZ varchar(100),
    Raumnutzungsart_LANGBEZ varchar(500)
);

create table BL_Inventar (
    Inventar_ID int primary key auto_increment,
    Inventar_KNZ varchar(50),
    Inventar_Raum_KNZ varchar(50),
    Inventar_LANGBEZ varchar(500),
    Inventar_Nutzungsdauer int,
    Inventar_Anschaffungsdatum varchar(20)
);

--
-- BaseLayer Views
--

create view BL_Raum_VW as 
select bl.Raum_ID,
    il.Raum_KNZ as Raum_KNZ,
    il.Raum_KURZBEZ as Raum_KURZBEZ,
    il.Raum_LANGBEZ as Raum_LANGBEZ,
    il.Raum_FLAECHE as Raum_FLAECHE,
    il.Gebaeude_KNZ  as Raum_Gebaeude_KNZ,
    il.Raumnutzungsart_KNZ  as Raum_Raumnutzungsart_KNZ,
    il.Nachbargebaeude_Gebaeude_KNZ  as Nachbargebaeude_Gebaeude_KNZ
from IL_Raum as il
    left outer join BL_Raum as bl
    on il.Raum_KNZ = bl.Raum_KNZ;

create view BL_Gebaeude_VW as 
select bl.Gebaeude_ID,
    il.Gebaeude_KNZ as Gebaeude_KNZ,
    il.Gebaeude_KURZBEZ as Gebaeude_KURZBEZ,
    il.Gebaeude_LANGBEZ as Gebaeude_LANGBEZ
from IL_Gebaeude as il
    left outer join BL_Gebaeude as bl
    on il.Gebaeude_KNZ = bl.Gebaeude_KNZ;

create view BL_Raumnutzungsart_VW as 
select bl.Raumnutzungsart_ID,
    il.Raumnutzungsart_KNZ as Raumnutzungsart_KNZ,
    il.Raumnutzungsart_KURZBEZ as Raumnutzungsart_KURZBEZ,
    il.Raumnutzungsart_LANGBEZ as Raumnutzungsart_LANGBEZ
from IL_Raumnutzungsart as il
    left outer join BL_Raumnutzungsart as bl
    on il.Raumnutzungsart_KNZ = bl.Raumnutzungsart_KNZ;

create view BL_Inventar_VW as 
select bl.Inventar_ID,
    il.Inventar_KNZ as Inventar_KNZ,
    il.Raum_KNZ  as Inventar_Raum_KNZ,
    il.Inventar_LANGBEZ as Inventar_LANGBEZ,
    il.Inventar_Nutzungsdauer as Inventar_Nutzungsdauer,
    il.Inventar_Anschaffungsdatum as Inventar_Anschaffungsdatum
from IL_Inventar as il
    left outer join BL_Inventar as bl
    on il.Inventar_KNZ = bl.Inventar_KNZ;



