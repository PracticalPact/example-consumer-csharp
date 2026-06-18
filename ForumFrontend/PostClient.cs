using System.Net;
using System.Net.Http.Json;

namespace ForumFrontend;

internal class PostClient : IDisposable
{
	private const string sessionIdKey = "session-id";
	private readonly HttpClient _httpClient;

	public PostClient(Uri baseUri)
	{
		_httpClient = new();
		_httpClient.BaseAddress = baseUri;
	}

	public async Task<Result<GetAllPostsResponse>> GetAllPosts(string sessionId)
	{
		HttpRequestMessage request = new(HttpMethod.Get, "/posts");
		request.Headers.Add(sessionIdKey, sessionId);

		var response = await _httpClient.SendAsync(request);

		return await ParseResponse<GetAllPostsResponse>(response);
	}

	public async Task<Result<GetPostDetailsResponse>> GetPostDetails(string sessionId)
	{
		HttpRequestMessage request = new(HttpMethod.Get, "/post");
		request.Headers.Add(sessionIdKey, sessionId);

		var response = await _httpClient.SendAsync(request);

		return await ParseResponse<GetPostDetailsResponse>(response);
	}

	public async Task<Result<CreateNewPostResponse>> CreateNewPost(string sessionId, string title, string content)
	{
		HttpRequestMessage request = new(HttpMethod.Post, "/posts");
		request.Headers.Add(sessionIdKey, sessionId);
		request.Content = JsonContent.Create(new { title, content });

		var response = await _httpClient.SendAsync(request);

		return await ParseResponse<CreateNewPostResponse>(response);
	}

	/// <summary>
	/// Turns an HTTP response into a Result object that internal systems understand.
	/// </summary>
	/// <typeparam name="TContent">The type of content in a successful result.</typeparam>
	/// <param name="response">The response given by the http client.</param>
	/// <returns>A parsed result without http-specifics.</returns>
	/// <exception cref="HttpRequestException">Thrown on an unexpected http response. Indicates that something is unexpectedly wrong.</exception>
	private static async Task<Result<TContent>> ParseResponse<TContent>(HttpResponseMessage response)
	{
		return response.StatusCode switch
		{
			HttpStatusCode.OK => new SuccessResult<TContent>(await ReadSuccessContent<TContent>(response)),
			HttpStatusCode.Unauthorized => new UnauthorizedErrorResult<TContent>(),
			HttpStatusCode.InternalServerError => new ServerErrorResult<TContent>(response.ReasonPhrase ?? ""),
			_ => throw new HttpRequestException($"Unexpected status code {response.StatusCode} with reason: {response.ReasonPhrase ?? "none"}.")
		};
	}

	/// <summary>
	/// Reads the http response body and deserializes it into a proper object.
	/// </summary>
	/// <typeparam name="TContent">The type to deserialize to.</typeparam>
	/// <param name="response">The response given by the http client.</param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	private static async Task<TContent> ReadSuccessContent<TContent>(HttpResponseMessage response)
	{
		TContent? content = await response.Content.ReadFromJsonAsync<TContent>();
		if (content == null)
		{
			throw new InvalidOperationException("Expected response body was empty.");
		}
		return content;
	}

	public void Dispose()
	{
		_httpClient.Dispose();
	}

	public class GetAllPostsResponse
	{
		public required List<string> Posts { get; set; }

		public override string ToString()
		{
			string result = "Your post ids:\n";
			foreach (string post in Posts)
			{
				result += $"{post}\n";
			}
			return result;
		}
	}

	public class GetPostDetailsResponse
	{
		public required string Title { get; set; }
		public required string Content { get; set; }

		public override string ToString()
		{
			return $"{Title}:\n{Content}\n";
		}
	}

	public class CreateNewPostResponse
	{
		public required string Id { get; set; }

		public override string ToString()
		{
			return $"Post successfully created with id {Id}\n";
		}
	}

	public abstract record Result<TContent>;

	public record SuccessResult<TContent>(TContent content) : Result<TContent>
	{
		public override string ToString()
		{
			return content.ToString();
		}
	}

	public record UnauthorizedErrorResult<TContent> : Result<TContent>
	{
		public override string ToString()
		{
			return $"An authorization error occured. Try logging in again.\n";
		}
	}

	public record ServerErrorResult<TContent>(string message) : Result<TContent>
	{
		public override string ToString()
		{
			return $"A server error occured: {message}.\n";
		}
	}
}
