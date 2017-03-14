namespace Kdv.CeusDL.Parser {
    public enum CeusDLParserState {
        // Initial State
        INITIAL, 
        // In Object-Type Identifier
        IN_OBJECTTYPE_NAME,
        // Comments 
        IN_OUTERCOMMENT, IN_INTERFACE_COMMENT, IN_ATTRIBUTE_COMMENT, IN_METRIC_COMMENT,
        // Interface States
        IN_INTERFACE_NAME, IN_INTERFACE_TYPE, IN_INTERFACE_BODY,
        IN_INTERFACE_ATTRIBUTE_NAME, IN_INTERFACE_ATTRIBUTE_TYPE,
        IN_INTERFACE_PARAM_LIST, BEHIND_INTERFACE_PARAM_LIST,
        IN_INTERFACE_PARAM_LEN, IN_INTERFACE_PARAM_LEN_VALUE,
        IN_INTERFACE_PARAM_PK, IN_INTERFACE_PARAM_PK_VALUE,
        IN_INTERFACE_PARAM_UNIT, IN_INTERFACE_PARAM_UNIT_VALUE,
        IN_INTERFACE_ATTRIBUTE_FOREIGN_IFA, IN_INTERFACE_ATTRIBUTE_REFERENCED_FIELD,
        BEFORE_INTERFACE_ATTRIBUTE_ALIAS, IN_INTERFACE_ATTRIBUTE_ALIAS,

        // Attribute States
        IN_ATTRIBUTE_NAME, IN_ATTRIBUTE_HEADER, IN_ATTRIBUTE_BODY,

        // Metric States
        IN_METRIC_NAME, IN_METRIC_BODY
    }
}