<#
.Synopsis
	Help script, https://github.com/nightroman/Helps
#>

Set-StrictMode -Version 3
Import-Module PSRest

$Environment = @'
		Specifies the environment, usually different from the current set by
		Set-RestEnvironment. In most cases this parameter is not used directly.
'@

### Invoke-RestHttp
@{
	command = 'Invoke-RestHttp'
	synopsis = 'Invokes VSCode REST Client files (.http, .rest).'
	description = @'
	Parses and invokes an HTTP request from VSCode REST Client file (.http,
	.rest) or provided as Text and returns the response body string, either
	as formatted JSON or as is.

	Unlike other cmdlets, Invoke-RestHttp does not require Set-RestEnvironment.
	If $RestEnvironment is not found, Invoke-RestHttp assumes default Name and
	Path, either the input file directory or current location with Text.
'@
	parameters = @{
		Path = 'The HTTP file.'
		Text = 'The HTTP text.'
		Environment = $Environment
	}
}

### Set-RestEnvironment
@{
	command = 'Set-RestEnvironment'
	synopsis = 'Sets the current environment.'
	description = @'
	Sets the current environment as $RestEnvironment in the current scope.
	This variable is used by other commands as the parameter Environment
	default.

	If Set-RestEnvironment is not invoked or its $RestEnvironment is not in
	the current or parent scope, other PSRest commands may fail. They might
	need their own Set-RestEnvironment invoked or $RestEnvironment exposed.

	Parameters DotEnvFile and SettingsFile give more control on settings.
	The default REST Client required files layout is strict: ".env" and
	".vscode/settings.json".
'@
	parameters = @{
		Name = @'
		Specifies the environment name, one of defined in ".vscode/settings.json".
		Default: $env:REST_ENV

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
'@
		Path = @'
		Specifies the directory used for files discovery.
		Default: The current location.
'@
		DotEnvFile = @'
		Specifies ".env" file explicitly.
'@
		SettingsFile = @'
		Specifies ".vscode/settings.json" file explicitly.
'@
	}
}

### Resolve-RestVariable
@{
	command = 'Get-RestVariable'
	synopsis = 'Gets the specified variable.'
	description = @'
	Gets the specified variable value or null if the variable is not found.
'@
	parameters = @{
		Name = 'The variable name.'
		Type = @'
		The variable type.
		Default: Any

		With Any, Name is the full name with a prefix like $dotenv,
		$processEnv, etc. With other types, Name is just the name.

'@
		Environment = $Environment
	}
}

### Resolve-RestVariable
@{
	command = 'Resolve-RestVariable'
	synopsis = 'Expands variables in input strings.'
	description = @'
	Replaces each variable embedded in the specified string with its value,
	then returns the result string.
'@
	parameters = @{
		Value = @'
		One or more strings to expand, specified as parameter or pipeline
		input. Nulls and empty strings are allowed and passed through.
'@
		Environment = $Environment
	}
}
