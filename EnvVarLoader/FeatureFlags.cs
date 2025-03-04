using System;

namespace Shared.EnvVarLoader
{
    public static class FeatureFlags
    {
        public static bool EVENTS_ON = Env.GetBoolEnvVar(nameof(FeatureFlags.EVENTS_ON));
        public static bool DB_SAVING_LOGS_ON = Env.GetBoolEnvVar(
            nameof(FeatureFlags.DB_SAVING_LOGS_ON)
        );
        public static bool DB_TRIGGERS_ON = Env.GetBoolEnvVar(nameof(FeatureFlags.DB_TRIGGERS_ON));
        public static bool ROUTINES_ON = Env.GetBoolEnvVar(nameof(FeatureFlags.ROUTINES_ON));
        public static bool QUEUE_ROUTINES_ON = Env.GetBoolEnvVar(
            nameof(FeatureFlags.QUEUE_ROUTINES_ON)
        );
    }
}
