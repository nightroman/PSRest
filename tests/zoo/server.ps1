# https://www.powershellgallery.com/packages/Start-HttpRoutes

function HttpRoute-Get {
	[CmdletBinding(DefaultParameterSetName = 'GET /')] param()
	$PID
}

function HttpRoute-GetTest {
	[CmdletBinding(DefaultParameterSetName = 'GET /test/*')] param()
	$Request.Url.AbsolutePath
}

function HttpRoute-PostShow {
	[CmdletBinding(DefaultParameterSetName = 'POST /show')] param()
	$Response.ContentType = 'application/json'
	[ordered]@{
		Headers = Convert-NameValue $Request.Headers
		Query = Convert-NameValue $Request.QueryString
		Data = ConvertFrom-Json (Read-Content)
	} | ConvertTo-Json -Depth 99
}

function HttpRoute-PostShowXml {
	[CmdletBinding(DefaultParameterSetName = 'POST /show/xml')] param()
	$Response.ContentType = 'application/xml'
	Read-Content
}

Start-HttpRoutes http://[::1]:55001
