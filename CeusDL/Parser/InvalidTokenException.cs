using System;

namespace Kdv.CeusDL.Parser
{
    public class InvalidTokenException : Exception {
        public string Token {
            get;
        }
        public InvalidTokenException(string c) : base($"Ungültiges Token {c}") {
            this.Token = c;
        }
    }
}