﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class Utils
    {
        public static string GetLogDir()
        {
            return System.IO.Path.Combine(
                           Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                           @"retroIngeneering");
        }

        public static string GetTimeStamp()
        {
            var now = DateTime.Now;
            return now.ToString("yyyy-MM-dd--HH-mm-ss-fff");
        }
    }
}
