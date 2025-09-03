
# https://www.powershellgallery.com/packages/Start-HttpRoutes
Start-HttpRoutes http://127.0.0.1:55001 @{
	'GET /' = {
		$PID
	}

	'POST /show' = {
		$Response.ContentType = 'application/json'
		[ordered]@{
			Headers = Convert-NameValue $Request.Headers
			Query = Convert-NameValue $Request.QueryString
			Data = ConvertFrom-Json (Read-Content)
		} | ConvertTo-Json -Depth 99
	}

	'POST /show/xml' = {
		$Response.ContentType = 'application/xml'
		Read-Content
	}
}
