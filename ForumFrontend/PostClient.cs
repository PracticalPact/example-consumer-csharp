using System.Net;
using System.Net.Http.Json;

namespace ForumFrontend;

/// <summary>
/// This class handles communication with the Forum Backend. It allows the creation and viewing of Posts.
/// The backend expects the user to be logged in on most requests, and this is proven by passing along a session id in the header.
/// For each request, there are a number of expected HTTP response codes. If something else happens, an error is thrown.
/// For expected HTTP errors, a suitable value is returned without panicking.
/// </summary>
public class PostClient : IDisposable
{
	private const string SessionIdKey = "session-id";
	private readonly HttpClient _httpClient;

	public PostClient(Uri baseUri)
	{
		_httpClient = new();
		_httpClient.BaseAddress = baseUri;
	}

	public async Task<ClientResult> GetAllPosts(string sessionId)
	{
		HttpRequestMessage request = new(HttpMethod.Get, "/posts");
		request.Headers.Add(SessionIdKey, sessionId);

		var response = await _httpClient.SendAsync(request);

		return response.StatusCode switch
		{
			HttpStatusCode.OK => new SuccessResult<GetAllPostsResponse>(await ReadSuccessContent<GetAllPostsResponse>(response)),
			HttpStatusCode.Unauthorized => new UnauthorizedErrorResult(),
			HttpStatusCode.InternalServerError => new ServerErrorResult(response.ReasonPhrase ?? ""),
			_ => throw new HttpRequestException($"Unexpected status code {response.StatusCode} with reason: {response.ReasonPhrase ?? "none"}.")
		};
	}

	public async Task<ClientResult> GetPostDetails(string postId)
	{
		HttpRequestMessage request = new(HttpMethod.Get, $"/posts/{postId}");

		var response = await _httpClient.SendAsync(request);

		return response.StatusCode switch
		{
			HttpStatusCode.OK => new SuccessResult<GetPostDetailsResponse>(await ReadSuccessContent<GetPostDetailsResponse>(response)),
			HttpStatusCode.Unauthorized => new UnauthorizedErrorResult(),
			HttpStatusCode.InternalServerError => new ServerErrorResult(response.ReasonPhrase ?? ""),
			_ => throw new HttpRequestException($"Unexpected status code {response.StatusCode} with reason: {response.ReasonPhrase ?? "none"}.")
		};
	}

	public async Task<ClientResult> CreateNewPost(string sessionId, string title, string content)
	{
		HttpRequestMessage request = new(HttpMethod.Post, "/posts");
		request.Headers.Add(SessionIdKey, sessionId);
		request.Content = JsonContent.Create(new { title, content });

		var response = await _httpClient.SendAsync(request);

		return response.StatusCode switch
		{
			HttpStatusCode.Created => new SuccessResult<CreateNewPostResponse>(await ReadSuccessContent<CreateNewPostResponse>(response)),
			HttpStatusCode.Unauthorized => new UnauthorizedErrorResult(),
			HttpStatusCode.InternalServerError => new ServerErrorResult(response.ReasonPhrase ?? ""),
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

	/// <summary>
	/// Represents the response body to GetAllPosts.
	/// Will be deserialized from JSON.
	/// </summary>
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

	/// <summary>
	/// Represents the response body to GetPostDetails.
	/// Will be deserialized from JSON.
	/// </summary>
	public class GetPostDetailsResponse
	{
		public required string Title { get; set; }
		public required string Content { get; set; }

		public override string ToString()
		{
			return $"{Title}:\n{Content}\n";
		}
	}

	/// <summary>
	/// Represents the response body to CreateNewPost.
	/// Will be deserialized from JSON.
	/// </summary>
	public class CreateNewPostResponse
	{
		public required string Id { get; set; }

		public override string ToString()
		{
			return $"Post successfully created with id {Id}\n";
		}
	}
}
