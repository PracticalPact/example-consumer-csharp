using ForumFrontend;

public static class Program
{
	public static void Main(string[] args)
	{
		PostClient postClient = new(new Uri("http:localhost:3000"));

		Console.WriteLine("Hello world!");
		while(true)
		{
			Console.WriteLine(postClient.GetAllPosts("some id"));
		}
	}
}
