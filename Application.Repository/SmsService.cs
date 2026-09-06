using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Newtonsoft.Json;

namespace Application.Infrastructure
{
    public class SmsService : ISmsService
    {

        public SmsService()
        {
        }
        public async Task Send(string mobileNumber, string message)
        {
            string apiKey = "1NeXi5YVFY6lyieg9LkB";
            //string senderId = "8809617612435";
            string senderId = "AP FEED";
            string smsApiUrl = $"http://139.99.39.237/api/smsapi?api_key={apiKey}&type=text&number={mobileNumber}&senderid={senderId}&message={message}";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(smsApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    SMSAPIResponse result = JsonConvert.DeserializeObject<SMSAPIResponse>(responseBody);
                    if (result.response_code != 202) throw new BadRequestException(result.error_message ?? "Something Went Wrong!!");
                }
                else
                {
                    throw new Exception("The API request was not successful. Status code: " + response.StatusCode);
                }
            }
        }
    }
}


public class SMSAPIResponse
{
    public int response_code { get; set; }
    public int message_id { get; set; }
    public string? success_message { get; set; }
    public string? error_message { get; set; }
}