# Import-RestDotEnv
## Name
```
Import-RestDotEnv : Imports variables from ".env".
```
## Syntax
```
[[-Path] String]

[[-Path] String] -AsDictionary 

[[-Path] String] -AsKeyValue 
```
## Description
```
It imports variables from the specified or default ".env" file and applies
them to the current process environment, unless As* is used. In that case
the process environment is not changed and variables are returned.
```
## Parameters
```
-Path
    Specifies the existing file to import.
    
    The default file, omitted or empty, is ".env" in the current location
    or the first found in parents. If the file is not found, no variables
    are imported.
    
    Required?                    false
    Position?                    0

-AsDictionary
    Tells to get variables as `[Dictionary[string, string]]`, case sensitive.
    Duplicates are processed as `TakeLast`.
    
    Required?                    true
    Position?                    named

-AsKeyValue
    Tells to get variables as `[KeyValuePair[string, string]]`, including
    duplicates.
    
    Required?                    true
    Position?                    named
```