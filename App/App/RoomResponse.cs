using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace App
{
    internal class RoomResponse
    {

        private string status;
        private byte[] data = null;

        private RoomResponse(string status, byte[] data)
        {
            this.status = status;
            this.data = data;
        }



        public static void addReservation(RoomRequest req)
        {
            string reservationText = "Reservation Room: " + req.roomName + " Day: " + numToDay(req.resDay) +
                " Start: " + req.resStartHour + " End: " + req.resEndHour;
            File.AppendAllText("../../../example.txt", reservationText + Environment.NewLine);

        }

        public static void removeRoom(RoomRequest req)
        {
            string path = "../../../example.txt";
            string[] lines = File.ReadAllLines(path);

            File.WriteAllText(path, String.Empty);

            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (string s in lines)
                {
                    if (!s.Equals(req.roomName))
                    {
                        writer.WriteLine(s);
                    }
                }
            }
        }


        public static void addRoom(RoomRequest req)
        {
            File.AppendAllText("../../../example.txt", req.roomName + Environment.NewLine);
        }

        public static List<string> returnBusyHoursList(RoomRequest req) {

            List<string> busyHours = new List<string>();
            if (returnBusyHoursDict().Keys.Contains(req.roomName))
            {
                foreach (KeyValuePair<string, IDictionary<string, List<string>>> kv in returnBusyHoursDict())
                {
                    foreach (KeyValuePair<string, List<string>> kvp in kv.Value)
                    {
                        foreach (string val in kvp.Value)
                        {

                            if (kvp.Key == numToDay(req.resDay) && kv.Key == req.roomName)
                                busyHours.Add(val);
                        }
                    }

                }
            }
            return busyHours;
        }

        public static IDictionary<string, IDictionary<string, List<string>>> returnBusyHoursDict()
        {
            string path = "../../../example.txt";
            string[] lines = File.ReadAllLines(path);

            IDictionary<string, List<string>> dayAndBusyHours = new Dictionary<string, List<string>>();
            IDictionary<string, IDictionary<string, List<string>>> roomDayAndBusyHours = new Dictionary<string,
                IDictionary<string, List<string>>>();
            foreach (string s in lines)
            {
                string[] tokens = s.Split(' ');
                
                if (tokens[0] == "Reservation")
                {
                    List<string> busyHours = new List<string>();

                    string rName = tokens[2];
                    string rDay = tokens[4];
                    int start = Int32.Parse(tokens[6]);
                    int end = Int32.Parse(tokens[8]);

                    for (int hour = 9; hour <= 17; hour++)
                    {
                        if (hour >= start && hour <= end)
                        {
                            if (roomDayAndBusyHours.ContainsKey(rName))
                            {
                                foreach (KeyValuePair<string, List<string>> kvp in roomDayAndBusyHours[rName])
                                {
                                    if (!kvp.Value.Contains(Convert.ToString(hour)))
                                        busyHours.Add(Convert.ToString(hour));
                                }
                            }
                            else
                                busyHours.Add(Convert.ToString(hour));
                        }
                    }
                    if (!dayAndBusyHours.ContainsKey(rDay))
                    {
                        dayAndBusyHours.Add(rDay, busyHours);
                    }
                    else
                    {
                        dayAndBusyHours[rDay].AddRange(busyHours);
                    }
                    if (!roomDayAndBusyHours.ContainsKey(rName))
                    {
                        roomDayAndBusyHours.Add(rName, dayAndBusyHours);
                    }
                    else
                    {
                        roomDayAndBusyHours[rName].Concat(dayAndBusyHours);
                    }
                }       
            }     
            return roomDayAndBusyHours;
        }

        public static string numToDay(int number)
        {
            string day;
            switch (number)
            {
                case 1:
                    day = "Monday";
                    break;
                case 2:
                    day = "Tuesday";
                    break;
                case 3:
                    day = "Wednesday";
                    break;
                case 4:
                    day = "Thursday";
                    break;
                case 5:
                    day = "Friday";
                    break;
                case 6:
                    day = "Saturday";
                    break;
                case 7:
                    day = "Sunday";
                    break;
                default:
                    day = null;
                    break;
            }
            return day;


        }

        public static bool resValuesAreValid(RoomRequest req)
        {
            if (req.resStartHour < 9 || req.resEndHour > 17 || req.resDay < 1 ||
                req.resDay > 7 || req.resDuration == 0)
            {
                return false;
            }

            return true;
        }

        public static bool resCanBeAdded(RoomRequest req)
        {
            string path = "../../../example.txt";
            string[] lines = File.ReadAllLines(path);
            foreach (string s in lines)
            {
                string[] tokens = s.Split(' ');
                if (tokens[0] == "Reservation")
                {

                    string rName = tokens[2];
                    string rDay = tokens[4];
                    int start = Int32.Parse(tokens[6]);
                    int end = Int32.Parse(tokens[8]);

                    if (rName == req.roomName && (((req.resStartHour >= start) && (req.resStartHour < end))
                        || ((req.resEndHour > start) && (req.resEndHour <= end))) && numToDay(req.resDay) == rDay)
                    {
                        return false;
                    }
                }
            }
            return true;
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

        public static RoomResponse From(RoomRequest req)
        {
            if (req.operation == "add" && !contains(req.roomName))
            {
                addRoom(req);
                return roomAdded200OK(req);
            }
            else if (req.operation == "add" && contains(req.roomName))
            {
                return roomAlreadyExists403Forbidden(req);
            }
            else if (req.operation == "remove" && contains(req.roomName))
            {
                removeRoom(req);
                return roomRemoved200OK(req);
            }
            else if (req.operation == "remove" && !contains(req.roomName))
            {
                return roomDoesNotExist403Forbidden(req);
            }
            else if (req.operation == "reserve" && resCanBeAdded(req))
            {
                if (resValuesAreValid(req))
                {
                    addReservation(req);
                    return reservationAdded200OK(req);
                }
                else
                {
                    return resValuesAreNotValid400BadReq(req);
                }
            }
            else if (req.operation == "reserve" && !resCanBeAdded(req))
            {
                return roomIsAlreadyReserved403Forbidden(req);

            }
            else if (req.operation == "checkavailability" && (req.resDay < 1 || req.resDay > 7))
            {
                return checkedDayIsNotValid400BadReq(req);
            }
            else if (req.operation == "checkavailability" && contains(req.roomName))
            {
                return availableHours200OK(req);
            }
            else if (req.operation == "checkavailability" && !contains(req.roomName))
            {
                return checkedRoomDoesNotExist404NotFound(req);
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

        public static RoomResponse checkedDayIsNotValid400BadReq(RoomRequest req)
        {
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Error</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>400 Bad Request</h1>" +
                    "Day is not valid.</BODY>\n" +
                    "</HTML>";

            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new RoomResponse("400 Bad Request", d);
        }

        public static RoomResponse checkedRoomDoesNotExist404NotFound(RoomRequest req)
        {
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Error</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>404 Not Found</h1>" +
                    "Room with name " + req.roomName + " does not exist.</BODY>\n" +
                    "</HTML>";

            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new RoomResponse("404 Not Found", d);
        }

        public static RoomResponse availableHours200OK(RoomRequest req)
        {
            string availableHours = "";
            for(int hour = 9; hour <= 17; hour++)
            {
                if(!returnBusyHoursList(req).Contains(Convert.ToString(hour)))
                    availableHours += " " + Convert.ToString(hour);
            }
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Available Hours</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>200 OK</h1>" +
                    "On " + numToDay(req.resDay) + ", Room " + req.roomName + "  is available for the following hours:" +
                    availableHours + ".</BODY>\n" +
                    "</HTML>";

            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new RoomResponse("200 OK", d);
        }

        public static RoomResponse resValuesAreNotValid400BadReq(RoomRequest req)
        {
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Error</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>400 Bad Request</h1>" +
                    "Values entered are not valid.</BODY>\n" +
                    "</HTML>";

            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new RoomResponse("400 Bad Request", d);
        }

        public static RoomResponse roomIsAlreadyReserved403Forbidden(RoomRequest req)
        {
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Error</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>403 Forbidden</h1>" +
                    "Room with name " + req.roomName + " cannot be reserved. </BODY>\n" +
                    "</HTML>";

            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new RoomResponse("403 Forbidden", d);
        }
        public static RoomResponse reservationAdded200OK(RoomRequest req)
        {
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Reservation Successful</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>200 OK</h1>" +
                    "Room with name " + req.roomName + " is reserved on " + numToDay(req.resDay) + " " + req.resStartHour
                    + ":00-" + req.resEndHour + ":00. </BODY>\n" +
                    "</HTML>";

            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new RoomResponse("200 OK", d);
        }
        public static RoomResponse roomDoesNotExist403Forbidden(RoomRequest req)
        {
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Error</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>403 Forbidden</h1>" +
                    "Room with name " + req.roomName + " does not exist. </BODY>\n" +
                    "</HTML>";

            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new RoomResponse("403 Forbidden", d);
        }


        public static RoomResponse roomRemoved200OK(RoomRequest req)
        {
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Room Removed</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>200 OK</h1>" +
                    "Room with name " + req.roomName + " is successfully removed. </BODY>\n" +
                    "</HTML>";

            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new RoomResponse("200 OK", d);
        }

        public static RoomResponse roomAlreadyExists403Forbidden(RoomRequest req)
        {
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Error</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>403 Forbidden</h1>" +
                    "Room with name " + req.roomName + " already exists. </BODY>\n" +
                    "</HTML>";

            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new RoomResponse("403 Forbidden", d);
        }

        public static RoomResponse roomAdded200OK(RoomRequest req)
        {
            string output = "<HTML>\n" +
                    "<HEAD>\n" +
                    "<link rel=\"icon\" href=\"data:,\">\n" +
                    "<TITLE>Room Added</TITLE>\n" +
                    "</HEAD>\n" +
                    "<BODY> <h1>200 OK</h1>" +
                    "Room with name " + req.roomName + " is successfully added.</BODY>\n" +
                    "</HTML>";

            BinaryReader br = new BinaryReader(stringToStream(output));
            byte[] d = new byte[stringToStream(output).Length];
            br.Read(d, 0, d.Length);

            return new RoomResponse("200 OK", d);
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
