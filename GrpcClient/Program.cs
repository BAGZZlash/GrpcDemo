using Grpc.Net.Client;
using GrpcServer;
using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Threading.Tasks;

namespace GrpcClient
{
    public class Shapefile
    {
        public string sFilename;
        private Layer Layer;

        public void InitLayer(string sFilename)
        {
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll(); // Register all drivers
            DataSource ds = Ogr.Open(sFilename, 0); // 0 means read-only, 1 means modifiable
            if (ds == null)
            {
                Console.WriteLine("Failed to open file [{0}]!", sFilename);
            }

            Layer = ds.GetLayerByIndex(0);
            if (Layer == null)
            {
                Console.WriteLine("Get the {0}th layer failed! n", "0");
            }
        }
    }

    class Program
    {
        public int firstLayer = 0;
        public Shapefile LoadShapeFile(string sFilename)
        {
            Shapefile MyShapeFile = new Shapefile();
            MyShapeFile.sFilename = sFilename;
            MyShapeFile.InitLayer(MyShapeFile.sFilename);
            if (firstLayer == 0)
            {
                firstLayer = 1;
            }

            return(MyShapeFile);
        }

        static async Task Main(string[] args)
        {
            Shapefile TestShapeFile;
            TestShapeFile = LoadShapeFile("C:/Temp/Cloud/ownCloud/WFLO/Vortex/Demo/Testdata/Point_4326.shp");

            Console.WriteLine("Please enter your name: ");
            string Username = Console.ReadLine();

            var input = new HelloRequest { Name = Username };

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
