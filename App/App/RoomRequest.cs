using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    internal class RoomRequest
    {
        public string operation { get; set; }
        public string roomName { get; set; }
        public int resDay { get; set; }
        public int resStartHour { get; set; }
        public int resEndHour { get; set; }
        public int resDuration { get; set; }

        public RoomRequest(string operation, string roomName, int resDay,
            int resStartHour, int resEndHour, int resDuration)
        {
            this.operation = operation;
            this.roomName = roomName;
            this.resDay = resDay;
            this.resStartHour = resStartHour;
            this.resEndHour = resEndHour;
            this.resDuration = resDuration;
        }

        public static bool checkValueIsValid(string value)
        {

            int intValue;
            if (int.TryParse(value, out intValue))
            {
                return true;
            }
            return false;

        }

        public static RoomRequest GetRequest(String request)
        {
            if (string.IsNullOrEmpty(request))
                return null;

            int startIndexofOp = request.IndexOf('/');
            int endIndexofOp = request.IndexOf('?');
            string op = request.Substring(startIndexofOp + 1, endIndexofOp - startIndexofOp - 1);


            int startIndexofRoomName = request.IndexOf('=');
            int endIndexofRoomName;
            if (op == "add" || op == "remove")
                endIndexofRoomName = request.IndexOf(' ', startIndexofRoomName);
            else
                endIndexofRoomName = request.IndexOf('&', startIndexofRoomName);
            string rName = request.Substring(startIndexofRoomName + 1, endIndexofRoomName - startIndexofRoomName - 1);


            int startIndexofDay = 0; 
            int endIndexofDay = 0;
            int day = 0;

            int startIndexofHour = 0;
            int endIndexofHour = 0;
            int sHour = 0;

            int startIndexofDuration = 0;
            int endIndexofDuration = 0;
            int dur = 0;

            if (op == "checkavailability" || op == "reserve")
            {
                startIndexofDay = request.IndexOf('=', endIndexofRoomName);
                if (op == "checkavailability")
                    endIndexofDay = request.IndexOf(' ', startIndexofDay);
                else
                    endIndexofDay = request.IndexOf('&', startIndexofDay);
                string rDay = request.Substring(startIndexofDay + 1, endIndexofDay - startIndexofDay - 1);

                if (checkValueIsValid(rDay))
                    day = Int32.Parse(rDay);
            }



            if (op == "reserve")
            {
                startIndexofHour = request.IndexOf('=', endIndexofDay); ;
                endIndexofHour = request.IndexOf('&', startIndexofHour);
                string startHour = request.Substring(startIndexofHour + 1, endIndexofHour - startIndexofHour - 1);

                if (checkValueIsValid(startHour))
                    sHour = Int32.Parse(startHour);

                startIndexofDuration = request.IndexOf('=', endIndexofHour); ;
                endIndexofDuration = request.IndexOf(' ', startIndexofDuration);
                string duration = request.Substring(startIndexofDuration + 1, endIndexofDuration - startIndexofDuration - 1);

                if (checkValueIsValid(duration))
                    dur = Int32.Parse(duration);
            }

            int endHour = sHour + dur;

            return new RoomRequest(op, rName, day, sHour, endHour, dur);
        }
    }
}
