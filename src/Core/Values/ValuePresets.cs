namespace dotMigrata.Core.Values;

/// <summary>
/// Provides preset values for common scenarios to improve code readability and consistency.
/// These presets help prevent "zero migration" issues by providing well-tested value combinations.
/// </summary>
public static class ValuePresets
{
    /// <summary>
    /// Normalized value presets for common ratios.
    /// </summary>
    public static class Normalized
    {
        /// <summary>Zero (0.0)</summary>
        public static NormalizedValue Zero => NormalizedValue.Zero;

        /// <summary>Quarter (0.25)</summary>
        public static readonly NormalizedValue Quarter = NormalizedValue.FromRatio(0.25);

        /// <summary>Half (0.5)</summary>
        public static NormalizedValue Half => NormalizedValue.Half;

        /// <summary>Three quarters (0.75)</summary>
        public static readonly NormalizedValue ThreeQuarters = NormalizedValue.FromRatio(0.75);

        /// <summary>Full (1.0)</summary>
        public static NormalizedValue Full => NormalizedValue.One;

        /// <summary>Low willingness (0.3)</summary>
        public static readonly NormalizedValue Low = NormalizedValue.FromRatio(0.3);

        /// <summary>Medium willingness (0.5)</summary>
        public static NormalizedValue Medium => NormalizedValue.Half;

        /// <summary>High willingness (0.7)</summary>
        public static readonly NormalizedValue High = NormalizedValue.FromRatio(0.7);

        /// <summary>Very high willingness (0.9)</summary>
        public static readonly NormalizedValue VeryHigh = NormalizedValue.FromRatio(0.9);
    }

    /// <summary>
    /// Sensitivity value presets for common sensitivities.
    /// </summary>
    public static class Sensitivity
    {
        /// <summary>Neutral sensitivity (0.0)</summary>
        public static SensitivityValue Neutral => SensitivityValue.Zero;

        /// <summary>Low positive sensitivity (2.0)</summary>
        public static readonly SensitivityValue LowPositive = SensitivityValue.FromRaw(2.0);

        /// <summary>Medium positive sensitivity (5.0)</summary>
        public static readonly SensitivityValue MediumPositive = SensitivityValue.FromRaw(5.0);

        /// <summary>High positive sensitivity (8.0)</summary>
        public static readonly SensitivityValue HighPositive = SensitivityValue.FromRaw(8.0);

        /// <summary>Low negative sensitivity (-2.0)</summary>
        public static readonly SensitivityValue LowNegative = SensitivityValue.FromRaw(-2.0);

        /// <summary>Medium negative sensitivity (-5.0)</summary>
        public static readonly SensitivityValue MediumNegative = SensitivityValue.FromRaw(-5.0);

        /// <summary>High negative sensitivity (-8.0)</summary>
        public static readonly SensitivityValue HighNegative = SensitivityValue.FromRaw(-8.0);
    }

    /// <summary>
    /// Intensity value presets for common values.
    /// </summary>
    public static class Intensity
    {
        /// <summary>Zero intensity (0.0)</summary>
        public static IntensityValue Zero => IntensityValue.Zero;

        /// <summary>Unit intensity (1.0)</summary>
        public static IntensityValue One => IntensityValue.One;
    }
    
    /// <summary>
    /// Common person attribute presets to prevent zero migration scenarios.
    /// </summary>
    public static class PersonAttributes
    {
        /// <summary>
        /// Preset for a highly mobile person (high moving willingness, low retention).
        /// Use this to ensure active migration in the simulation.
        /// </summary>
        public static class HighlyMobile
        {
            /// <summary>Moving willingness for highly mobile persons (0.8).</summary>
            public static readonly NormalizedValue MovingWillingness = NormalizedValue.FromRatio(0.8);

            /// <summary>Retention rate for highly mobile persons (0.2).</summary>
            public static readonly NormalizedValue RetentionRate = NormalizedValue.FromRatio(0.2);
        }

        /// <summary>
        /// Preset for a moderately mobile person (medium values).
        /// Use this for balanced migration behavior.
        /// </summary>
        public static class ModeratelyMobile
        {
            /// <summary>Moving willingness for moderately mobile persons (0.5).</summary>
            public static NormalizedValue MovingWillingness => NormalizedValue.Half;

            /// <summary>Retention rate for moderately mobile persons (0.5).</summary>
            public static NormalizedValue RetentionRate => NormalizedValue.Half;
        }

        /// <summary>
        /// Preset for a settled person (low moving willingness, high retention).
        /// Use this for populations that are resistant to migration.
        /// </summary>
        public static class Settled
        {
            /// <summary>Moving willingness for settled persons (0.2).</summary>
            public static readonly NormalizedValue MovingWillingness = NormalizedValue.FromRatio(0.2);

            /// <summary>Retention rate for settled persons (0.8).</summary>
            public static readonly NormalizedValue RetentionRate = NormalizedValue.FromRatio(0.8);
        }
    }
}