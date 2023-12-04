using Licenta.Commons.Utils;
using Licenta.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;

namespace Helpers.CurveDetectorDataSetGenerator
{
    public static class IAMXMLTranslate
    {
        static string IAM_XML_PATH = @"D:\Users\Stefan\Datasets\IAM\xml";
        static string IAM_BEZ_PATH = @"D:\Users\Stefan\licenta\proiect_licenta\app\Licenta\Helpers.CurveDetectorDataSetGenerator\bin\Debug\iam_bez\data";

        static Dictionary<string, Corner[]> paths = new Dictionary<string, Corner[]>();


        public static void Run()
        {
            foreach (var file in Directory.EnumerateFiles(IAM_BEZ_PATH, "*.txt", SearchOption.AllDirectories)) 
            {
                string id = Path.GetFileNameWithoutExtension(file);
                paths[id] = File.ReadAllLines(file)
                    .Select(line =>
                    {
                        var split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToArray();
                        if (split.Length == 0) return null;
                        return new Corner((split[0], split[1]), (split[2], split[3]), (split[4], split[5])) { Alpha = split[6] };
                    })
                    .Where(_ => _ != null).ToArray();
            }            

            foreach (var file in Directory.EnumerateFiles(IAM_XML_PATH))
            {
                string id = Path.GetFileNameWithoutExtension(file);
                Console.WriteLine(id);

                var xml = new XmlDocument();
                xml.Load(file);

                var printed_lines = xml.GetElementsByTagName("machine-printed-part").Item(0);

                string text = "";
                int lines_count = 0;
                foreach (XmlElement tag in printed_lines.ChildNodes) 
                {                    
                    var line = HttpUtility.HtmlDecode(tag.Attributes["text"].Value);
                    text += line + "\r\n";
                    lines_count++;
                }

                if(!paths.ContainsKey(id))
                {
                    Debug.WriteLine($"Key not found: {id}");
                    continue;
                }                

                int x0 = 4000, y0 = 4000, x1 = 0, y1 = 0;

                foreach(XmlElement cmp in xml.GetElementsByTagName("cmp"))
                {
                    int x = int.Parse(cmp.GetAttribute("x"));
                    int y = int.Parse(cmp.GetAttribute("y"));
                    int w = int.Parse(cmp.GetAttribute("width"));
                    int h = int.Parse(cmp.GetAttribute("height"));
                    x0 = Math.Min(x0, x);
                    y0 = Math.Min(y0, y);
                    x1 = Math.Max(x1, x + w);
                    y1 = Math.Max(y1, y + h);
                }

                Console.WriteLine($"Bounds = {x0} {y0} {x1} {y1}");

                var corners = paths[id].Where(c =>
                    c.A.X.IsBetween(x0, x1) && c.A.Y.IsBetween(y0, y1) &&
                    c.B0.X.IsBetween(x0, x1) && c.B0.Y.IsBetween(y0, y1) &&
                    c.B1.X.IsBetween(x0, x1) && c.B1.Y.IsBetween(y0, y1)
                ).ToArray();                

                StringBuilder sb = new StringBuilder();

                sb.AppendLine(lines_count.ToString());
                sb.AppendLine(text);

                sb.AppendLine($"{x0} {y0}");
                sb.AppendLine(corners.Length.ToString());

                foreach (var corner in corners)
                    sb.AppendLine($"{corner.B0.X - x0:F1} {corner.B0.Y - y0:F1} {corner.A.X - x0:F1} {corner.A.Y - y0:F1} {corner.B1.X - x0:F1} {corner.B1.Y - y0:F1} {(corner.Alpha):F3}");

                File.WriteAllText(
                    $@"D:\Users\Stefan\licenta\proiect_licenta\app\Licenta\Helpers.CurveDetectorDataSetGenerator\bin\Debug\iam_bez_form\{id}.txt",
                    sb.ToString());



            }
        }

    }
}
