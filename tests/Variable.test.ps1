
Set-StrictMode -Version 3
Import-Module PSRest

Enter-Build {
	$REST_ENV =  $env:REST_ENV
	$env:REST_ENV = $null
}

Exit-Build {
	$env:REST_ENV = $REST_ENV
}

task processEnv {
	Set-RestEnvironment

	$r = Get-RestVariable missing -Type ProcessEnv
	equals $r $null

	$r = Get-RestVariable PSModulePath -Type ProcessEnv
	assert $r
	equals $r $env:PSModulePath
}

task dotEnv {
	$env:user = $null
	Set-RestEnvironment

	$r = Get-RestVariable missing -Type DotEnv
	equals $r $null

	$r = Get-RestVariable user -Type DotEnv
	equals $r admin

	#! cover .env is not loaded into process
	equals $env:user $null
}

task env {
	Set-RestEnvironment

	$r = Get-RestVariable missing
	equals $r $null

	$r = Get-RestVariable version
	equals $r v1
}

task local {
	Set-RestEnvironment local

	$r = Get-RestVariable missing
	equals $r $null

	$r = Get-RestVariable version
	equals $r v2

	$r = Get-RestVariable token
	equals $r bar
}

task production {
	Set-RestEnvironment production

	$r = Get-RestVariable missing
	equals $r $null

	$r = Get-RestVariable version
	equals $r v1

	$r = Get-RestVariable token
	equals $r foo
}

task resolve_shared {
	$env:test = 42
	Set-RestEnvironment

	$1, $2 = '<<{{version}} // {{$shared token}}>>', '<<{{$dotenv user}} // {{$processEnv test}}>>' | Resolve-RestVariable
	equals $1 '<<v1 // {{$shared token}}>>'
	equals $2 '<<admin // 42>>'
}

task resolve_local {
	$env:test = 42
	Set-RestEnvironment local

	$1, $2 = Resolve-RestVariable '<<{{version}} // {{$shared token}}>>', '<<{{$dotenv user}} // {{$processEnv test}}>>'
	equals $1 '<<v2 // {{$shared token}}>>'
	equals $2 '<<admin // 42>>'
}
