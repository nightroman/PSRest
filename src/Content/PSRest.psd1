@{
	Author = 'Roman Kuzmin'
	ModuleVersion = '0.0.0'
	Description = 'PowerShell module'
	CompanyName = 'https://github.com/nightroman'
	Copyright = 'Copyright (c) 2025 Roman Kuzmin'

	RootModule = 'PSRest.dll'
	RequiredAssemblies = 'PSRest.dll'

	PowerShellVersion = '7.4'
	GUID = '843fe469-93a0-43b7-a639-b282b75754d1'

	AliasesToExport = @()
	VariablesToExport = @()
	FunctionsToExport = @()
	CmdletsToExport = @(
		'Set-RestEnvironment'
		'Get-RestVariable'
		'Resolve-RestVariable'
	)

	PrivateData = @{
		PSData = @{
			Tags = 'PSRest'
			ProjectUri = 'https://github.com/nightroman/PSRest'
			LicenseUri = 'https://github.com/nightroman/PSRest/blob/main/LICENSE'
			ReleaseNotes = 'https://github.com/nightroman/PSRest/blob/main/Release-Notes.md'
		}
	}
}
