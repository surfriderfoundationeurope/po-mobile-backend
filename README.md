# po-mobile-backend

## Settings file

Create a `local.settings.json` within the `functions` folder. Paste this content and obviously replace with your values.

```json
{
	"IsEncrypted": false,
	"Values": {
		"AzureWebJobsTraceStorage": "UseDevelopmentStorage=true;",
		"AzureWebJobsStorage": "UseDevelopmentStorage=true;",
		"JwtTokenKey": "jsutAVeryLongRandomString",
		"PostgresqlDbConnectionString": "",
		"MailjetApiKey": "",
		"MailjetApiSecret": "",
		"BaseFunctionUrl": "http://localhost:7071"
	}
}
```

Remarks: MailjetApiKeys are only required for sending emails.
