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
		HeadersVariable = @'
		The response content headers variable name.
		[System.Net.Http.Headers.HttpContentHeaders]
'@
		Tag = @'
		The HTTP source separator comment text or line number.
		Default: $env:REST_TAG

		HTTP source may have several requests separated by comments like `### ...`.
		In order to tag a request to invoke:
		- for the separator `### Run Me`, use the string "Run Me"
		- or specify any line number between separator comments
'@
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
	"http-client.env.json" or ".vscode/settings.json".
'@
	parameters = @{
		Name = @'
		Specifies the environment name, one of defined in ".vscode/settings.json".

		If Name is empty then not empty $env:REST_ENV is used or default '$shared'.

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

### Import-RestDotEnv
@{
	command = 'Import-RestDotEnv'
	synopsis = 'Imports variables from ".env".'
	description = @'
	It imports variables from the specified or default ".env" file and applies
	them to the current process environment, unless As* is used. In that case
	the process environment is not changed and variables are returned.
'@
	parameters = @{
		Path = @'
		Specifies the existing file to import.

		The default file, omitted or empty, is ".env" in the current location
		or the first found in parents. If the file is not found, no variables
		are imported.
'@
		AsDictionary = @'
		Tells to get variables as `[Dictionary[string, string]]`, case sensitive.
		Duplicates are processed as `TakeLast`.
'@
		AsKeyValue = @'
		Tells to get variables as `[KeyValuePair[string, string]]`, including
		duplicates.
'@
	}
	outputs = @(
		@{
			type = 'System.Collections.Generic.Dictionary[System.String, System.String]'
			description = 'when -AsDictionary'
		}
		@{
			type = 'System.Collections.Generic.KeyValuePair[System.String, System.String]'
			description = 'when -AsKeyValue'
		}
	)
}

### Reset-Rest
@{
	command = 'Reset-Rest'
	synopsis = 'Resets the module state.'
	description = @'
	It tells to reset the module state without reloading it or restarting the
	session. This includes cached results of named requests. After Reset-Rest
	named requests are invoked once again.
'@
}
