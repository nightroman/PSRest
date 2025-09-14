# Invoke-RestHttp

## Name
```
Invoke-RestHttp : Invokes VSCode REST Client files (.http, .rest).
```

## Syntax
```
[-Path] String [-Environment RestEnvironment] [-HeadersVariable String] [-Tag String]

-Text String [-Environment RestEnvironment] [-HeadersVariable String] [-Tag String]
```

## Description
```
Parses and invokes an HTTP request from VSCode REST Client file (.http,
.rest) or provided as Text and returns the response body string, either
as formatted JSON or as is.

Unlike other cmdlets, Invoke-RestHttp does not require Set-RestEnvironment.
If $RestEnvironment is not found, Invoke-RestHttp assumes default Name and
Path, either the input file directory or current location with Text.
```

## Parameters
```
-Path
    The HTTP file.
    
    Required?                    true
    Position?                    0

-Environment
    Specifies the environment, usually different from the current set by
    Set-RestEnvironment. In most cases this parameter is not used directly.
    
    Required?                    false
    Position?                    named

-HeadersVariable
    The response content headers variable name.
    [System.Net.Http.Headers.HttpContentHeaders]
    
    Required?                    false
    Position?                    named

-Tag
    The HTTP source separator comment text or line number.
    Default: $env:REST_TAG
    
    HTTP source may have several requests separated by comments like `### ...`.
    In order to tag a request to invoke:
    - for the separator `### Run Me`, use the string "Run Me"
    - or specify any line number between separator comments
    
    Required?                    false
    Position?                    named

-Text
    The HTTP text.
    
    Required?                    true
    Position?                    named
```

## Outputs
```
String
    JSON, XML, HTML, text, etc.
```
