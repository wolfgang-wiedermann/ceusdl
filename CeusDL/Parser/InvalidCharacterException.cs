using System;

namespace Kdv.CeusDL.Parser
{
    public class InvalidCharacterException : Exception {
        public char Character {
            get;
        }
        public InvalidCharacterException(char c) : base($"Ungültiges Zeichen {c}") {
            this.Character = c;
        }
    }
}