# Import-RestDotEnv

```text
Imports variables from ".env".
```

## Syntax

```text
Import-RestDotEnv [[-Path] String]
```

```text
Import-RestDotEnv [[-Path] String] -AsDictionary
```

```text
Import-RestDotEnv [[-Path] String] -AsKeyValue
```

## Description

```text
It imports variables from the specified or default ".env" file and applies
them to the current process environment, unless As* is used. In that case
the process environment is not changed and variables are returned.

PSRest reads .env files using DotNetEnv. See ".env file structure":
https://github.com/tonerdo/dotnet-env/blob/master/README.md#env-file-structure
```

## Parameters

```text
-Path
    Specifies the existing file to import.
    
    The default file, omitted or empty, is ".env" in the current location
    or the first found in parents. If the file is not found, no variables
    are imported.
    
    Required?                    false
    Position?                    0
```

```text
-AsDictionary
    Tells to get variables as `[Dictionary[string, string]]`, case sensitive.
    Duplicates are processed as `TakeLast`.
    
    Required?                    true
    Position?                    named
```

```text
-AsKeyValue
    Tells to get variables as `[KeyValuePair[string, string]]`, including
    duplicates.
    
    Required?                    true
    Position?                    named
```

## Outputs

```text
Dictionary[String, String]
    when -AsDictionary
```

```text
KeyValuePair[String, String]
    when -AsKeyValue
```
