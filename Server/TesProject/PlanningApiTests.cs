using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace TesProject;

public sealed class PlanningApiTests(
    ApiFactory factory
) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task RulesEndpointRequiresAdmin()
    {
        using var client = factory.CreateClient();

        Assert.Equal(
            HttpStatusCode.Unauthorized,

            (
                await client.GetAsync(
                    "/api/v1/planning/rules"
                )
            ).StatusCode
        );

        await Login(
            client,
            ApiFactory.TeacherEmail
        );

        Assert.Equal(
            HttpStatusCode.Forbidden,

            (
                await client.GetAsync(
                    "/api/v1/planning/rules"
                )
            ).StatusCode
        );

        await Login(
            client,
            ApiFactory.PlannerEmail
        );

        var rules = await client.GetFromJsonAsync<JsonElement>(
            "/api/v1/planning/rules"
        );

        Assert.Equal(
            10,
            rules.GetArrayLength()
        );
    }

    private static async Task Login(
        HttpClient client,
        string email
    )
    {
        client.DefaultRequestHeaders.Authorization = null;

        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login",

            new
            {
                email,
                password = ApiFactory.Password
            }
        );

        login.EnsureSuccessStatusCode();

        var body =
            await login.Content.ReadFromJsonAsync<JsonElement>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",

                body.GetProperty("accessToken")
                    .GetString()
            );
    }
}