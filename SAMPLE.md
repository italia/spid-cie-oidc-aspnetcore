# Sample WebApp
The AspNetCore MVC webapp contained in the `samples/1_SimpleSPWebApp` folder, is a sample implementation of a RelyingParty that uses the SDK.

If you have a look at the `Startup.cs`, you can find the statements to add the necessary middlewares and services, and a basic configuration of a RelyingParty that uses a self-signed X509 signing certificate in the `appsettings.json` file (if you need to change some of the settings, e.g. the OPs hostnames, the trustmarks, etc., this is the place).

Included in the WebApp is also a `docker-compose.yml` file, you can set up a complete Spid-Cie OIDC federation by cloning the [Django repository](https://github.com/italia/spid-cie-oidc-django/) and by adding the following section into the docker-compose.yml file contained in the Django repo's root folder.

```yaml
  aspnetcore.relying-party.org:
    build:
      context: /<path_to_cloned_repo>/samples/1_SimpleSPWebApp
      dockerfile: Spid.Cie.OIDC.AspNetCore.WebApp/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5000
    ports:
      - "5000:5000"
    volumes:
      - ${APPDATA}/Microsoft/UserSecrets:/root/.microsoft/usersecrets:ro
      - ${APPDATA}/ASP.NET/Https:/root/.aspnet/https:ro
    networks:
      - oidcfed
```

Add the following mappings into your hosts file:

```
127.0.0.1 trust-anchor.org
127.0.0.1 cie-provider.org
127.0.0.1 relying-party.org
127.0.0.1 aspnetcore.relying-party.org
```

Now you should be able to run the entire federation by just opening a Terminal session into the Django repo's root folder and running
`docker-compose up`

Now your relying party should respond to http://aspnetcore.relying-party.org:5000/ and the Trust Anchor should respond to http://trust-anchor.org:8000/ 

Now navigate to the endpoint that shows the openid federation configuration of the RP as a decoded json (http://aspnetcore.relying-party.org:5000/.well-known/openid-federation/json), you will receive the following output.

![image](https://user-images.githubusercontent.com/58780951/160616885-4047c644-d017-46b3-b68f-4d09dd986877.png)

Please, take note of the `keys` field value in the json, you will need it later in the onboarding phase.

Now you should be able to navigate the TA admin panel at the following url: http://trust-anchor.org:8000/admin
Please enter the admin credentials, and you will be presented with the TA admin panel main page.

The onboarding process can be summarized as follows:

- Click on the "Onboarding registrations" link on the left to start the RP onboarding phase, the Onboarding page will show.
- Click on the "Add onboarding registration" button on the top-right corner, and a form will show.
- Fill the form with the requested data (now you can paste the `keys` value in the `Public Jwks` field) and click `Save`. The newly added RP should appear in the onboarded entities list.
- Now click on the "Federation entity descendants" button, in order to add the onboarded RelyingParty as a direct descendand of the TrustAnchor.
- Click on the "Add federation entity descendand" button on the top-right corner, and a form will show.
- Fill the form with the requested data (you should paste the same `keys` value in the `Public Jwks` field) and click `Save`. The newly added descendand should appear in the descendants list.
- Now click on the "Federation entity descendants assigned profiles" button, in order to assign a profile to the newly created descendant and have the trust mark generated for it.
- Click on the "Add federation entity descendand assigned profile" button on the top-right corner, and a form will show.
- Select the RP as the descendant, a profile (e.g. the Public one) and the Issuer (the Trust Anchor at http://trust-anchor.org:8000/) and click Save
- The RP descendant should now appear in the associated policies.
- If you click again on the RP in the "Federation entity descendants assigned profiles" page, now you should see the generated Trust Mark

![SpidCieOIDCOnboarding](https://user-images.githubusercontent.com/58780951/161080408-4023bd73-6326-482f-b620-7254215637c4.gif)

Copy the newly generated Trust Mark in the proper configuration section for the RP.
```json
  "SpidCie": {
    // ...
    "RelyingParties": [
      {
        // ....
        "TrustMarks": [
          {
            "Id": "https://www.spid.gov.it/openid-federation/agreement/sp-public/",
            "Issuer": "http://trust-anchor.org:8000/",
            "TrustMark": "eyJhbGciOiJSUzI1NiIsImtpZCI6ImRCNjdnTDdjazNURmlJQWY3TjZfN1NIdnFrME1EWU1FUWNvR0dsa1VBQXciLCJ0eXAiOiJ0cnVzdC1tYXJrK2p3dCJ9.eyJpc3MiOiJodHRwOi8vMTI3LjAuMC4xOjgwMDAvb2lkYy9vcC8iLCJzdWIiOiJodHRwOi8vMTI3LjAuMC4xOjUwMDAvIiwiaWF0IjoxNjQ4NTYwMjgzLCJpZCI6Imh0dHBzOi8vd3d3LnNwaWQuZ292Lml0L2NlcnRpZmljYXRpb24vcnAiLCJtYXJrIjoiaHR0cHM6Ly93d3cuYWdpZC5nb3YuaXQvdGhlbWVzL2N1c3RvbS9hZ2lkL2xvZ28uc3ZnIiwicmVmIjoiaHR0cHM6Ly9kb2NzLml0YWxpYS5pdC9pdGFsaWEvc3BpZC9zcGlkLXJlZ29sZS10ZWNuaWNoZS1vaWRjL2l0L3N0YWJpbGUvaW5kZXguaHRtbCJ9.M6T42JXb9wHBhwy2cueHEFoMNcaHQZKvMTMR3aavZVBvW14hps_IZ_MT3yqA5wTEZTgAC_-M8G33wjpTMw26ITXgOW6rMUqWHWj7639BfbqnGkoZdMuMxo96nSOIaxXElvfPRZu6wQ9LOrXe_kyR3eo6p8iZLKbnp1e1D5VTr_dSEYQsaTlVmiT6I2SyaiWtpXCD1DWZxNw2YKTie0lEmDCMO4WJo3kfr_ak9kvMryF8-5crecOs6o33DYGR5zSzJ3JQYAz2huLKZ_y7nzmvkEjDQNjtg1R2cusNHvxLt8Su3T0hUbT_Vl5-b_VvBqo2yUTc7Z7WOJPwzId8rATujA"
          }
        ],
        // ...
```

Now you should stop and restart the federation in order to have the RP to reload the configuration.
You can stop the federation by a Ctrl+C in the terminal window where the compose is running, and by running the following command:

`docker-compose build aspnetcore.relying-party.org && docker-compose up`

Now you can perform a sample login/logout flow, since the OP will successfully resolve the RP's trust chain, by navigating the RP's home page (http://aspnetcore.relying-party.org:5000/)

![SpidCieOIDCLoginLogout](https://user-images.githubusercontent.com/58780951/161081768-7e5d5ebf-baa2-444a-ab95-a1546134d741.gif)

