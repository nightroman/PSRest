# Resolve-RestVariable
## Name
```
Resolve-RestVariable : Expands variables in input strings.
```
## Syntax
```
[-Value] String[] [-Environment RestEnvironment]
```
## Description
```
Replaces each variable embedded in the specified string with its value,
then returns the result string.
```
## Parameters
```
-Value
    One or more strings to expand, specified as parameter or pipeline
    input. Nulls and empty strings are allowed and passed through.
    
    Required?                    true
    Position?                    0
    Accept pipeline input?       true (ByValue)

-Environment
    Specifies the environment, usually different from the current set by
    Set-RestEnvironment. In most cases this parameter is not used directly.
    
    Required?                    false
    Position?                    named
```