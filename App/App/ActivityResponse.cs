using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    internal class ActivityResponse
    {

        private string status;
        private byte[] data = null;

        private ActivityResponse(string status, byte[] data)
        {
            this.status = status;
            this.data = data;
        }

        public static void removeRoom(ActivityRequest req)
        {
            string path = "../../../example.txt";
            string[] lines = File.ReadAllLines(path);

            File.WriteAllText(path, String.Empty);

            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (string s in lines)
                {
                    if (!s.Equals(req.activityName))
                    {
                        writer.WriteLine(s);
                    }
                }
            }
        }

        public static void addRoom(ActivityRequest req)
        {
            File.AppendAllText("../../../example.txt", req.activityName + Environment.NewLine);
        }
        public static bool contains(string name)
        {
            string[] lines = File.ReadAllLines("../../../example.txt");
            foreach (string s in lines)
            {
                if (s == name) return true;
            }
            return false;
        }

        public static ActivityResponse From(ActivityRequest req)
        {
            if (req.operation == "add" && !contains(req.activityName))
            {
                addRoom(req);
                return activityAdded200OK(req);
            }
            else if (req.operation == "add" && contains(req.activityName))
            {
                return activityAlreadyExists403Forbidden(req);
            }
            else if (req.operation == "remove" && contains(req.activityName))
            {
                removeRoom(req);
                return activityRemoved200OK(req);
            }
            else if (req.operation == "remove" && !contains(req.activityName))
            {
                return activityDoesNotExistToRemove403Forbidden(req);
            }
            else if (req.operation == "check" && contains(req.activityName))
            {
                return checkedActivityExists200OK(req);
            }
            else if (req.operation == "check" && !contains(req.activityName))
            {
                return checkedActivityDoesNotExist404NotFound(req);
            }
            return null;


        }

        public static Stream stringToStream(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static ActivityResponse checkedActivityDoesNotExist404NotFound(ActivityRequest req)
        {
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Error</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>404 Not Found</h1>" +
                    "Activity with name " + req.activityName + " does not exist. </BODY>\n" +
                    "</HTML>";

            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new ActivityResponse("404 Not Found", d);
        }

        public static ActivityResponse checkedActivityExists200OK(ActivityRequest req)
        {
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Activity Exists</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>200 OK</h1>" +
                    "Activity with name " + req.activityName + " exists. </BODY>\n" +
                    "</HTML>";

            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new ActivityResponse("200 OK", d);
        }

        public static ActivityResponse activityDoesNotExistToRemove403Forbidden(ActivityRequest req)
        {
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Error</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>403 Forbidden</h1>" +
                    "Activity with name " + req.activityName + " does not exist. </BODY>\n" +
                    "</HTML>";

            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new ActivityResponse("403 Forbidden", d);
        }

        public static ActivityResponse activityRemoved200OK(ActivityRequest req)
        {
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Activity Removed</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>200 OK</h1>" +
                    "Activity with name " + req.activityName + " is successfully removed. </BODY>\n" +
                    "</HTML>";

            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new ActivityResponse("200 OK", d);
        }

        public static ActivityResponse activityAlreadyExists403Forbidden(ActivityRequest req)
        {
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Error</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>403 Forbidden</h1>" +
                    "Activity with name " + req.activityName + " already exists. </BODY>\n" +
                    "</HTML>";


            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new ActivityResponse("403 Forbidden", d);
        }

        public static ActivityResponse activityAdded200OK(ActivityRequest req)
        {
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Activity Added</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>200 OK</h1>" +
                    "Activity with name " + req.activityName + " is successfully added.</BODY>\n" +
                    "</HTML>";

            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new ActivityResponse("200 OK", d);
        }

        public void Post(NetworkStream stream)
        {
            StringBuilder sbHeader = new StringBuilder();
            sbHeader.AppendLine("HTTP/1.1" + " " + status);
            sbHeader.AppendLine();

            List<byte> response = new List<byte>();
            response.AddRange(Encoding.ASCII.GetBytes(sbHeader.ToString()));
            response.AddRange(data);

            byte[] responseByte = response.ToArray();
            stream.Write(responseByte, 0, responseByte.Length);
        }

    }

}
