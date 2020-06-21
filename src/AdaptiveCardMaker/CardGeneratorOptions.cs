using System.Collections.Generic;
using System.Reflection;

namespace AdaptiveCardMaker
{
    /// <summary>
    /// Options needed by ICardGenerator to access the appropriate AdaptiveCard files embedded in your assembly.
    /// </summary>
    public class CardGeneratorOptions
    {
        /// <summary>
        /// Assembly to pull Adaptive Card JSON embedded resources from.
        /// </summary>
        public Assembly AssemblyWithEmbeddedResources { get; set; } = Assembly.GetEntryAssembly();
        /// <summary>
        /// Folder Path in which the card JSON files can be found.
        /// Used in deriving path to embedded resources from assmebly.
        /// Default is [ "Cards" ]
        /// </summary>
        public IEnumerable<string> RelativeOrderedPathToCards { get; set; } = new List<string>() { "Cards" };

        internal string ManifestResourceRoot { get; set; }
    }

    /// <summary>
    /// Generically typed Options to support multiple DI registration. See implementation in <see cref="CardGeneratorOptions"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CardGeneratorOptions<T> : CardGeneratorOptions
    {
    }
}
