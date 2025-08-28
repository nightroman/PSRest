
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
	equals $d.user "nightroman"
	equals $d.oops "{{missing}}"
	equals $d.age 42L
}

task GitHubIssues {
	Set-RestEnvironment local
	($r = Invoke-RestHttp http/GitHubIssues-2.http)
	$r = ConvertFrom-Json $r

	$h = $r.Headers
	equals $h."User-Agent" "admin"

	$d = $r.Data
	assert ($d.query -match '(?s)^query GitHubIssues.*}$')
	equals $d.variables.login admin
}
