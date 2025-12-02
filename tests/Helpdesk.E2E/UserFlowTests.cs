using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Helpdesk.E2E;

public class userFlowTests : PageTest
{
    private const string baseURL= "http://localhost:5173";

    [Test]
    public async Task Admin_Should_Be_Able_To_Login()
    {
        await Page.GotoAsync($"{baseURL}/login");

        await Page.GetByLabel("Email").FillAsync("admin@helpdesk.com");
        await Page.GetByLabel("Hasło").FillAsync("Admin123!");


        await Page.GetByRole(AriaRole.Button, new () { Name = "Zaloguj się"}).ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{baseURL}/admin");

        await Expect(Page.GetByText("Panel Administratora").First).ToBeVisibleAsync();
    }
    [Test]

    public async Task Customer_Should_Create_Ticket_And_See_It_On_List()
    {
        await Page.GotoAsync($"{baseURL}/login");
        await Page.GetByLabel("Email").FillAsync("customer@helpdesk.com");
        await Page.GetByLabel("Hasło").FillAsync("Customer123!");
        await Page.GetByRole(AriaRole.Button, new () {Name = "Zaloguj się"}).ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{baseURL}/dashboard");


        await Page.GetByRole(AriaRole.Button, new () {Name = "Zgłoszenia"}).ClickAsync();
        await Expect(Page).ToHaveURLAsync($"{baseURL}/tickets");

        await Page.GetByRole(AriaRole.Button, new () {Name = "+ Nowe zgłoszenie"}).ClickAsync();


        var uniqueTitle = $"Awaria E2E {Guid.NewGuid()}";

        await Page.GetByLabel("Tytuł Zgłoszenia").FillAsync(uniqueTitle);
        await Page.GetByLabel("Opis Problemu").FillAsync("Problemik");

        await Page.Locator("button[role = 'combobox']").First.ClickAsync();
        await Page.GetByLabel("Wysoki").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new () {Name = "Utwórz zgłoszenie"}).ClickAsync();

        await Expect(Page).ToHaveURLAsync($"{baseURL}/tickets");


        await Expect(Page.GetByText(uniqueTitle)).ToBeVisibleAsync();
    }

}