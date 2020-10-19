using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace CompressInMemory
{
    static class Program
    {
        static List<XmlDocument> xmlDocuments = new List<XmlDocument>();
        static int threadCount = 0;
        static List<byte[]> bytesList = new List<byte[]>();

        static void Main(string[] args)
        {
            int files = 0;

            Console.WriteLine("How many files you want to manipulate?");
            files = int.Parse(Console.ReadLine());


            Console.WriteLine(string.Format("Generate {0} big XML file", files));

            XmlDocument xmlDocumentPrincipal = GenereteBigXmlFile();

            //Generate files in memory to test.
            for(int i = 0; i < files; i++)
            {
                xmlDocuments.Add(xmlDocumentPrincipal);
            }

            Console.WriteLine("Start process to zip files...");
            Console.WriteLine();

            Parallel.ForEach(xmlDocuments, (item) => {
                bytesList.Add(SerializeAndCompress(item.OuterXml));
                threadCount++;
            });

            //foreach(XmlDocument item in xmlDocuments)
            //{
            //    new Thread(delegate() {
            //        bytesList.Add(SerializeAndCompress(item.OuterXml));
            //        threadCount++;
            //    }).Start();
            //}

            while(threadCount < files)
            {
                Thread.Sleep(500);
            }

            Console.WriteLine();
            Console.WriteLine("Finish processes to zip files...");
            Console.WriteLine();
            Console.WriteLine("Start processes to unzip files...");
            Console.WriteLine();

            threadCount = 0;

            Parallel.ForEach(bytesList, (item) =>
            {
                DecompressAndDeserialize<string>(item);
                threadCount++;
            });

            //foreach(byte[] item in bytesList)
            //{
            //    new Thread(delegate () {
            //        DecompressAndDeserialize<string>(item);
            //        threadCount++;
            //    }).Start();
            //}

            while (threadCount < files)
            {
                Thread.Sleep(500);
            }

            Console.WriteLine();
            Console.WriteLine("Finish processes to unzip files...");
            Console.WriteLine();

            Console.ReadLine();
        }

        static XmlDocument GenereteBigXmlFile()
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlNode principal = xmlDocument.CreateElement("principal");
            xmlDocument.AppendChild(principal);
            XmlNode xmlNode = null;

            for(int i = 0; i <= 200000; i++)
            {
                xmlNode = xmlDocument.CreateNode(XmlNodeType.Element, "pessoa-" + i.ToString(), string.Empty);
                xmlNode.AppendChild(xmlDocument.CreateNode(XmlNodeType.Element, "teste-" + i.ToString(), string.Empty));
                xmlNode.InnerText = "dasdasdasdasdasdasdadapdaspdoaipsodapodkampod foasnaosinfvoasdnva[ósioncvaoiofvnaodiofvnodifvaoidfnaoivndf'voahndfvóahdfvoías'dfoviovbdfóibvdfoibva'dfvba''jfvba'bdfvá'djfvádf";
                xmlNode.AppendChild(xmlDocument.CreateNode(XmlNodeType.Element, "teste-" + i.ToString(), string.Empty));
                xmlNode.InnerText = "dasdasdasdasdasdasdadapdaspdoaipsodapodkampod foasnaosinfvoasdnva[ósioncvaoiofvnaodiofvnodifvaoidfnaoivndf'voahndfvóahdfvoías'dfoviovbdfóibvdfoibva'dfvba''jfvba'bdfvá'djfvádf";
                xmlNode.AppendChild(xmlDocument.CreateNode(XmlNodeType.Element, "teste-" + i.ToString(), string.Empty));
                xmlNode.InnerText = "dasdasdasdasdasdasdadapdaspdoaipsodapodkampod foasnaosinfvoasdnva[ósioncvaoiofvnaodiofvnodifvaoidfnaoivndf'voahndfvóahdfvoías'dfoviovbdfóibvdfoibva'dfvba''jfvba'bdfvá'djfvádf";
                xmlNode.AppendChild(xmlDocument.CreateNode(XmlNodeType.Element, "teste-" + i.ToString(), string.Empty));
                xmlNode.InnerText = "dasdasdasdasdasdasdadapdaspdoaipsodapodkampod foasnaosinfvoasdnva[ósioncvaoiofvnaodiofvnodifvaoidfnaoivndf'voahndfvóahdfvoías'dfoviovbdfóibvdfoibva'dfvba''jfvba'bdfvá'djfvádf";
                xmlNode.AppendChild(xmlDocument.CreateNode(XmlNodeType.Element, "teste-" + i.ToString(), string.Empty));
                xmlNode.InnerText = "dasdasdasdasdasdasdadapdaspdoaipsodapodkampod foasnaosinfvoasdnva[ósioncvaoiofvnaodiofvnodifvaoidfnaoivndf'voahndfvóahdfvoías'dfoviovbdfóibvdfoibva'dfvba''jfvba'bdfvá'djfvádf";
                xmlNode.AppendChild(xmlDocument.CreateNode(XmlNodeType.Element, "teste-" + i.ToString(), string.Empty));
                xmlNode.InnerText = "dasdasdasdasdasdasdadapdaspdoaipsodapodkampod foasnaosinfvoasdnva[ósioncvaoiofvnaodiofvnodifvaoidfnaoivndf'voahndfvóahdfvoías'dfoviovbdfóibvdfoibva'dfvba''jfvba'bdfvá'djfvádf";
                xmlNode.AppendChild(xmlDocument.CreateNode(XmlNodeType.Element, "teste-" + i.ToString(), string.Empty));

                principal.AppendChild(xmlNode);
            }

            return xmlDocument;
        }

        public static byte[] SerializeAndCompress(this object obj)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml((string)obj);

            using (MemoryStream stream = new MemoryStream())
            {
                xmlDocument.Save(stream);
                Console.WriteLine("Uncompress size: " + (stream.Length / 1024).ToString() + "MB on memory");
            }

            var watch = System.Diagnostics.Stopwatch.StartNew();
            byte[] result = null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream zs = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(zs, obj);
                    result = ms.ToArray();
                }
            }

            watch.Stop();

            Console.WriteLine("Compress size is: " + (result.Length / 1024).ToString());
            Console.WriteLine("Time to execute: " + watch.ElapsedMilliseconds.ToString());

            return result;
        }

        public static T DecompressAndDeserialize<T>(this byte[] data)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            T result;

            using (MemoryStream ms = new MemoryStream(data))
            using (GZipStream zs = new GZipStream(ms, CompressionMode.Decompress, true))
            {
                BinaryFormatter bf = new BinaryFormatter();
                result = (T)bf.Deserialize(zs);
            }

            Console.WriteLine("Time to execute: " + watch.ElapsedMilliseconds.ToString());

            return result;
        }
    }
}
