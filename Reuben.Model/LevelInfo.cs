﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Reuben.Model
{
    [DataContract]
    public class LevelInfo
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string File { get; set; }
    }
}