// //Tests/WorkerTaskRunnerTests.cs
//
// using System.Net;
// using Dto;
// using Microsoft.Extensions.Logging;
// using Moq;
// using Moq.Protected;
// using Worker.Interfaces;
// using Worker.Services;
//
// namespace Tests;
//
// public class WorkerTaskRunnerTests
// {
//     [Fact]
//     public async Task RunTaskAsync_ShouldCallBruteForce_AndSendPatch()
//     {
//         var hashCrackMock = new Mock<IHashCrackService>();
//         hashCrackMock
//             .Setup(s => s.BruteForce("fake-hash", 4, 1, 1))
//             .Returns(new List<string> { "word1", "word2" });
//
//         var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
//
//         handlerMock
//             .Protected()
//             .Setup<Task<HttpResponseMessage>>(
//                 "SendAsync",
//                 ItExpr.IsAny<HttpRequestMessage>(),
//                 ItExpr.IsAny<CancellationToken>()
//             )
//             .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
//             {
//                 Assert.Equal(HttpMethod.Patch, request.Method);
//
//                 return new HttpResponseMessage
//                 {
//                     StatusCode = HttpStatusCode.OK
//                 };
//             });
//
//         var httpClient = new HttpClient(handlerMock.Object)
//         {
//             BaseAddress = new System.Uri("http://fake-manager")
//         };
//
//         var loggerMock = new Mock<ILogger<WorkerTaskRunner>>();
//
//         var runner = new WorkerTaskRunner(
//             loggerMock.Object,
//             httpClient,
//             hashCrackMock.Object
//         );
//
//         var taskDto = new WorkerTaskDto
//         {
//             RequestId = "test-123",
//             Hash = "fake-hash",
//             MaxLength = 4,
//             PartNumber = 1,
//             PartCount = 1
//         };
//
//         await runner.RunTaskAsync(taskDto);
//
//         hashCrackMock.Verify(s => s.BruteForce("fake-hash", 4, 1, 1), Times.Once);
//
//         handlerMock.Protected().Verify(
//             "SendAsync",
//             Times.Once(),
//             ItExpr.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Patch),
//             ItExpr.IsAny<CancellationToken>()
//         );
//     }
//
//     [Fact]
//     public async Task RunTaskAsync_PatchFails_ShouldLogError()
//     {
//         var hashCrackMock = new Mock<IHashCrackService>();
//         hashCrackMock
//             .Setup(s => s.BruteForce(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
//             .Returns(new List<string> { "dummy" });
//
//         var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
//         
//         handlerMock
//             .Protected()
//             .Setup<Task<HttpResponseMessage>>("SendAsync",
//                 ItExpr.IsAny<HttpRequestMessage>(),
//                 ItExpr.IsAny<CancellationToken>())
//             .ReturnsAsync(new HttpResponseMessage
//             {
//                 StatusCode = HttpStatusCode.InternalServerError
//             });
//
//
//         var httpClient = new HttpClient(handlerMock.Object)
//         {
//             BaseAddress = new Uri("http://fake-manager")
//         };
//
//         var loggerMock = new Mock<ILogger<WorkerTaskRunner>>();
//         var runner = new WorkerTaskRunner(
//             loggerMock.Object,
//             httpClient,
//             hashCrackMock.Object
//         );
//
//         var dto = new WorkerTaskDto
//         {
//             RequestId = "test-999",
//             Hash = "some-hash",
//             MaxLength = 4,
//             PartNumber = 1,
//             PartCount = 1
//         };
//
//         await runner.RunTaskAsync(dto);
//
//         hashCrackMock.Verify(s => s.BruteForce("some-hash", 4, 1, 1), Times.Once);
//
//         handlerMock.Protected().Verify(
//             "SendAsync",
//             Times.Once(),
//             ItExpr.IsAny<HttpRequestMessage>(),
//             ItExpr.IsAny<CancellationToken>()
//         );
//
//         loggerMock.Verify(
//             x => x.Log(
//                 LogLevel.Error,
//                 It.IsAny<EventId>(),
//                 It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Error sending results to manager")),
//                 It.IsAny<Exception>(),
//                 It.IsAny<Func<It.IsAnyType, Exception, string>>()),
//             Times.Once);
//     }
// }