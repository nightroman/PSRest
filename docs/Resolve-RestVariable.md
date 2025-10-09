# Resolve-RestVariable

```text
Expands variables in input strings.
```

## Syntax

```text
Resolve-RestVariable [-Value] String[] [-Environment RestEnvironment]
```

## Description

```text
Replaces each variable embedded in the specified string with its value,
then returns the result string.
```

## Parameters

```text
-Value
    One or more strings to expand, specified as parameter or pipeline
    input. Nulls and empty strings are allowed and passed through.
    
    Required?                    true
    Position?                    0
    Accept pipeline input?       true (ByValue)
```

```text
-Environment
    Specifies the environment, usually different from the current set by
    Set-RestEnvironment. In most cases this parameter is not used directly.
    
    Required?                    false
    Position?                    named
```

## Inputs

```text
String
    String with variables to expand.
```

## Outputs

```text
String
    String with expanded variables.
```
