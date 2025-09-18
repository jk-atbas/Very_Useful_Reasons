using System.Collections.Immutable;
using System.Text.Json;

namespace Very_Useful_Reasons;

public static class UsefulReasons
{
	private const string Url = "https://bofh-api.bombeck.io/v1/excuses/all";

	private static ImmutableArray<Response> reasons = [];
	private static readonly SemaphoreSlim Gate = new(1, 1);
	private static readonly HttpClient Client = new()
	{
		Timeout = TimeSpan.FromSeconds(30),
	};

	private static readonly JsonSerializerOptions serializerOptions = new()
	{
		PropertyNameCaseInsensitive = true,
	};

	static UsefulReasons()
	{
		Client.DefaultRequestHeaders.UserAgent.ParseAdd($"Useful_Reasons_Library_{Environment.Version}");
	}

	public static async Task<string> GetReason(CancellationToken cancellationToken = default)
	{
		await EnsureReasonsLoaded(cancellationToken);

		if (reasons.IsDefaultOrEmpty)
		{
			return string.Empty;
		}

		int index = Random.Shared.Next(0, reasons.Length);
		string quote = reasons[index].Quote;

		return string.IsNullOrWhiteSpace(quote) ? string.Empty : quote;
	}

	private static async Task EnsureReasonsLoaded(CancellationToken cancellationToken)
	{
		if (!reasons.IsDefaultOrEmpty)
		{
			return;
		}

		await Gate.WaitAsync(cancellationToken);

		if (reasons.IsDefaultOrEmpty)
		{
			ImmutableArray<Response> loaded = await FetchAllReasons(cancellationToken);

			if (!loaded.IsDefault)
			{
				reasons = loaded;
			}
		}
	}

	private static async Task<ImmutableArray<Response>> FetchAllReasons(CancellationToken cancellationToken)
	{
		try
		{
			using HttpResponseMessage response = await Client.GetAsync(Url, cancellationToken);
			response.EnsureSuccessStatusCode();

			await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

			Response[]? data = await JsonSerializer.DeserializeAsync<Response[]>(
				stream,
				serializerOptions,
				cancellationToken);

			return data is { Length: > 0 }
				? ImmutableArray.Create(data)
				: [];
		}
		catch (Exception)
		{
			return [];
		}
	}
}
