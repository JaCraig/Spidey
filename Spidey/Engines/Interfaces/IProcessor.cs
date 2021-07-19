namespace Spidey.Engines.Interfaces
{
    /// <summary>
    /// Processor interface
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Processes the item found.
        /// </summary>
        /// <param name="resultFile">The result file.</param>
        void Process(ResultFile resultFile);
    }
}