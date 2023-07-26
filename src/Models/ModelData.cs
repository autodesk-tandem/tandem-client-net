using Newtonsoft.Json;

namespace Autodesk.Tandem.Client.Models
{
    public class ModelData
    {
        public class Camera
        {
            [JsonProperty("aspect")]
            public double Aspect { get; set; }

            [JsonProperty("fov")]
            public long Fov { get; set; }

            [JsonProperty("isPerspective")]
            public bool IsPerspective { get; set; }

            [JsonProperty("orthoScale")]
            public double OrthoScale { get; set; }

            [JsonProperty("position")]
            public XYZ Position { get; set; }

            [JsonProperty("target")]
            public XYZ Target { get; set; }

            [JsonProperty("up")]
            public XYZ Up { get; set; }
        }

        public class CustomValues
        {
            [JsonProperty("angleToTrueNorth")]
            public double AngleToTrueNorth { get; set; }

            [JsonProperty("refPointTransform")]
            public double[] RefPointTransform { get; set; }
        }

        public class DefaultCamera
        {
            [JsonProperty("index")]
            public long Index { get; set; }
        }

        public class DisplayUnit
        {
            [JsonProperty("value")]
            public string Value { get; set; }
        }

        public class XYZ
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
        }

        [JsonProperty("cameras")]
        public Camera[] Cameras { get; set; }

        //[JsonProperty("custom values")]
        //public ModelData.CustomValues CustomValues { get; set; }

        //[JsonProperty("default camera")]
        //public ModelData.DefaultCamera DefaultCamera { get; set; }

        [JsonProperty("default display unit")]
        public DisplayUnit DefaultDisplayUnit { get; set; }

        [JsonProperty("distance unit")]
        public DisplayUnit DistanceUnit { get; set; }

        //[JsonProperty("double sided geometry")]
        //public DoubleSidedGeometry DoubleSidedGeometry { get; set; }

        //[JsonProperty("fragmentTransformsOffset")]
        //public FragmentTransformsOffset FragmentTransformsOffset { get; set; }

        //[JsonProperty("georeference")]
        //public Georeference Georeference { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("navigation hint")]
        public DisplayUnit NavigationHint { get; set; }

        //[JsonProperty("stats")]
        //public Stats Stats { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }

        //[JsonProperty("view to model transform")]
        //public ViewToModelTransform ViewToModelTransform { get; set; }

        [JsonProperty("world bounding box")]
        public Dictionary<string, double[]> WorldBoundingBox { get; set; }

        //[JsonProperty("world front vector")]
        //public WorldVector WorldFrontVector { get; set; }

        //[JsonProperty("world north vector")]
        //public WorldVector WorldNorthVector { get; set; }

        //[JsonProperty("world up vector")]
        //public WorldVector WorldUpVector { get; set; }
    }
}
