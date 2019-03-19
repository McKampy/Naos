﻿namespace Naos.Core.Common
{
    using System.IO;

    public static partial class Extensions
    {
        public static byte[] ReadAllBytes(this BinaryReader source)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
                {
                    ms.Write(buffer, 0, count);
                }

                return ms.ToArray();
            }
        }
    }
}
