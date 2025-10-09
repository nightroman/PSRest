# Differences with REST Client

## CLI, not GUI

The main difference: PSRest is CLI, REST Client is GUI. PSRest is designed for
getting request results not just for viewing. This implies some stricter rules.

## Undefined variables

PSRest fails on undefined variables instead of leaving them unresolved by REST
Client like `{{var}}`. Unresolved variables cause cryptic invalid input errors
in some cases and various processing issues in others.

## X-GraphQL-Operation

PSRest GraphQL operations support header `X-GraphQL-Operation: operationName`
for the exact operation in multi-operation GraphQL. REST Client invokes the
first operation.

## Request variables

Request variables JSONPath:
- Missing path: PSRest gets empty string, REST Client gets unresolved variable.

Request variables XPath:
- Missing path: PSRest gets empty string, REST Client gets unresolved variable.
- Element with children inner XML: PSRest trims spaces, REST Client does not.

## Named requests

PSRest automatically calls required named requests, REST Client does not.

## .env file structure

[DotNetEnv]: https://github.com/tonerdo/dotnet-env
[.env file structure]: https://github.com/tonerdo/dotnet-env/blob/master/README.md#env-file-structure

PSRest reads `.env` files using [DotNetEnv].
See [.env file structure] for interpolation, quoting, etc.

Thus, values with `$ENVVAR`, `${ENVVAR}` are interpolated,
depending on quoting and escaping.

REST Client reads `.env` files in its own way.
Presumably it does not support interpolation.
