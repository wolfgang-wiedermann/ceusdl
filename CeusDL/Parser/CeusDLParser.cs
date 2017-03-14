using System;
using System.IO;
using Kdv.CeusDL.Parser.TmpModel;
using Kdv.CeusDL.Parser.Model;
using static Kdv.CeusDL.Parser.CeusDLParserState;

namespace Kdv.CeusDL.Parser
{
    public class CeusDLParser {
        // Ein- und Ausschalten der Debug-Ausgaben ...
        private static bool isDebug = false;
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
                    case IN_INTERFACE_TYPE:
                        onInInterfaceType(i, code);
                        break;                        
                    case IN_INTERFACE_BODY:
                        onInInterfaceBody(i, code);
                        break;
                    case IN_INTERFACE_COMMENT:
                        onInInterfaceComment(i, code);
                        break;
                    case IN_INTERFACE_ATTRIBUTE_NAME:
                        onInInterfaceAttributeName(i, code);
                        break;
                    case IN_INTERFACE_ATTRIBUTE_TYPE:
                        onInInterfaceAttributeType(i, code);
                        break;
                    case IN_INTERFACE_PARAM_LIST:
                        onInInterfaceParamList(i, code);
                        break;
                    case IN_INTERFACE_PARAM_LEN:
                        onInInterfaceParamLen(i, code);
                        break;
                    case IN_INTERFACE_PARAM_LEN_VALUE:
                        onInInterfaceParamLenValue(i, code);
                        break;
                    case IN_INTERFACE_PARAM_PK:
                        onInInterfaceParamPk(i, code);
                        break;
                    case IN_INTERFACE_PARAM_PK_VALUE:
                        onInInterfaceParamPkValue(i, code);
                        break;
                    case BEHIND_INTERFACE_PARAM_LIST:
                        onBehindInterfaceParamList(i, code);
                        break;
                    case IN_INTERFACE_PARAM_UNIT:
                        onInInterfaceParamUnit(i, code);
                        break;
                    case IN_INTERFACE_PARAM_UNIT_VALUE:
                        onInInterfaceParamUnitValue(i, code);
                        break;
                    case IN_INTERFACE_ATTRIBUTE_FOREIGN_IFA:
                        onInInterfaceAttributeForeignIfa(i, code);
                        break;
                    case IN_INTERFACE_ATTRIBUTE_REFERENCED_FIELD:
                        onInInterfaceAttributeReferencedField(i, code);
                        break;
                    case BEFORE_INTERFACE_ATTRIBUTE_ALIAS:
                        onBeforeInterfaceAttributeAlias(i, code);
                        break;
                    case IN_INTERFACE_ATTRIBUTE_ALIAS:
                        onInInterfaceAttributeAlias(i, code);
                        break;
                    default:
                        Console.WriteLine($"Reached unhandled State {state}");
                        return null;
                        //break;
                }
            }

            var converter = new TmpModelConverter(result);
            return converter.ToParserResult();
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
                //Log(buf);
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
            } else if(c == ':') {
                // Wechsel zu Interface-Type
                Log($"InterfaceName: {buf}");
                this.currentInterface.Name = buf;
                buf = "";
                this.state = IN_INTERFACE_TYPE;
            } else if(c == '{') {
                // Wechsel zu Interface-Body
                Log($"InterfaceName: {buf}");
                this.currentInterface.Name = buf;
                buf = "";
                this.state = IN_INTERFACE_BODY;
            } else if(IsValidObjectNameChar(c)) {
                buf += c;
            } else {
                throw new InvalidCharacterException(c);
            }
        }

        private void onInInterfaceType(int pos, string code) {
            char c = code[pos];
            if(IsWhitespaceChar(c)) {
                // Ignorieren: TODO: Blanks in Namen verbieten!
            } else if(c == '{') {
                // Wechsel zu Interface-Body
                Log($"InterfaceType: {buf}");
                this.currentInterface.Type = buf;
                buf = "";
                this.state = IN_INTERFACE_BODY;
            } else if(IsValidObjectNameChar(c)) {
                buf += c;
            } else {
                throw new InvalidCharacterException(c);
            }
        }

        private void onInInterfaceBody(int pos, string code) {
            char c = code[pos];
            if(c == '/' && buf.Length == 0) {
                if(code[pos-1] == '/') {
                    this.state = IN_INTERFACE_COMMENT;
                } else if(!IsWhitespaceChar(code[pos-1])) {
                    throw new InvalidTokenException(code[pos-1]+"/");
                } 
            } else if(IsWhitespaceChar(c) && buf.Length == 0) {
                // Leerzeichen vor dem nächsten Token ignorieren
            } else if(IsWhitespaceChar(c) && buf.Length > 0) {
                // Leerzeichen nach dem nächsten Token!
                Log($"AttributeType: {buf}");
                switch(buf) {
                    case "base":
                        this.currentInterfaceAttribute = new TmpInterfaceAttribute();
                        this.currentInterface.Attributes.Add(this.currentInterfaceAttribute);
                        this.currentInterfaceAttribute.AttributeType = TmpInterfaceAttributeType.BASE;
                        this.state = IN_INTERFACE_ATTRIBUTE_NAME;
                        break;
                    case "ref":
                        this.currentInterfaceAttribute = new TmpInterfaceAttribute();
                        this.currentInterface.Attributes.Add(this.currentInterfaceAttribute);
                        this.currentInterfaceAttribute.AttributeType = TmpInterfaceAttributeType.REF;
                        this.state = IN_INTERFACE_ATTRIBUTE_FOREIGN_IFA;                            break;
                    default:
                        throw new InvalidTokenException(buf);
                }
                buf = "";
            } else if(c == '}') {
                buf = "";
                this.state = INITIAL;
            } else {
                buf += c;
            }
        }

        private void onInInterfaceComment(int pos, string code) {
            char c = code[pos];
            if(c == '\n') {
                this.state = IN_INTERFACE_BODY;
            } 
        }

        private void onInInterfaceAttributeName(int pos, string code) {
            char c = code[pos];
            if(c == ':') {
                Log($"AttributeName: {buf}");
                this.currentInterfaceAttribute.Name = buf;
                buf = "";
                this.state = IN_INTERFACE_ATTRIBUTE_TYPE;
            } else if (!(IsWhitespaceChar(c) && buf.Length > 0)) {
                buf += c;
            } else {
                new InvalidCharacterException(' ');
            }
        }

        private void onInInterfaceAttributeType(int pos, string code) {
            char c = code[pos];
            if(c == '(') {
                // wechsel in Parameterliste
                Log($"DataType     : {buf}");
                this.currentInterfaceAttribute.DataType = buf;
                buf = "";
                this.state = IN_INTERFACE_PARAM_LIST;
            } else if (c == ';') {
                // Attribut abschließen
                this.currentInterfaceAttribute.DataType = buf;
                buf = "";
                this.state = IN_INTERFACE_BODY;
            } else if (IsWhitespaceChar(c)) {
                // Ignorieren, später sicherstellen dass keine Lücken im Typbezeichner sind
            } else if (IsValidObjectNameChar(c)) {
                buf += c;
            } else {
                throw new InvalidCharacterException(c);
            }
        }

        private void onInInterfaceParamList(int pos, string code) {
            char c = code[pos];
            if(c == '=') {
                switch(buf) {
                    case "len":
                        this.state = IN_INTERFACE_PARAM_LEN;
                        break;
                    case "primary_key":
                        this.state = IN_INTERFACE_PARAM_PK;
                        break;
                    case "unit":
                        this.state = IN_INTERFACE_PARAM_UNIT;
                        break;
                    default:
                        throw new InvalidTokenException(buf);
                }
                buf = "";
            } else if (IsValidObjectNameChar(c)) {
                buf += c;
            } 
        }

        private void onInInterfaceParamLen(int pos, string code) {
            char c = code[pos];
            if(c == ',') {
                this.state = IN_INTERFACE_PARAM_LIST;
            } else if(c == ')') {
                this.state = BEHIND_INTERFACE_PARAM_LIST;
            } else if(c == '"') {
                this.state = IN_INTERFACE_PARAM_LEN_VALUE;
            } else if(!IsWhitespaceChar(c)) {
                throw new InvalidCharacterException(c);
            }
        }

        private void onInInterfaceParamLenValue(int pos, string code) {
            char c = code[pos];
            if(c == '"' && buf.Length > 0) {
                Log($"Length       : {buf}");
                this.state = IN_INTERFACE_PARAM_LEN;
                this.currentInterfaceAttribute.Length = buf;
                buf = "";
            } else if(IsValidNumber(c) || c == ',') {
                buf += c;
            } else {
                throw new InvalidCharacterException(c);
            }
        }

        private void onInInterfaceParamPk(int pos, string code) {
            char c = code[pos];
            if(c == ',') {
                this.state = IN_INTERFACE_PARAM_LIST;
            } else if(c == ')') {
                this.state = BEHIND_INTERFACE_PARAM_LIST;
            } else if(c == '"') {
                this.state = IN_INTERFACE_PARAM_PK_VALUE;
            } else if(!IsWhitespaceChar(c)) {
                throw new InvalidCharacterException(c);
            }
        }

        private void onInInterfaceParamPkValue(int pos, string code) {
            char c = code[pos];
            if(c == '"' && buf.Length > 0) {
                Log($"Primary Key  : {buf}");
                this.state = IN_INTERFACE_PARAM_PK;
                this.currentInterfaceAttribute.PrimaryKey = buf;
                buf = "";
            } else if(IsValidLetter(c)) {
                buf += c;
            } else {
                throw new InvalidCharacterException(c);
            }
        }

        private void onInInterfaceParamUnit(int pos, string code) {
            char c = code[pos];
            if(c == ',') {
                this.state = IN_INTERFACE_PARAM_LIST;
            } else if(c == ')') {
                this.state = BEHIND_INTERFACE_PARAM_LIST;
            } else if(c == '"') {
                this.state = IN_INTERFACE_PARAM_UNIT_VALUE;
            } else if(!IsWhitespaceChar(c)) {
                throw new InvalidCharacterException(c);
            }
        }

        private void onInInterfaceParamUnitValue(int pos, string code) {
            char c = code[pos];
            if(c == '"' && buf.Length > 0) {
                Log($"Unit         : {buf}");
                this.state = IN_INTERFACE_PARAM_UNIT;
                this.currentInterfaceAttribute.Unit = buf;
                buf = "";
            } else if(IsValidLetter(c)) {
                buf += c;
            } else {
                throw new InvalidCharacterException(c);
            }
        }

        private void onBehindInterfaceParamList(int pos, string code) {
            char c = code[pos];
            if(c == ';') {
                this.state = IN_INTERFACE_BODY;
            }
        }

        private void onInInterfaceAttributeForeignIfa(int pos, string code) {
            char c = code[pos];
            if(c == '.') {
                Log($"Foreign Ifa  : {buf}");
                this.currentInterfaceAttribute.ForeignInterface = buf;
                buf = "";
                this.state = IN_INTERFACE_ATTRIBUTE_REFERENCED_FIELD;
            } else if (IsValidObjectNameChar(c)) {
                buf += c;
            } else if(!IsWhitespaceChar(c)) {
                throw new InvalidCharacterException(c);
            }
        }

        private void onInInterfaceAttributeReferencedField(int pos, string code) {
            char c = code[pos];
            if(c == ';') {
                Log($"ReferencedFld: {buf}");
                this.state = IN_INTERFACE_BODY;
                this.currentInterfaceAttribute.ReferencedField = buf;
                buf = "";
            } else if(IsValidObjectNameChar(c)) {
                buf += c;
            } else if(IsWhitespaceChar(c)) {
                Log($"ReferencedFld: {buf}");
                this.state = BEFORE_INTERFACE_ATTRIBUTE_ALIAS;
                this.currentInterfaceAttribute.ReferencedField = buf;
                buf = "";
            } else {
                throw new InvalidCharacterException(c);
            }
        }

        private void onBeforeInterfaceAttributeAlias(int pos, string code) {
            char c = code[pos];
            if(c == 'a' || c == 's') {
                buf += c;
            } else if(buf.Equals(buf) && IsWhitespaceChar(c)) {
                buf = "";
                this.state = IN_INTERFACE_ATTRIBUTE_ALIAS;
            } else if(buf.Length == 0 && IsWhitespaceChar(c)) {
                // Ignorieren
            } else {
                throw new InvalidCharacterException(c);
            }
        }

        private void onInInterfaceAttributeAlias(int pos, string code) {
            char c = code[pos];
            if(c == ';') {
                Log($"IntAttributeA: {buf}");
                this.currentInterfaceAttribute.As = buf;
                buf = "";
                this.state = IN_INTERFACE_BODY;
            } else if(IsValidObjectNameChar(c)) {
                buf += c;
            } else if (!(IsWhitespaceChar(c) && buf.Length == 0)){
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
            return IsValidNumber(c) 
                || IsValidLetter(c)
                || c == '_'
                || c == '-';
        }

        private void Log(string message) {
            if(isDebug) {
                System.Console.WriteLine(message);
            }
        }

        #endregion
    }
}