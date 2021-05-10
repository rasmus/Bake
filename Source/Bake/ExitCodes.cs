namespace Bake
{
    public static class ExitCodes
    {
        public static class Core
        {
            public const int UnexpectedError = -256;
        }

        public static class Plan
        {
            public const int PlanFileAlreadyExists = -1;
        }

        public static class Apply
        {
            public const int PlanFileNotFound = -2;
        }
    }
}
