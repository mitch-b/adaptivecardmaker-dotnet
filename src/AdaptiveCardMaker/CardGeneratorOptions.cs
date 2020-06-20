using System.Reflection;

namespace AdaptiveCardMaker
{
    public class CardGeneratorOptions
    {
        /// <summary>
        /// Assembly to make the GetManifestResourceStream() call from. The assembly used 
        /// must match the project which has the embedded card JSON files.
        /// </summary>
        public Assembly AssemblyWithEmbeddedResources { get; set; } = Assembly.GetEntryAssembly();
        /// <summary>
        /// Folder Path in which the card JSON files can be found.
        /// Used in deriving path to embedded resources via Assembly.GetManifestResourceStream().
        /// </summary>
        public string ManifestResourcePathFromNamespace { get; set; } = "Cards";
        /// <summary>
        /// This value should match the project's namespace which has the embedded JSON files.
        /// Used in deriving path to embedded resources via Assembly.GetManifestResourceStream().
        /// </summary>
        public string ProjectNamespace { get; set; }

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
