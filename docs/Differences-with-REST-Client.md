# Differences with REST Client

The main difference: PSRest is CLI, REST Client is GUI. PSRest is designed for
getting request results not just for viewing. This implies some stricter rules.

PSRest fails on undefined variables instead of leaving them unresolved by REST
Client like `{{var}}`. Unresolved variables cause cryptic invalid input errors
in some cases and various processing issues in others.

PSRest GraphQL operations support header `X-GraphQL-Operation: operationName`
for the exact operation in multi-operation GraphQL. REST Client invokes the
first operation.

Request variables JSONPath:
- Missing path: PSRest gets empty string, REST Client gets unresolved variable.

Request variables XPath:
- Missing path: PSRest gets empty string, REST Client gets unresolved variable.
- Element with children inner XML: PSRest trims spaces, REST Client does not.

PSRest automatically calls required named requests, REST Client does not.
