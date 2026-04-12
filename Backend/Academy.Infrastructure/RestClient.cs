using Academy.Core.Abstraction.Infrastructure;
using System.Net.Http.Headers;
using System.Text.Json;
namespace Academy.Infrastructure
{
    public class RestClient : IRestClient
    {
        private Dictionary<string, object> response = [];
        public async Task<Dictionary<string, object>> SendAsync(string endpoint, HttpMethod httpMethod, string token, HttpContent content = null)
        {
            //SSL connection issue fixes
            HttpClientHandler clientHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            };
            using HttpClient _httpClient = new(clientHandler);
            string responseString = default;
            HttpResponseMessage response_message = null;
            response = [];
            response.Add("time_stamp", DateTime.Now.ToString("ddMMyyyyHHmmssfff"));
            try
            {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(httpMethod == HttpMethod.Patch ? "application/json-patch+json" : "application/json"));
                if (!string.IsNullOrEmpty(token))
                {
                    if (!token.ToLower().StartsWith("bearer"))
                        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    else
                        _httpClient.DefaultRequestHeaders.Add("Authorization", token);
                }

                if (httpMethod.Equals(HttpMethod.Get))
                {
                    response_message = await _httpClient.GetAsync(endpoint);
                    responseString = await response_message.Content.ReadAsStringAsync();
                }
                else if (httpMethod.Equals(HttpMethod.Post))
                {
                    response_message = await _httpClient.PostAsync(endpoint, content);
                    responseString = await response_message.Content.ReadAsStringAsync();
                }
                else if (httpMethod.Equals(HttpMethod.Put))
                {
                    response_message = await _httpClient.PutAsync(endpoint, content);
                    responseString = await response_message.Content.ReadAsStringAsync();
                }
                else if (httpMethod.Equals(HttpMethod.Delete))
                {
                    response_message = await _httpClient.DeleteAsync(endpoint);
                    responseString = await response_message.Content.ReadAsStringAsync();
                }
                else if (httpMethod.Equals(HttpMethod.Patch))
                {
                    response_message = await _httpClient.PatchAsync(endpoint, content);
                    responseString = await response_message.Content.ReadAsStringAsync();
                }
                else if (httpMethod.Equals(HttpMethod.Options))
                {
                    var request = new HttpRequestMessage(HttpMethod.Options, endpoint);
                    response_message = await _httpClient.SendAsync(request);
                    responseString = await response_message.Content.ReadAsStringAsync();
                }
                response["result"] = JsonSerializer.Deserialize<Type>(responseString);
                response["is_success"] = response_message.IsSuccessStatusCode;
                response["message"] = response_message.ReasonPhrase ?? string.Empty;
                response["status_code"] = response_message.StatusCode;
                response["raw_response"] = responseString ?? string.Empty;
            }
            catch (Exception e)
            {
                response["result"] = string.Empty;
                response["is_success"] = response_message.IsSuccessStatusCode;
                response["message"] = response_message.ReasonPhrase ?? string.Empty;
                response["error_message"] = e.Message;
                response["status_code"] = response_message.StatusCode;
                response["raw_response"] = responseString ?? string.Empty;
            }
            return response;
        }
    }
}
