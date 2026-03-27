using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Text;
using HospitalManagementSystem.Core.DTOs;
using HospitalManagementSystem.Web.Communication;
using HospitalManagementSystem.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace HospitalManagementSystem.Web.Helper
{
    public class IdpClient
    {
        public IConfiguration configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public IdpClient(IConfiguration _configuration, IHttpClientFactory httpClientFactory)
        {
            configuration = _configuration;
            _httpClientFactory = httpClientFactory;
        }

        public string GetAuthorizationUrl(string nonce, string state)
        {
            try
            {
                /*Prepare jwtToken object to generate jwt token which is send form
                 request parameter in query string*/
                var requestObject = new JWTokenDTO();
                requestObject.Expiry = 60;
                requestObject.Audience = configuration["dtidp:Issuer"];
                requestObject.Issuer = configuration["dtidp:ClientId"];
                requestObject.ResponseType = "code";
                requestObject.RedirecUri = configuration["dtidp:RedirectUri"];
                requestObject.Scope = configuration["dtidp:Scopes"];
                requestObject.State = state;
                requestObject.Nonce = nonce;

                var authorizationURL = configuration["dtidp:AuthorizationEndpoint"];
                var scope = configuration["dtidp:Scopes"];

                var openid = configuration.GetValue<Boolean>("OpenId_Connect");

                if (openid)
                {
                    var response = JWTTokenManager.GenerateJWTToken(requestObject);
                    authorizationURL = configuration.
                    GetValue<string>("dtidp:AuthorizationEndpoint") +
                    "?client_id={0}&redirect_uri={1}&response_type=code&scope={2}&state={3}&" +
                    "nonce={4}&request={5}";

                    return String.Format(authorizationURL,
                                     configuration.GetValue<string>("dtidp:ClientId"),
                                     configuration.GetValue<string>("dtidp:RedirectUri"),
                                     configuration.GetValue<string>("dtidp:Scopes"),
                                     state, nonce, response);
                }
                authorizationURL = authorizationURL +
                       "?client_id={0}&redirect_uri={1}&response_type=code&scope={2}&state={3}";
                return String.Format(authorizationURL, configuration.GetValue<string>("dtidp:ClientId"),
                                     configuration.GetValue<string>("dtidp:RedirectUri"),
                                     scope.Replace("openid ", ""), state);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetLogoutUrl(string idToken, string state)
        {
            try
            {
                var isOpenId = configuration.GetValue<bool>("OpenId_Connect");
                if (isOpenId == true)
                {
                    var LogoutURl = configuration.GetValue<string>("dtidp:EndSessionEndpoint") +
                    "?id_token_hint={0}&post_logout_redirect_uri={1}&state={2}";

                    //generate idp logout url using id_token,PostLogoutRedirectUri and state value
                    return String.Format(LogoutURl, idToken,
                                 configuration.GetValue<string>("dtidp:logout_url"),
                                 state);
                }
                else
                {
                    var url = String.Format(configuration.GetValue<string>("dtidp:signOutUrl"), configuration.GetValue<string>("dtidp:logout_url"));
                    return url;
                    //return String.Format(configuration.GetValue<string>("dtidp:signOutUrl"), configuration.GetValue<string>("dtidp:logout_url"));
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<ClaimsPrincipal> ValidateIdentityToken(string idToken)
        {
            try
            {
                //validate id_token
                var user = await ValidateJwt(idToken);

                //return id_token claim values
                return user;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ClaimsPrincipal> ValidateJwt(string jwt)
        {
            try
            {
                //set options for jwt signature validation
                var parameters = new TokenValidationParameters
                {
                    IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                    {
                        var httpClientHandler = new HttpClientHandler();
                        httpClientHandler.ServerCertificateCustomValidationCallback = (message,
                            cert,
                            chain,
                            sslPolicyErrors) =>
                        {
                            return true;
                        };
                        /*get key from idp jwks url to validate id_token signature*/
                        var client = new HttpClient(httpClientHandler);
                        var response = client.GetAsync(configuration["dtidp:JwksUri"]).Result;
                        var responseString = response.Content.ReadAsStringAsync().Result;
                        var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(responseString);
                        return keys.Keys;
                    },
                    //set flag true for validate issuer
                    ValidateIssuer = true,
                    //set flag true for validate Audience
                    ValidateAudience = true,
                    //set valid issuer to verify in token issuer
                    ValidIssuer = configuration["dtidp:Issuer"],
                    //set valid Audience to verify in token Audience
                    ValidAudience = configuration["dtidp:ClientId"],
                    NameClaimType = "name"
                };

                var handler = new JwtSecurityTokenHandler();
                handler.InboundClaimTypeMap.Clear();

                //validate jwt token
                // if token is valid it return claim otherwise throw exception
                var user = handler.ValidateToken(jwt, parameters, out var _);
                return user;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<JObject> GetAccessTokenWithoutUgHub(string code)
        {
            var httpClientHandler = new HttpClientHandler();
            //httpClientHandler.ServerCertificateCustomValidationCallback = (message,
            //    cert,
            //    chain,
            //    sslPolicyErrors) =>
            //{
            //    return true;
            //};
            var TokenUrl = configuration.GetValue<string>("dtidp:UgPassBaseUrl");

            //set client assertion type
            var ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

            /*Prepare jwtToken object to generate jwt token which is send form
              client_assertion parameter in query string*/
            var requestObject = new JWTokenDTO();
            requestObject.Expiry = 60;
            requestObject.Audience = configuration["dtidp:TokenEndpoint"];
            requestObject.Issuer = configuration["dtidp:ClientId"];
            requestObject.Subject = configuration["dtidp:ClientId"];

            var Jwtresponse = JWTTokenManager.GenerateJWTToken(requestObject);
            if (null == Jwtresponse)
            {
                throw new Exception("Fail to generate JWT token for Token request.");
            }
            var ClientAssertion = Jwtresponse;

            //prepare data object which is send with token endpoint url 
            var data1 = new Dictionary<string, string>
                {
                   { "code", code },
                   { "client_id", configuration.GetValue<string>("dtidp:ClientId") },
                   { "redirect_uri", configuration.GetValue<string>("dtidp:RedirectUri") },
                   { "grant_type", "authorization_code" },
                   { "client_assertion_type", ClientAssertionType},
                   { "client_assertion", ClientAssertion}
                };

            //convert data object in url encoded form
            var content = new FormUrlEncodedContent(data1);


            var client1 = new HttpClient(httpClientHandler);
            //client1.DefaultRequestHeaders.Add("Authorization", "Bearer " +
            //    accessToken);
            client1.BaseAddress = new Uri(TokenUrl);

            var response1 = await client1.PostAsync(TokenUrl, content);
            if (response1 == null)
            {
                throw new Exception("GetAccessToken responce getting null");
            }
            if (!response1.IsSuccessStatusCode)
            {
                dynamic error = new JObject();
                error.error = response1.StatusCode;
                error.error_description = response1.ReasonPhrase;
                return error;
            }
            else
            {
                var responseString = await response1.Content.ReadAsStringAsync();

                return JObject.Parse(responseString);
            }
        }

        public async Task<JObject> GetToken()
        {
            var client = new HttpClient();
            var tokenUrl = configuration["dtidp:dt_tokenUrl"];
            var clientId = configuration["dtidp:ClientId"];
            var clientSecret = configuration["dtidp:ClientSecret"];

            var data = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "grant_type", "client_credentials" }
            };

            var authString = $"{clientId}:{clientSecret}";
            var authBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));
            var authorizationHeader = configuration.GetValue<string>("dtidp:AuthorizationHeader");

            var authHeader = "Basic " + authBase64;
            client.DefaultRequestHeaders.Add(authorizationHeader, authHeader);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", authBase64);

            var response = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(data));
            if (!response.IsSuccessStatusCode)
            {
                dynamic error = new JObject();
                error.error = response.StatusCode;
                error.error_description = response.ReasonPhrase;
                return error;
            }
            else
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return JObject.Parse(responseString);
            }
        }

        public async Task<JObject> GetAccessToken(string code)
        {
            try
            {
                var clientId = configuration["dtidp:ClientId"];
                var clientSecret = configuration["dtidp:ClientSecret"];
                //if (configuration.GetValue<bool>("EncryptionEnabled"))
                //{
                //    clientId = PKIMethods.Instance.
                //            PKIDecryptSecureWireData(clientId);
                //    clientSecret = PKIMethods.Instance.
                //            PKIDecryptSecureWireData(clientSecret);
                //};
                //get token endpoint url from appsetting.Development.json file
                var TokenUrl = configuration.GetValue<string>("dtidp:dt_tokenUrl");

                //prepare data object which is send with token endpoint url 
                var data = new Dictionary<string, string>
                {
                   { "code", code },
                   { "client_id", clientId },
                   { "redirect_uri", configuration.GetValue<string>("dtidp:RedirectUri") },
                   { "grant_type", "authorization_code" }
                };



                var isOpenId = configuration.GetValue<bool>("OpenId_Connect");
                if (isOpenId == true)
                {
                    //set client assertion type
                    var ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

                    /*Prepare jwtToken object to generate jwt token which is send form
                      client_assertion parameter in query string*/
                    var requestObject = new JWTokenDTO();
                    requestObject.Expiry = 60;
                    requestObject.Audience = configuration["dtidp:dt_tokenUrl"];
                    requestObject.Issuer = clientId;
                    requestObject.Subject = clientId;

                    var ClientAssertion = JWTTokenManager.GenerateJWTToken(requestObject);
                    if (null == ClientAssertion)
                    {
                        throw new Exception("Fail to generate JWT token for Token request.");
                    }

                    data.Add("client_assertion_type", ClientAssertionType);
                    data.Add("client_assertion", ClientAssertion);

                }

                //convert data object in url encoded form
                var content = new FormUrlEncodedContent(data);

                var authToken = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");

                HttpClient client = _httpClientFactory.CreateClient("ignoreSSL");
                client.BaseAddress = new Uri(TokenUrl);

                var authzHeader = "Basic  " + Convert.ToBase64String(authToken);
                client.DefaultRequestHeaders.Add(configuration["dtidp:AuthorizationHeader"],
                    authzHeader);
                client.BaseAddress = new Uri(TokenUrl);

                var response = await client.PostAsync(TokenUrl, content);
                if (response == null)
                {
                    throw new Exception("GetAccessToken responce getting null");
                }
                if (!response.IsSuccessStatusCode)
                {
                    dynamic error = new JObject();
                    error.error = response.StatusCode;
                    error.error_description = response.ReasonPhrase;
                    return error;
                }
                else
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    return JObject.Parse(responseString);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
 

        public async Task<JObject> GetUserInfo(string accessToken)
        {
            try
            {
                var UserInfoUrl = configuration.GetValue<string>(
                    "dtidp:dt_userinfoUrl");

                HttpClient client = _httpClientFactory.CreateClient("ignoreSSL");
                client.BaseAddress = new Uri(UserInfoUrl);
                var authzHeader = "Bearer  " + accessToken;
                client.DefaultRequestHeaders.Add(configuration["dtidp:AuthorizationHeader"],
                    authzHeader);

                var response = await client.GetAsync(UserInfoUrl);
                if (response == null)
                {
                    throw new Exception("get user info responce getting null");
                }
                if (!response.IsSuccessStatusCode)
                {
                    dynamic error = new JObject();
                    error.error = response.StatusCode;
                    error.error_description = response.ReasonPhrase;
                    return error;
                }
                else
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    JObject info = JObject.Parse(responseString);
                    return info;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<JObject> GetUserImage(string accessToken)
        {
            try
            {
                var UserImageUrl = configuration.GetValue<string>(
                    "dtidp:dt_userimage");

                HttpClient client = _httpClientFactory.CreateClient("ignoreSSL");
                client.BaseAddress = new Uri(UserImageUrl);
                var authzHeader = "Bearer  " + accessToken;
                client.DefaultRequestHeaders.Add(configuration["dtidp:AuthorizationHeader"],
                    authzHeader);

                var response = await client.GetAsync(UserImageUrl);
                if (response == null)
                {
                    throw new Exception("get user info responce getting null");
                }
                if (!response.IsSuccessStatusCode)
                {
                    dynamic error = new JObject();
                    error.error = response.StatusCode;
                    error.error_description = response.ReasonPhrase;
                    return error;
                }
                else
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    JObject info = JObject.Parse(responseString);
                    return info;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<APIResponse> GetUserProfile(string accessToken, GetUserProfileRequest request)
        {
            try
            {
                var userInfoUrl = configuration.GetValue<string>("dtidp:dt_userprofileUrl");
                var authorizationHeader = configuration.GetValue<string>("dtidp:AuthorizationHeader");

                using (HttpClient client = new HttpClient()) // ✅ Use 'using' to prevent leaks
                {
                    client.BaseAddress = new Uri(userInfoUrl);
                    client.DefaultRequestHeaders.Add(authorizationHeader, $"Bearer {accessToken}");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); // ✅ Explicitly set Accept header

                    var json = JsonConvert.SerializeObject(request);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = client.PostAsync(userInfoUrl, content).Result;


                    if (!response.IsSuccessStatusCode)
                    {
                        return new APIResponse
                        {
                            Success = false,
                            Result = response.StatusCode.ToString(),
                            Message = response.ReasonPhrase
                        };
                    }

                    var responseString = await response.Content.ReadAsStringAsync();


                    //var data = JsonConvert.DeserializeObject<APIResponse>(responseString);
                    //if(data.Success == false)
                    //{
                    //    return data;
                    //}
                    var info = await getData1(responseString);

                    if (info != null && info.success)
                    {
                        return new APIResponse
                        {
                            Success = true,
                            Result = info.result,
                            Message = "Successfully retrieved data"
                        };
                    }
                    else
                    {
                        return new APIResponse
                        {
                            Success = false,
                            Result = info.result,
                            Message = info.message
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new APIResponse(false, "Exception while fetching user info: " + ex.Message);
            }
        }


        //public async Task<GetUserProfileResponse> getData(string responseString)
        //{
        //    string clientName = configuration["Client"];

        //    try
        //    {
        //        var response = JsonConvert.DeserializeObject<GetUserProfileResponse1>(responseString);
        //        if (response?.result == null) return null;

        //        var data = response.result;

        //        var userdata = new GetUserProfileResult
        //        {
        //            suid = data.subscriberUid,
        //            birthdate = data.dateOfBirth,
        //            name = data.displayName,
        //            phone_number = data.mobileNumber,
        //            email = data.email,
        //            gender = data.gender,
        //            country = data.country,
        //            photo = data.photo
        //        };


        //        if (null != userdata.birthdate)
        //        {
        //            DateTime birthdate;
        //            try
        //            {
        //                CultureInfo culture = new CultureInfo("en-US");
        //                string format = "dd-MM-yyyy HH:mm:ss";
        //                //birthdate = DateTime.ParseExact(userProfileResponse.result.birthdate, format, null);
        //                birthdate = Convert.ToDateTime(
        //                    userdata.birthdate, culture);
        //                DateTime date1 = new DateTime(birthdate.Year, birthdate.Month, birthdate.Day);
        //                CultureInfo ci = CultureInfo.InvariantCulture;
        //                userdata.birthdate = date1.ToString("yyyy-MM-dd", ci);
        //            }
        //            catch (Exception ex)
        //            {
        //                return null;
        //            }

        //        }

        //        return new GetUserProfileResponse
        //        {
        //            success = response.success,
        //            message = response.message,
        //            result = userdata // ✅ Ensure this is not null
        //        };
        //    }
        //    catch (JsonSerializationException jex)
        //    {
        //        return null; // ✅ Prevent crash on JSON error
        //    }
        //}





        //public async Task<GetUserProfileResponse> GetUserProfile1(string accessToken,
        //    GetUserProfileRequest request)
        //{
        //    try
        //    {
        //        var httpClientHandler = new HttpClientHandler();
        //        var client = new HttpClient(httpClientHandler);

        //        string clientName = configuration["Client"];
        //        var ConsentProfileEndpoint = configuration.GetValue<string>("dtidp:dt_userprofileUrl");
        //        var TokenHeaderName = configuration.GetValue<string>("dtidp:AuthorizationHeader");


        //        if (string.Equals(clientName, "UgPass", StringComparison.OrdinalIgnoreCase))
        //        {
        //            client.BaseAddress = new Uri(ConsentProfileEndpoint);
        //            var authHeader = "Bearer " + accessToken;
        //            client.DefaultRequestHeaders.Add(TokenHeaderName, authHeader);

        //        }
        //        else
        //        {
        //            client.BaseAddress = new Uri(ConsentProfileEndpoint);
        //            var authHeader = "Bearer " + accessToken;
        //            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
        //            //"Bearer", accessToken);
        //            client.DefaultRequestHeaders.Add(TokenHeaderName, authHeader);
        //        }


        //        var content = GetStringContent(request);
        //        var response = client.PostAsync(ConsentProfileEndpoint, content).Result;
        //        if (response == null)
        //        {
        //            throw new Exception("get user info responce getting null");
        //        }
        //        if (!response.IsSuccessStatusCode)
        //        {
        //            throw new Exception(response.ReasonPhrase);
        //        }
        //        else
        //        {
        //            var responseString = await response.Content.ReadAsStringAsync();
        //            var info = await getData1(responseString);
        //            return info;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public StringContent GetStringContent(this object obj)
        //{
        //    var jsonContent = JsonConvert.SerializeObject(obj);
        //    var contentString = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        //    contentString.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        //    return contentString;
        //}

        public async Task<GetUserProfileResponse> getData(string responseString)
        {
            string clientName = configuration["Client"];
            if (string.Equals(clientName, "DTStg", StringComparison.OrdinalIgnoreCase))
            {
                var response = JsonConvert.DeserializeObject<GetUserProfileResponse1>(responseString);

                if (!response.success)
                {
                    GetUserProfileResponse result1 = new GetUserProfileResponse()
                    {
                        success = response.success,
                        message = response.message
                    };

                    return result1;
                }

                var data = response.result;

                GetUserProfileResult userdata = new GetUserProfileResult()
                {
                    suid = data.subscriberUid,
                    birthdate = data.dateOfBirth,
                    name = data.displayName,
                    phone_number = data.mobileNumber,
                    email = data.email,
                    gender = data.gender,
                    country = data.country,
                    photo = data.photo
                };

                GetUserProfileResponse result = new GetUserProfileResponse()
                {
                    success = response.success,
                    message = response.message,
                    result = userdata
                };

                return result;
            }
            else
            {
                //return JsonSerializer.Deserialize<GetUserProfileResponse>(responseString);
                //JObject object1=JObject.Parse(responseString);
                try
                {
                    var jsonString = JsonConvert.DeserializeObject<string>(responseString);
                    return JsonConvert.DeserializeObject<GetUserProfileResponse>(jsonString);
                }
                catch (Exception ex)
                {
                    return JsonConvert.DeserializeObject<GetUserProfileResponse>(responseString);
                }
            }
        }


        public async Task<GetUserProfileResponse> getData1(string responseString)
        {
            string clientName = configuration["Client"];
            var response = JsonConvert.DeserializeObject<GetUserProfileResponse1>(responseString);

            if (!response.success)
            {
                GetUserProfileResponse result1 = new GetUserProfileResponse()
                {
                    success = response.success,
                    message = response.message
                };

                return result1;
            }

            var data = response.result;

            if (data.gender.ToUpper() == "MALE" || data.gender.ToUpper() == "M") data.gender = "Male";
            if (data.gender.ToUpper() == "FEMALE" || data.gender.ToUpper()=="F") data.gender = "Female";

            GetUserProfileResult userdata = new GetUserProfileResult()
            {
                suid = data.subscriberUid,
                birthdate = data.dateOfBirth,
                name = data.displayName,
                phone_number = data.mobileNumber,
                email = data.email,
                gender = data.gender,
                country = data.country,
                photo = data.photo
            };

            GetUserProfileResponse result = new GetUserProfileResponse()
            {
                success = response.success,
                message = response.message,
                result = userdata
            };

            return result;
        }

    }
}
