using System.Text.Json;

namespace Very_Useful_Reasons;

internal sealed class RawResponse
{
	public JsonElement Id { get; set; }

	public string Quote { get; set; } = string.Empty;

	public string Source { get; set; } = string.Empty;

	public string? Date { get; set; }
}