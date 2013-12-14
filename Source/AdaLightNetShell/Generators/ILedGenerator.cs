using System;

namespace AdaLightNetShell.Generators
{
    /// <summary>
    /// Generate data for led array
    /// </summary>
    public interface ILedGenerator : IDisposable
    {
        /// <summary>
        /// Initialize all shared and used variables in the generator here.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Generate data in led array
        /// </summary>
        /// <param name="ledArray"></param>
        bool Generate(byte[] ledArray);
    }
}