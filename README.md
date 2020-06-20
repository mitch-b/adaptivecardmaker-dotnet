# AdaptiveCardMaker

[![Nuget install](https://img.shields.io/nuget/vpre/AdaptiveCardMaker.svg)](https://www.nuget.org/packages/AdaptiveCardMaker) ![nuget release](https://github.com/mitch-b/adaptivecardmaker-dotnet/workflows/nuget%20release/badge.svg?branch=master)

Package to help simplify templating language/generation of Adaptive Cards from .NET.

See:
* https://adaptivecards.io
* [Adaptive Cards Designer](https://adaptivecards.io/designer/) (easily create your JSON templates here)
* [Adaptive Cards Templating documentation](https://docs.microsoft.com/en-us/adaptive-cards/templating/)

## Getting Started

Add package to your project:

```pwsh
dotnet add package AdaptiveCardMaker
```

This library will pull JSON files as embedded resources from your assembly manifest. 

In your `.csproj`, ensure your JSON files are listed as an embedded resource. 

```xml
  <ItemGroup>
    <EmbeddedResource Include="Cards\welcomeCard.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </EmbeddedResource>
  </ItemGroup>
```

This 
can also be verified in Visual Studio by looking at Properties pane and ensuring 
the following is true:

| Property                 | Value                        |
| ------------------------ | ---------------------------- |
| Build Action             | Embedded resource            |
| Copy to Output Directory | Copy if newer (or always)    |

## Usage

If you do not need to pull resources across multiple assemblies, follow **Simple** guidance. 
Otherwise, check out the **Advanced** section below. 

### Simple

The simple instructions assume your Adaptive Card JSON files are configured to be 
embedded resources within your primary executing project (not from a shared library). 
For example, if your application is a Web app (like it would for a Bot Framework app), 
the assumption is you would have your cards in a folder named `Cards` directly in 
your web app project. 

Then, in your service registration (typically `Startup.cs`), add this library:

```csharp
namespace Sample.Bot
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAdaptiveCardGenerator();
            // ...
        }
        // ...
    }
}

```

Now, you can start using the CardGenerator from your Dialog/other classes. Here's an example 
within a Dialog of a Bot Skill:

```csharp
namespace Sample.Bot.Dialogs
{
    public class WelcomeDialog : SkillDialogBase
    {
        private readonly ICardGenerator _cardGenerator;
        public WelcomeDialog(
            IServiceProvider serviceProvider,
            ICardGenerator cardGenerator)
            : base(nameof(WelcomeDialog), serviceProvider)
        {
            _cardGenerator = cardGenerator;
            // ...
        }

        private async Task<DialogTurnResult> DisplayWelcomeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardData = new
            {
                name = "Jane"
            };
            var responseCard = await this._cardGenerator.CreateAdaptiveCardAttachmentAsync("welcomeCard", cardData);
            var response = MessageFactory.Attachment(responseCard);
            await stepContext.Context.SendActivityAsync(response);
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }
        // ...
    }
}
```

To override defaults:

```csharp
services.AddAdaptiveCardGenerator(options =>
{
    // folder containing JSON files from root of project
    options.RelativeOrderedPathToCards = new[] { "Cards" };
});
```

See the [Test projects](https://github.com/mitch-b/adaptivecardmaker-dotnet/tree/master/src/Tests) for examples.

### Advanced

If your JSON assets are served by a referenced project (not same assembly as your web bot, for example), 
you will want to ensure you can pass the Assembly via reflection to the service registration. It 
supports a generic type in registration so you can register as many instances of the AdaptiveCardMaker 
as you need.

In this example, say the referenced library (`TestCardConsumer.CardLibrary`) containing 
embedded resources had a simple class we can reference:

```csharp
namespace TestCardConsumer.CardLibrary
{
    // can be completely empty, just used with reflection to get Assembly by type
    public class ImportedCardLibrary
    {
    }
}

```

Then, in our service registration, we use the typed service registration method:

```csharp
services.AddAdaptiveCardGenerator<ImportedCardLibrary>(options =>
{
    options.AssemblyWithEmbeddedResources = Assembly.GetAssembly(typeof(ImportedCardLibrary));
    options.RelativeOrderedPathToCards = new[] { "MyCards" };
});
```

Then when you need this service injected, use the typed interface:

```csharp
// ...
namespace Sample.Bot.Dialogs
{
    public class WelcomeDialog : SkillDialogBase
    {
        private readonly ICardGenerator<ImportedCardLibrary> _cardGenerator;
        public WelcomeDialog(
            IServiceProvider serviceProvider,
            ICardGenerator<ImportedCardLibrary> cardGenerator)
            : base(nameof(WelcomeDialog), serviceProvider)
        {
            _cardGenerator = cardGenerator;
            // ...
        }

        private async Task<DialogTurnResult> DisplayWelcomeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardData = new
            {
                name = "Jane"
            };
            var responseCard = await this._cardGenerator.CreateAdaptiveCardAttachmentAsync("welcomeCard", cardData);
            var response = MessageFactory.Attachment(responseCard);
            await stepContext.Context.SendActivityAsync(response);
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }
        // ...
    }
}
```
