/*

-- Beispiel für ein mögliches generiertes Load-Skript zu einer DimTable, DimView oder FactTable ...

use FH_AP_BaseLayer

truncate table [dbo].[AP_BT_D_Land]

insert into [dbo].[AP_BT_D_Land] (
       [Land_ID]
      ,[Land_KNZ]
      ,[Land_KURZBEZ]
      ,[Land_LANGBEZ]
      ,[Land_Laengengrad]
      ,[Land_Breitengrad]
-- Interessant:
	  ,Kontinent_ID 
      ,[Kontinent_KNZ]
-- /
      ,[T_Modifikation]
      ,[T_Bemerkung]
      ,[T_Benutzer]
      ,[T_System]
      ,[T_Erst_DAT]
      ,[T_Aend_DAT]
      ,[T_Ladelauf_NR]
)
SELECT [Land_ID]
      ,[Land_KNZ]
      ,[Land_KURZBEZ]
      ,[Land_LANGBEZ]
      ,[Land_Laengengrad]
      ,[Land_Breitengrad]
-- Interessant:
	  ,cKontinent.Kontinent_ID as Kontinent_ID
      ,b.[Kontinent_KNZ]
-- /
      ,b.[T_Modifikation]
      ,b.[T_Bemerkung]
      ,b.[T_Benutzer]
      ,b.[T_System]
      ,b.[T_Erst_DAT]
      ,b.[T_Aend_DAT]
      ,b.[T_Ladelauf_NR]
  FROM [FH_AP_BaseLayer].[dbo].[AP_BL_D_Land] as b
  -- Interessant:
  inner join FH_AP_BaseLayer.dbo.AP_BL_D_Kontinent as cKontinent 
  on b.Kontinent_KNZ = cKontinent.Kontinent_KNZ
  -- /

 */