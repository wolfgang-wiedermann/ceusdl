using System;
using System.IO;
using Kdv.CeusDL.Parser.TmpModel;
using Kdv.CeusDL.Parser.Model;
using static Kdv.CeusDL.Parser.CeusDLParserState;

namespace Kdv.CeusDL.Parser
{
    ///
    /// Parser-Bestandteil, der das Einlesen des Config-Blocks übernimmt
    ///
    public partial class CeusDLParser {

        TmpConfig currentConfig = new TmpConfig();
        TmpConfigItem currentConfigItem = new TmpConfigItem();

         private void onInConfig(int pos, string code) {
            char c = code[pos];
            if(IsWhitespaceChar(c)) {
                // Ignorieren: TODO: Blanks in Namen verbieten!
            } else if(c == '{') {
                // Wechsel zu Interface-Body                                
                buf = "";
                this.state = IN_CONFIG_PARAM_NAME;
            } else {
                throw new InvalidCharacterException(c);
            }
        }

        private void onInConfigParamName(int pos, string code) {
            char c = code[pos];
            if(IsWhitespaceChar(c)) {
                // Ignorieren: TODO: Blanks in Namen verbieten!
            } else if(c == '}' && buf.Length == 0) {
                buf = "";
                this.state = INITIAL;
            } else if(c == '=') {
                // Wechsel zu Interface-Body
                Log($"ConfigItem.Name: {buf}");
                this.currentConfigItem.Name = buf;
                buf = "";
                this.state = IN_CONFIG_PARAM_VALUE;
            } else if(IsValidObjectNameChar(c)) {
                buf += c;
            } else {
                throw new InvalidCharacterException(c);
            }
        }
        
        private void onInConfigParamValue(int pos, string code) {
            char c = code[pos];
            if(IsWhitespaceChar(c)) {
                // Ignorieren: TODO: Blanks in Namen verbieten!
            } else if(c == '"') {                
                buf = "";
                this.state = IN_CONFIG_PARAM_STRING;
            } else if(c == ';') {                
                buf = "";
                this.state = IN_CONFIG_PARAM_NAME;
            } else if(c == '}') {                                
                buf = "";
                this.state = INITIAL;                
            } else {
                throw new InvalidCharacterException(c);
            }
        }

        private void onInConfigParamString(int pos, string code) {
            char c = code[pos];
            if(IsWhitespaceChar(c)) {
                // Ignorieren: TODO: Blanks in Namen verbieten!
            } else if(c == '"') {
                // Wechsel zu Interface-Body
                Log($"ConfigItem.Value: {buf}");
                this.currentConfigItem.Value = buf;
                this.currentConfig.Items.Add(this.currentConfigItem);
                this.currentConfigItem = new TmpConfigItem();
                buf = "";
                this.state = IN_CONFIG_PARAM_VALUE;
            } else if(IsValidObjectNameChar(c)) {
                buf += c;
            } else {
                throw new InvalidCharacterException(c);
            }
        }        
    }
}