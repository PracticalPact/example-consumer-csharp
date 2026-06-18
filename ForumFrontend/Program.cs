using ForumFrontend;

public static class Program
{
	public static void Main(string[] args)
	{
		Run().GetAwaiter().GetResult();
	}

	private static async Task Run()
	{
		using PostClient postClient = new(new Uri("http://localhost:3000"));
		using SsoClient ssoClient = new(new Uri("http://localhost:3001"));

		Console.WriteLine("Hello world!");

		while (true)
		{
			Console.WriteLine();

			if (ssoClient.sessionId == null)
			{
				Console.WriteLine("You are logged out. Type in your credentials.");
				string? username = Console.ReadLine();
				string? password = Console.ReadLine();
				if (username == null || password == null)
				{
					continue;
				}
				bool result = await ssoClient.LogIn(username, password);
				if (result)
				{
					Console.WriteLine("Successfully logged in.");
				}
				else
				{
					Console.WriteLine("Something went wrong when logging in.");
				}
			}
			else
			{
				Console.WriteLine($"Perform one of the following operations:\n" +
					$"1: Log out\n" +
					$"2: See all your posts\n" +
					$"3: See specific post\n" +
					$"4: Create new post");

				int choice;
				try
				{
					choice = Int32.Parse(Console.ReadLine()!);
				}
				catch
				{
					Console.WriteLine("Invalid input.");
					continue;
				}

				switch (choice)
				{
					case 1:
						throw new NotImplementedException();
						break;
					case 2:
						Console.WriteLine(await postClient.GetAllPosts(ssoClient.sessionId));
						break;
					case 3:
						throw new NotImplementedException();
						break;
					case 4:
						throw new NotImplementedException();
						break;
				}
			}

		}
	}
}
