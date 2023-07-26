# Authentication

This sample demonstrates how to authenticate `TandemClient` which is starting point for interaction with Tandem using REST API.

Tandem using using access token which can be obtained using [APS Authentication service](https://aps.autodesk.com/en/docs/oauth/v2/developers_guide/overview/). Both 2-legged and 3-legged token are supported.

## Configuring Tandem
Before accessing Tandem REST API it's necessary to enable access for your service. This can be done either on facility level or on account level.

Follow steps below to enable access on facility level:
1. Open facility.
2. One the left side click **Users** to open **User Management Panel**.
3. Select **Service** under **Invite user / service to this facility**.
4. Enter client ID of your service.
5. Specify permission for your service.
6. Click **Add** to save changes.

## Authenticate the client
When creating instance of `TandenClient` class you need to provide deletage to function which provides access token, i.e.:
```cs
var client = new TandemClient(() => GetToken());
```
The `GetToken` function should provide valid access token. Usually this is done by calling [POST token](https://developer.api.autodesk.com/authentication/v2/token) endpoint = in example below this is done by `GetAccessToken` function:

```cs
string GetToken()
{
    // obtain access token
    var token = GetAccessToken().ConfigureAwait(false).GetAwaiter().GetResult();

    return token.AccessToken;
}
```

## Authenticate using 2-legged access token
2-legged access token is usually used for server to server scenarios - i.e. automated processing of data without end user input - see more details [here](https://aps.autodesk.com/en/docs/oauth/v2/developers_guide/App-types/Machine-to-machine/).

To create 2-legged token you can utilize pre-built class `TwoLeggedAuth` - this takes three parameters:
* `clientID` - client ID of the APS application.
* `clientSecret`- client secret of the APS application.
* `scope` - concated string of scopes

The class provides `GetToken` method which returns access token:
```cs
// replace YOUR_CLIENT_ID, YOUR_CLIENT_SECRET with your own credentials
var auth = new TwoLeggedAuth("YOUR_CLIENT_ID",
  "YOUR_CLIENT_SECRET",
  "data:read data:write");
var client = new TandemClient(() => auth.GetToken());
```
