# Set-RestEnvironment

```
Sets the current environment.
```

## Syntax

```
Set-RestEnvironment [[-Name] String] [[-Path] String] [-DotEnvFile String] [-SettingsFile String]
```

## Description

```
Sets the current environment as $RestEnvironment in the current scope.
This variable is used by other commands as the parameter Environment
default.

If Set-RestEnvironment is not invoked or its $RestEnvironment is not in
the current or parent scope, other PSRest commands may fail. They might
need their own Set-RestEnvironment invoked or $RestEnvironment exposed.

Parameters DotEnvFile and SettingsFile give more control on settings.
The default REST Client required files layout is strict: ".env" and
"http-client.env.json" or ".vscode/settings.json".
```

## Parameters

```
-Name
    Specifies the environment name, one of defined in
    "http-client.env.json" or ".vscode/settings.json".
    
    If Name is empty then not empty $env:REST_ENV is used or '$shared'.
    
    Example: '$shared' (default), 'local', 'production':
    
        {
          "rest-client.environmentVariables": {
            "$shared": {
              ...
            },
            "local": {
              ...
            },
            "production": {
              ...
            }
          }
        }
    
    Required?                    false
    Position?                    0
```

```
-Path
    Specifies the directory used for files discovery.
    Default: The current location.
    
    Required?                    false
    Position?                    1
```

```
-DotEnvFile
    Specifies ".env" file explicitly.
    
    Required?                    false
    Position?                    named
```

```
-SettingsFile
    Specifies ".vscode/settings.json" file explicitly.
    
    Required?                    false
    Position?                    named
```
