# Sample WebApp
The AspNetCore MVC webapp contained in the `samples/1_SimpleSPWebApp` folder, is a sample implementation of a RelyingParty that uses the SDK.
You could just clone the repo and execute the webapp as-is (both with `dotnet run` or `docker`), and it will just run and be accessible at the Url: http://127.0.0.1:5000/
If you have a look at the `Startup.cs`, you can find a basic configuration of a RelyingParty that uses a self-signed X509 signing certificate, and the statements to add the necessary middlewares and services.

![image](https://user-images.githubusercontent.com/58780951/160617044-125cd807-1e89-4eb6-8dc7-a5e2e40b268b.png)

If you run the webapp and navigate to the endpoint that shows the openid federation configuration as a decoded json (http://127.0.0.1:5000/.well-known/openid-federation/json), you will receive the following output.

![image](https://user-images.githubusercontent.com/58780951/160616885-4047c644-d017-46b3-b68f-4d09dd986877.png)

Please, take note of the `keys` field value in the json, you will need it later in the onboarding phase.

Now you should follow [this guide](https://github.com/italia/spid-cie-oidc-django/blob/main/docs/SETUP.md) to run the Django sample OP and TA in order to have a full federation running. After the installation you should be able to navigate the TA admin panel at the following url: http://127.0.0.1:8000/admin
Please enter the admin credentials, and you will be presented with the TA admin panel main page.

Click on the "Onboarding registrations" link on the left to start the RP onboarding phase, the Onboarding page will show.

![image](https://user-images.githubusercontent.com/58780951/160620799-bd977e76-5a8e-4b70-a18c-5648243afeea.png)

Click on the "Add onboarding registration" button on the top-right corner, and a form will show.

![image](https://user-images.githubusercontent.com/58780951/160620963-ee03114b-ca04-4fa0-9f26-9573d9f31bf5.png)

Fill the form with the requested data (now you can paste the `keys` value in the `Public Jwks` field) and click `Save`. The newly added RP should appear in the onboarded entities list.

![image](https://user-images.githubusercontent.com/58780951/160620988-514d9e4e-3dec-4a16-b10a-a22ccbd8f8d2.png)

Now click on the "Federation entity descendants" button, in order to add the onboarded RelyingParty as a direct descendand of the TrustAnchor.

![image](https://user-images.githubusercontent.com/58780951/160621023-d222ba8b-3a6c-4c00-9062-4651175db02d.png)

Click on the "Add federation entity descendand" button on the top-right corner, and a form will show.

![image](https://user-images.githubusercontent.com/58780951/160621121-ea73f138-2a04-4a9c-b4b9-6999c8e47498.png)

![image](https://user-images.githubusercontent.com/58780951/160621147-2827852b-671d-4393-9169-f5705aad701f.png)

Fill the form with the requested data (you should paste the same `keys` value in the `Public Jwks` field) and click `Save`. The newly added descendand should appear in the descendants list.

![image](https://user-images.githubusercontent.com/58780951/160621175-d9c08f37-90bd-4205-a329-c49c15258fe4.png)

Now click on the "Federation entity descendants assigned profiles" button, in order to assign a profile to the newly created descendant and have the trust mark generated for it.

![image](https://user-images.githubusercontent.com/58780951/160621198-d2fc1556-9172-4e6a-aa33-f9d708d6ae92.png)

Click on the "Add federation entity descendand assigned profile" button on the top-right corner, and a form will show.

![image](https://user-images.githubusercontent.com/58780951/160621223-e1effd4a-6ff3-469a-a766-12d006a6ae54.png)

Select the RP as the descendant, a profile (e.g. the Public one) and the Issuer (the Trust Anchor at http://127.0.0.1:8000/) and click Save

![image](https://user-images.githubusercontent.com/58780951/160621237-e0b1f393-5ebc-4278-bb2c-60a402f5767b.png)

![image](https://user-images.githubusercontent.com/58780951/160621262-a6cabccd-1d98-45fd-b5fa-5235b5995b3b.png)

![image](https://user-images.githubusercontent.com/58780951/160621298-5b4dbc90-e470-4017-9462-831e2251c375.png)

![image](https://user-images.githubusercontent.com/58780951/160621324-0af80048-4f72-4bcd-8538-e891c66cfe34.png)

The RP descendant should now appear in the associated policies.

![image](https://user-images.githubusercontent.com/58780951/160621347-a427f49f-2a8e-4c13-a94f-632e177a1c30.png)

If you click again on the RP in the "Federation entity descendants assigned profiles" page, now you should see the generated Trust Mark

![image](https://user-images.githubusercontent.com/58780951/160621370-f00f8b50-f3ef-4edc-a460-92cc641cd102.png)

Copy the newly generated Trust Mark in the proper configuration section for the RP.

![image](https://user-images.githubusercontent.com/58780951/160621452-98380f1d-edcf-4ec9-9148-5656a76ed217.png)

Now you can perform a sample login/logout flow, since the OP will successfully resolve the RP's trust chain.

![image](https://user-images.githubusercontent.com/58780951/160621644-ba55d7e6-702e-4e59-b0a0-695020cf3fb0.png)

![image](https://user-images.githubusercontent.com/58780951/160621672-966aed34-e777-443a-a0eb-f92ed7cf857c.png)

![image](https://user-images.githubusercontent.com/58780951/160621694-72a75c4f-3023-4d9e-98ea-0d22abcd5dbd.png)

![image](https://user-images.githubusercontent.com/58780951/160621716-0f425bc4-7260-401d-9490-174797769eaf.png)

![image](https://user-images.githubusercontent.com/58780951/160621737-00090f62-d916-4f8b-a213-512fd87f102d.png)
