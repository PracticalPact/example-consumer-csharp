namespace ForumFrontend;

public abstract record ClientResult;

public record SuccessResult<TContent>(TContent content) : ClientResult
{
	public override string ToString()
	{
		return content.ToString();
	}
}

public record UnauthorizedErrorResult : ClientResult
{
	public override string ToString()
	{
		return $"An authorization error occured. Try logging in again.\n";
	}
}

public record ServerErrorResult(string message) : ClientResult
{
	public override string ToString()
	{
		return $"A server error occured: {message}.\n";
	}
}
