using System;
using System.Collections.Generic;

namespace UWED.Platform
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        public static void Register<TService>(TService implementation)
        {
            var type = typeof(TService);

            if (!services.TryAdd(type, implementation))
            {
                services[type] = implementation;
            }
        }

        public static TService Get<TService>()
        {
            if (services.TryGetValue(typeof(TService), out var implementation))
            {
                return (TService)implementation;
            }

            throw new InvalidOperationException(
                $"No implementation registered for {typeof(TService).Name}.");
        }

        public static bool TryGet<TService>(out TService implementation)
        {
            if (services.TryGetValue(typeof(TService), out var raw))
            {
                implementation = (TService)raw;
                return true;
            }

            implementation = default;
            return false;
        }

        public static void Clear()
        {
            services.Clear();
        }
    }
}
