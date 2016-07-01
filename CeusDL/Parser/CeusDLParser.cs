using System;
using System.IO;
using Kdv.CeusDL.Parser.TmpModel;
using static Kdv.CeusDL.Parser.CeusDLParserState;

namespace Kdv.CeusDL.Parser
{
    public class CeusDLParser {

        TmpParserResult result = new TmpParserResult();
        TmpInterface currentInterface = new TmpInterface();
        TmpInterfaceAttribute currentInterfaceAttribute = new TmpInterfaceAttribute();
        CeusDLParserState state = CeusDLParserState.INITIAL;
        string buf = "";

        public ParserResult Parse(string code) {
            result = new TmpParserResult();
            state = CeusDLParserState.INITIAL;
            buf = "";

            for(int i = 0; i < code.Length; i++) {
                switch(state) {
                    case INITIAL:
                        onInitial(i, code);
                        break;
                    case IN_OUTERCOMMENT:
                        onInOuterComment(i, code);
                        break;
                    case IN_OBJECTTYPE_NAME:
                        onInObjectTypeName(i, code);
                        break;
                    case IN_INTERFACE_NAME:
                        onInInterfaceName(i, code);
                        break;
                    default:
                        Console.WriteLine($"Reached unhandled State {state}");
                        return null;
                        //break;
                }
            }

            // TODO neu setzen...
            return null;
        }

        public ParserResult ParseFile(string fileName) {
            if(!File.Exists(fileName)) {
                throw new FileNotFoundException($"Die Datei {fileName} konnte nicht gefunden werden");
            }

            return Parse(File.ReadAllText(fileName));
        }

        #region InnerParserFunctions

        private void onInitial(int pos, string code) {
            char c = code[pos];
            if(c == '/') {
                if(pos > 0 && code[pos-1] == '/') {
                    this.state = IN_OUTERCOMMENT;
                }
            } else if(IsWhitespaceChar(c)) {
                // Zeichen ignorieren
            } else if(IsValidLetter(c)) {
                buf += c;
                this.state = IN_OBJECTTYPE_NAME;
            } else {
                throw new InvalidCharacterException(c);
            }
        }

        private void onInOuterComment(int pos, string code) {
            char c = code[pos];
            if(c == '\n') {
                this.state = INITIAL;
            }
        }

        private void onInObjectTypeName(int pos, string code) {
            char c = code[pos];
            if(IsWhitespaceChar(c)) {
                // DEBUG:
                Console.WriteLine(buf);
                switch(buf) {
                    case "interface":
                        this.currentInterface = new TmpInterface();
                        // TODO: evtl. erst am Ende des Interfaces ...
                        this.result.Interfaces.Add(this.currentInterface);
                        this.state = IN_INTERFACE_NAME;
                        break;
                    case "attribute":
                        this.state = IN_ATTRIBUTE_NAME;
                        break;
                    case "metric":
                        this.state = IN_METRIC_NAME;
                        break;
                    default:
                        throw new InvalidTokenException(buf);
                }
                buf = "";
            } else if (IsValidLetter(c)) {
                buf += c;
            } else {
                throw new InvalidCharacterException(c);
            }
        }

        private void onInInterfaceName(int pos, string code) {
            char c = code[pos];
            if(IsWhitespaceChar(c)) {
                // Ignorieren: TODO: Blanks in Namen verbieten!
            } else if(c == '{') {
                // Wechsel zu Interface-Body
                Console.WriteLine($"InterfaceName: {buf}");
                this.currentInterface.Name = buf;
                buf = "";
                this.state = IN_INTERFACE_BODY;
            } else if(IsValidObjectNameChar(c)) {
                buf += c;
            } else {
                throw new InvalidCharacterException(c);
            }
        }
            
        #endregion
        #region HelperFunctions

        ///
        /// Prüft, ob das übergebene Zeichen ein Leerzeichen, Tab oder
        /// Zeilenumbruch ist.
        ///
        private bool IsWhitespaceChar(char c) {
            return c == ' ' 
                || c == '\t'
                || c == '\n'
                || c == '\r';
        }

        private bool IsValidLetter(char c) {
            return (c >= 'a' && c <= 'z')
                || (c >= 'A' && c <= 'Z');
        }

        private bool IsValidNumber(char c) {
            return c >= '0' && c <= '9';
        }

        private bool IsValidObjectNameChar(char c) {
            return IsValidNumber(c) || IsValidLetter(c);
        }

        #endregion
    }
}