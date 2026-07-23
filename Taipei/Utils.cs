using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taipei.Utils
{
    class Utils
    {
        public static LoginConfig? InitConfig()
        {
            string exeDir = AppContext.BaseDirectory;
            string configPath = Path.Combine(exeDir, "config.json");

            if (!File.Exists(configPath)) return null;

            string json = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<LoginConfig>(json);
        }
    }
}
