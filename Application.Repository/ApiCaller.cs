using Application.Core;
using Application.Core.Common;
using Application.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure
{

    public class ApiCaller : IApiCaller
    {
        private readonly IConfiguration _configuration;
        public ApiCaller(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<bool> AddReceiptAsync(MoneyReceiptUploadDto creationDto)
        {
            var baseApiUrl = _configuration.GetValue<string>("BaseApiUrl");
            string apiUrl = $"{baseApiUrl}upload-money-receipt";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var formData = new MultipartFormDataContent();

                    // Add the ReceivePaymentId as form data
                    formData.Add(new StringContent(creationDto.ReceivePaymentId.ToString()), "ReceivePaymentId");

                    // Add the file if it exists
                    if (creationDto.FileDetails != null && creationDto.FileDetails.Length > 0)
                    {
                        var fileContent = new StreamContent(creationDto.FileDetails.OpenReadStream());
                        fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                        {
                            Name = "FileDetails",
                            FileName = creationDto.FileDetails.FileName
                        };
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(creationDto.FileDetails.ContentType);

                        formData.Add(fileContent);
                    }

                    // Log the form data being sent (without the file content)
                    Console.WriteLine($"Sending Request to {apiUrl} with ReceivePaymentId: {creationDto.ReceivePaymentId}");

                    // Send POST request
                    HttpResponseMessage response = await client.PostAsync(apiUrl, formData);

                    // Log the response status
                    Console.WriteLine($"Response Status: {response.StatusCode} - {response.ReasonPhrase}");

                    // Throw if not a success code.
                    response.EnsureSuccessStatusCode();

                    // Read and process the response
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Body: {responseBody}"); // Log the response body

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseBody);
                    return apiResponse!.succeeded;
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw;
            }
        }






    }

    // Define classes to match the JSON structure
    public class ApiResponse
    {
        public int statusCode { get; set; }
        public bool succeeded { get; set; }
        public string? message { get; set; }
        public int data { get; set; }
    }


}
