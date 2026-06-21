namespace ForumFrontend;

public abstract record ClientResult;

public record SuccessResult<TContent>(TContent Content) : ClientResult
{
	public override string ToString()
	{
		return Content.ToString();
	}
}

public record UnauthorizedErrorResult : ClientResult
{
	public override string ToString()
	{
		return $"An authorization error occured. Try logging in again.\n";
	}
}

public record ServerErrorResult(string Message) : ClientResult
{
	public override string ToString()
	{
		return $"A server error occured: {Message}.\n";
	}
}
