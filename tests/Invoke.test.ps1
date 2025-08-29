<#
.Synopsis
	Tests Invoke-RestHttp.

.Notes
	Do not move this file to "http" to make it simpler (tempting).
	Instead use Set-RestEnvironment with "http" to cover issues.
#>

Set-StrictMode -Version 3
Import-Module PSRest

task invokeText {
	$r = Invoke-RestHttp -Text 'GET https://example.com'
	assert $r.StartsWith('<!doctype html>')
}

task Basic-1 {
	$r = Invoke-RestHttp http/Basic-1.http
	assert $r.StartsWith('<!doctype html>')
}

task Basic-2 {
	$r = Invoke-RestHttp http/Basic-2.http
	$r = ConvertFrom-Json $r

	$h = $r.Headers
	equals $h.Age "42"

	$q = $r.Query
	equals $q.param1 "42"

	$d = $r.Data
	equals $d.version "v1"
	equals $d.user "admin"
	equals $d.oops "{{missing}}"
	equals $d.age 42L
}

task Basic-3 {
	$r = Invoke-RestHttp http/Basic-3.json.http
	$r = ConvertFrom-Json $r

	$h = $r.Headers
	equals $h.Age "42"

	$q = $r.Query
	equals $q.param1 "42"

	$d = $r.Data
	equals $d.version "v1"
	equals $d.user "admin"
	equals $d.oops "{{missing}}"
	equals $d.age 42L
}

task Continents-1 {
	Set-RestEnvironment '' http
	($r = Invoke-RestHttp http/Continents-1.http)
	$r = ConvertFrom-Json $r

	equals 5 $r.Data.continents.Count
}

task Continents-1-local {
	Set-RestEnvironment local http
	($r = Invoke-RestHttp http/Continents-1.http)
	$r = ConvertFrom-Json $r -AsHashtable

	$h = $r.Headers
	equals ($h['User-Agent']) admin

	$d = $r.Data
	assert ($d.query -match '(?s)^query Continents.*}$')
	assert (!$d.Contains('variables'))
	assert (!$d.Contains('operationName'))
}

task Continents-2 {
	Set-RestEnvironment '' http
	($r = Invoke-RestHttp http/Continents-2.graphql.http)
	$r = ConvertFrom-Json $r

	equals 5 $r.Data.continents.Count
}

task Continents-2-local {
	Set-RestEnvironment local http
	($r = Invoke-RestHttp http/Continents-2.graphql.http)
	$r = ConvertFrom-Json $r

	$d = $r.Data
	assert ($d.query -match '(?s)^query Continents.*}$')
	equals $d.variables.filter.code.regex A
}

task Continents-3 {
	Set-RestEnvironment '' http
	($r = Invoke-RestHttp http/Continents-3.graphql.http)
	$r = ConvertFrom-Json $r

	equals 5 $r.Data.continents.Count
}

task Continents-3-local {
	Set-RestEnvironment local http
	($r = Invoke-RestHttp http/Continents-3.graphql.http)
	$r = ConvertFrom-Json $r -AsHashtable

	# X-* headers are removed
	$h = $r.Headers
	equals (($h.Keys | Sort-Object) -join '|') 'Content-Length|Content-Type|Host'

	# yes `query` // no `variables`, `operationName`
	$d = $r.Data
	assert ($d.query -match '(?s)^query Continents.*}$')
	assert (!$d.Contains('variables'))
	assert (!$d.Contains('operationName'))
}

task Continents-4 {
	($r = Invoke-RestHttp http/Continents-4.graphql.http)
	$r = ConvertFrom-Json $r -AsHashtable

	# X-* headers are removed
	$h = $r.Headers
	equals (($h.Keys | Sort-Object) -join '|') 'Content-Length|Content-Type|Host'

	# yes `query`, `variables`, `operationName`
	$d = $r.Data
	assert ($d.query -match '(?s)^query Continents.*}$')
	equals $d.variables.regex1 A
	equals $d.variables.regex2 N
	equals $d.operationName Continents2
}
