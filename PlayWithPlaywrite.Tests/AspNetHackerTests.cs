namespace PlayWithPlaywrite.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class AspNetHackerTests : PageTest
{
    [Test]
    public async Task MyTest()
    {
        await Page.GotoAsync("https://asp.net-hacker.rocks/");

        await Page.GetByRole(AriaRole.Link, new() { Name = "I accept" }).ClickAsync();
        await Expect(Page).ToHaveTitleAsync(new Regex("ASP.NET Hacker"));

        var archive = Page.GetByRole(AriaRole.Link, new() { Name = "archive", Exact = true });
        await Expect(archive).ToHaveAttributeAsync("href", "/archive.html");
        await archive.ClickAsync();
        await Expect(Page).ToHaveTitleAsync(new Regex("ASP.NET Hacker"));

        var about = Page.GetByRole(AriaRole.Link, new() { Name = "about", Exact = true });
        await Expect(about).ToHaveAttributeAsync("href", "/about.html");
        await about.ClickAsync();
        await Expect(Page).ToHaveTitleAsync(new Regex("About me and about this blog"));

        var privacy = Page.GetByRole(AriaRole.Link, new() { Name = "privacy" });
        await Expect(privacy).ToHaveAttributeAsync("href", "/privacy.html");
        await privacy.ClickAsync();
        await Expect(Page).ToHaveTitleAsync(new Regex("This blog's privacy policy"));

        Assert.Pass();
    }
}
