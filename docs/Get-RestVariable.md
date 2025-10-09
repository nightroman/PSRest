# Get-RestVariable

```text
Gets the specified variable.
```

## Syntax

```text
Get-RestVariable [-Name] String [-Environment RestEnvironment] [-Type VariableType]
```

## Description

```text
Gets the specified variable value or null if the variable is not found.
```

## Parameters

```text
-Name
    The variable name.
    
    Required?                    true
    Position?                    0
```

```text
-Environment
    Specifies the environment, usually different from the current set by
    Set-RestEnvironment. In most cases this parameter is not used directly.
    
    Required?                    false
    Position?                    named
```

```text
-Type
    The variable type.
    Default: Any
    
    With Any, Name is the full name with a prefix like $dotenv,
    $processEnv, etc. With other types, Name is just the name.
    
    Values : Any, Env, Shared, DotEnv, ProcessEnv
    
    Required?                    false
    Position?                    named
```

## Outputs

```text
String
    Variable value.
```
