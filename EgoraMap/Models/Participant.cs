using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EgoraMap.Models
{
    public class Participant
    {
        public int ParticipantId { get; set; }
        public string Name { set; get; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DateReg { get; set; }
    }
}
