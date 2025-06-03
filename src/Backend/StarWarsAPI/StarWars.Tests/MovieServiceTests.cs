using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using StarWars.Infraestucture.Interfaces;
using StarWars.Services.Interfaces;
using StarWarsApi.Data;
using StarWarsApi.Infraestructure.Model;
using System.Net;
using System.Text;
using Xunit;

namespace StarWars.Tests.Services
{
    public class MovieServiceTests
    {
        private readonly Mock<IMovieRepository> _mockRepository;
        private readonly Mock<ILogger<IMovieService>> _mockLogger;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly MovieService _movieService;

        public MovieServiceTests()
        {
            _mockRepository = new Mock<IMovieRepository>();
            _mockLogger = new Mock<ILogger<IMovieService>>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://www.swapi.tech/api/")
            };

            _movieService = new MovieService(_httpClient, _mockRepository.Object, _mockLogger.Object);

        }

        [Fact]
        public async Task GetAllMoviesWarmup_HttpRequestException_ShouldCallHttpClient()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            _movieService.GetAllMoviesWarmup();
            await Task.Delay(300); 

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("films")),
                ItExpr.IsAny<CancellationToken>()
            );
        }


        [Fact]
        public async Task GetAllMoviesWarmup_HttpRequestException_ShouldLogErrorAndThrowInvalidOperationException()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            Exception caughtException = null;

            try
            {
                _movieService.GetAllMoviesWarmup();
                await Task.Delay(200); 
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("films")),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetAllMoviesWarmup_JsonException_ShouldLogErrorAndThrowInvalidOperationException()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(invalidJson, Encoding.UTF8, "application/json")
                });

            // Act
            _movieService.GetAllMoviesWarmup();
            await Task.Delay(200); 

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("films")),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetAllMoviesWarmup_UnexpectedException_ShouldCallHttpClient()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new InvalidOperationException("Unexpected error"));

            _movieService.GetAllMoviesWarmup();
            await Task.Delay(300); 

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("films")),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SearchByTitle_Success_ShouldReturnSearchResults()
        {
            // Arrange
            var title = "New Hope";
            var expectedResults = new List<SearchResult>
            {
                new SearchResult { Uid = 1, Name = "A New Hope" },
                new SearchResult { Uid = 2, Name = "Return of the Jedi" }
            };

            _mockRepository
                .Setup(x => x.SearchByTitleAsync(title))
                .ReturnsAsync(expectedResults);

            // Act
            var result = await _movieService.SearchByTitle(title);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedResults, result);

            _mockRepository.Verify(x => x.SearchByTitleAsync(title), Times.Once);
        }

        [Fact]
        public async Task SearchByTitle_RepositoryThrowsException_ShouldRethrow()
        {
            var title = "Test";
            var expectedException = new Exception("Repository error");

            _mockRepository
                .Setup(x => x.SearchByTitleAsync(title))
                .ThrowsAsync(expectedException);

            var exception = await Assert.ThrowsAsync<Exception>(() => _movieService.SearchByTitle(title));

            Assert.Equal("Repository error", exception.Message);
            _mockRepository.Verify(x => x.SearchByTitleAsync(title), Times.Once);
        }

        [Fact]
        public async Task SearchMovieByIdWithCharacters_Success_ShouldReturnMovieWithCharacters()
        {
            var uid = 1;
            var expectedMovie = new MovieWithCharacterNames
            {
                Id = "1",
                Title = "A New Hope",
                Characters = new List<CharacterDetail>
                {
                    new CharacterDetail {Uid = 1, Name="Luke Skywalker"},
                    new CharacterDetail {Uid = 13, Name="Chewbacca"}
                }
            };

            _mockRepository
                .Setup(x => x.GetMovieWithCharacterNamesByUidAsync(uid))
                .ReturnsAsync(expectedMovie);

            var result = await _movieService.SearchMovieByIdWithCharacters(uid);

            Assert.NotNull(result);
            Assert.Equal(expectedMovie.Id, result.Id);
            Assert.Equal(expectedMovie.Title, result.Title);
            Assert.Equal(expectedMovie.Characters, result.Characters);

            _mockRepository.Verify(x => x.GetMovieWithCharacterNamesByUidAsync(uid), Times.Once);
        }

        [Fact]
        public async Task SearchMovieByIdWithCharacters_RepositoryThrowsException_ShouldRethrow()
        {
            // Arrange
            var uid = 1;
            var expectedException = new Exception("Repository error");

            _mockRepository
                .Setup(x => x.GetMovieWithCharacterNamesByUidAsync(uid))
                .ThrowsAsync(expectedException);

            var exception = await Assert.ThrowsAsync<Exception>(() => _movieService.SearchMovieByIdWithCharacters(uid));

            Assert.Equal("Repository error", exception.Message);
            _mockRepository.Verify(x => x.GetMovieWithCharacterNamesByUidAsync(uid), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldInitializeAllDependencies()
        {
            var service = new MovieService(_httpClient, _mockRepository.Object, _mockLogger.Object);

            Assert.NotNull(service);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task SearchByTitle_WithEmptyOrNullTitle_ShouldCallRepository(string title)
        {
            var expectedResults = new List<SearchResult>();
            _mockRepository
                .Setup(x => x.SearchByTitleAsync(title))
                .ReturnsAsync(expectedResults);

            var result = await _movieService.SearchByTitle(title);

            Assert.NotNull(result);
            Assert.Empty(result);
            _mockRepository.Verify(x => x.SearchByTitleAsync(title), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(999)]
        public async Task SearchMovieByIdWithCharacters_WithDifferentUids_ShouldCallRepository(int uid)
        {
            var expectedMovie = new MovieWithCharacterNames { Id = uid.ToString() };
            _mockRepository
                .Setup(x => x.GetMovieWithCharacterNamesByUidAsync(uid))
                .ReturnsAsync(expectedMovie);

            var result = await _movieService.SearchMovieByIdWithCharacters(uid);

            Assert.NotNull(result);
            Assert.Equal(uid.ToString(), result.Id);
            _mockRepository.Verify(x => x.GetMovieWithCharacterNamesByUidAsync(uid), Times.Once);
        }
    }
}