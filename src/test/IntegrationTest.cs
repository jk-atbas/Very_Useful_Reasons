using Very_Useful_Reasons;

namespace Very_Useful_Reasons_Test;

public class IntegrationTest
{
	[Fact]
	public async Task ShouldReturnReason()
	{
		string reason = await UsefulReasons.GetReason();

		Assert.True(!string.IsNullOrEmpty(reason));
	}
}
