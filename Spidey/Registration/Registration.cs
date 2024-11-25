/*
Copyright 2017 James Craig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using BigBook.Registration;
using Canister.Interfaces;
using Microsoft.IO;
using Spidey;
using Spidey.Engines.Interfaces;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Registration extension methods
    /// </summary>
    public static class RegistrationExtension
    {
        /// <summary>
        /// Registers the library with the bootstrapper.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <returns>The bootstrapper</returns>
        public static ICanisterConfiguration? RegisterSpidey(this ICanisterConfiguration? bootstrapper)
        {
            return bootstrapper?.AddAssembly(typeof(RegistrationExtension).Assembly)
                               .RegisterFileCurator()
                               .RegisterBigBookOfDataTypes();
        }

        /// <summary>
        /// Registers the Spidey services with the specified service collection.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection? RegisterSpidey(this IServiceCollection? services)
        {
            if (services.Exists<Crawler>())
                return services;
            return services?.AddTransient<Crawler>()
                    ?.AddAllTransient<ILinkDiscoverer>()
                    ?.AddAllTransient<IEngine>()
                    ?.AddAllTransient<IContentParser>()
                    ?.AddAllTransient<IProcessor>()
                    ?.AddAllTransient<IPipeline>()
                    ?.AddAllTransient<IScheduler>()
                    ?.AddTransient(_ => Options.Default)
                    ?.AddSingleton<RecyclableMemoryStreamManager>()
                    ?.RegisterFileCurator()
                    ?.RegisterBigBookOfDataTypes();
        }
    }
}