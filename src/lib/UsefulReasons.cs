using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.Json;

namespace Very_Useful_Reasons;

public static class UsefulReasons
{
	private const string Url = "https://bofh-api.bombeck.io/v1/excuses/all";

	private static ImmutableArray<Reason> reasons = [];
	private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);
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

	public static async Task<string> GetReason(ILogger? logger = null, CancellationToken cancellationToken = default)
	{
		await EnsureReasonsLoaded(logger, cancellationToken);

		if (reasons.IsDefaultOrEmpty)
		{
			return string.Empty;
		}

		int index = Random.Shared.Next(0, reasons.Length);
		string quote = reasons[index].Quote;

		return string.IsNullOrWhiteSpace(quote) ? string.Empty : quote;
	}

	private static async Task EnsureReasonsLoaded(ILogger? logger, CancellationToken cancellationToken)
	{
		if (!reasons.IsDefaultOrEmpty)
		{
			return;
		}

		try
		{
			await SemaphoreSlim.WaitAsync(cancellationToken);

			if (reasons.IsDefaultOrEmpty)
			{
				ImmutableArray<Reason> loaded = await FetchAllReasons(logger, cancellationToken);

				if (!loaded.IsDefault)
				{
					reasons = loaded;
				}
			}

		}
		finally
		{
			SemaphoreSlim.Release();
		}
	}

	private static async Task<ImmutableArray<Reason>> FetchAllReasons(
		ILogger? logger,
		CancellationToken cancellationToken)
	{
		try
		{
			using HttpResponseMessage response = await Client.GetAsync(Url, cancellationToken);
			response.EnsureSuccessStatusCode();

			await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

			RawResponse[]? data = await JsonSerializer.DeserializeAsync<RawResponse[]>(
				stream,
				serializerOptions,
				cancellationToken);

			if (data?.Length <= 0)
			{
				return [];
			}

			ImmutableArray<Reason>? reasons = data?.Select(Reason.TryMap).OfType<Reason>().ToImmutableArray();

			return reasons ?? [];
		}
		catch (Exception e)
		{
			logger?.LogWarning(e, "Something went wrong!");
			logger?.LogError(e, "{message}", e.Message);

			return [];
		}
	}
}
