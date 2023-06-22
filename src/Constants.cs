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
        public const string ElementFlags        = "a";
        public const string CategoryId          = "c";
        public const string Name                = "n";
        public const string Level               = "l";
        public const string LevelOverride       = "!l";
        public const string SystemClass         = "b";
        public const string SystemClassOverride = "!b";
        public const string Rooms               = "r";
    }

    public static class ElementFlags
    {
        public const long Room       = 0x00000005;
        public const long FamilyType = 0x01000000;
        public const long Level      = 0x01000001;
    }

    public static class QualifiedColumns
    {
        public const string ElementFlags           = "n:a";
        public const string CategoryId             = "n:c";
        public const string Name                   = "n:n";
        public const string SystemClass            = "n:b";
        public const string SystemClassOverride    = "n:!b";
        public const string Level                  = "l:l";
        public const string Room                   = "l:r";
        public const string XRoom                  = "x:r";
    }

    public static class Prefixes
    {
        public const string Model = "urn:adsk.dtm:";
    }
}
