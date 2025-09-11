<#
.Synopsis
	Tests Invoke-RestHttp.

.Notes
	Do not move this file to "http" to make it simpler (tempting).
	Instead use Set-RestEnvironment with "http" to cover issues.
#>

Set-StrictMode -Version 3
Import-Module PSRest
Import-Module ./zoo
Start-Server

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
	equals $d.age 42L
}

task Basic-3 {
	$r = Invoke-RestHttp http/Basic-3.json.http -HeadersVariable Headers
	$r = ConvertFrom-Json $r

	assert ('application/json' -eq $Headers.ContentType)

	$h = $r.Headers
	equals $h.Age "42"

	$q = $r.Query
	equals $q.param1 "42"

	$d = $r.Data
	equals $d.version "v1"
	equals $d.user "admin"
	equals $d.age 42L
}

task Basic-4 {
	($r = (Invoke-RestHttp http/Basic-4.http -HeadersVariable Headers) -split '\r?\n')

	assert ('application/xml' -eq $Headers.ContentType)

	equals $r[0] '<request>'
	equals $r[1] '  <version>v1</version>' #! indent
}

task Basic-Tags {
	$lines = [System.IO.File]::ReadAllLines("$PSScriptRoot/http/Basic-Tags.http")
	$text = $lines -join "`n"
	$n1 = 1 + [array]::IndexOf($lines, '###')
	$n2 = 1 + [array]::IndexOf($lines, '### bar')

	$env:REST_TAG = 'bar'
	equals /test/3 (Invoke-RestHttp -Text $text)

	$env:REST_TAG = ''
	equals /test/1 (Invoke-RestHttp -Text $text)
	equals /test/1 (Invoke-RestHttp -Text $text -Tag ($n1 - 1))

	equals /test/2 (Invoke-RestHttp -Text $text -Tag $n1)
	equals /test/2 (Invoke-RestHttp -Text $text -Tag ($n2 - 1))

	equals /test/3 (Invoke-RestHttp -Text $text -Tag $n2)
	equals /test/3 (Invoke-RestHttp -Text $text -Tag 12345)
}

task Basic-Prompt {
	$text = [System.IO.File]::ReadAllText("$PSScriptRoot/http/Basic-Prompt.http")

	Set-Alias Read-Host Read-Host2
	function Read-Host2($Prompt, [switch]$MaskInput) {
		$log.Prompt = $Prompt
		$log.MaskInput = $MaskInput
		"input-$Prompt"
	}

	$log = @{}
	$r = Invoke-RestHttp -Text $text
	equals $r /test/1
	equals $log.Count 0

	$log = @{}
	$r = Invoke-RestHttp -Text $text -Tag bar
	equals $r /test/2/input-bar
	equals $log.Prompt bar
	equals $log.MaskInput $false

	$log = @{}
	$r = Invoke-RestHttp -Text $text -Tag pass
	equals $r /test/3/input-Password
	equals $log.Prompt Password
	equals $log.MaskInput $true
}

task Basic-Request {
	$text = [System.IO.File]::ReadAllText("$PSScriptRoot/http/Basic-Request.http")

	# first, producer request
	$null = Invoke-RestHttp -Text $text

	# then, consumer request
	($r = Invoke-RestHttp -Text $text -Tag Consumer)
	$r = ConvertFrom-Json $r -AsHashtable

	equals $r.Data.requestHeader "42"
	equals $r.Data.responseHeader "application/json"
	equals $r.Data.requestBody.user "Joe"
	equals $r.Data.responseBody.Headers.Age "42" #? capitalized `Age`
	equals $r.Data.responseBody.Data.user "Joe"
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
