using System.Text.Json;

namespace Very_Useful_Reasons;

internal sealed record Reason(int Id, string Quote, string Source, DateOnly Date)
{
	public static Reason? TryMap(RawResponse response)
	{
		int id = response.Id.ValueKind switch
		{
			JsonValueKind.Number => response.Id.TryGetInt32(out int value) ? value : 0,
			JsonValueKind.String => int.TryParse(response.Id.GetString(), out int value) ? value : 0,
			_ => 0,
		};

		return id <= 0
			? null
			: string.IsNullOrWhiteSpace(response.Quote)
				? null
				: !DateOnly.TryParse(response.Date, out var date)
					? null
					: new Reason(id, response.Quote, response.Source, date);
	}
}
