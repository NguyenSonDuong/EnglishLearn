using English.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace English.Entity.DTOs.ViewModelDto
{
    public class CEFRLevelOption
    {
        public CEFRLevel Level { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ColorHex { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;
    }
}
