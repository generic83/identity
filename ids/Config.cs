using IdentityServer4.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Test;

namespace ids
{
    public static class Config
    {
        public static List<TestUser> Users
        {
            get
            {
                var address = new
                {
                    street_address = "One Hacker Way",
                    locality = "Heidelberg",
                    postal_code = 69118,
                    country = "Germany"
                };

                return new List<TestUser>
        {
          new TestUser
          {
            SubjectId = "818727",
            Username = "alice",
            Password = "alice",
            Claims =
            {
              new Claim(JwtClaimTypes.Name, "Alice Smith"),
              new Claim(JwtClaimTypes.GivenName, "Alice"),
              new Claim(JwtClaimTypes.FamilyName, "Smith"),
              new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
              new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
              new Claim(JwtClaimTypes.Role, "admin"),
              new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
              new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address),
                IdentityServerConstants.ClaimValueTypes.Json)
            }
          },
          new TestUser
          {
            SubjectId = "88421113",
            Username = "bob",
            Password = "bob",
            Claims =
            {
              new Claim(JwtClaimTypes.Name, "Bob Smith"),
              new Claim(JwtClaimTypes.GivenName, "Bob"),
              new Claim(JwtClaimTypes.FamilyName, "Smith"),
              new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
              new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
              new Claim(JwtClaimTypes.Role, "user"),
              new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
              new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address),
                IdentityServerConstants.ClaimValueTypes.Json)
            }
          }
        };
            }
        }

        public static IEnumerable<IdentityResource> IdentityResources =>
          new[]
          {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            // You need this to expose roles as IdentityResource
                new IdentityResource
                {
                  Name = "role",
                  UserClaims = new List<string> { JwtClaimTypes.Role }
                }
          };

        public static IEnumerable<ApiScope> ApiScopes =>
          new[]
              {
            new ApiScope("weatherapi.read"),
            new ApiScope("weatherapi.write"),
              };

        public static IEnumerable<ApiResource> ApiResources => new[]
        {
          new ApiResource("weatherapi")
          {
            Scopes = new List<string> {"weatherapi.read", "weatherapi.write"},
            ApiSecrets = new List<Secret> {new Secret("ScopeSecret".Sha256())},
            //UserClaims = new List<string> {"role" }
          }
        };

        public static IEnumerable<Client> Clients =>
          new[]
          {
        // m2m client credentials flow client
        new Client
        {
          ClientId = "m2m.client", // machine to machine
          ClientName = "Client Credentials Client",

          AllowedGrantTypes = GrantTypes.ClientCredentials,
          ClientSecrets = {new Secret("SuperSecretPassword".Sha256())},

          AllowedScopes = {"weatherapi.read", "weatherapi.write"},
        },

        // interactive client using code flow + pkce
        new Client
        {
          ClientId = "interactive",  // Web client
          ClientSecrets = {new Secret("SuperSecretPassword".Sha256())},
          AllowedGrantTypes = GrantTypes.Code,

          //https://stackoverflow.com/questions/45458612/identityserver-4-openidconnect-redirect-to-external-sign-in-url
          RedirectUris = {"https://localhost:5444/signin-oidc"},

          FrontChannelLogoutUri = "https://localhost:5444/signout-oidc",
          PostLogoutRedirectUris = {"https://localhost:5444/signout-callback-oidc"},

          //Specifies whether this client can request refresh tokens - offline_access scope should also be added on the client side
          AllowOfflineAccess = true,

          
          //If you uncomment this, then the same refresh token will be used and no new refresh token is sent on every refresh token
          //If you set this to OneTimeOnly, it means you will get a new refresh token every time you send a refresh token request, so it is basically enabling refresh token rotation   
          //RefreshTokenUsage = TokenUsage.ReUse,

          //You need to add role scope if you want the roles to be retrieved
          AllowedScopes = {"openid", "profile", "weatherapi.read", "role"},
          RequirePkce = true,
          RequireConsent = true,
          AllowPlainTextPkce = false,
         
          //Even if you set this to 20 seconds, the minimum lifetime is 5 min due to clock skew
          //AccessTokenLifetime = 20,

           /*
           When a Client receives an ID token, it will generally do something like convert it to a ClaimsIdentity, and persist this, eg using a cookie.
           The ID token has to be un-expired at this point of use (which it should be, since it has just been issued). 
           But after this it is not used again, so it does not matter if it expires while the user still has an active session.
           https://stackoverflow.com/questions/25686484/what-is-intent-of-id-token-expiry-time-in-openid-connect
           https://vxcompany.com/insight/cookies-tokens-and-session-lifetime-with-identity-server/
           */
           //IdentityTokenLifetime = 20
        },

        new Client
            {
                ClientId = "js",
                ClientName = "JavaScript Client",
                AllowedGrantTypes = GrantTypes.Implicit,

                //AllowAccessTokensViaBrowser controls if we allow an access token to be returned to the client in the URL from the authorize endpoint (as opposed to the normal mechanism of the token endpoint).
                //This is needed in Implicit flow
                AllowAccessTokensViaBrowser = true,

                RedirectUris =           { "http://localhost:3000/callback.html","http://localhost:3000/silent-redirect.html"  },
                PostLogoutRedirectUris = { "http://localhost:3000/index.html" },
                AllowedCorsOrigins =     { "http://localhost:3000" },

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                "weatherapi.read","role"
                },
               // Even if you set this to 20 seconds, the minimum lifetime is 5 min due to clock skew
               // AccessTokenLifetime = 20,
            },

                 new Client
                {
                  ClientId = "js_code",  // Web client
                  RequireClientSecret = false,
                  AllowedGrantTypes = GrantTypes.Code,


                RedirectUris =           { "http://localhost:3000/callback.html","http://localhost:3000/silent-redirect.html"  },
                  PostLogoutRedirectUris = { "http://localhost:3000/index.html" },
                  AllowedCorsOrigins =     { "http://localhost:3000" },
                  
                  AllowOfflineAccess = true,
                  
                  AllowedScopes =
                  {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "weatherapi.read","role"
                  },
                  RequirePkce = true,
                  AllowPlainTextPkce = false,
                  AccessTokenLifetime = 20,
                },
          };
    }
}
