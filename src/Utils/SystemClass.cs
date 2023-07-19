namespace Autodesk.Services.Tandem.Utils
{
    internal class SystemClass
    {
        private readonly static string[] SystemClassNames =
        {
            "Supply Air", //0
	        "Return Air", //1
	        "Exhaust Air", //2
	        "Hydronic Supply", //3
	        "Hydronic Return", //4
	        "Domestic Hot Water", //5
	        "Domestic Cold Water", //6
	        "Sanitary", //7
	        "Power", //8
	        "Vent", //9
	        "Controls", //10
	        "Fire Protection Wet", //11
	        "Fire Protection Dry", //12
	        "Fire Protection Pre-Action", //13
	        "Other Air", //14
	        "Other", //15
	        "Fire Protection Other", //16
	        "Communication", //17
	        "Data Circuit", //18
	        "Telephone", //19
	        "Security", //20
	        "Fire Alarm", //21
	        "Nurse Call", //22
	        "Switch Topology", //23
	        "Cable Tray Conduit", //24
	        "Storm", //25
        };

        public static string[] ToList(int flags)
        {
			var names = new List<string>();

            for (var i = 0; i < SystemClassNames.Length; i++)
            {
                if ((flags & (1 << i)) > 0)
                {
                    names.Add(SystemClassNames[i]);
                }
            }
            names.Sort();
			return names.ToArray();
        }

        public static string ToString(int flags)
		{
			return string.Join(",", ToList(flags));
		}
    }
}
