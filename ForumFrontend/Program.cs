using ForumFrontend;

public static class Program
{
	public static void Main(string[] args)
	{
		Run().GetAwaiter().GetResult();
	}

	private static async Task Run()
	{
		// Example usage of the frontend
		// This will not work as it expects a running backend and SSO microservice.
		// That is fine, because we are only interested in CDCT, which does not require dependencies to be live.
		using PostClient postClient = new(new Uri("http://localhost:3000"));
		using SsoClient ssoClient = new(new Uri("https://localhost:8463"));

		await ssoClient.LogIn("user", "pass");
		
		// Create new post
		var postResult = await postClient.CreateNewPost(ssoClient.SessionId, "Title", "Content");
		Console.WriteLine(postResult.ToString());
		
		// Look at all posts
		var getResult = await postClient.GetAllPosts(ssoClient.SessionId);
		Console.WriteLine(getResult.ToString());
		
		Console.ReadKey();
	}
}
