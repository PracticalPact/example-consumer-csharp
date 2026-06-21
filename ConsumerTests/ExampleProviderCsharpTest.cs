using System.Net;
using PactNet;
using PactNet.Matchers;
using PactNet.Output.Xunit;
using Xunit.Abstractions;
using ForumFrontend;

namespace ConsumerTests;

/// <summary>
/// This performs Consumer tests for the Provider called example-provider-csharp and creates the Contract.
/// </summary>
public class ExampleProviderCsharpTest
{
	private readonly IPactBuilderV4 _pact;

	public ExampleProviderCsharpTest(ITestOutputHelper output)
	{
		var config = new PactConfig
		{
			Outputters = [new XunitOutput(output)],
			PactDir = Path.Combine("..", "..", "..", "..", "pacts")
		};

		_pact = Pact.V4(Environment.GetEnvironmentVariable("REPOSITORY_NAME") ?? "example-consumer-csharp", "example-provider-csharp", config).WithHttpInteractions();

	}

	[Fact]
	[Trait("Category", "Contract")]
	[Trait("ContractRole", "Consumer")]
	public async Task GetAllPosts_IsLoggedIn_ReturnsAllIds()
	{
		string sessionId = "valid-token";

		_pact
			.UponReceiving("a request for all posts with a valid session id")
			.Given($"a user with at least one post is logged in with session id: {sessionId}")
			.WithRequest(HttpMethod.Get, "/posts")
			.WithHeader("session-id", sessionId)
			.WillRespond()
			.WithStatus(HttpStatusCode.OK)
			// This specifies the response body. It must have a field called posts.
			// MinType means that it must be an array of a particular type with length of at least 1.
			// post-id-1 is an example entry, which we can then check for below. It also lets the type matcher infer that posts needs to be an array of strings.
			.WithJsonBody(new
			{
				posts = Match.MinType("post-id-1", 1)
			});

		await _pact.VerifyAsync(async ctx =>
		{
			using PostClient postClient = new(ctx.MockServerUri);
			var result = await postClient.GetAllPosts(sessionId);

			var sucResult = Assert.IsType<SuccessResult<PostClient.GetAllPostsResponse>>(result);
			// Test that we have read the response body correctly.
			// We can check for post-id-1 exactly because that is what we stated above would be the example response.
			// Remember that this part here only is for the Consumer; this part has no influence on the final Contract.
			Assert.NotNull(sucResult.Content);
			Assert.Equal(["post-id-1"], sucResult.Content.Posts);
		});
	}

	[Fact]
	[Trait("Category", "Contract")]
	[Trait("ContractRole", "Consumer")]
	public async Task GetAllPosts_IsNotLoggedIn_GivesUnauthorizedError()
	{
		string sessionId = "invalid-token";

		_pact
			.UponReceiving("a request for all posts with invalid session id")
			.Given($"no user exists with session id: {sessionId}")
			.WithRequest(HttpMethod.Get, "/posts")
			.WithHeader("session-id", sessionId)
			.WillRespond()
			.WithStatus(HttpStatusCode.Unauthorized);

		await _pact.VerifyAsync(async ctx =>
		{
			using PostClient postClient = new(ctx.MockServerUri);
			var result = await postClient.GetAllPosts(sessionId);

			Assert.IsType<UnauthorizedErrorResult>(result);
		});
	}

	[Fact]
	[Trait("Category", "Contract")]
	[Trait("ContractRole", "Consumer")]
	public async Task CreateNewPost_IsLoggedIn_ReturnsNewId()
	{
		string sessionId = "valid-token";
		string title = "A Post Title";
		string content = "Post content";

		_pact
			.UponReceiving("a request to create a new post")
			.Given($"a user is logged in with session id: {sessionId}")
			.WithRequest(HttpMethod.Post, "/posts")
			.WithHeader("session-id", sessionId)
			.WithJsonBody(new{
				title = title,
				content = content
			})
			.WillRespond()
			.WithStatus(HttpStatusCode.Created)
			// This specifies the response body. It must have a field called id of type string.
			// It must also have fields called title and content, and they must have exactly the same value as in the request body.
			// We can do this because they are controlled by us, the Consumer.
			// The reason we add this to the Contract is to ensure that the Provider actually recognises the body we sent in the request.
			.WithJsonBody(new
			{
				id = Match.Type("new-post-id"),
				title = title,
				content = content
			});

		await _pact.VerifyAsync(async ctx =>
		{
			using PostClient postClient = new(ctx.MockServerUri);
			var result = await postClient.CreateNewPost(sessionId, title, content);

			var sucResult = Assert.IsType<SuccessResult<PostClient.CreateNewPostResponse>>(result);
			Assert.NotNull(sucResult.Content);
			Assert.Equal("new-post-id", sucResult.Content.Id);
		});
	}
	
	[Fact]
	[Trait("Category", "Contract")]
	[Trait("ContractRole", "Consumer")]
	public async Task GetPostDetails_PostExists_ReturnsDetails()
	{
		string postId = "some-post-id";
		string exampleTitle = "Some Title";
		string exampleContent = "Some Content";
		
		_pact
			.UponReceiving("a request for an existing post")
			.Given($"a post exists with id: {postId}")
			.WithRequest(HttpMethod.Get, $"/posts/{postId}")
			.WillRespond()
			.WithStatus(HttpStatusCode.OK)
			.WithJsonBody(new
			{
				title = Match.Type(exampleTitle),
				content = Match.Type(exampleContent)
			});

		await _pact.VerifyAsync(async ctx =>
		{
			using PostClient postClient = new(ctx.MockServerUri);
			var result = await postClient.GetPostDetails(postId);

			var sucResult = Assert.IsType<SuccessResult<PostClient.GetPostDetailsResponse>>(result);
			// Test that we have read the response body correctly.
			// We can check for post-id-1 exactly because that is what we stated above would be the example response.
			// Remember that this part here only is for the Consumer; this part has no influence on the final Contract
			Assert.NotNull(sucResult.Content);
			Assert.Equal(exampleTitle, sucResult.Content.Title);
			Assert.Equal(exampleContent, sucResult.Content.Content);
		});
	}
}