
Set-StrictMode -Version 3
Import-Module PSRest

task invokeText {
	Set-RestEnvironment
	$r = Invoke-RestHttp -Text 'GET https://example.com'
	assert $r.StartsWith('<!doctype html>')
}

task Basic-1 {
	Set-RestEnvironment
	$r = Invoke-RestHttp http/Basic-1.http
	assert $r.StartsWith('<!doctype html>')
}
