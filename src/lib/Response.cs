namespace Very_Useful_Reasons;

internal sealed class Response
{
	public int Id { get; set; }

	public required string Quote { get; set; }

	public required string Source { get; set; }

	public required DateTime Date { get; set; }
}
