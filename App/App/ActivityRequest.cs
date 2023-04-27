using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    internal class ActivityRequest
    {
        public string operation { get; set; }
        public string activityName { get; set; }


        public ActivityRequest(string operation, string activityName)
        {
            this.operation = operation;
            this.activityName = activityName;

        }

        public static ActivityRequest GetRequest(String request)
        {
            if (string.IsNullOrEmpty(request))
                return null;
            int startIndexofOp = request.IndexOf('/');
            int endIndexofOp = request.IndexOf('?');
            string op = request.Substring(startIndexofOp + 1, endIndexofOp - startIndexofOp - 1);

            int startIndexofRoomName = request.IndexOf('=');
            int endIndexofRoomName = request.IndexOf(' ', startIndexofRoomName);
            string actName = request.Substring(startIndexofRoomName + 1, endIndexofRoomName - startIndexofRoomName - 1);

            return new ActivityRequest(op, actName);
        }
    }
}
