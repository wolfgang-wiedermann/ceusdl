using System;
using System.IO;

namespace Kdv.CeusDL.Utils {
    public class ResultFileFinisher {
        public static void AggregateDrops(string folder) {
            string targetFile = Path.Combine(folder, "Drop_All.sql");

            if(!Directory.Exists(folder)) {
                throw new InvalidOperationException($"Der Ordner {folder} muss zum Aufruf existieren!");
            }

            string buf = "";
            buf = File.ReadAllText(Path.Combine(folder, "AL_Drop.sql"));
            File.WriteAllText(targetFile, buf+"\n\n");

            buf = File.ReadAllText(Path.Combine(folder, "BT_Drop.sql"));
            File.AppendAllText(targetFile, buf+"\n\n");

            buf = File.ReadAllText(Path.Combine(folder, "BL_Drop.sql"));
            File.AppendAllText(targetFile, buf+"\n\n");

            buf = File.ReadAllText(Path.Combine(folder, "IL_Drop.sql"));
            File.AppendAllText(targetFile, buf+"\n\n");
        }

        public static void AggregateCreates(string folder) {
            string targetFile = Path.Combine(folder, "Create_All.sql");

            if(!Directory.Exists(folder)) {
                throw new InvalidOperationException($"Der Ordner {folder} muss zum Aufruf existieren!");
            }

            string buf = "";
            buf = File.ReadAllText(Path.Combine(folder, "IL_Create.sql"));
            File.WriteAllText(targetFile, buf+"\ngo\n\n");

            buf = File.ReadAllText(Path.Combine(folder, "BL_Create.sql"));
            File.AppendAllText(targetFile, buf+"\ngo\n\n");

            buf = File.ReadAllText(Path.Combine(folder, "BT_Create.sql"));
            File.AppendAllText(targetFile, buf+"\n\n");

            buf = File.ReadAllText(Path.Combine(folder, "AL_Create.sql"));
            File.AppendAllText(targetFile, buf+"\n\n");
        }
    }
}