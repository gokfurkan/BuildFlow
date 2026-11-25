namespace FurkanGok.BuildFlow
{
    /// <summary>
    /// BuildFlow target platforms. Steam is a distribution destination, not a build target.
    /// </summary>
    public enum BuildFlowTargetPlatform
    {
        WindowsStandaloneX64 = 0,
        AndroidApk = 1,
        AndroidAab = 2,
        IosXcodeProject = 3
    }

    /// <summary>
    /// High-level build configuration variant.
    /// </summary>
    public enum BuildFlowBuildConfiguration
    {
        Development = 0,
        Release = 1
    }

    /// <summary>
    /// Compression method applied where the platform supports it.
    /// </summary>
    public enum BuildFlowCompressionMethod
    {
        Default = 0,
        None = 1,
        Lz4 = 2,
        Lz4HC = 3
    }

    /// <summary>
    /// Outcome of a BuildFlow execution based on Unity BuildReport.
    /// </summary>
    public enum BuildFlowResultStatus
    {
        Unknown = 0,
        Succeeded = 1,
        Failed = 2,
        Cancelled = 3,
        ValidationBlocked = 4
    }

    /// <summary>
    /// Severity for validation rules.
    /// </summary>
    public enum ValidationSeverity
    {
        Information = 0,
        Warning = 1,
        Error = 2
    }
}
