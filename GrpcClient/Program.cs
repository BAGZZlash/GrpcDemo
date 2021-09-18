using Google.Protobuf;
using Grpc.Net.Client;
using GrpcServer;
using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Threading.Tasks;

//using System.Text.Json;
//using System.Text.Json.Serialization;

//using System.Runtime.Serialization.Formatters.Binary;
//using System.Runtime.Serialization;
//using System.IO;

//using System.Xml;
//using System.Xml.Serialization;
using System.IO;

using System.Runtime.Serialization;

namespace GrpcClient
{
    public class Shapefile
    {
        public string sFilename;
        public int firstLayer = 0;
        public Boolean Loaded = false;
        public DataSource ds;
        public Layer Layer;

        public Boolean LoadShapeFile(string sFilename)
        {            
            Shapefile MyShapeFile = new Shapefile();
            MyShapeFile.sFilename = sFilename;
            bool RetVal = MyShapeFile.InitLayer(MyShapeFile.sFilename);
            if (firstLayer == 0) firstLayer = 1;
            return (RetVal);
        }

        public Boolean InitLayer(string sFilename)
        {
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll(); // Register all drivers
            ds = Ogr.Open(sFilename, 0); // 0 means read-only, 1 means modifiable

            if (ds == null)
            {
                Console.WriteLine("Failed to open file [{0}]!", sFilename);
                return (false);
            }

            Layer = ds.GetLayerByIndex(0);
            if (Layer == null)
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
            Shapefile TestShapeFile = new Shapefile();
            TestShapeFile.sFilename = "C:/Users/ccroo/ownCloud/WFLO/Vortex/Demo/Testdata/Point_4326.shp";
            Boolean LoadSuccess = TestShapeFile.LoadShapeFile(TestShapeFile.sFilename);

            Console.WriteLine("Shapefile loaded successfully: {0}", LoadSuccess);

            byte[] TestArray = { 17, 22, 19, 5, 93 };
            TestArray[0] = 18;
            ByteString MyBytes = ByteString.CopyFrom(TestArray);
            string Banane = MyBytes.ToStringUtf8();

            //byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(TestShapeFile); //Using-Zeug siehe oben!
            //MyBytes = ByteString.CopyFrom(jsonUtf8Bytes);
            //Banane = MyBytes.ToStringUtf8();

            //Banane = System.Runtime.Serialization(TestShapeFile);

            //IFormatter formatter = new BinaryFormatter(); //Using-Zeug siehe oben! https://docs.microsoft.com/de-de/dotnet/api/system.runtime.serialization.serializationinfo?view=net-5.0
            //MemoryStream memStream = new MemoryStream(100); //https://docs.microsoft.com/de-de/dotnet/api/system.io.memorystream?view=net-5.0
            //formatter.Serialize(memStream, TestShapeFile);

            //memStream.Seek(0, 0);
            //TestShapeFile = (Shapefile)formatter.Deserialize(memStream);
            //memStream.Close();
            //Console.WriteLine("Filename: {0}", TestShapeFile.sFilename);

            //XmlSerializer serializer = new XmlSerializer(obj.GetType());
            //using (StringWriter writer = new StringWriter())
            //{
            //    serializer.Serialize(writer, obj);
            //    return writer.ToString();
            //}

            DataContractSerializer ser = new DataContractSerializer(typeof(Shapefile)); //https://docs.microsoft.com/de-de/dotnet/api/system.runtime.serialization.datacontractserializer?view=net-5.0
            FileStream writer = new FileStream("C:/Temp/Test.dta", FileMode.Create);
            ser.WriteObject(writer, TestShapeFile);

            //XmlSerializer serializer = new XmlSerializer(typeof(Shapefile)); //https://docs.microsoft.com/en-us/dotnet/api/system.xml.serialization.xmlserializer?view=net-5.0
            //MemoryStream memStream = new MemoryStream(100);
            //serializer.Serialize(memStream, TestShapeFile);

            var input = new HelloRequest { Name = Banane };

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
