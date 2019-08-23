using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Copyfile.Models
{
    public class AppView
    {
        public string Id { get; set; }
        public string AppName { get; set; }
        public string AppPhysicalPath { get; set; }
        public string AppAlias { get; set; }
        public int Status { get; set; }

    }
}
