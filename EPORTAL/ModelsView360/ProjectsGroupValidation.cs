using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPORTAL.ModelsView360
{
    public class ProjectsGroupValidation
    {
        public int IDGroup { get; set; }
        public string GroupName { get; set; }
        // Nullable parent ID - root groups co ParentIDGroup = null
        public int? ParentIDGroup { get; set; }
    }
}