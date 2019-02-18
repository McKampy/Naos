﻿namespace Naos.Core.FileStorage.Domain
{
    using System;
    using System.Diagnostics;
    using Humanizer;
    using Naos.Core.Common;
    using Naos.Core.Domain.Model;

    [DebuggerDisplay("Path = {Path}, Created = {Created}, Modified = {Modified}, Size = {Size} bytes")]
    public class FileInformation
    {
        public string Path { get; set; }

        public string Name { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        /// <summary>
        /// Size in Bytes
        /// </summary>
        public long Size { get; set; }

        public string PrettySize => this.Size.Bytes().ToString("#.##");

        public DataDictionary Properties => new DataDictionary();

        public ContentType ContentType =>
            !this.Name.IsNullOrEmpty() ? ContentTypeExtensions.FromFilename(this.Name) : ContentTypeExtensions.FromFilename(this.Path);
    }
}
