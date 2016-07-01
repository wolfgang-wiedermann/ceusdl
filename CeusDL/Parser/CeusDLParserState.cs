namespace Kdv.CeusDL.Parser {
    public enum CeusDLParserState {
        // Initial State
        INITIAL, 
        // In Object-Type Identifier
        IN_OBJECTTYPE_NAME,
        // Comments 
        IN_OUTERCOMMENT, IN_INTERFACECOMMENT, IN_ATTRIBUTECOMMENT, IN_METRICCOMMENT,
        // Interface States
        IN_INTERFACE_NAME, IN_INTERFACE_BODY,

        // Attribute States
        IN_ATTRIBUTE_NAME, IN_ATTRIBUTE_HEADER, IN_ATTRIBUTE_BODY,

        // Metric States
        IN_METRIC_NAME, IN_METRIC_BODY
    }
}