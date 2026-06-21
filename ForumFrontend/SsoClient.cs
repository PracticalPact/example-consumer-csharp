using System.Net.Http.Json;

namespace ForumFrontend;

/// <summary>
/// Handles communication with the SSO microservice (which doesn't exist).
/// When logging in, it will receive a session id and hold on to it.
/// Then the PostClient can use it to perform actions on behalf of the logged in user.
/// 
/// Again, the SSO microservice does not actually exist, so running this will not work.
/// That is because it is irrelevant to the example of CDCT.
/// </summary>
public class SsoClient : IDisposable
{
	public string? SessionId { get; private set; }

	private readonly HttpClient _httpClient;

	public SsoClient(Uri baseUri)
	{
		_httpClient = new();
		_httpClient.BaseAddress = baseUri;
	}

	public async Task LogOut()
	{
		SessionId = null;
	}

	public async Task<bool> LogIn(string username, string password)
	{
		HttpRequestMessage request = new(HttpMethod.Post, "/login");
		request.Content = JsonContent.Create(new { username, password });

		var response = await _httpClient.SendAsync(request);
		if (response.IsSuccessStatusCode)
		{
			var content = await response.Content.ReadFromJsonAsync<LogInResponseContent>();
			if (content == null)
			{
				throw new InvalidOperationException("Expected response body was empty.");
			}
			SessionId = content.SessionId;
			return true;
		}
		else
		{
			return false;
		}
	}

	public void Dispose()
	{
		_httpClient.Dispose();
	}

	private class LogInResponseContent
	{
		public required string SessionId { get; set; }
	}
}