Idee zu NoHistory-Tabellen
==========================

Ich muss das grundsätzlich über eine Klasse im Model behandeln, sodass die eigentliche Codegenerierung dann unabhängig von diesem Aspekt bleibt. 

=> Nur im Rahmen der Vorbereitung der Daten wird darauf Rücksicht genommen.

Am besten wäre es, wenn die Generierung des Codes für Create, Drop und Load 
überhaupt nicht beeinflusst würde. Bei Load ist das aber kaum denkbar, da wir
hier die Situation haben, dass die Quelle der Daten und die Logik für
das Laden von der der anderen Tabellen abweicht.