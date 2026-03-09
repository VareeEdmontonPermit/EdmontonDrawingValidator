using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Query;
using EdmontonDrawingValidator.Model;
using System.Linq.Expressions;
using System.Collections;
using SharedClasses;
using SharedClasses.Constants;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace EdmontonDrawingValidator
{
    public sealed class HttpOperation
    {
        public async Task<string> GetUpdateRuleCheckingStatusAsync(string path)
        {
            string responseMessage = "";

            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(path);
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Response " + response.IsSuccessStatusCode);
                if (response.IsSuccessStatusCode)
                {
                    responseMessage = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Failed to update project status");
                }
            }
            catch { }
            return responseMessage;
        }

        public async Task<string> AddBuildingName(Uri u, HttpContent c)
        {
            //var response = string.Empty;
            string responseMessage = "";
            using (var client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = u,
                    Content = c
                };

                HttpResponseMessage response = await client.SendAsync(request);
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Response " + response.IsSuccessStatusCode);
                if (response.IsSuccessStatusCode)
                {
                    responseMessage = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Failed to update project status");
                }

                //if (result.IsSuccessStatusCode)
                //{
                //    //response = result.StatusCode.ToString();
                //    responseMessage = await response.Content.ReadAsStringAsync();
                //}
            }
            return responseMessage;
        }
    }
}
