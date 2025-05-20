using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ater.Common.Options;
public class SMSOption
{

    public const string ConfigPath = "SMS";
    public required string AccessKeyId { get; set; }
    public required string AccessKeySecret { get; set; }

    public required string Sign { get; set; }
}
