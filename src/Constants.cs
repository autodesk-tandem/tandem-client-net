namespace TandemSDK
{
    public static class ColumnFamilies
    {
        public const string Status          = "s";
        public const string Attributes      = "p";
        public const string AttributeHashes = "h";
        public const string AccessControl   = "c";
        public const string LMV             = "0";
        public const string Standard        = "n";
        public const string Refs            = "l";
        public const string Xrefs           = "x";
        public const string Source          = "r";
        public const string DtProperties    = "z";
        public const string Tags            = "t";
        public const string UserInfo        = "u";
        public const string ChangeInfo      = "c";
        public const string Virtual         = "v";
    }

    public static class ColumnNames
    {
        public const string Key                    = "k";
        public const string CategoryId             = "c";
        public const string Classification         = "v";
        public const string ClassificationOverride = "!v";
        public const string ElementFlags           = "a";
        public const string Name                   = "n";
        public const string Level                  = "l";
        public const string LevelOverride          = "!l";
        public const string Rooms                  = "r";
        public const string SystemClass            = "b";
        public const string SystemClassOverride    = "!b";
        public const string UniformatClass         = "u";
        public const string UniformatClassOverride = "!u";
    }

    public static class ElementFlags
    {
        public const long Room       = 0x00000005;
        public const long FamilyType = 0x01000000;
        public const long Level      = 0x01000001;
    }

    public static class QualifiedColumns
    {
        public const string ElementFlags           = $"{ColumnFamilies.Standard}:{ColumnNames.ElementFlags}";
        public const string CategoryId             = $"{ColumnFamilies.Standard}:{ColumnNames.CategoryId}";
        public const string Name                   = $"{ColumnFamilies.Standard}:{ColumnNames.Name}";
        public const string SystemClass            = $"{ColumnFamilies.Standard}:{ColumnNames.SystemClass}";
        public const string SystemClassOverride    = $"{ColumnFamilies.Standard}:{ColumnNames.SystemClassOverride}";
        public const string Classification         = $"{ColumnFamilies.Standard}:{ColumnNames.Classification}";
        public const string ClassificationOverride = $"{ColumnFamilies.Standard}:{ColumnNames.ClassificationOverride}";
        public const string UniformatClass         = $"{ColumnFamilies.Standard}:{ColumnNames.UniformatClass}";
        public const string UniformatClassOverride = $"{ColumnFamilies.Standard}:{ColumnNames.UniformatClassOverride}";
        public const string Level                  = $"{ColumnFamilies.Refs}:{ColumnNames.Level}";
        public const string LevelOverride          = $"{ColumnFamilies.Refs}:{ColumnNames.LevelOverride}";
        public const string Room                   = $"{ColumnFamilies.Refs}:{ColumnNames.Rooms}";
        public const string XRoom                  = $"{ColumnFamilies.Xrefs}:{ColumnNames.Rooms}";
    }

    public static class Prefixes
    {
        public const string Model = "urn:adsk.dtm:";
    }
}
