using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using static System.Net.WebRequestMethods;

namespace ForumFrontend
{
	internal class SsoClient : IDisposable
	{
		public string? sessionId { get; private set; }

		private readonly HttpClient _httpClient;

		public SsoClient(Uri baseUri)
		{
			_httpClient = new();
			_httpClient.BaseAddress = baseUri;
		}

		public async Task LogOut()
		{
			sessionId = null;
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
				sessionId = content.SessionId;
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
}
