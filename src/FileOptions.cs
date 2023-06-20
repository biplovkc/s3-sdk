using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biplov.S3.Sdk;

public class FileOptions
{
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string Region { get; set; }
    public string ServiceUrl { get; set; }
    public string BucketName { get; set; }
}