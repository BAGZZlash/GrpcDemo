using Google.Protobuf;
using Grpc.Net.Client;
using GrpcServer;

using OSGeo.GDAL;
using OSGeo.OGR;

using System;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace GrpcClient
{
    public static class DataSourceExtensions
    {
        public static string SerializeToJson(this DataSource self)
        {
            var jsonRoot = new JObject();

            var name = self.GetName();
            jsonRoot.Add("Name", name);

            var layerCount = self.GetLayerCount();
            jsonRoot.Add("LayerCount", layerCount);

            var jsonLayerList = new JObject();
            for (int layerIndex = 0; layerIndex < layerCount; ++layerIndex)
            {
                using (var layer = self.GetLayerByIndex(layerIndex))
                {
                    var jsonLayer = new JObject();

                    var featureCount = layer.GetFeatureCount(0);
                    jsonLayer.Add("FeatureCount", featureCount);

                    var jsonFeatureList = new JObject();
                    for (int featureIndex = 0; featureIndex < featureCount; ++featureIndex)
                    {
                        using (var feature = layer.GetFeature(featureIndex))
                        {
                            using (var geo = feature.GetGeometryRef())
                            {
                                var options = new string[] { };
                                var json = geo.ExportToJson(options);
                                var jsonFeature = JObject.Parse(json);

                                jsonFeatureList.Add($"{featureIndex}", jsonFeature);

                            }
                        }
                    }
                    jsonLayer.Add("Features", jsonFeatureList);

                    jsonLayerList.Add($"{layerIndex}", jsonLayer);
                }
            }
            jsonRoot.Add("Layers", jsonLayerList);

            return jsonRoot.ToString();
        }
    }

    public class Shapefile
    {
        private string filename;
        public DataSource ds;
        public string JSONString;
        public string sFilename
        {
            get { return filename; }
            set { filename = value; }
        }

        public Shapefile() { }

        public Shapefile(string FileName) { filename = FileName; }

        public Boolean LoadShapeFile(string LoadName)
        {            
            filename = LoadName;
            bool RetVal = InitLayer(LoadName);
            return (RetVal);
        }

        public Boolean InitLayer(string LoadName)
        {
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Gdal.SetConfigOption("PROJ_DEBUG", "5");
            Ogr.RegisterAll(); // Register all drivers
            
            ds = Ogr.Open(LoadName, 0); // 0 means read-only, 1 means modifiable
            JSONString = ds.SerializeToJson();

            if (ds == null)
            {
                Console.WriteLine("Failed to open file [{0}]!", sFilename);
                return (false);
            }

            if (ds.GetLayerByIndex(0) == null)
            {
                Console.WriteLine("Getting the {0}th layer failed! \n", "0");
                return (false);
            }
            return (true);
        }
    }

    public class Program
    {
        static async Task Main(string[] args)
        {
            //Environment.SetEnvironmentVariable("PROJ_LIB", "C:\\Users\\croonenbroeck\\source\\repos\\GrpcDemo\\GrpcClient\\bin\\Debug\\netcoreapp3.1\\runtimes\\win-x64\\native\\maxrev.gdal.core.libshared");
            // "PROJ_LIB" muss in den Umgebungsvariablen eingetragen sein siehe hier: https://github.com/OSGeo/gdal/issues/1647 und hier: https://stackoverflow.com/questions/4788398/changes-via-setenvironmentvariable-do-not-take-effect-in-library-that-uses-geten

            Shapefile TestShapeFile = new Shapefile("C:/Temp/Cloud/ownCloud/WFLO/Vortex/Demo/Testdata/Gemeinde.shp");
            //Shapefile TestShapeFile = new Shapefile("C:/Users/ccroo/ownCloud/WFLO/Vortex/Demo/Testdata/Point_4326.shp");
            Boolean LoadSuccess = TestShapeFile.LoadShapeFile(TestShapeFile.sFilename);

            Console.WriteLine("Shapefile loaded successfully: {0}", LoadSuccess);

            Console.WriteLine(TestShapeFile.JSONString);

            var input = new HelloRequest { Name = TestShapeFile.JSONString };

            Console.WriteLine("\nOpening channel.");
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);

            Console.WriteLine("Sending greeter request.");
            var reply = await client.SayHelloAsync(input);

            Console.WriteLine("\nServer replied:");
            Console.WriteLine(reply.Message);

            Console.ReadLine();
        }
    }
}
