# HttpLoadRunner
This is a helper class for load testing http api endpoints. You can specify global parameters for the class, then re-use the class for various tests. You can specify the following parameters :

* ServiceUrl       - this is the main url for your service
* EndPoint         - this is the route to your endpoint from the service url 
* BearerToken      - in case the service is secured with Auth0 you will need an access token
* UsedHttpMethod   - the HttpMethod used
* NumCalls         - the number of times each endpoint is hit
* NumThreads       - the number of threads that are used

When the tests have completed, the following class properties are filled with results:
* ResponseTimes    - the list of response times for each call
* ResponseMessages - the list of HttpResponseMessages for each call

The following is a basic example of how to use the HttpLoadRunner in your testclass and use it in 2 test methods:

```
namespace ExampleTests
{
    [TestClass]
    public class ExampleTest
    {
        private static HttpLoadRunner _httpLoadRunner;

        private int _maxResponseTime;
        private int _avgResponseTime;

        [TestInitialize]
        public void Init()
        {
            _maxResponseTime = 10000;
            _avgResponseTime = 50000;

            //-- Create Helper --
            _httpLoadRunner = new HttpLoadRunner
            {
                ServiceUrl = "https://test.api.com/",
                NumCalls = 10,
                NumThreads = 2
            };

            _httpLoadRunner.AddHttpRequestHeader("X-ClientId", "myClientId");
        }

        [TestMethod]
        public void Test01_Get()
        {
            //arrange
            _httpLoadRunner.ClearResults();
            _httpLoadRunner.EndPoint = "/api/version";
            _httpLoadRunner.UsedHttpMethod = HttpMethod.Get;

            //act
            _httpLoadRunner.Run();

            //assert
            _httpLoadRunner.ResponseMessages.All(p => p.IsSuccessStatusCode).Should().BeTrue();
            _httpLoadRunner.ResponseTimes.Max().Should().BeLessOrEqualTo(_maxResponseTime);
            _httpLoadRunner.ResponseTimes.Average().Should().BeLessOrEqualTo(_avgResponseTime);
        }

        [TestMethod]
        public void Test02_Post()
        {
            //arrange
            _httpLoadRunner.ClearResults();
            _httpLoadRunner.EndPoint = "/api/postmethod";
            _httpLoadRunner.UsedHttpMethod = HttpMethod.Post;
            _httpLoadRunner.AddPostData(new {id = 25});

            //act
            _httpLoadRunner.Run();

            //assert
            _httpLoadRunner.ResponseMessages.All(p => p.IsSuccessStatusCode).Should().BeTrue();
            _httpLoadRunner.ResponseTimes.Max().Should().BeLessOrEqualTo(_maxResponseTime);
            _httpLoadRunner.ResponseTimes.Average().Should().BeLessOrEqualTo(_avgResponseTime);
        }
    }
}

```

You can add various PostData in case you want to call a Post method with different data for each call, just use the AddPostData method to add an object to the list. You should add the same amount of elements as numCalls, but if there is no corresponding object in the PostData the class will use the first element in PostData.

You can add multiple HttpRequestHeaders by calling AddHttpRequestHeader. 
