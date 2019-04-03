using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sita.BagDrop.DcsConnect.LoadTests.Helpers
{
    public class HttpLoadRunner
    {
        public string ServiceUrl { get; set; }
        public string EndPoint { get; set; }
        public string BearerToken { get; set; }
        public HttpMethod UsedHttpMethod { get; set; }
        public int NumCalls { get; set; }
        public int NumThreads { get; set; }

        public readonly List<long> ResponseTimes = new List<long>();
        public readonly List<HttpResponseMessage> ResponseMessages = new List<HttpResponseMessage>();

        private readonly List<KeyValuePair<string, string>> _usedHttpRequestHeaders = new List<KeyValuePair<string, string>>();
        private readonly List<object> _usedPostData = new List<object>();

        public void ClearResults()
        {
            ResponseTimes.Clear();
            ResponseMessages.Clear();
            _usedPostData.Clear();
        }

        public void AddPostData(object obj)
        {
            _usedPostData.Add(obj);
        }

        public void AddHttpRequestHeader(string name, string value)
        {
            _usedHttpRequestHeaders.Add(new KeyValuePair<string, string>(name, value));
        }

        public void Run()
        {
            var taskList = new List<Task>();

            for (var x = 0; x < NumCalls; x++)
            {
                if (taskList.Count == NumThreads)
                {
                    var taskIndex = Task.WaitAny(taskList.ToArray());
                    taskList.RemoveAt(taskIndex);
                }

                var index = x;
                taskList.Add(Task.Run(() => (ExecuteHttpCall(index))));
            }

            if (taskList.Count > 0)
                Task.WaitAll(taskList.ToArray());
        }

        private async Task ExecuteHttpCall(int index)
        {
            var timer = new Stopwatch();

            using (var client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(BearerToken))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                foreach(var header in _usedHttpRequestHeaders)
                    client.DefaultRequestHeaders.Add(header.Key, new[] { header.Value });

                using (var request = new HttpRequestMessage(UsedHttpMethod, ServiceUrl + EndPoint))
                {
                    if (_usedPostData.Count > 0)
                    {
                        string json;

                        if ((index + 1) <= _usedPostData.Count)
                            json = JsonConvert.SerializeObject(_usedPostData[index]);
                        else
                            json = JsonConvert.SerializeObject(_usedPostData[0]);

                        request.Content = new StringContent(json, Encoding.UTF8, "application/json-patch+json");
                    }

                    timer.Start();

                    var responseMsg = await client.SendAsync(request);

                    timer.Stop();

                    ResponseMessages.Add(responseMsg);
                }
            }

            ResponseTimes.Add(timer.ElapsedMilliseconds);
        }
    }
}
