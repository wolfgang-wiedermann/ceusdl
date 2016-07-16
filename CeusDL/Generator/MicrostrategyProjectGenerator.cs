///
/// Die Details zum Microstrategy Project Generator
///
/// Die Projektgenerierung erfolgt mittels generierter Command Manager Skripte
/// die dann vom C#-Code via Commandline ausgeführt werden.
///
/// Siehe dazu z. B. 
/// https://community.microstrategy.com/t5/Server/TN34827-In-Command-Manager-the-command-quot-ADD-WHTABLE-quot-and/ta-p/185379
/// * ADD WHTABLE and UPDATE STRUCTURE FOR WHTABLE
/// http://search.cpan.org/~cgrady/Business-Intelligence-MicroStrategy-CommandManager-0.01/lib/Business/Intelligence/MicroStrategy/CommandManager.pm
/// * CREATE ATTRIBUTE "<attribute_name>" [DESCRIPTION "<description>"] IN FOLDER "<location_path>" [HIDDEN TRUE | FALSE] ATTRIBUTEFORM "<form_name>" [FORMDESC "<form_description>"] [FORMTYPE (NUMBER | TEXT | DATETIME | DATE | TIME | URL | EMAIL | HTML | PICTURE | BIGDECIMAL)] [SORT (NONE | ASC | /// DESC)] EXPRESSION "<form_expression>" [EXPSOURCETABLES "<sourcetable1>" [, "<sourcetable1> [, ..."<sourcetableN>"]]] LOOKUPTABLE "<lookup_table>" FOR PROJECT "<project_name>";
/// * http://stackoverflow.com/questions/12966225/where-can-i-find-the-documentation-for-microstrategy-command-manager
///
/// Executing Command Manager Scripts from Commandline-Tool
/// * https://community.microstrategy.com/t5/Server/TN6392-How-to-execute-a-command-manager-script-through-a-command/ta-p/167428
///
/// Sonstiges:
/// * https://github.com/mstr-dev/command-manager
/// * http://khaidoan.wikidot.com/mstr-command-manager-procedures
/// * http://docplayer.net/13841793-Automate-your-bi-administration-to-save-millions-with-command-manager-and-system-manager.html
