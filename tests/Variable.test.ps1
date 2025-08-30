<#
.Synopsis
	Tests Get-RestVariable, Resolve-RestVariable.
#>

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
	Set-RestEnvironment '' http

	$r = Get-RestVariable missing -Type DotEnv
	equals $r $null

	$r = Get-RestVariable user -Type DotEnv
	equals $r admin

	#! cover .env is not loaded into process
	equals $env:user $null
}

task env {
	Set-RestEnvironment '' http

	$r = Get-RestVariable missing
	equals $r $null

	$r = Get-RestVariable version
	equals $r v1
}

task local {
	Set-RestEnvironment local http

	$r = Get-RestVariable missing
	equals $r $null

	$r = Get-RestVariable version
	equals $r v2

	$r = Get-RestVariable token
	equals $r bar
}

task production {
	Set-RestEnvironment production http

	$r = Get-RestVariable missing
	equals $r $null

	$r = Get-RestVariable version
	equals $r v1

	$r = Get-RestVariable token
	equals $r foo
}

task resolve_shared {
	$env:test = 42
	Set-RestEnvironment '' http

	$1, $2 = '<<{{version}} // {{$shared token}}>>', '<<{{$dotenv user}} // {{$processEnv test}}>>' | Resolve-RestVariable
	equals $1 '<<v1 // {{$shared token}}>>'
	equals $2 '<<admin // 42>>'
}

task resolve_local {
	$env:test = 42
	Set-RestEnvironment local http

	$1, $2 = Resolve-RestVariable '<<{{version}} // {{$shared token}}>>', '<<{{$dotenv user}} // {{$processEnv test}}>>'
	equals $1 '<<v2 // {{$shared token}}>>'
	equals $2 '<<admin // 42>>'
}

task guid {
	Set-RestEnvironment

	($r = Get-RestVariable '$guid')
	equals $r ([guid]::Parse($r).ToString())
}

task randomInt {
	Set-RestEnvironment

	($r = Get-RestVariable '$randomInt 1 2')
	equals $r ([int]::Parse($r).ToString())

	try { throw Get-RestVariable '$randomInt' }
	catch { equals "$_" 'Parsing ''$randomInt'': expected ''$randomInt min max''.' }

	try { throw Get-RestVariable '$randomInt 1' }
	catch { equals "$_" 'Parsing ''$randomInt 1'': expected ''$randomInt min max''.' }

	try { throw Get-RestVariable '$randomInt 1 x' }
	catch { equals "$_" 'Parsing ''$randomInt 1 x'': expected ''$randomInt min max''.' }

	try { throw Get-RestVariable '$randomInt x 1' }
	catch { equals "$_" 'Parsing ''$randomInt x 1'': expected ''$randomInt min max''.' }
}

task timespan {
	Set-RestEnvironment

	($r = Get-RestVariable '$timestamp')
	$r1 = [long]::Parse($r)

	($r = Get-RestVariable '$timestamp 1 s')
	$r2 = [long]::Parse($r)
	equals 1L ($r2 - $r1)

	try { throw Get-RestVariable '$timestamp 1' }
	catch { equals "$_" "Parsing '`$timestamp 1': expected '... int64 unit'." }

	try { throw Get-RestVariable '$timestamp x' }
	catch { equals "$_" "Parsing '`$timestamp x': expected '... int64 unit'." }

	try { throw Get-RestVariable '$timestamp 1 x' }
	catch { equals "$_" "Parsing '`$timestamp 1 x': expected '... int64 unit'." }

	try { throw Get-RestVariable '$timestamp x s' }
	catch { equals "$_" "Parsing '`$timestamp x s': expected '... int64 unit'." }
}

task datetime {
	Set-RestEnvironment

	# no offset
	($r = Get-RestVariable '$datetime iso8601')
	equals ($r[-1]) ([char]'Z')
	equals $r ([datetime]::Parse($r).ToUniversalTime().ToString('u'))

	# yes offset
	($r2 = Get-RestVariable '$datetime iso8601 1 s')
	equals 1 ([int]([datetime]::Parse($r2) - [datetime]::Parse($r)).TotalSeconds)

	# at least one localDatetime, different from datetime unless same zone
	($r = Get-RestVariable '$localDatetime iso8601 1 s')

	($r = Get-RestVariable '$datetime rfc1123')
	equals $r ([datetime]::Parse($r).ToUniversalTime().ToString('r'))

	($r = Get-RestVariable '$datetime "yyyy-MM-dd hh:mm:ss"') # universal
	equals $r ([datetime]::Parse($r).ToString('yyyy-MM-dd hh:mm:ss')) # universal, no ToUniversalTime

	($r = Get-RestVariable '$datetime ''yyyy-MM-dd hh:mm:ss''') # universal
	equals $r ([datetime]::Parse($r).ToString('yyyy-MM-dd hh:mm:ss')) # universal, no ToUniversalTime

	try { throw Get-RestVariable '$datetime x' }
	catch { equals "$_" "Parsing '`$datetime x': expected '`$datetime rfc1123|iso8601|`"custom format`"|'custom format' ...'." }

	try { throw Get-RestVariable '$datetime "yyyy-MM-dd' }
	catch { equals "$_" "Parsing '`$datetime `"yyyy-MM-dd': expected '`$datetime rfc1123|iso8601|`"custom format`"|'custom format' ...'." }

	try { throw Get-RestVariable '$datetime ''yyyy-MM-dd' }
	catch { equals "$_" "Parsing '`$datetime 'yyyy-MM-dd': expected '`$datetime rfc1123|iso8601|`"custom format`"|'custom format' ...'." }

	try { throw Get-RestVariable '$datetime iso8601 x' }
	catch { equals "$_" "Parsing '`$datetime iso8601 x': expected '... int64 unit'." }
}
