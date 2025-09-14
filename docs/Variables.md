# Variables

See [REST Client Variables](https://github.com/Huachao/vscode-restclient?tab=readme-ov-file#variables) for more details.

**Environment and file (user defined)**

- `{{variableName}}` and `{{$shared variableName}}`
- `{{$processEnv [%]envVarName}}`
- `{{$dotenv [%]variableName}}`

**System (dynamic)**

- `{{$guid}}`
- `{{$randomInt min max}}`
- `{{$timestamp [offset option]}}`
- `{{$datetime rfc1123|iso8601 [offset option]}}`
- `{{$localDatetime rfc1123|iso8601 [offset option]}}`

**Prompt (input)**

- `# @prompt var [prompt]`
- `// @prompt var [prompt]`

**Request**

- `# @name requestName`
- `// @name requestName`
- `{{requestName.(response|request).(body|headers).(*|JSONPath|XPath|Header Name)}}`
